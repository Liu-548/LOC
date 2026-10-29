using System.Linq;
using UnityEngine;

// [29/10] Đồ vật còn thiếu theo kịch bản v3.3 (VIEC_CON_LAI_GAMEPLAY.md mục 1.5 + model thật cho đồ mới của LocCotTruyen_V34).
//  Model mới: Models/V35 (nguồn LOC_Nguon/V35: tex.py + build_dovat.py). Chén hầm dùng bộ có sẵn Assets/Prefab/BoChen.
//  Gọi trong Build() ngay sau Tang3(), TRƯỚC rà soát: đồ kiểu San được thả chạm mặt đỡ + tách chồng lấn như mọi đồ khác.
//  Tên đối tượng ở đây là hợp đồng với LocCotTruyen_V34 (VP_ChenSo3, VT_GoiHang, LoiNhanMe_Giay, ChenHam_TrangThai/…) — đổi tên thì sửa cả hai.
public static partial class LocHouseBuilder
{
    static void DoVatV35()
    {
        DenGanTuong();
        // ── phòng Nhím: tủ đầu giường đã có cốc + vỉ thuốc gộp trong lưới (thô, cốc không có nước) → bỏ hai phần đó, đặt model thật đúng chỗ cũ
        var ban = Tim("BanCanhGiuong_Thuoc");
        var mf = ban ? ban.GetComponentInChildren<MeshFilter>() : null;
        var mr = mf ? mf.GetComponent<MeshRenderer>() : null;
        if (mf && mr)
        {
            var m = Object.Instantiate(mf.sharedMesh); m.name = mf.sharedMesh.name + "_V35";
            var ms = mr.sharedMaterials;
            for (int i = 0; i < m.subMeshCount && i < ms.Length; i++)
                if (ms[i] && (ms[i].name.StartsWith("M_CocTrong") || ms[i].name.StartsWith("M_ThuocVi"))) m.SetTriangles(new int[0], i);
            mf.sharedMesh = m;
            var old = cur; cur = ban.parent;
            float top = mr.bounds.min.y + 0.55f;   // mặt tủ (tủ cao 0,55 trong file)
            A("CocNuoc_Nhim", 7.49f, 9.64f, top, 0);            // khe trống giữa ấm (trước), gạc (sau) và chân đèn — chỗ cốc cũ đè lên gạc
            A("ViThuoc_HaSot", 7.28f, 9.545f, top, 12);        // chỗ vỉ cũ (trước, phía cửa)
            cur = old;
        }

        // ── phòng bố mẹ, Đêm 3: lời nhắn của mẹ trên bàn trang điểm, nửa trống phía cửa sổ (đồ trang điểm ở nửa kia, z > 2,5)
        var btd = Tim("BanTrangDiem_Bo");
        if (btd)
        {
            var old = cur; cur = G(PathOf(btd.parent) + "/Dem3");
            A("LoiNhanMe_Giay", 0.29f, 2.22f, 3.6f + 0.73f, 104);   // chữ quay về phía người đứng trước bàn (+X)
            cur = old;
        }

        // ── tầng 3: chén số ba (chén anh Phong, vỡ một lần, bố dán lại — dùng ChenC_GanLai) trên bàn thờ ông bà, trước, giữa nến trái và bát hương (khe trống, không đè đồ thờ)
        var anh = Tim("KhungAnhOngBa");
        if (anh)
        {
            var old = cur; cur = anh.parent.parent;   // ngang hàng bộ bàn thờ, không lồng trong món khác (rà soát xử lý từng món)
            var c = A("ChenC_GanLai", 3.60f, 5.95f, 8.3f, 20);   // đặt cao hơn mặt bàn: rà soát thả xuống mặt đỡ cao nhất
            if (c) c.name = "VP_ChenSo3";
            cur = old;
        }

        // ── Đêm 3: gói hàng giấy dầu ngay trong cửa chính (chỉ lấy phần gói đã buộc của bộ gd6, nằm dẹt)
        {
            var old = cur; cur = G("Tang_1/PhongKhach/Dem3");
            var g = A("LOC_KienHang_ThayDong", 5.0f, 0.45f, 0f, 15, hide: new[] { "PROXY", "LaBua", "NutChuong", "ChuongDien", "KienHang_Mo", "ManhGiay", "VFX", "LIGHT" });
            if (g)
            {
                g.name = "VT_GoiHang";
                g.transform.rotation = Quaternion.Euler(90f, 15f, 0f);   // file dựng gói đứng (dày theo trục sâu) → đặt nằm
                Align(g, 5.0f, 0f, 0.45f);
                VaChamBao(g, 0.2f);
            }
            cur = old;
        }

        // ── hầm: năm chỗ chén đúng kịch bản (hai cái đầu vỡ · chỗ ba trống · cái bốn nứt dọc · cái năm mẻ miệng) + các trạng thái về sau
        //    (dán chén Đêm 2 · chén số ba đặt vào · Bad: cái năm vỡ · Death: cái bốn vỡ) — LocCotTruyen.ChenHam(...) bật/tắt lúc chơi
        var bo = Tim("Bo_Chen_Tren_De");
        if (bo)
        {
            var nhom = new GameObject("ChenHam_TrangThai").transform; nhom.SetParent(bo.parent, false);
            var old = cur; cur = nhom;
            var cho = new (string ten, string model, bool bat)[][]
            {
                new[] { ("Chen1_Vo", "ChenA_Tach", true), ("Chen1_Dan", "ChenA_GanLai", false) },
                new[] { ("Chen2_Vo", "ChenB_Tach", true), ("Chen2_Dan", "ChenB_GanLai", false) },
                new[] { ("Chen3_Dan", "ChenC_GanLai", false) },
                new[] { ("Chen4", "ChenNut", true), ("Chen4_Vo", "ChenC_Tach", false) },
                new[] { ("Chen5", "ChenMe", true), ("Chen5_Vo", "ChenB_Tach", false) },
            };
            for (int i = 0; i < 5; i++)
            {
                var goc = bo.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "ChenNguyen_" + (i + 1) && t.GetComponent<Renderer>())
                       ?? bo.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "ChenNguyen_" + (i + 1));
                if (!goc) continue;
                var r = goc.GetComponentInChildren<Renderer>(); var b = r.bounds;
                foreach (var t in bo.GetComponentsInChildren<Transform>(true)) if (t.name == "ChenNguyen_" + (i + 1)) t.gameObject.SetActive(false);
                foreach (var (ten, model, bat) in cho[i])
                {
                    var g = Inst(model); if (!g) continue;
                    g.name = ten;
                    g.transform.SetPositionAndRotation(new Vector3(b.center.x, b.min.y, b.center.z), Quaternion.Euler(0, 37f * i + 10f, 0));
                    g.SetActive(bat);
                }
            }
            cur = old;
        }
    }

    // [29/10] đèn có ánh sáng mà không có đèn (VIEC_CON_LAI 2, mục đèn): chiếu nghỉ thang T1/T2 (trên đầu là gầm vế thang trên, không có trần để treo)
    //  và giếng trời (lộ thiên) → đèn tuýp gắn tường như nhà phố 2002: tìm bức tường gần nhất ngang đèn, gắn máng + ống, dời nguồn sáng ra trước ống.
    //  Sân phơi đã có bóng chao tôn kéo dây (BongDenSan_DayDien) → dời nguồn sáng về ngay dưới bóng đó. Luật dự án: không bóng trần trần trụi ngoài hầm.
    static void DenGanTuong()
    {
        Physics.SyncTransforms();
        Light Den(string ten) => root.GetComponentsInChildren<Light>(true).FirstOrDefault(l => l.name == "Den_" + ten);
        foreach (var ten in new[] { "ChieuNghi_T1", "ChieuNghi_T2", "GiengTroi" })
        {
            var l = Den(ten); if (!l) continue;
            var p = l.transform.position; RaycastHit tot = default; float gan = 1.6f;
            // chiếu nghỉ: tường gần nhất là đầu vách giữa hai vế thang (hẹp hơn máng 0,62 m → máng lơ lửng trên bậc) → ưu tiên tường cuối chiếu nghỉ (+Z)
            var huong = ten.StartsWith("ChieuNghi") ? new[] { Vector3.forward } : new[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            foreach (var h in huong)
                if (Physics.Raycast(p, h, out var hit, gan, ~0, QueryTriggerInteraction.Ignore) && !hit.collider.GetComponentInParent<LocDoor>()) { gan = hit.distance; tot = hit; }
            if (!tot.collider) { Debug.LogWarning("[LOC] đèn " + ten + ": không thấy tường trong 1,6 m"); continue; }
            var n = tot.normal; n.y = 0; n.Normalize();
            var g = new GameObject("DenTuong_" + ten).transform; g.SetParent(l.transform.parent, false);
            // độ cao: đèn cũ đặt 1,2–1,3 m trên mặt chiếu nghỉ (ngang ngực) → nâng lên 2,1 m trên sàn ngay dưới, chừa 0,3 m dưới trần/sàn tầng trên nếu có
            var q = tot.point + n * 0.15f; float cao = p.y;
            if (Physics.Raycast(q, Vector3.down, out var san, 3f, ~0, QueryTriggerInteraction.Ignore)) cao = san.point.y + 2.1f;
            if (Physics.Raycast(q, Vector3.up, out var tran, 3f, ~0, QueryTriggerInteraction.Ignore)) cao = Mathf.Min(cao, tran.point.y - 0.3f);
            cao = Mathf.Max(cao, p.y); var dat = tot.point;
            // ở độ cao mới mặt tường có thể khác (T1: từ cao độ sàn T2 trở lên là tường SauChieuNghi dày hơn 10 cm) → bắn lại tia ngang
            if (Physics.Raycast(new Vector3(p.x, cao, p.z), -n, out var lai, 1.6f, ~0, QueryTriggerInteraction.Ignore)) dat = lai.point;
            g.SetPositionAndRotation(new Vector3(dat.x, cao, dat.z), Quaternion.LookRotation(n, Vector3.up));   // +Z = ra khỏi tường
            void Khoi(string tk, Vector3 tam, Vector3 co, string hex)
            {
                var k = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(k.GetComponent<Collider>());
                k.name = tk; k.transform.SetParent(g, false); k.transform.localPosition = tam; k.transform.localScale = co;
                var r = k.GetComponent<Renderer>(); r.sharedMaterial = M("M_DenTuong_" + tk, hex); r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            Khoi("Mang", new Vector3(0, 0, 0.02f), new Vector3(0.62f, 0.05f, 0.04f), "#D8D4C8");              // máng sắt sơn trắng ngà, ngả vàng
            Khoi("Ong", new Vector3(0, -0.005f, 0.055f), new Vector3(0.6f, 0.028f, 0.028f), "#F4F2EA");         // ống tuýp 0,6 m
            var mo = M("M_DenTuong_Ong", "#F4F2EA"); mo.EnableKeyword("_EMISSION"); mo.SetColor("_EmissionColor", new Color(1f, 0.95f, 0.85f) * 1.6f); UnityEditor.EditorUtility.SetDirty(mo);   // ống sáng; LocCongTac tự tắt phát sáng trong 0,9 m quanh đèn khi gạt công tắc
            Khoi("DayDien", new Vector3(0.33f, -0.25f, 0.008f), new Vector3(0.008f, 0.5f, 0.008f), "#2A2620");  // dây điện chạy dọc tường xuống
            l.transform.position = g.TransformPoint(new Vector3(0, -0.12f, 0.25f));   // hơi dưới ống: mặt dưới máng cũng sáng khi nhìn từ dưới thang lên
        }
        var sp = Den("SanPhoi"); var bong = Tim("BongDenSan_DayDien");
        if (sp && bong)
        {
            var rs = bong.GetComponentsInChildren<Renderer>(); var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
            sp.transform.position = new Vector3(b.center.x, b.min.y - 0.08f, b.min.z + 0.04f);   // dưới chao (đầu thấp của dây)
            sp.range = 7.5f;   // một bóng cho cả sân phơi 7,6 m
            // đèn điểm không bóng đổ xuyên tường sau lưng (trần phòng trong sáng một vệt) → đèn rọi 170° chúc xuống, quay lưng vào tường
            sp.type = LightType.Spot; sp.spotAngle = 170f; sp.innerSpotAngle = 100f;
            sp.transform.rotation = Quaternion.LookRotation(new Vector3(0, -0.82f, -0.57f));
        }
    }

    static string PathOf(Transform t)
    {
        var s = t.name;
        for (var p = t.parent; p && p != root; p = p.parent) s = p.name + "/" + s;
        return s;
    }
}
