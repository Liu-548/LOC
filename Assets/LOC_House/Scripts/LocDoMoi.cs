using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// [03/11] Thay đồ vật low-poly bằng bản dựng lại chi tiết cao (Blender, LOC_Nguon/ThucTe) — chỉ khi bật "Thực tế".
//  Làm LÚC CHẠY, không sửa scene / .glb gốc: tắt "Thực tế" là trả lại y nguyên; không cần chạy công cụ dựng nhà trong Unity.
//  1. Vòng hoa tang (VongHoaTang_1 / _2 trong bộ bàn thờ + vòng đứng riêng): ẩn vành / hoa / lá / giá cũ, GIỮ băng chữ
//     "THÀNH KÍNH PHÂN ƯU" (chữ do LocCotTruyen gắn lúc chạy), đặt vòng hoa HQ khít vào hộp bao của vòng cũ.
//     HQ ~59k tam giác + LOD1 ~18k (xa thì đổi, bóng đổ dùng LOD1).
//  2. Ảnh thờ (vật liệu Anh_Tho_Atlas): ảnh pixel → ảnh cũ đen trắng ngả nâu (Resources/LOC_ThucTe/Mo/AnhTho_Atlas).
//  3. Bộ bàn thờ (BanTho_HQ.glb).
//  4. [03/11] Đồ đạc (NoiThat_HQ.glb, LOC_Nguon/ThucTe/noi_that_hq.py): ghế salon dài, tủ TV, ghế đẩu, quạt trần (thân + từng cánh), 2 cánh cửa chính.
//     Chỉ ẩn renderer cũ + đặt bản mới khít hộp bao (cục bộ) — va chạm, chỗ bấm, cánh cửa / cánh quạt quay vẫn là object cũ (bản mới là con nên quay theo).
public class LocDoMoi : MonoBehaviour
{
    static readonly string[] PHAN_CU = { "_Gia", "_Vanh", "_HoaTrang", "_HoaVang", "_HoaNho", "_LaDuongXi", "_VanLot" };

    readonly List<Renderer> daAn = new();
    readonly List<(MeshFilter mf, Mesh luoi, Material[] vl)> daCat = new();
    readonly List<(Transform t, Vector3 p)> daDoi = new();
    readonly List<(Renderer r, Material[] vl)> daDoiVL = new();
    readonly List<GameObject> daTao = new();
    static Dictionary<string, Material> vlHQ;

    public static LocDoMoi Tao(Transform cha, Bounds vung)
    {
        var go = new GameObject("LocThucTe_DoMoi"); go.transform.SetParent(cha, false);
        var dm = go.AddComponent<LocDoMoi>();
        try { dm.VongHoa(vung); } catch (System.Exception e) { Debug.LogWarning("[LocDoMoi] vòng hoa: " + e.Message); }
        try { dm.AnhTho(vung); } catch (System.Exception e) { Debug.LogWarning("[LocDoMoi] ảnh thờ: " + e.Message); }
        try { dm.BanTho(vung); } catch (System.Exception e) { Debug.LogWarning("[LocDoMoi] bàn thờ: " + e.Message); }
        try { dm.NoiThat(vung); } catch (System.Exception e) { Debug.LogWarning("[LocDoMoi] đồ đạc: " + e.Message); }
        return dm;
    }

    // ───────── vòng hoa
    void VongHoa(Bounds vung)
    {
        var mau = Resources.Load<GameObject>("LOC_ThucTe/Mo/VongHoa_HQ");
        if (!mau) { Debug.LogWarning("[LocDoMoi] thiếu Resources/LOC_ThucTe/Mo/VongHoa_HQ.glb"); return; }
        var hq = TimHQ(mau, out var lod1, out tenVL);
        if (!hq) { Debug.LogWarning("[LocDoMoi] VongHoa_HQ không có lưới"); return; }
        int dem = 0;
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!t || !(t.name == "VongHoaTang_1" || t.name == "VongHoaTang_2")) continue;
            var mf = t.GetComponent<MeshFilter>(); var mr = t.GetComponent<MeshRenderer>();
            if (mf && mr && mf.sharedMesh) { if (vung.Contains(t.position) && VongDon(t, mf, mr, hq, lod1)) dem++; }
            else if (t.childCount > 0) { if (VongNhieuPhan(t, hq, lod1, vung)) dem++; }
        }
        Debug.Log($"[LocDoMoi] thay {dem} vòng hoa tang bằng bản chi tiết cao");
    }

    string[] tenVL;   // tên vật liệu theo submesh của bản HQ (từ .glb) → chọn màu đúng dù thứ tự submesh có đổi
    static Mesh TimHQ(GameObject mau, out Mesh lod1, out string[] tenVL)
    {
        Mesh chinh = null; lod1 = null; tenVL = null;
        foreach (var f in mau.GetComponentsInChildren<MeshFilter>(true))
        {
            if (!f.sharedMesh) continue;
            if (f.name.EndsWith("LOD1") || f.sharedMesh.name.EndsWith("LOD1")) lod1 = f.sharedMesh;
            else
            {
                chinh = f.sharedMesh; var r = f.GetComponent<Renderer>();
                if (r) { var ms = r.sharedMaterials; tenVL = new string[ms.Length]; for (int i = 0; i < ms.Length; i++) tenVL[i] = ms[i] ? ms[i].name : ""; }
            }
        }
        return chinh;
    }

    // vòng trong bộ bàn thờ: cha rỗng + các phần con
    bool VongNhieuPhan(Transform t, Mesh hq, Mesh lod1, Bounds vung)
    {
        var an = new List<Renderer>(); var bang = new List<Renderer>();
        foreach (var r in t.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (r.transform.parent != t) continue;
            bool cu = false; foreach (var h in PHAN_CU) if (r.name.EndsWith(h)) { cu = true; break; }
            if (cu) an.Add(r); else if (r.name.Contains("Bang")) bang.Add(r);
        }
        if (an.Count == 0 || !vung.Contains(t.position)) return false;   // vị trí transform, không dùng bounds: bộ bàn thờ của đêm khác đang ẩn thì bounds = 0
        var hop = HopCucBo(t, an);
        foreach (var r in an) if (r.enabled) { r.enabled = false; daAn.Add(r); }
        DatHQ(t, hop, hq, lod1, bang);
        return true;
    }

    // vòng đứng riêng: một lưới nhiều vật liệu → cắt còn phần băng chữ
    bool VongDon(Transform t, MeshFilter mf, MeshRenderer mr, Mesh hq, Mesh lod1)
    {
        var luoi = mf.sharedMesh; var vl = mr.sharedMaterials;
        var giu = new List<int>();
        for (int i = 0; i < vl.Length && i < luoi.subMeshCount; i++) if (vl[i] && vl[i].name.StartsWith("Bang_Chu")) giu.Add(i);
        var hop = luoi.bounds;
        if (giu.Count > 0 && luoi.isReadable)
        {
            var bang = Object.Instantiate(luoi); bang.name = luoi.name + "_chiBang";
            bang.subMeshCount = giu.Count;
            var vlMoi = new Material[giu.Count];
            for (int k = 0; k < giu.Count; k++) { bang.SetTriangles(luoi.GetTriangles(giu[k]), k); vlMoi[k] = vl[giu[k]]; }
            daCat.Add((mf, luoi, vl)); mf.sharedMesh = bang; mr.sharedMaterials = vlMoi;
            // băng nằm chung lưới với t → không dời được băng; lùi vòng HQ ra sau 6 cm thay vào đó
            int truc = hop.size.x < hop.size.z ? 0 : 2; var tb = bang.bounds.center;
            float dau = (truc == 0 ? tb.x - hop.center.x : tb.z - hop.center.z) < 0 ? -1f : 1f;
            DatHQ(t, hop, hq, lod1, null, (truc == 0 ? Vector3.right : Vector3.forward) * -dau * 0.06f);
        }
        else { mr.enabled = false; daAn.Add(mr); DatHQ(t, hop, hq, lod1, null); }
        return true;
    }

    static Bounds HopCucBo(Transform goc, List<Renderer> ds)
    {
        bool dau = true; var b = new Bounds();
        var w2l = goc.worldToLocalMatrix;
        foreach (var r in ds)
        {
            var mf = r.GetComponent<MeshFilter>(); if (!mf || !mf.sharedMesh) continue;
            var mb = mf.sharedMesh.bounds; var m = w2l * r.transform.localToWorldMatrix;
            for (int i = 0; i < 8; i++)
            {
                var p = m.MultiplyPoint3x4(mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1)));
                if (dau) { b = new Bounds(p, Vector3.zero); dau = false; } else b.Encapsulate(p);
            }
        }
        return b;
    }

    // đặt HQ làm con của t, khít hộp bao (toạ độ cục bộ của t): cùng chiều cao, đáy chạm đất, tâm ngang trùng
    void DatHQ(Transform t, Bounds hop, Mesh hq, Mesh lod1, List<Renderer> bang, Vector3 lui = default)
    {
        var hb = hq.bounds;
        // vừa theo CHIỀU CAO (giá chạm đất, băng chữ cũ vắt ngang vòng như bản gốc). Vòng HQ có thể rộng hơn vòng cũ vài cm.
        float s = hop.size.y / Mathf.Max(0.01f, hb.size.y);
        var go = new GameObject("VongHoa_HQ"); go.transform.SetParent(t, false);
        go.transform.localScale = Vector3.one * s;
        go.transform.localPosition = new Vector3(hop.center.x - hb.center.x * s, hop.min.y - hb.min.y * s, hop.center.z - hb.center.z * s) + lui;
        var vl = VatLieu(hq, tenVL);
        var r0 = TaoLuoi(go.transform, "LOD0", hq, vl, ShadowCastingMode.Off);
        if (lod1)
        {
            var r1 = TaoLuoi(go.transform, "LOD1", lod1, vl, ShadowCastingMode.Off);
            var bong = TaoLuoi(go.transform, "Bong", lod1, vl, ShadowCastingMode.ShadowsOnly);   // bóng đổ từ bản nhẹ
            var lg = go.AddComponent<LODGroup>();
            lg.SetLODs(new[] { new LOD(0.22f, new Renderer[] { r0 }), new LOD(0.01f, new Renderer[] { r1 }) });
            lg.RecalculateBounds();
        }
        else r0.shadowCastingMode = ShadowCastingMode.On;
        daTao.Add(go);
        // băng chữ của vòng cũ: đẩy ra trước mặt hoa (hoa HQ dày hơn vành cũ)
        if (bang != null) foreach (var r in bang) DayBang(t, r.transform, hop);
    }

    // mặt trước = phía băng chữ lệch so với tâm vòng theo trục mỏng nhất của hộp bao
    void DayBang(Transform t, Transform b, Bounds hop)
    {
        int truc = hop.size.x < hop.size.z ? 0 : 2;
        var mf = b.GetComponent<MeshFilter>(); if (!mf || !mf.sharedMesh) return;
        var tamBang = t.InverseTransformPoint(b.TransformPoint(mf.sharedMesh.bounds.center));
        float lech = truc == 0 ? tamBang.x - hop.center.x : tamBang.z - hop.center.z;
        float dau = lech < 0 ? -1f : 1f;
        var doi = (truc == 0 ? Vector3.right : Vector3.forward) * dau * 0.06f;   // 6 cm (toạ độ cục bộ của vòng)
        daDoi.Add((b, b.localPosition));
        b.localPosition += b.parent ? b.parent.InverseTransformVector(t.TransformVector(doi)) : t.TransformVector(doi);
    }

    static MeshRenderer TaoLuoi(Transform cha, string ten, Mesh m, Material[] vl, ShadowCastingMode bong)
    {
        var g = new GameObject(ten); g.transform.SetParent(cha, false);
        g.AddComponent<MeshFilter>().sharedMesh = m;
        var r = g.AddComponent<MeshRenderer>(); r.sharedMaterials = vl; r.shadowCastingMode = bong;
        return r;
    }

    // vật liệu Lit theo tên vật liệu trong .glb (giữ đúng thứ tự submesh). Cánh hoa / lá là mặt đơn → tắt cull (hai mặt).
    static Material[] VatLieu(Mesh m, string[] tenGlb)
    {
        var lit = Shader.Find("Universal Render Pipeline/Lit");
        vlHQ ??= new Dictionary<string, Material>();
        // thứ tự submesh = thứ tự vật liệu khi xuất (vong_hoa_hq.py: MATS)
        string[] ten = { "cuc_trang", "cuc_trang_nga", "cuc_vang", "cuc_nho", "nhuy", "la", "loi", "tre" };
        Color[] mau = { C("#EFEBE1"), C("#E4DCC8"), C("#E2C24E"), C("#E8DEC4"), C("#B8A04A"), C("#3D5434"), C("#2E3A26"), C("#9C8455") };
        float[] bong = { 0.18f, 0.18f, 0.2f, 0.15f, 0.1f, 0.35f, 0.05f, 0.4f };
        var ds = new Material[Mathf.Max(1, m.subMeshCount)];
        for (int i = 0; i < ds.Length; i++)
        {
            int k = Mathf.Min(i, ten.Length - 1);
            if (tenGlb != null && i < tenGlb.Length) k = LoaiVL(tenGlb[i], k);
            if (!vlHQ.TryGetValue(ten[k], out var v) || !v)
            {
                v = new Material(lit) { name = "HQ_" + ten[k], enableInstancing = true };
                v.SetColor("_BaseColor", mau[k]); v.SetFloat("_Smoothness", bong[k]); v.SetFloat("_Metallic", 0);
                if (k <= 5) { v.SetFloat("_Cull", 0); v.doubleSidedGI = true; }
                vlHQ[ten[k]] = v;
            }
            ds[i] = v;
        }
        return ds;
    }
    static int LoaiVL(string n, int macDinh)
    {
        if (n.Contains("trang_nga")) return 1; if (n.Contains("cuc_trang")) return 0; if (n.Contains("vang")) return 2;
        if (n.Contains("cuc_nho")) return 3; if (n.Contains("nhuy")) return 4; if (n.Contains("duong_xi")) return 5;
        if (n.Contains("Canh_Hoa")) return 6; if (n.Contains("tre")) return 7; return macDinh;
    }
    static Color C(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }

    // ───────── bộ bàn thờ (BanThoVong_Dem1/2/3.glb): bàn phủ khăn, bát hương, lọ hoa, bát cơm quả trứng, đĩa trầu cau
    //  Giữ nguyên: ảnh thờ (đã đổi texture), chén nước (LocNuocChen), nhang đang cháy, đèn dầu, nến, vàng mã, mọi va chạm / chỗ bấm.
    static readonly (string hq, string cha, bool chaDauTien, string[] con, bool theoTruc)[] BO =
    {
        ("BanThoVong_HQ", "BanThoVong", false, new[] { "Khan_Phu", "Khan_MatBan", "Khan_GocGapLoi", "Than_Ban_Go", "Bac_Ke" }, true),
        ("BatHuong_HQ", "BatHuong", false, new[] { "BatHuong_Than", "BatHuong_MatTro" }, false),
        ("LoHoaCuc_HQ", "LoHoaCuc", false, new[] { "LoHoaCuc_" }, false),
        ("BatCom_HQ", "BatComQuaTrung_", true, new[] { "BatCom_" }, false),
        ("DiaTrauCau_HQ", "DiaTrauCau", false, new[] { "DiaTrauCau_" }, false),
    };
    void BanTho(Bounds vung)
    {
        var mau = Resources.Load<GameObject>("LOC_ThucTe/Mo/BanTho_HQ");
        if (!mau) { Debug.LogWarning("[LocDoMoi] thiếu Resources/LOC_ThucTe/Mo/BanTho_HQ.glb"); return; }
        var hq = new Dictionary<string, (Mesh m, Material[] vl)>();
        foreach (var f in mau.GetComponentsInChildren<MeshFilter>(true))
        {
            var r = f.GetComponent<Renderer>(); if (!f.sharedMesh || !r) continue;
            string k = f.name; foreach (var b in BO) if (f.name.StartsWith(b.hq) || f.sharedMesh.name.StartsWith(b.hq)) k = b.hq;
            hq[k] = (f.sharedMesh, r.sharedMaterials);
        }
        int dem = 0;
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!t || !vung.Contains(t.position)) continue;
            foreach (var b in BO)
            {
                if (!(b.chaDauTien ? t.name.StartsWith(b.cha) : t.name == b.cha) || !hq.TryGetValue(b.hq, out var h)) continue;
                var an = new List<Renderer>();
                foreach (var r in t.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (r.transform.parent != t) continue;
                    foreach (var c in b.con) if (r.name.StartsWith(c)) { an.Add(r); break; }
                }
                if (an.Count == 0) continue;
                var hop = HopCucBo(t, an); var hb = h.m.bounds;
                Vector3 s3;
                if (b.theoTruc) s3 = new Vector3(hop.size.x / Mathf.Max(1e-3f, hb.size.x), hop.size.y / Mathf.Max(1e-3f, hb.size.y), hop.size.z / Mathf.Max(1e-3f, hb.size.z));
                else { float k = (hop.size.x / Mathf.Max(1e-3f, hb.size.x) + hop.size.z / Mathf.Max(1e-3f, hb.size.z)) * 0.5f; s3 = new Vector3(k, k, k); }
                var go = new GameObject(b.hq); go.transform.SetParent(t, false);
                go.transform.localScale = s3;
                go.transform.localPosition = new Vector3(hop.center.x - hb.center.x * s3.x, hop.min.y - hb.min.y * s3.y, hop.center.z - hb.center.z * s3.z);
                go.AddComponent<MeshFilter>().sharedMesh = h.m;
                var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterials = h.vl; mr.shadowCastingMode = ShadowCastingMode.On;
                foreach (var r in an) if (r.enabled) { r.enabled = false; daAn.Add(r); }
                daTao.Add(go); dem++;
            }
        }
        Debug.Log($"[LocDoMoi] bàn thờ: thay {dem} món bằng bản chi tiết");
    }

    // ───────── đồ đạc (NoiThat_HQ.glb). Bản HQ dựng đúng hộp bao của bản gốc → tỉ lệ ≈ 1; vẫn khít theo từng trục cho chắc.
    //  chiAn = null: ẩn mọi MeshRenderer bên dưới object; khác null: chỉ ẩn con trực tiếp tên đó (cửa chính: tấm "Canh", giữ núm đồng + bản lề).
    static readonly (string hq, string ten, bool tienTo, string chiAn)[] NOI_THAT =
    {
        ("SalonGheDai_HQ", "Salon_GheDai", false, null),
        ("TuTVDung_HQ", "TuTV_Dung", false, null),
        ("GheDauGo_HQ", "GheDau_Go", false, null),
        ("QuatTranThan_HQ", "QuatTran_Than", false, null),
        ("QuatTranCanh_HQ", "QuatTran_Canh", false, null),
        ("CuaChinh_HQ", "CuaChinh_", true, "Canh"),
    };
    static bool TenKhop(string n, string ten) => n == ten || n.StartsWith(ten + " (");
    void NoiThat(Bounds vung)
    {
        var mau = Resources.Load<GameObject>("LOC_ThucTe/Mo/NoiThat_HQ");
        if (!mau) { Debug.LogWarning("[LocDoMoi] thiếu Resources/LOC_ThucTe/Mo/NoiThat_HQ.glb"); return; }
        var hq = new Dictionary<string, (Mesh m, Material[] vl)>();
        foreach (var f in mau.GetComponentsInChildren<MeshFilter>(true))
        {
            var r = f.GetComponent<Renderer>(); if (!f.sharedMesh || !r) continue;
            foreach (var b in NOI_THAT) if (f.name.StartsWith(b.hq) || f.sharedMesh.name.StartsWith(b.hq)) hq[b.hq] = (f.sharedMesh, r.sharedMaterials);
        }
        var dem = new Dictionary<string, int>();
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!t || !vung.Contains(t.position)) continue;
            foreach (var b in NOI_THAT)
            {
                if (!(b.tienTo ? t.name.StartsWith(b.ten) : TenKhop(t.name, b.ten)) || !hq.TryGetValue(b.hq, out var h)) continue;
                var an = new List<Renderer>();
                foreach (var r in t.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (!r.enabled) continue;   // đang bị ẩn (cất đi / logic game) → không hiện bản mới thay nó
                    if (b.chiAn != null ? (r.transform.parent == t && r.name == b.chiAn) : true) an.Add(r);
                }
                if (an.Count == 0) continue;
                var hop = HopCucBo(t, an); var hb = h.m.bounds;
                var s3 = new Vector3(hop.size.x / Mathf.Max(1e-3f, hb.size.x), hop.size.y / Mathf.Max(1e-3f, hb.size.y), hop.size.z / Mathf.Max(1e-3f, hb.size.z));
                if (s3.x < 0.5f || s3.x > 2f || s3.y < 0.5f || s3.y > 2f || s3.z < 0.5f || s3.z > 2f)
                { Debug.LogWarning($"[LocDoMoi] {t.name}: hộp bao lệch quá xa bản HQ (tỉ lệ {s3}) — giữ bản cũ"); continue; }
                var go = new GameObject(b.hq); go.transform.SetParent(t, false); go.layer = t.gameObject.layer;
                go.transform.localScale = s3;
                go.transform.localPosition = new Vector3(hop.center.x - hb.center.x * s3.x, hop.center.y - hb.center.y * s3.y, hop.center.z - hb.center.z * s3.z);
                go.AddComponent<MeshFilter>().sharedMesh = h.m;
                var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterials = h.vl; mr.shadowCastingMode = ShadowCastingMode.On;
                foreach (var r in an) { r.enabled = false; daAn.Add(r); }
                daTao.Add(go); dem[b.hq] = dem.TryGetValue(b.hq, out var d) ? d + 1 : 1;
            }
        }
        var sb = new System.Text.StringBuilder("[LocDoMoi] đồ đạc:");
        foreach (var b in NOI_THAT) sb.Append($" {b.hq}×{(dem.TryGetValue(b.hq, out var d) ? d : 0)}");
        Debug.Log(sb.ToString());
    }

    // ───────── ảnh thờ
    void AnhTho(Bounds vung)
    {
        var tex = Resources.Load<Texture2D>("LOC_ThucTe/Mo/AnhTho_Atlas");
        if (!tex) return;
        var moi = new Dictionary<Material, Material>();
        foreach (var r in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!r || !vung.Contains(r.transform.position)) continue;
            var ms = r.sharedMaterials; bool doi = false;
            for (int i = 0; i < ms.Length; i++)
            {
                var m = ms[i]; if (!m || !m.name.StartsWith("Anh_Tho_Atlas")) continue;
                if (!moi.TryGetValue(m, out var n))
                {
                    n = new Material(m) { name = m.name + "_AnhCu" };
                    foreach (var p in new[] { "_BaseMap", "_MainTex", "baseColorTexture" }) if (n.HasProperty(p)) n.SetTexture(p, tex);
                    moi[m] = n;
                }
                if (!doi) { daDoiVL.Add((r, ms)); ms = (Material[])ms.Clone(); doi = true; }
                ms[i] = n;
            }
            if (doi) r.sharedMaterials = ms;
        }
        Debug.Log($"[LocDoMoi] ảnh thờ: thay {moi.Count} vật liệu");
    }

    // ───────── tắt "Thực tế": trả lại y nguyên
    void OnDestroy()
    {
        foreach (var g in daTao) if (g) Destroy(g);
        foreach (var r in daAn) if (r) r.enabled = true;
        foreach (var (mf, luoi, vl) in daCat) if (mf) { var b = mf.sharedMesh; mf.sharedMesh = luoi; var mr = mf.GetComponent<MeshRenderer>(); if (mr) mr.sharedMaterials = vl; if (b && b != luoi) Destroy(b); }
        foreach (var (t, p) in daDoi) if (t) t.localPosition = p;
        foreach (var (r, vl) in daDoiVL) if (r) r.sharedMaterials = vl;
    }
}
