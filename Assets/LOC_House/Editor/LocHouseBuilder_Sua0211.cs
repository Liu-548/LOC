using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// [2/11] Sửa theo khu (chơi tay 02/11): phòng khách (bàn thờ vong), bếp, hầm. Gọi trong Build() ngay sau Sua0111 (Đêm 2/3 còn bật).
//  Ghi kết quả: Assets/LOC_House/BaoCao_Sua0211.txt
public static partial class LocHouseBuilder
{
    static void Sua0211()
    {
        var log = new StringBuilder("SỬA 02/11 — " + System.DateTime.Now.ToString("dd/MM HH:mm") + "\n");
        void Buoc(string ten, System.Action a) { try { a(); } catch (System.Exception e) { log.AppendLine("LỖI " + ten + ": " + e.Message); Debug.LogException(e); } }
        Buoc("bàn thờ vong", () => BanThoVong0211(log));
        Buoc("bếp", () => Bep0211(log));
        Buoc("nồi to", () => NoiTo0211(log));
        Buoc("chén vỡ hầm", () => ChenVo0211(log));
        // chồng khăn tang: mặt đáy hai mặt (doubleSided) nằm đúng mặt bàn nhựa, lộ qua khe giữa tấm khăn trên và hộp khăn → nhấc 2 mm
        Buoc("khăn tang", () => { foreach (var n in new[] { "SM_MourningCloth_Stack_Can", "SM_MourningCloth_Stack_Voi" }) foreach (var t in TatCa(n).Where(t => t.GetComponent<LocProp>())) { t.position += Vector3.up * 0.002f; log.AppendLine("  nhấc 2 mm: " + PathOf(t)); } });
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_Sua0211.txt", log.ToString());
    }

    static Bounds? BaoR(Transform t)
    {
        var rs = t.GetComponentsInChildren<Renderer>(); if (rs.Length == 0) return null;
        var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b;
    }
    // dời t để đáy hộp bao nằm ở y, (tuỳ chọn) tâm hộp bao ngang ở (x, z)
    static void DatDay(Transform t, float y, float? x = null, float? z = null)
    {
        var b = BaoR(t); if (b == null) return; var c = b.Value.center;
        t.position += new Vector3(x.HasValue ? x.Value - c.x : 0, y - b.Value.min.y, z.HasValue ? z.Value - c.z : 0);
    }
    static Transform Con(Transform p, string ten) => p.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == ten);

    // ── bàn thờ vong (BanThoVong_Dem1/2/3 lấy lại .glb gốc: còn ảnh thờ + bậc kê; bản nâng cấp 29/10 gộp lưới làm mất ảnh, mất bậc nên ảnh lơ lửng).
    //   Bố cục trên bậc kê: [ảnh mẹ][linh vị][ảnh bố]; bát hương nhích lên trước cho khỏi lấn bậc; hai vòng hoa dạt ra ngoài cho dải băng không rũ lên mặt bàn.
    static void BanThoVong0211(StringBuilder log)
    {
        Vector3? linhVi = null; float yBac = 0;
        foreach (var set in root.GetComponentsInChildren<Transform>(true).Where(t => t.name.StartsWith("BanThoVong_Dem") && t.GetComponent<LocProp>()))
        {
            Transform T(string n) => Con(set, n);
            var bh = T("BatHuong"); var chen = T("ChenNuoc_BanTho_02"); var lo = T("LoHoaCuc"); var den = T("DenDau");
            if (!bh || !chen || !lo || !den) { log.AppendLine($"  {set.name}: thiếu nút (glb chưa phải bản gốc?)"); continue; }
            var truoc = chen.position - bh.position; truoc.y = 0; truoc.Normalize();
            var phai = lo.position - den.position; phai.y = 0; phai -= truoc * Vector3.Dot(phai, truoc); phai.Normalize();
            var bh0 = bh.position;
            void Doi(string n, Vector3 d) { var t = T(n); if (t) t.position += d; else log.AppendLine($"  {set.name}: không thấy {n}"); }
            Doi("AnhTho_Me", -0.035f * phai); Doi("AnhTho_Bo", 0.04f * phai);
            Doi("BatHuong", 0.035f * truoc); Doi("NhangDangChay", 0.035f * truoc);
            var ch = set.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name.StartsWith("ChanHuong_Dem")); if (ch) ch.position += 0.035f * truoc;
            Doi("VongHoaTang_1", -0.31f * phai); Doi("VongHoaTang_2", 0.31f * phai);
            Doi("SapVangMa", 0.05f * phai);
            var bac = T("Bac_Ke");
            if (bac && linhVi == null) { yBac = bac.GetComponent<Renderer>().bounds.max.y; linhVi = bh0 - 0.135f * truoc; }
            log.AppendLine($"  {set.name}: ảnh tách sang hai bên, bát hương +3,5 cm, vòng hoa ±31 cm, sấp vàng mã vào 5 cm");
        }
        var lv = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name is "PH_BaiVi_Vong" or "BaiVi_Vong");
        if (lv && linhVi != null) { DatDay(lv, yBac, linhVi.Value.x, linhVi.Value.z); log.AppendLine($"  linh vị lên bậc kê giữa hai ảnh: {lv.position:F3}"); }
        else log.AppendLine("  linh vị: không đặt được");
    }

    // ── bếp: bếp ga/nồi cơm/phích bị rà soát thả lọt xuống 3,5 cm trong mặt bếp xây; ấm không ngồi đúng mâm đốt
    static void Bep0211(StringBuilder log)
    {
        var bep = root.Find("Tang_1/Bep"); var mat = bep ? bep.Find("MatBepXay_180") : null; var ga = bep ? bep.Find("BepGas_Doi") : null;
        if (!mat || !ga) { log.AppendLine("  bếp: thiếu mặt bếp / bếp ga"); return; }
        float yMat = BaoR(mat).Value.max.y;
        float nang = yMat - BaoR(ga).Value.min.y;
        var am = bep.Find("AmNhom"); var chao = bep.Find("NoiChao_Rong");
        foreach (var t in new[] { ga, am, chao }) if (t) t.position += Vector3.up * nang;
        float yGa = BaoR(ga).Value.max.y;
        var mam = Con(ga, "Men_OVang_QuanhMamDot.001");
        if (am && mam) { var c = mam.GetComponent<Renderer>().bounds.center; DatDay(am, yGa, c.x - 0.056f, c.z); }   // vòi ấm chĩa −x: thân lệch +5,6 cm so với tâm hộp bao
        if (chao) DatDay(chao, yGa);
        foreach (var (n, x) in new[] { ("LonSuaDac_DungBut", 1.16f), ("NoiComDien", 1.43f), ("PhichHoa", 1.70f) })
        { var t = bep.Find(n); if (t) DatDay(t, yMat, x, 16.1f); }
        log.AppendLine($"  bếp: nâng bếp ga {nang * 100:0.0} cm lên mặt bếp {yMat:0.000}; ấm vào mâm đốt phải; lon đũa / nồi cơm / phích dàn đều x 1,16 · 1,43 · 1,70");
    }

    // ── nồi nhôm to trên bếp kiềng: lọt 5 cm vào mặt bếp ga, lệch khỏi tâm bếp → ngồi đúng lên kiềng, giữa bếp
    static void NoiTo0211(StringBuilder log)
    {
        var bepKieng = root.GetComponentsInChildren<Transform>(true).Where(t => t.name == "BepGas_Doi" && t.parent && t.parent.name.StartsWith("View_BepGas_Kieng")).ToList();
        foreach (var noi in root.GetComponentsInChildren<Transform>(true).Where(t => t.name == "NoiNhom_To"))
        {
            var bn = BaoR(noi); if (bn == null) continue;
            foreach (var g in bepKieng)
            {
                var bg = BaoR(g); if (bg == null) continue;
                var d = bn.Value.center - bg.Value.center; d.y = 0;
                if (d.magnitude > 0.3f || bn.Value.min.y < bg.Value.max.y - 0.2f) continue;   // nồi úp dưới sàn (Đêm 3) không đụng
                DatDay(noi, bg.Value.max.y, bg.Value.center.x, bg.Value.center.z);
                log.AppendLine($"  nồi to {PathOf(noi)} → giữa bếp kiềng, đáy {bg.Value.max.y:0.000}");
            }
        }
    }

    // ── chén 1, chén 2 dưới hầm: ba mảnh vẫn dựng nguyên hình chén → cho đổ ra: mảnh ngả/lật ra ngoài, văng vài cm, xoay ngẫu nhiên, nằm đúng mặt khay/bàn
    static void ChenVo0211(StringBuilder log)
    {
        var tam = new List<Collider>();
        foreach (var n in new[] { "Bo_Chen_Tren_De", "BanThoHam" })
            foreach (var t in TatCa(n)) foreach (var mf in t.GetComponentsInChildren<MeshFilter>().Where(f => f.sharedMesh && f.sharedMesh.triangles.Length > 0)) { var mc = mf.gameObject.AddComponent<MeshCollider>(); mc.sharedMesh = mf.sharedMesh; tam.Add(mc); }
        Physics.SyncTransforms();
        var rd = new System.Random(211);
        float R(float a, float b) => a + (float)rd.NextDouble() * (b - a);
        var goc = new[] { (82f, 0.025f), (104f, 0.04f), (24f, 0.006f) };   // (độ ngả, văng xa): hai mảnh nằm, một mảnh còn tựa nghiêng
        foreach (var ten in new[] { "Chen1_Vo", "Chen2_Vo" })
        {
            var chen = TatCa(ten).FirstOrDefault(); if (!chen) continue;
            var b0 = BaoR(chen); if (b0 == null) continue; var tamChen = b0.Value.center;
            var manh = chen.GetComponentsInChildren<MeshRenderer>(true).Select(r => r.transform).ToList();
            for (int i = 0; i < manh.Count; i++)
            {
                var m = manh[i]; var bm = m.GetComponent<Renderer>().bounds;
                var ra = bm.center - tamChen; ra.y = 0; if (ra.sqrMagnitude < 1e-6f) ra = Quaternion.Euler(0, R(0, 360), 0) * Vector3.forward; ra.Normalize();
                var (doNga, vang) = goc[i % goc.Length];
                var chan = new Vector3(bm.center.x, bm.min.y, bm.center.z) + ra * bm.extents.magnitude * 0.5f;   // mép đáy phía ngoài làm điểm xoay
                m.RotateAround(chan, Vector3.Cross(Vector3.up, ra), doNga + R(-8, 8));
                m.RotateAround(chan, Vector3.up, R(-35, 35));
                m.position += ra * (vang + R(0, 0.01f));
                Physics.SyncTransforms();
                var b = m.GetComponent<Renderer>().bounds;
                // dò 5 điểm (tâm + 4 góc đáy): nằm lên chỗ đỡ cao nhất → mảnh vắt qua mép khay thì gác lên khay chứ không cắm vào thành khay
                float y = float.MinValue;
                foreach (var (sx, sz) in new[] { (0f, 0f), (-1f, -1f), (-1f, 1f), (1f, -1f), (1f, 1f) })
                {
                    var o = new Vector3(b.center.x + sx * b.extents.x * 0.8f, b.max.y + 0.3f, b.center.z + sz * b.extents.z * 0.8f);
                    var hit = Physics.RaycastAll(o, Vector3.down, 1f).Where(h => tam.Contains(h.collider)).OrderBy(h => h.distance).FirstOrDefault();
                    if (hit.collider) y = Mathf.Max(y, hit.point.y);
                }
                if (y == float.MinValue) y = b0.Value.min.y;
                m.position += Vector3.up * (y + 0.0005f - b.min.y);
            }
            log.AppendLine($"  {ten}: {manh.Count} mảnh đổ ra quanh chỗ chén");
        }
        foreach (var c in tam) Object.DestroyImmediate(c);
    }
}
