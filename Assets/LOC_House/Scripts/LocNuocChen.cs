using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// [03/11] Nước trong chén hầm + năm chén mới của cảnh Evil "năm năm sau".
//  1. Năm chén dưới hầm là "chén nước" (sổ của bố: mỗi người một cái) nhưng model chén rỗng. Thêm mặt nước cho hai cái còn đứng từ đầu
//     (Chen4 nứt dọc, Chen5 mẻ miệng); ba cái dán lại ở Đêm 2 không ai rót nên để rỗng.
//     v3.3 (WALKTHROUGH Đêm 3 ❌): xuống hầm Đêm 3 thì chén số 5 vơi còn nửa — mức nước Chen5 tụt khi LocMa.dem ≥ 3.
//  2. Bước sửa nhà 01/11 (LocHouseBuilder_Sua0111 mục 12) bỏ năm chén liền khỏi lưới Bo_Chen_Tren_De → không còn vật "ChenNguyen_*" nào,
//     nên cảnh "năm năm sau" bật chén mới không thấy gì và chỉ còn cái đế trống. ChenMoi() dựng lại năm chén lành theo đúng số đo
//     ChenNguyen.glb (12 múi, cùng UV, texture gốc chép vào Resources/LOC_Chen).
public class LocNuocChen : MonoBehaviour
{
    // số đo lòng chén (ChenNguyen/ChenMe/ChenNut.glb): đáy trong y 0,008 r 0,0195 → miệng trong y 0,045 r 0,027
    const float DAY_Y = 0.008f, DAY_R = 0.0195f, MIENG_Y = 0.045f, MIENG_R = 0.027f;
    public const float MUC_DAY = 0.84f, MUC_VOI = 0.5f;   // phần chiều sâu lòng chén có nước: bình thường · chén 5 từ Đêm 3
    static readonly string[] CO_NUOC = { "Chen4", "Chen5" };

    Transform nuoc5; float mucDangDat = -1f;
    static Mesh luoiDia, luoiChen; static Material vlNuoc, vlChen;

    public static void Gan(Scene sc)
    {
        var nhom = Tim(sc, "ChenHam_TrangThai"); if (!nhom || nhom.GetComponent<LocNuocChen>()) return;
        var lnc = nhom.gameObject.AddComponent<LocNuocChen>();
        foreach (var ten in CO_NUOC)
        {
            var c = nhom.Find(ten); if (!c) continue;
            var n = ThemNuoc(c, MUC_DAY); if (ten == "Chen5") lnc.nuoc5 = n;
        }
        lnc.CapNhat();
    }

    void Update() => CapNhat();
    void CapNhat()
    {
        float muc = LocMa.dem >= 3 ? MUC_VOI : MUC_DAY;
        if (!nuoc5 || Mathf.Approximately(muc, mucDangDat)) return;
        mucDangDat = muc; DatMuc(nuoc5, muc);
    }

    static Transform Tim(Scene sc, string ten)
    {
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == ten && t.gameObject.scene == sc) return t;
        return null;
    }

    // mặt nước: đĩa 12 cạnh, con của nút có lưới chén (toạ độ cục bộ = toạ độ model)
    static Transform ThemNuoc(Transform chen, float muc)
    {
        var mr = chen.GetComponentInChildren<MeshRenderer>(true); var goc = mr ? mr.transform : chen;
        var g = new GameObject("MatNuoc"); g.transform.SetParent(goc, false);
        g.AddComponent<MeshFilter>().sharedMesh = LuoiDia();
        var r = g.AddComponent<MeshRenderer>(); r.sharedMaterial = VlNuoc(); r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        DatMuc(g.transform, muc);
        return g.transform;
    }

    static void DatMuc(Transform nuoc, float muc)
    {
        float y = Mathf.Lerp(DAY_Y, MIENG_Y, muc), r = Mathf.Lerp(DAY_R, MIENG_R, muc) * 0.99f;
        nuoc.localPosition = new Vector3(0, y, 0); nuoc.localScale = new Vector3(r, 1f, r);
    }

    static Mesh LuoiDia()
    {
        if (luoiDia) return luoiDia;
        const int N = 12; var v = new Vector3[N + 1]; var t = new int[N * 3];
        for (int i = 0; i < N; i++) { float a = -i * Mathf.PI * 2f / N; v[i + 1] = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)); }   // góc giảm dần → mặt quay lên
        for (int i = 0; i < N; i++) { t[i * 3] = 0; t[i * 3 + 1] = i + 1; t[i * 3 + 2] = (i + 1) % N + 1; }
        luoiDia = new Mesh { name = "MatNuocChen" }; luoiDia.vertices = v; luoiDia.triangles = t; luoiDia.RecalculateNormals(); luoiDia.RecalculateBounds();
        return luoiDia;
    }

    static Shader Lit() => Shader.Find("Universal Render Pipeline/Lit");
    static Material VlNuoc()
    {
        if (vlNuoc) return vlNuoc;
        var sh = Lit(); if (!sh) return null;
        vlNuoc = new Material(sh) { name = "M_NuocChen (tự tạo)" };
        vlNuoc.SetColor("_BaseColor", new Color(0.16f, 0.17f, 0.15f));   // nước trong, lòng chén tối dưới hầm
        vlNuoc.SetFloat("_Smoothness", 0.93f); vlNuoc.SetFloat("_Metallic", 0f);
        return vlNuoc;
    }

    // ───────── năm chén mới (Evil, năm năm sau) đặt đúng chỗ năm chén cũ
    public static List<GameObject> ChenMoi(Transform nhom)
    {
        var ds = new List<GameObject>(); if (!nhom || !nhom.parent) return ds;
        for (int i = 1; i <= 5; i++)
        {
            Transform cu = null;
            foreach (Transform c in nhom) if (c.name.StartsWith("Chen" + i)) { cu = c; break; }
            if (!cu) continue;
            var mr = cu.GetComponentInChildren<MeshRenderer>(true); var goc = mr ? mr.transform : cu;
            var g = new GameObject("ChenNguyen_" + i); g.transform.SetParent(nhom.parent, false);
            g.transform.SetPositionAndRotation(goc.position, goc.rotation); g.transform.localScale = goc.lossyScale;
            g.AddComponent<MeshFilter>().sharedMesh = LuoiChen();
            g.AddComponent<MeshRenderer>().sharedMaterial = VlChen(mr ? mr.sharedMaterial : null);
            var n = new GameObject("MatNuoc"); n.transform.SetParent(g.transform, false);
            n.AddComponent<MeshFilter>().sharedMesh = LuoiDia();
            var nr = n.AddComponent<MeshRenderer>(); nr.sharedMaterial = VlNuoc(); nr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            DatMuc(n.transform, MUC_DAY);
            ds.Add(g);
        }
        return ds;
    }

    static Material VlChen(Material mau)
    {
        if (vlChen) return vlChen;
        var tex = Resources.Load<Texture2D>("LOC_Chen/ChenNguyen_256");
        var sh = Lit();
        vlChen = sh ? new Material(sh) : (mau ? new Material(mau) : null);
        if (!vlChen) return null;
        vlChen.name = "M_ChenNguyen (tự tạo)";
        if (tex) { vlChen.SetTexture("_BaseMap", tex); vlChen.mainTexture = tex; vlChen.SetColor("_BaseColor", Color.white); }
        else vlChen.SetColor("_BaseColor", new Color(0.9f, 0.88f, 0.82f));   // thiếu texture: men trắng ngà trơn
        vlChen.SetFloat("_Smoothness", 0.48f); vlChen.SetFloat("_Metallic", 0f);   // roughness 0,52 như file .glb
        return vlChen;
    }

    // chén tiện tròn 12 múi, mặt phẳng (PSX), theo đúng hồ sơ + UV của ChenNguyen.glb:
    //  tâm đáy → mép đáy r 0,0225 → miệng ngoài r 0,03 (y 0,045) → miệng trong r 0,027 → đáy trong r 0,0195 (y 0,008) → tâm lòng
    static Mesh LuoiChen()
    {
        if (luoiChen) return luoiChen;
        var P = new[] { new Vector3(0f, 0f, 0f), new Vector3(0.0225f, 0f, 0.062f), new Vector3(0.03f, 0.045f, 0.562f), new Vector3(0.027f, 0.045f, 0.625f), new Vector3(0.0195f, 0.008f, 0.938f), new Vector3(0f, 0.008f, 1f) };   // (r, y, v của glTF)
        const int N = 12; var vs = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
        Vector3 Diem(Vector3 p, float a) => new Vector3(-p.x * Mathf.Cos(a), p.y, p.x * Mathf.Sin(a));   // glTF → Unity: lật trục x như glTFast
        for (int k = 0; k < N; k++)
        {
            float a0 = Mathf.Deg2Rad * (90f - 30f * k), a1 = a0 - Mathf.Deg2Rad * 30f, am = (a0 + a1) / 2f;
            float u0 = k / (float)N, u1 = (k + 1) / (float)N, um = (u0 + u1) / 2f;
            for (int i = 0; i + 1 < P.Length; i++)
            {
                Vector3 p = P[i], q = P[i + 1];
                var rad = new Vector3(-Mathf.Cos(am), 0, Mathf.Sin(am));
                var en = rad * (q.y - p.y) + Vector3.up * -(q.x - p.x);   // pháp tuyến hướng ra ngoài bề mặt: xoay tiếp tuyến hồ sơ 90° theo chiều kim
                var A = (Diem(p, a0), new Vector2(p.x == 0 ? um : u0, 1f - p.z)); var B = (Diem(p, a1), new Vector2(p.x == 0 ? um : u1, 1f - p.z));
                var C = (Diem(q, a1), new Vector2(q.x == 0 ? um : u1, 1f - q.z)); var D = (Diem(q, a0), new Vector2(q.x == 0 ? um : u0, 1f - q.z));
                void Tam((Vector3 v, Vector2 t) a, (Vector3 v, Vector2 t) b, (Vector3 v, Vector2 t) c)
                {
                    var n = Vector3.Cross(b.v - a.v, c.v - a.v); if (n.sqrMagnitude < 1e-14f) return;   // tam giác suy biến ở tâm
                    if (Vector3.Dot(n, en) < 0) (b, c) = (c, b);
                    foreach (var x in new[] { a, b, c }) { tri.Add(vs.Count); vs.Add(x.v); uv.Add(x.t); }
                }
                Tam(A, B, C); Tam(A, C, D);
            }
        }
        luoiChen = new Mesh { name = "ChenNguyen_TuDung" }; luoiChen.SetVertices(vs); luoiChen.SetUVs(0, uv); luoiChen.SetTriangles(tri, 0);
        luoiChen.RecalculateNormals(); luoiChen.RecalculateBounds();
        return luoiChen;
    }
}
