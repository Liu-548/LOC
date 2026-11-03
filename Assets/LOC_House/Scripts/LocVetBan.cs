using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// [03/11] Vết bẩn / ố / mốc / nứt cho bản "Thực tế" — thứ làm căn nhà trông như có người ở, có thời gian trôi qua (kiểu Outlast),
// thay vì tường sàn sạch tinh như nhựa.
//  · Không cần asset ngoài: atlas 8 loại vết sinh thủ tục (LOC_Nguon/ThucTe/tao_texture.py → Resources/LOC_ThucTe/VetBan_Atlas.png).
//  · Đặt bằng tia dò lên vỏ nhà (tường / sàn / trần): chỉ đặt khi cả tâm và 2 góc vết đều nằm trên CÙNG một mặt phẳng → không treo
//    ra ô cửa, không đè lên đồ vật. Vị trí sinh từ toạ độ (seed cố định) → lần nào chạy cũng y hệt.
//  · Gộp TẤT CẢ vết vào MỘT lưới, MỘT vật liệu (nhân màu — multiply, không tự sáng trong bóng tối) → 1 draw call, gần như không tốn FPS.
//  · Tắt "Thực tế" thì xoá sạch.
public class LocVetBan : MonoBehaviour
{
    const int GIOI_HAN = 2600;          // số vết tối đa (mỗi vết 4 đỉnh)
    const float NHO = 0.005f;           // nhô khỏi mặt 5 mm (chống nhấp nháy z)

    MeshFilter mf; MeshRenderer mr; Mesh luoi;
    readonly List<Vector3> v = new(); readonly List<Vector2> uv = new(); readonly List<Color> mau = new(); readonly List<int> tg = new();
    static Material vl;

    public static LocVetBan Tao(Transform cha, Bounds vung, System.Func<string, bool> laVo)
    {
        var tex = Resources.Load<Texture2D>("LOC_ThucTe/VetBan_Atlas");
        var sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (!tex || !sh) { Debug.LogWarning("[LocVetBan] thiếu atlas hoặc shader Particles/Unlit — bỏ qua vết bẩn."); return null; }
        if (!vl) { var co = Resources.Load<Material>("LOC_ThucTe/BienThe_VetBan"); if (co) vl = new Material(co) { name = "LocThucTe_VetBan" }; }   // bản có sẵn (giữ biến thể shader trong bản build)
        if (!vl)
        {
            vl = new Material(sh) { name = "LocThucTe_VetBan" };
            vl.SetTexture("_BaseMap", tex); vl.mainTexture = tex;
            vl.SetColor("_BaseColor", Color.white);
            // nhân màu: kết quả = màu tường × màu vết (alpha kéo về trắng) → chỉ làm tối đi, trong bóng tối cũng không lộ ra
            vl.SetFloat("_Surface", 1); vl.SetFloat("_Blend", 3); vl.SetFloat("_ZWrite", 0); vl.SetFloat("_Cull", 0);   // hai mặt: không phụ thuộc chiều xếp đỉnh
            vl.SetFloat("_SrcBlend", (float)BlendMode.DstColor); vl.SetFloat("_DstBlend", (float)BlendMode.Zero);
            vl.SetOverrideTag("RenderType", "Transparent");
            vl.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); vl.EnableKeyword("_ALPHAMODULATE_ON");
            vl.renderQueue = 2990;   // sau vật đục, trước kính / khói
        }
        if (vl.GetTexture("_BaseMap") != tex) { vl.SetTexture("_BaseMap", tex); vl.mainTexture = tex; }
        var go = new GameObject("LocThucTe_VetBan"); go.transform.SetParent(cha, false);
        var vb = go.AddComponent<LocVetBan>();
        vb.mf = go.AddComponent<MeshFilter>(); vb.mr = go.AddComponent<MeshRenderer>();
        vb.mr.sharedMaterial = vl; vb.mr.shadowCastingMode = ShadowCastingMode.Off; vb.mr.receiveShadows = false;
        vb.mr.lightProbeUsage = LightProbeUsage.Off; vb.mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
        vb.StartCoroutine(vb.Dung(vung, laVo));
        return vb;
    }

    // ô trong atlas 4×2
    enum O { ChanTuong = 0, Moc = 1, VetNuoc = 2, QuangO = 3, VetTay = 4, Nut = 5, BuiSan = 6, Gi = 7 }

    IEnumerator Dung(Bounds vung, System.Func<string, bool> laVo)
    {
        yield return null;
        var ds = new List<MeshRenderer>();
        foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (r && r.enabled && laVo(r.name) && vung.Intersects(r.bounds)) ds.Add(r);
        int dem = 0;
        foreach (var r in ds)
        {
            if (v.Count / 4 >= GIOI_HAN) break;
            try { RaiVo(r); } catch (System.Exception e) { Debug.LogWarning("[LocVetBan] bỏ qua " + r.name + ": " + e.Message); }
            if (++dem % 8 == 0) yield return null;   // rải ra nhiều khung
        }
        GhepLuoi();
    }

    // một khối vỏ nhà (hộp gần như thẳng trục): xét các mặt lớn của nó
    void RaiVo(MeshRenderer r)
    {
        var b = r.bounds; var s = b.size;
        var rd = new System.Random(Hash(b.center));
        bool tuongX = s.x < 0.6f && s.z > 0.8f && s.y > 1.2f, tuongZ = s.z < 0.6f && s.x > 0.8f && s.y > 1.2f;
        bool ngang = s.y < 0.6f && s.x > 0.8f && s.z > 0.8f;
        if (tuongX || tuongZ)
        {
            var n = tuongX ? Vector3.right : Vector3.forward; var doc = tuongX ? Vector3.forward : Vector3.right;
            float dai = tuongX ? s.z : s.x, nua = (tuongX ? s.x : s.z) * 0.5f;
            foreach (var huong in new[] { n, -n })
            {
                var mat = b.center + huong * nua;   // mặt tường phía này
                for (float u = 0.45f; u < dai - 0.3f; u += 0.9f)
                {
                    var goc = mat + doc * (u - dai * 0.5f);
                    float day = b.min.y, dinh = b.max.y;
                    // bẩn chân tường (gần như dọc khắp tường)
                    if (rd.NextDouble() < 0.6) { float w = R(rd, 0.9f, 1.6f), h = R(rd, 0.30f, 0.60f); DatTuong(r, goc, day + h * 0.5f + 0.01f, huong, doc, w, h, O.ChanTuong, R(rd, 0.55f, 0.95f)); }
                    if (rd.NextDouble() < 0.12) { float w = R(rd, 0.45f, 0.9f); DatTuong(r, goc, day + w * 0.45f, huong, doc, w, w, O.Moc, R(rd, 0.5f, 0.9f)); }
                    if (rd.NextDouble() < 0.16) { float w = R(rd, 0.4f, 0.8f), h = R(rd, 0.8f, Mathf.Min(1.7f, dinh - day - 0.3f)); DatTuong(r, goc, dinh - h * 0.5f - 0.02f, huong, doc, w, h, O.VetNuoc, R(rd, 0.5f, 0.85f)); }
                    if (rd.NextDouble() < 0.07) { float w = R(rd, 0.25f, 0.45f); DatTuong(r, goc, day + R(rd, 0.9f, 1.5f), huong, doc, w, w, O.VetTay, R(rd, 0.4f, 0.8f)); }
                    if (rd.NextDouble() < 0.06) { float w = R(rd, 0.4f, 0.9f); DatTuong(r, goc, day + R(rd, 0.6f, Mathf.Max(0.7f, dinh - day - 0.5f)), huong, doc, w, w, O.Nut, R(rd, 0.6f, 1f)); }
                    if (rd.NextDouble() < 0.05) { float w = R(rd, 0.4f, 0.7f); DatTuong(r, goc, day + R(rd, 0.3f, 1.2f), huong, doc, w, w, O.Gi, R(rd, 0.4f, 0.7f)); }
                }
            }
        }
        else if (ngang)
        {
            // sàn (mặt trên) và trần (mặt dưới) của tấm ngang, lưới ô 1,4 m
            for (float x = b.min.x + 0.7f; x < b.max.x - 0.3f; x += 1.4f)
                for (float z = b.min.z + 0.7f; z < b.max.z - 0.3f; z += 1.4f)
                {
                    var p = new Vector3(x + R(rd, -0.4f, 0.4f), 0, z + R(rd, -0.4f, 0.4f));
                    if (rd.NextDouble() < 0.28) { float w = R(rd, 0.8f, 1.7f); DatNgang(r, p, b, true, w, O.BuiSan, R(rd, 0.35f, 0.7f), rd); }
                    if (rd.NextDouble() < 0.08) { float w = R(rd, 0.6f, 1.3f); DatNgang(r, p, b, false, w, O.QuangO, R(rd, 0.45f, 0.8f), rd); }
                }
        }
    }

    // vết trên tường: tâm ở chiều cao y, mặt theo huong (pháp tuyến hướng ra), trục ngang doc
    void DatTuong(MeshRenderer r, Vector3 goc, float y, Vector3 huong, Vector3 doc, float w, float h, O o, float dam)
    {
        var p = new Vector3(goc.x, y, goc.z);
        if (!Cham(r, p, huong, out var tam)) return;
        // hai góc chéo cũng phải nằm trên cùng mặt (không treo ra ô cửa / mép tường)
        if (!Cham(r, p + doc * w * 0.45f + Vector3.up * h * 0.45f, huong, out var g1) || Mathf.Abs(Vector3.Dot(g1 - tam, huong)) > 0.03f) return;
        if (!Cham(r, p - doc * w * 0.45f - Vector3.up * h * 0.45f, huong, out var g2) || Mathf.Abs(Vector3.Dot(g2 - tam, huong)) > 0.03f) return;
        var phai = Vector3.Cross(huong, Vector3.up).normalized;   // nhìn từ phía huong vào tường: bên phải màn hình
        Them(tam + huong * NHO, phai * w * 0.5f, Vector3.up * h * 0.5f, o, dam);
    }

    void DatNgang(MeshRenderer r, Vector3 p, Bounds b, bool san, float w, O o, float dam, System.Random rd)
    {
        var huong = san ? Vector3.up : Vector3.down;
        var tu = new Vector3(p.x, san ? b.max.y + 0.3f : b.min.y - 0.3f, p.z);
        if (!Ban(r, tu, -huong, 0.6f, out var tam)) return;
        float goc = (float)rd.NextDouble() * 360f;
        var a = Quaternion.AngleAxis(goc, Vector3.up) * Vector3.right; var c = Vector3.Cross(a, huong);
        foreach (var k in new[] { a + c, a - c, -a + c, -a - c })
            if (!Ban(r, tam + k * w * 0.32f + huong * 0.3f, -huong, 0.6f, out var g) || Mathf.Abs(g.y - tam.y) > 0.03f) return;
        Them(tam + huong * NHO, a * w * 0.5f, c * w * 0.5f, o, dam);
    }

    bool Cham(MeshRenderer r, Vector3 p, Vector3 huong, out Vector3 diem) => Ban(r, p + huong * 0.3f, -huong, 0.6f, out diem);

    // tia trúng đúng khối vỏ nhà r (không phải đồ vật / cánh cửa chắn trước)
    static readonly RaycastHit[] hb = new RaycastHit[8];
    static bool Ban(MeshRenderer r, Vector3 tu, Vector3 huong, float xa, out Vector3 diem)
    {
        diem = default;
        int k = Physics.RaycastNonAlloc(tu, huong, hb, xa, ~0, QueryTriggerInteraction.Ignore);
        float gan = float.MaxValue; Collider tot = null;
        for (int i = 0; i < k; i++) if (hb[i].distance < gan) { gan = hb[i].distance; tot = hb[i].collider; diem = hb[i].point; }
        if (!tot) return false;
        var t = tot.transform; var rt = r.transform;
        return t == rt || t.IsChildOf(rt) || rt.IsChildOf(t);
    }

    void Them(Vector3 tam, Vector3 nuaNgang, Vector3 nuaDoc, O o, float dam)
    {
        if (v.Count / 4 >= GIOI_HAN) return;
        int i0 = v.Count, oi = (int)o;
        float u0 = (oi % 4) * 0.25f, v0 = (oi / 4) * 0.5f;
        v.Add(tam - nuaNgang - nuaDoc); uv.Add(new Vector2(u0, v0));
        v.Add(tam + nuaNgang - nuaDoc); uv.Add(new Vector2(u0 + 0.25f, v0));
        v.Add(tam + nuaNgang + nuaDoc); uv.Add(new Vector2(u0 + 0.25f, v0 + 0.5f));
        v.Add(tam - nuaNgang + nuaDoc); uv.Add(new Vector2(u0, v0 + 0.5f));
        var c = new Color(1, 1, 1, Mathf.Clamp01(dam)); for (int j = 0; j < 4; j++) mau.Add(c);
        // thuận chiều kim đồng hồ khi nhìn từ phía pháp tuyến (mặt trước của Unity); vật liệu vẫn tắt cull cho chắc
        tg.Add(i0); tg.Add(i0 + 2); tg.Add(i0 + 1); tg.Add(i0); tg.Add(i0 + 3); tg.Add(i0 + 2);
    }

    void GhepLuoi()
    {
        if (v.Count == 0) { Debug.Log("[LocVetBan] không đặt được vết nào"); return; }
        luoi = new Mesh { name = "LocThucTe_VetBan", indexFormat = IndexFormat.UInt32 };
        luoi.SetVertices(v); luoi.SetUVs(0, uv); luoi.SetColors(mau); luoi.SetTriangles(tg, 0);
        luoi.RecalculateNormals(); luoi.RecalculateBounds();
        mf.sharedMesh = luoi;
        Debug.Log($"[LocVetBan] {v.Count / 4} vết bẩn · 1 lưới · 1 draw call");
        v.Clear(); uv.Clear(); mau.Clear(); tg.Clear();
    }

    void OnDestroy() { if (luoi) Destroy(luoi); }

    static float R(System.Random rd, float a, float b) => a + (float)rd.NextDouble() * (b - a);
    static int Hash(Vector3 p) => unchecked(Mathf.RoundToInt(p.x * 100) * 73856093 ^ Mathf.RoundToInt(p.y * 100) * 19349663 ^ Mathf.RoundToInt(p.z * 100) * 83492791);
}
