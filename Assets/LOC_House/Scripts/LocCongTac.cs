using System.Collections.Generic;
using UnityEngine;

// Công tắc điện bật/tắt được. Nhấn E khi nhìn vào cụm công tắc (LocFirstPerson) hoặc gọi Toggle() từ script game.
// dens: các đèn nối với công tắc này (có thể để trống — chỉ lật phím). Phím = con tên "Phim" của cụm (lật ±7°).
// [27/10 chơi thử] trước đây chỉ công tắc hầm được nối đèn, các công tắc khác bấm chỉ lật phím ("bật/tắt đèn ở bếp không thấy gì").
//  Nay dens để trống thì tự nối theo tên cụm (bảng NOI bên dưới, tên đèn "Den_<...>" do LocHouseBuilder.L đặt).
//  · Hai công tắc cùng một đèn (bếp, hành lang T2) = công tắc hai chiều: bấm bên nào cũng đảo đèn.
//  · Đèn tắt thì bóng/tuýp phát sáng gần đèn cũng tắt theo.
//  · Đang mất điện (LocMa) thì bấm chỉ ghi nhớ, có điện lại mới áp.
//  · Bấm công tắc làm con ma đang lấp ló rụt đi và lùi lần lấp ló sau (LocMa.KhiBamCongTac).
public class LocCongTac : MonoBehaviour
{
    public string ten = "công tắc";
    public Light[] dens;
    public GameObject[] hienKhiBat;   // vật chỉ hiện khi bật (bóng sáng…)
    public bool bat = true;

    Transform[] phim;
    bool tuNoi;   // nối tự động: giữ nguyên trạng thái đèn lúc dựng, không ép theo phím

    // tên cụm (bỏ "CongTac_") → tên đèn (bỏ "Den_"). PhongNhim không nối: đèn ngủ của Nhím do cốt truyện điều khiển.
    static readonly Dictionary<string, string[]> NOI = new Dictionary<string, string[]>
    {
        ["Hien"] = new[] { "Rap_Trai", "Rap_Phai" },
        ["PhongKhach"] = new[] { "PhongKhach_Tuyp", "PhongKhach_Chum", "PhongKhach_Sau" },
        ["SanhSau"] = new[] { "SanhSau", "ChieuNghi_T1" },
        ["Bep_Cua"] = new[] { "Bep", "GiengTroi" },
        ["Bep_GiengTroi"] = new[] { "Bep" },
        ["WC_T1"] = new[] { "WC_T1" },
        ["BoMe"] = new[] { "BoMe_OpTran" },
        ["HanhLang_ChieuNghi"] = new[] { "ChieuNghi_T2", "HanhLang_T2" },
        ["HanhLang_Khoi"] = new[] { "HanhLang_T2" },
        ["PhongKhoi"] = new[] { "Khoi_Tran", "DenBan_Khoi" },
        ["PhongTho"] = new[] { "PhongTho_1", "PhongTho_2" },
        ["Sanh_T3"] = new[] { "Sanh_T3" },
    };
    static readonly Dictionary<Light, bool> muon = new Dictionary<Light, bool>();      // trạng thái người chơi muốn của từng đèn
    static readonly Dictionary<Light, Renderer[]> bong = new Dictionary<Light, Renderer[]>();
    static readonly Dictionary<Renderer, Material[]> gocMat = new Dictionary<Renderer, Material[]>(), tatMat = new Dictionary<Renderer, Material[]>();
    static Renderer[] tatCaR;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset0() { muon.Clear(); bong.Clear(); gocMat.Clear(); tatMat.Clear(); tatCaR = null; }

    void Awake()
    {
        var l = new List<Transform>();
        foreach (var t in GetComponentsInChildren<Transform>(true)) if (t.name == "Phim") l.Add(t);
        phim = l.ToArray();
        if ((dens == null || dens.Length == 0) && name.StartsWith("CongTac_") && NOI.TryGetValue(name.Substring(8), out var ds)) { dens = TimDen(ds); tuNoi = true; }
        if (name == "CongTac_Ham") SuaDenHam();
        Ap();
    }

    static Light[] TimDen(string[] ten)
    {
        var kq = new List<Light>();
        foreach (var lt in FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            foreach (var t in ten) if (lt.name == "Den_" + t) { kq.Add(lt); break; }
        return kq.ToArray();
    }

    // [27/10 chơi thử] hầm: "đèn một nơi, ánh sáng một nơi" — đèn Den_BongDayToc_Ham dựng lệch 0,7 m so với bóng của model DenHam_FullAssembly,
    //  và bóng không sáng. Dời đèn về ngay dưới bóng, cho bóng phát sáng khi bật.
    void SuaDenHam()
    {
        GameObject g = null;
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == "BongDayToc" && t.position.y < -0.4f) { g = t.gameObject; break; }
        if (dens == null) return;
        foreach (var d in dens) if (d && d.name != "Den_BongDayToc_Ham") TaoBong(d);   // đèn chân thang, đèn chiếu tới: có sáng mà không có bóng
        if (!g) return;
        var r = g.GetComponentInChildren<Renderer>();
        var tam = r ? r.bounds.center : g.transform.position;
        foreach (var d in dens) if (d && d.name == "Den_BongDayToc_Ham") d.transform.position = tam + Vector3.down * 0.06f;
        if (r && r.sharedMaterial)
        {
            var m = new Material(r.sharedMaterial) { name = "M_BongDayToc_Sang" };
            m.SetColor("_EmissionColor", new Color(1f, 0.6f, 0.24f) * 3.5f); m.EnableKeyword("_EMISSION");
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", new Color(1f, 0.85f, 0.6f));
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            var tat = new Material(r.sharedMaterial) { name = "M_BongDayToc_Tat" };
            gocMat[r] = new[] { m }; tatMat[r] = new[] { tat };
            foreach (var d in dens) if (d && d.name == "Den_BongDayToc_Ham") bong[d] = new[] { r };
        }
    }

    // bóng dây tóc trần + dây điện thả từ trần (chỉ dùng ở hầm — luật dự án: không bóng trần ngoài hầm)
    static void TaoBong(Light d)
    {
        var p = d.transform.position;
        if (tatCaR == null) tatCaR = FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var r0 in tatCaR) if (r0 && (r0.bounds.center - p).sqrMagnitude < 0.09f && r0.bounds.size.magnitude < 0.6f) return;   // đã có bóng/đèn
        Material mau = null;
        foreach (var r0 in tatCaR) if (r0 && r0.sharedMaterial && r0.sharedMaterial.name.StartsWith("M_CT_Neon")) { mau = r0.sharedMaterial; break; }
        if (!mau) return;
        var sang = new Material(mau) { name = "M_BongHam_Sang" }; sang.SetColor("_EmissionColor", new Color(1f, 0.6f, 0.24f) * 3f); sang.EnableKeyword("_EMISSION");
        if (sang.HasProperty("_BaseColor")) sang.SetColor("_BaseColor", new Color(1f, 0.85f, 0.6f));
        var tat = new Material(mau) { name = "M_BongHam_Tat" }; tat.SetColor("_EmissionColor", Color.black); tat.DisableKeyword("_EMISSION");
        if (tat.HasProperty("_BaseColor")) tat.SetColor("_BaseColor", new Color(0.55f, 0.5f, 0.42f));
        var day = new Material(mau) { name = "M_DayDien_Ham" }; day.SetColor("_EmissionColor", Color.black); day.DisableKeyword("_EMISSION");
        if (day.HasProperty("_BaseColor")) day.SetColor("_BaseColor", new Color(0.08f, 0.07f, 0.06f));
        var bongG = GameObject.CreatePrimitive(PrimitiveType.Sphere); Destroy(bongG.GetComponent<Collider>());
        bongG.name = "BongTuDong_" + d.name; bongG.transform.SetParent(d.transform.parent, true);
        bongG.transform.position = p + Vector3.up * 0.05f; bongG.transform.localScale = new Vector3(0.06f, 0.08f, 0.06f);
        var rb = bongG.GetComponent<Renderer>(); rb.sharedMaterial = sang; rb.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        if (Physics.Raycast(p + Vector3.up * 0.1f, Vector3.up, out var tran, 1.6f, ~0, QueryTriggerInteraction.Ignore))
        {
            var dai = tran.distance + 0.05f; var dg = GameObject.CreatePrimitive(PrimitiveType.Cylinder); Destroy(dg.GetComponent<Collider>());
            dg.name = "DayDienTuDong_" + d.name; dg.transform.SetParent(bongG.transform.parent, true);
            dg.transform.position = p + Vector3.up * (0.09f + dai / 2f); dg.transform.localScale = new Vector3(0.008f, dai / 2f, 0.008f);
            var rd = dg.GetComponent<Renderer>(); rd.sharedMaterial = day; rd.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        gocMat[rb] = new[] { sang }; tatMat[rb] = new[] { tat }; bong[d] = new[] { rb };
    }

    public void Toggle()
    {
        bat = !bat;
        // hai chiều: đảo theo đèn đang sáng hay tắt, không theo phím của riêng cụm này
        bool sang = false; bool coDen = false;
        if (dens != null) foreach (var d in dens) if (d) { coDen = true; if (muon.TryGetValue(d, out var v) ? v : d.enabled) sang = true; }
        if (coDen) foreach (var d in dens) if (d) muon[d] = !sang;
        Ap();
        LocMa.KhiBamCongTac();
    }
    public void Dat(bool b) { bat = b; if (dens != null) foreach (var d in dens) if (d) muon[d] = b; Ap(); }

    void Ap()
    {
        if (dens != null) foreach (var d in dens) if (d) { if (!muon.ContainsKey(d)) muon[d] = tuNoi ? d.enabled : bat; ApDen(d); }
        if (hienKhiBat != null) foreach (var g in hienKhiBat) if (g) g.SetActive(bat && !LocMa.MatDien);
        if (phim != null) foreach (var p in phim) if (p) p.localRotation = Quaternion.Euler(bat ? -7f : 7f, 0, 0);
    }

    static void ApDen(Light d)
    {
        bool on = muon[d] && !LocMa.MatDien;
        d.enabled = on;
        foreach (var r in BongCua(d)) if (r && gocMat.TryGetValue(r, out var g) && tatMat.TryGetValue(r, out var t)) r.sharedMaterials = on ? g : t;
    }

    // có điện lại (LocMa) → áp lại mọi đèn người chơi đã bật/tắt
    public static void ApTatCa() { foreach (var d in new List<Light>(muon.Keys)) if (d) ApDen(d); }

    // vật phát sáng (bóng, tuýp, chao) trong 0,9 m quanh đèn
    static Renderer[] BongCua(Light d)
    {
        if (bong.TryGetValue(d, out var kq)) return kq;
        if (tatCaR == null) tatCaR = FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var l = new List<Renderer>(); var p = d.transform.position;
        foreach (var r in tatCaR)
        {
            if (!r || (r.bounds.center - p).sqrMagnitude > 0.81f) continue;
            var ms = r.sharedMaterials; bool co = false; var tat = new Material[ms.Length];
            for (int i = 0; i < ms.Length; i++)
            {
                tat[i] = ms[i];
                if (ms[i] && ms[i].HasProperty("_EmissionColor") && ms[i].GetColor("_EmissionColor").maxColorComponent > 0.02f)
                { var m = new Material(ms[i]) { name = ms[i].name + "_CongTacTat" }; m.SetColor("_EmissionColor", Color.black); m.DisableKeyword("_EMISSION"); tat[i] = m; co = true; }
            }
            if (!co) continue;
            if (!gocMat.ContainsKey(r)) { gocMat[r] = ms; tatMat[r] = tat; }
            l.Add(r);
        }
        return bong[d] = l.ToArray();
    }
}
