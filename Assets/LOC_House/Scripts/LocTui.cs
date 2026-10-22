using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// [6/10] Túi đồ + trang đọc + phụ đề độc thoại nối dòng (kịch bản v3.1 mục 6.1: Tab = xem đồ đã có, không HUD).
//  · Tab mở/đóng túi · ↑↓ chọn món · ←→ lật trang · Esc đóng.
//  · Chết → "chơi lại từ đầu đêm" nạp lại scene: túi + cờ trả về đúng lúc đầu đêm, món đã nhặt tự ẩn khỏi nhà.
[DefaultExecutionOrder(10)]
public class LocTui : MonoBehaviour
{
    static readonly List<string> ids = new();          // món trong túi, theo thứ tự nhặt
    static readonly HashSet<string> co = new();        // cờ cốt truyện: "doc:<id>", "ket_mo"…
    static readonly Dictionary<string, string> tenRieng = new();   // tên món nhận qua Nhan() (mini game) — không có LocVatPham trong nhà
    static List<string> idsDauDem = new(); static HashSet<string> coDauDem = new(); static int demChup = -1;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reset0() { ids.Clear(); co.Clear(); tenRieng.Clear(); idsDauDem.Clear(); coDauDem.Clear(); demChup = -1; DangCam = null; }

    public static LocTui I;
    public static bool CoMon(string id) => ids.Contains(id);
    public static bool Co(string co_) => co.Contains(co_);
    public static void Dat(string co_) => co.Add(co_);
    // [22/10] lưu/nạp đầu đêm (LocMenu)
    public static string ChuoiIds => string.Join("|", ids);
    public static string ChuoiCo => string.Join("|", co);
    public static void Nap(IEnumerable<string> i, IEnumerable<string> c) { Reset0(); if (i != null) ids.AddRange(i); if (c != null) co.UnionWith(c); }
    public static bool Mo => I && I.che != Che.Dong;

    enum Che { Dong, Tui, Doc }
    Che che; LocVatPham dangDoc; int chon, trangI;
    readonly Dictionary<string, LocVatPham> mon = new();
    Coroutine noi;

    void Awake()
    {
        I = this;
        if (demChup >= 0) { ids.Clear(); ids.AddRange(idsDauDem); co.Clear(); co.UnionWith(coDauDem); }   // nạp lại scene giữa đêm
        foreach (var v in FindObjectsByType<LocVatPham>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (string.IsNullOrEmpty(v.id)) continue;
            mon[v.id] = v;
            if (ids.Contains(v.id)) v.gameObject.SetActive(false);
        }
    }

    // ───── [21/10 gộp] API cho hệ mini game (LocTuongTacMG, LocMG_*): món theo id, không cần LocVatPham
    public static string DangCam;   // id món đang cầm (LocTuongTacMG tự cầm khi dùng), hoặc null
    public static string TenMon(string id) => I && I.mon.TryGetValue(id, out var v) ? v.ten : tenRieng.TryGetValue(id, out var t) ? t : LocId.Ten(id);
    public static void Nhan(string id, string ten = null, bool bao = true)   // ten = null: lấy LocId.Ten · bao: phụ đề "Nhặt được"
    {
        if (string.IsNullOrEmpty(id) || ids.Contains(id)) return;
        ids.Add(id);
        if (!string.IsNullOrEmpty(ten)) tenRieng[id] = ten;
        if (bao) LocFirstPerson.PhuDe("Nhặt được: " + TenMon(id), 2f, false);
    }
    public static void Bo(string id) { ids.Remove(id); if (DangCam == id) DangCam = null; }
    public static void BoTatCa(string[] d) { if (d != null) foreach (var id in d) Bo(id); }
    public static bool CoTatCa(string[] d) { if (d != null) foreach (var id in d) if (!ids.Contains(id)) return false; return true; }
    public static int DemCo(string[] d) { int n = 0; if (d != null) foreach (var id in d) if (ids.Contains(id)) n++; return n; }

    public static void Them(LocVatPham v) { if (!ids.Contains(v.id)) ids.Add(v.id); if (I) I.mon[v.id] = v; }

    public static void Doc(LocVatPham v)
    {
        if (!I) return;
        I.dangDoc = v; I.trangI = 0; I.MoChe(Che.Doc);
        if (v.DocDuoc) Dat("doc:" + v.id);
    }

    // độc thoại nội tâm nối dòng (chữ nghiêng, xám) — mỗi dòng đủ lâu để đọc
    public static void Noi(params string[] dong)
    {
        if (dong == null || dong.Length == 0 || (dong.Length == 1 && string.IsNullOrEmpty(dong[0]))) return;
        if (!I) { LocFirstPerson.PhuDe(dong[0], 2.5f); return; }
        if (I.noi != null) I.StopCoroutine(I.noi);
        I.noi = I.StartCoroutine(I.NoiLan(dong));
    }
    IEnumerator NoiLan(string[] dong)
    {
        foreach (var d in dong)
        {
            float t = Mathf.Max(1.8f, d.Length * 0.07f);
            LocFirstPerson.PhuDe(d, t); yield return new WaitForSeconds(t * LocFirstPerson.nhanChu + 0.15f);   // [22/10] cài đặt thời gian hiện chữ
        }
        noi = null;
    }

    public void DatChe(int c) { if (c == 0) dangDoc = null; MoChe((Che)c); }   // tự kiểm
    void MoChe(Che c)
    {
        bool dangMo = che != Che.Dong;
        che = c;
        if (c != Che.Dong && !dangMo) LocMa.tamDung = true;
        if (c == Che.Dong && dangMo) LocMa.tamDung = false;
    }

    void Update()
    {
        if (LocMa.dem != demChup) { demChup = LocMa.dem; idsDauDem = new List<string>(ids); coDauDem = new HashSet<string>(co); }
        var k = Keyboard.current; if (k == null) return;
        if (che == Che.Dong)
        {
            if (k.tabKey.wasPressedThisFrame && !LocFirstPerson.khoa) { chon = Mathf.Clamp(chon, 0, Mathf.Max(0, ids.Count - 1)); trangI = 0; MoChe(Che.Tui); }
            return;
        }
        if (k.escapeKey.wasPressedThisFrame || k.tabKey.wasPressedThisFrame) { MoChe(Che.Dong); dangDoc = null; return; }
        if (che == Che.Tui && ids.Count > 0)
        {
            int c0 = chon;
            if (k.upArrowKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame) chon = (chon + ids.Count - 1) % ids.Count;
            if (k.downArrowKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame) chon = (chon + 1) % ids.Count;
            if (c0 != chon) trangI = 0;
            dangDoc = mon.TryGetValue(ids[chon], out var v) ? v : null;
            if (dangDoc && dangDoc.DocDuoc) Dat("doc:" + dangDoc.id);
        }
        if (dangDoc && dangDoc.DocDuoc)
        {
            int n = dangDoc.trang.Length;
            if (k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame) trangI = Mathf.Min(n - 1, trangI + 1);
            if (k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame) trangI = Mathf.Max(0, trangI - 1);
        }
    }

    // ───── vẽ
    static GUIStyle sTieuDe, sMuc, sGiay, sGiayNghieng, sNho; static Texture2D tGiay;
    static void KhoiStyle()
    {
        if (sTieuDe != null) return;
        tGiay = new Texture2D(1, 1); tGiay.SetPixel(0, 0, new Color(0.89f, 0.85f, 0.75f)); tGiay.Apply();
        sTieuDe = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold }; sTieuDe.normal.textColor = new Color(0.92f, 0.9f, 0.85f);
        sMuc = new GUIStyle(GUI.skin.label) { fontSize = 21 }; sMuc.normal.textColor = new Color(0.8f, 0.78f, 0.72f);
        sGiay = new GUIStyle(GUI.skin.label) { fontSize = 21, wordWrap = true, richText = true }; sGiay.normal.textColor = new Color(0.2f, 0.16f, 0.12f);
        sGiayNghieng = new GUIStyle(sGiay) { fontStyle = FontStyle.Italic };
        sNho = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter }; sNho.normal.textColor = new Color(0.7f, 0.7f, 0.68f);
    }

    void OnGUI()
    {
        if (che == Che.Dong) return;
        KhoiStyle();
        float k = Screen.height / 1080f; GUI.matrix = Matrix4x4.Scale(new Vector3(k, k, 1));   // chữ cùng cỡ ở mọi độ phân giải (thiết kế ở 1080p)
        float W = Screen.width / k, H = 1080f;
        GUI.color = new Color(0, 0, 0, 0.72f); GUI.DrawTexture(new Rect(0, 0, W, H), Texture2D.whiteTexture); GUI.color = Color.white;
        Rect giay;
        if (che == Che.Tui)
        {
            GUI.Label(new Rect(W * 0.06f, H * 0.08f, W * 0.3f, 40), "Túi đồ", sTieuDe);
            if (ids.Count == 0) GUI.Label(new Rect(W * 0.06f, H * 0.16f, W * 0.3f, 30), "(chưa có gì)", sMuc);
            for (int i = 0; i < ids.Count; i++)
            {
                var ten = TenMon(ids[i]);
                var st = new GUIStyle(sMuc); if (i == chon) { st.normal.textColor = Color.white; st.fontStyle = FontStyle.Bold; }
                GUI.Label(new Rect(W * 0.06f, H * 0.16f + i * 34, W * 0.3f, 32), (i == chon ? "›  " : "   ") + ten, st);
            }
            giay = new Rect(W * 0.40f, H * 0.08f, W * 0.52f, H * 0.74f);
        }
        else giay = new Rect(W * 0.24f, H * 0.08f, W * 0.52f, H * 0.74f);

        if (dangDoc)
        {
            GUI.DrawTexture(giay, tGiay);
            var trong = new Rect(giay.x + 34, giay.y + 28, giay.width - 68, giay.height - 70);
            string noiDung; bool nghieng = false;
            if (dangDoc.DocDuoc) noiDung = dangDoc.trang[trangI];
            else if (dangDoc.trang != null && dangDoc.trang.Length > 0) { noiDung = dangDoc.chuaDoc; nghieng = true; }
            else { noiDung = $"<b>{dangDoc.ten}</b>\n\n{dangDoc.ghiChu}"; }
            GUI.Label(trong, noiDung, nghieng ? sGiayNghieng : sGiay);
            if (dangDoc.DocDuoc && dangDoc.trang.Length > 1)
            {
                var sTrang = new GUIStyle(sGiay) { fontSize = 16, alignment = TextAnchor.LowerRight };
                GUI.Label(new Rect(giay.x, giay.yMax - 34, giay.width - 24, 26), $"{trangI + 1} / {dangDoc.trang.Length}", sTrang);
            }
        }
        string phim = che == Che.Tui ? "[↑↓] chọn món    [←→] lật trang    [Tab] đóng" : "[←→] lật trang    [Esc] đóng";
        GUI.Label(new Rect(0, H * 0.9f, W, 30), phim, sNho);
        GUI.matrix = Matrix4x4.identity;
    }
}
