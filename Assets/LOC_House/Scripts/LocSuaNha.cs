using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// [02/11] Sửa nhà lúc chạy (không cần "Dựng nhà" lại — cảnh nhà đã sát 100 MB). Chạy mỗi lần cảnh nhà nạp (kể cả Chơi lại / Về menu):
//  1. Trần phòng Nhím hai màu (cam cạnh giường, xanh phần xa): phòng chỉ có đèn ngủ cạnh giường, phần trần xa chỉ còn ánh nền xanh của cả nhà.
//     → thêm một đèn phụ ấm, rất yếu, không đổ bóng, giữa trần phòng; bật / tắt / chập chờn y theo đèn ngủ (công tắc, mất điện, LocMa).
//  2. Tủ đứng hành lang tầng 2 trước phòng Khôi (TuDung_RuongChanMan): file .glb là 2 cánh + 2 tay nắm ở giữa, nhưng LocTuMo khai 1 cánh
//     → cả mặt tủ quay như một tấm, tay nắm nằm lại trên thân. Ở đây tách cánh làm đôi (bản lề hai mép ngoài), chuyển tay nắm theo cánh.
//     Hai cánh cùng tên, cùng cha → LocDoor tự ghép cửa đôi: bấm E vào cánh nào cũng mở / đóng cả hai.
//     Dựng nhà lại với LocTuMo.Bang mới (đã khai 2 cánh) thì tủ có sẵn 2 cánh → bước này tự bỏ qua.
//  Chi tiết + số đo: GHI_CHU_CHO_SUA_02-11.md mục 1–2.
public static class LocSuaNha
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Gan() { SceneManager.sceneLoaded -= KhiNap; SceneManager.sceneLoaded += KhiNap; }
    static void KhiNap(Scene sc, LoadSceneMode m)   // gọi sau Awake, trước Start của các vật trong cảnh
    {
        if (sc.name == LocGiacMo.TEN_CANH) return;
        try { DenPhuNhim(sc); } catch (System.Exception e) { Debug.LogException(e); }
        try { TachTuDung(sc); } catch (System.Exception e) { Debug.LogException(e); }
        try { LocNuocChen.Gan(sc); } catch (System.Exception e) { Debug.LogException(e); }   // [03/11] nước trong chén hầm, chén 5 vơi nửa từ Đêm 3
    }

    // ───────── 1. đèn phụ phòng Nhím
    public const float TI_LE_DEN_PHU = 0.28f;   // so với cường độ đèn ngủ
    static void DenPhuNhim(Scene sc)
    {
        Light den = null;
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (l.name == "Den_DenNgu_Nhim" && l.gameObject.scene == sc) { den = l; break; }
        if (!den || den.transform.Find("DenPhu_Nhim")) return;
        // giữa phòng: bắn tia ngang tầm đầu ra bốn phía tới tường (cửa mở thì tia lọt ra hành lang → chặn tối đa 4,5 m)
        var p = den.transform.position + Vector3.up * 0.6f;
        float Tia(Vector3 d) => Physics.Raycast(p, d, out var h, 4.5f, ~0, QueryTriggerInteraction.Ignore) ? h.distance : 4.5f;
        float xp = Tia(Vector3.right), xn = Tia(Vector3.left), zp = Tia(Vector3.forward), zn = Tia(Vector3.back), len = Tia(Vector3.up);
        var tam = new Vector3(p.x + (xp - xn) / 2f, p.y + Mathf.Max(0.3f, len - 0.35f), p.z + (zp - zn) / 2f);
        var g = new GameObject("DenPhu_Nhim"); g.transform.SetParent(den.transform, false); g.transform.position = tam;
        var phu = g.AddComponent<Light>();
        phu.type = LightType.Point; phu.color = den.color; phu.shadows = LightShadows.None;
        phu.range = Mathf.Clamp(Mathf.Max(xp + xn, zp + zn) * 0.85f, 2.5f, 5f);
        phu.intensity = den.intensity * TI_LE_DEN_PHU;
        g.AddComponent<LocDenTheo>().Gan(den, phu);
    }

    // ───────── 2. tủ đứng hành lang T2: 1 cánh → 2 cánh
    const string TEN_CANH_TU = "cửa tủ đứng";
    static void TachTuDung(Scene sc)
    {
        var nhom = new Dictionary<Transform, List<LocDoor>>();
        foreach (var d in Object.FindObjectsByType<LocDoor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (d.ten != TEN_CANH_TU || d.gameObject.scene != sc || !d.transform.parent) continue;
            if (!nhom.TryGetValue(d.transform.parent, out var l)) nhom[d.transform.parent] = l = new List<LocDoor>();
            l.Add(d);
        }
        foreach (var kv in nhom) if (kv.Value.Count == 1) TachMotCanh(kv.Value[0]);   // đã có 2 cánh (dựng lại nhà) → thôi
    }

    static void TachMotCanh(LocDoor d)
    {
        var mf = d.GetComponent<MeshFilter>(); var mr = d.GetComponent<MeshRenderer>();
        if (!mf || !mr || !mf.sharedMesh) return;
        var m = mf.sharedMesh;
        if (!m.isReadable) { Debug.LogWarning("[LOC] Tủ đứng T2: lưới cánh không đọc được, chưa tách 2 cánh"); return; }
        var v = m.vertices;
        if (v.Length == 0) return;
        var b = new Bounds(v[0], Vector3.zero); foreach (var p in v) b.Encapsulate(p);
        bool theoX = b.size.x >= b.size.z;   // cánh trải theo trục nào (gốc cánh = bản lề)
        float A(Vector3 p) => theoX ? p.x : p.z;
        float xa = Mathf.Abs(A(b.min)) > Mathf.Abs(A(b.max)) ? A(b.min) : A(b.max);   // mép xa bản lề
        float giua = xa / 2f;   // khe giữa hai cánh (đo .glb: hai tấm bằng nhau, khe ở chính giữa)
        bool Gan(Vector3 p) => Mathf.Abs(A(p)) < Mathf.Abs(giua);

        var gan = new List<int>[m.subMeshCount]; var xaL = new List<int>[m.subMeshCount]; int nG = 0, nX = 0;
        for (int s = 0; s < m.subMeshCount; s++)
        {
            gan[s] = new List<int>(); xaL[s] = new List<int>();
            var t = m.GetTriangles(s);
            for (int i = 0; i + 2 < t.Length; i += 3)
            {
                var c = (v[t[i]] + v[t[i + 1]] + v[t[i + 2]]) / 3f;
                var dst = Gan(c) ? gan[s] : xaL[s]; dst.Add(t[i]); dst.Add(t[i + 1]); dst.Add(t[i + 2]);
                if (Gan(c)) nG++; else nX++;
            }
        }
        if (nG == 0 || nX == 0) return;
        Mesh Cat(List<int>[] tri, string ten) { var k = Object.Instantiate(m); k.name = m.name + ten; for (int s = 0; s < tri.Length; s++) k.SetTriangles(tri[s], s); k.RecalculateBounds(); return k; }
        var mGan = Cat(gan, "_A"); var mXa = Cat(xaL, "_B");

        // cánh thứ hai: nhân bản lúc đang tắt (Awake của LocDoor chạy khi bật lại, lúc đó bản lề đã đặt đúng)
        bool bat = d.gameObject.activeSelf;
        d.gameObject.SetActive(false);
        var c2 = Object.Instantiate(d.gameObject, d.transform.parent); c2.name = d.name + "_Phai";
        d.gameObject.SetActive(bat);
        mf.sharedMesh = mGan;
        c2.GetComponent<MeshFilter>().sharedMesh = mXa;
        var d2 = c2.GetComponent<LocDoor>();
        d2.banLe = theoX ? new Vector3(xa, 0, 0) : new Vector3(0, 0, xa);   // bản lề ở mép xa (toạ độ cục bộ của cánh)
        d2.goc = -d.goc;                                                    // quay ngược chiều để cũng mở ra ngoài
        VaCham(d.gameObject, mGan.bounds); VaCham(c2, mXa.bounds);
        ChuyenTayNam(d, c2.transform, b, theoX, xa, giua);
        c2.SetActive(bat);
        Debug.Log($"[LOC] Tủ đứng T2: tách 1 cánh thành 2 ({nG} + {nX} tam giác)");
    }

    static void VaCham(GameObject g, Bounds lb)
    {
        var bc = g.GetComponent<BoxCollider>(); if (!bc) return;
        bc.center = lb.center; bc.size = new Vector3(Mathf.Max(lb.size.x, 0.03f), Mathf.Max(lb.size.y, 0.03f), Mathf.Max(lb.size.z, 0.03f));
    }

    // tay nắm (vật liệu đồng của model, nằm ngay trước mặt cánh) còn dính trên thân tủ → cắt khỏi thân, gắn vào đúng cánh
    static void ChuyenTayNam(LocDoor d, Transform c2, Bounds bc, bool theoX, float xa, float giua)
    {
        var cha = d.transform.parent; var canh = d.transform;
        float A(Vector3 p) => theoX ? p.x : p.z; float B(Vector3 p) => theoX ? p.z : p.x;
        var vG = new List<Vector3>(); var tG = new List<int>(); var vX = new List<Vector3>(); var tX = new List<int>(); Material dong = null;
        foreach (var mf in cha.GetComponentsInChildren<MeshFilter>(true))
        {
            if (mf.GetComponentInParent<LocDoor>() || !mf.sharedMesh || !mf.sharedMesh.isReadable) continue;
            var mr = mf.GetComponent<MeshRenderer>(); if (!mr) continue;
            var ms = mr.sharedMaterials; var m = mf.sharedMesh; Mesh moi = null; var v = m.vertices;
            for (int s = 0; s < m.subMeshCount && s < ms.Length; s++)
            {
                if (!ms[s] || !ms[s].name.Contains("B98700")) continue;   // vai__material_2_B98700 = đồng thau (tay nắm, núm)
                var t = m.GetTriangles(s); var giu = new List<int>(); bool doi = false;
                for (int i = 0; i + 2 < t.Length; i += 3)
                {
                    var p = new Vector3[3];
                    for (int j = 0; j < 3; j++) p[j] = canh.InverseTransformPoint(mf.transform.TransformPoint(v[t[i + j]]));
                    var c = (p[0] + p[1] + p[2]) / 3f;
                    bool trongCanh = Mathf.Sign(A(c)) == Mathf.Sign(xa) && Mathf.Abs(A(c)) < Mathf.Abs(xa) && c.y > bc.min.y && c.y < bc.max.y && Mathf.Abs(B(c) - B(bc.center)) < 0.06f;
                    if (!trongCanh) { giu.Add(t[i]); giu.Add(t[i + 1]); giu.Add(t[i + 2]); continue; }
                    doi = true; dong = ms[s];
                    var (vv, tt) = Mathf.Abs(A(c)) < Mathf.Abs(giua) ? (vG, tG) : (vX, tX);
                    for (int j = 0; j < 3; j++) { tt.Add(vv.Count); vv.Add(p[j]); }
                }
                if (!doi) continue;
                if (!moi) { moi = Object.Instantiate(m); moi.name = m.name + "_KhongTayNam"; }
                moi.SetTriangles(giu, s);
            }
            if (moi) mf.sharedMesh = moi;
        }
        if (!dong) return;
        void Gan(Transform canhT, List<Vector3> vv, List<int> tt)
        {
            if (tt.Count == 0) return;
            var mm = new Mesh { name = "TayNam_TuDung" }; mm.SetVertices(vv); mm.SetTriangles(tt, 0); mm.RecalculateNormals(); mm.RecalculateBounds();
            var g = new GameObject("TayNam"); g.transform.SetParent(canhT, false);
            g.AddComponent<MeshFilter>().sharedMesh = mm; g.AddComponent<MeshRenderer>().sharedMaterial = dong;
        }
        Gan(canh, vG, tG); Gan(c2, vX, tX);   // cánh 2 nhân bản từ cánh 1 nên cùng hệ toạ độ cục bộ
    }
}
