using UnityEngine;

// [14/11] ĐÊM 1 — CÂU ĐỐ MÃ KÉT (thay dòng "Ứng cho thằng Phong 1.980.000" trong sổ thu chi).
//  · Dụ: thăm em xong, xuống tầng 1 lại gần giếng trời → tiếng đập cửa sắt vọng từ dưới hầm → người chơi tự tìm tới cửa.
//  · Thấy cửa (KiemKhamPha) → Khôi nhớ: chìa gì quan trọng bố cũng khóa vào két.
//  · Bấm két lần đầu → tự mở mẩu giấy mẹ dán trên nóc két: bố hay quên mã, mẹ chép ra BỐN MẢNH giấu bốn chỗ bố hay ngồi.
//  · Bốn mảnh (nhặt vào túi, đọc lại được): mỗi mảnh một câu đố cho một chữ số → 1 · 9 · 8 · 0.
//      1 bàn nước phòng khách (chỗ bố pha trà)  · 2 bàn giấy phòng làm việc (chỗ bố tính sổ)
//      3 tủ đầu giường phòng Nhím (chỗ bố canh con ốm) · 4 bàn trang điểm phòng bố mẹ (chỗ mẹ chải đầu)
//  · Đủ bốn mảnh → Khôi ghép: "Một chín tám mươi. Năm anh Phong ra đời." (1980 = năm sinh Phong, kịch bản v3.1)
public partial class LocCotTruyen
{
    LocVatPham vpGiayKet;
    public static LocVatPham GiayKet => I ? I.vpGiayKet : null;
    static readonly string[] MANH_ID = { "manh_ma_1", "manh_ma_2", "manh_ma_3", "manh_ma_4" };
    public static int SoManh() { int n = 0; foreach (var id in MANH_ID) if (LocTui.CoMon(id)) n++; return n; }

    void KhoiTaoManhMa()
    {
        if (LocMa.dem != 1) return;
        try
        {
            // mẩu giấy mẹ dán trên nóc két
            var ket = GameObject.Find("KetSat_Dung");
            if (ket)
            {
                var g = TaoGiay("GiayMe_TrenKet", ket.transform, new Vector2(0.12f, 0.09f), true);
                if (g) vpGiayKet = Mon(g, "giay_ket", "mẩu giấy trên két", LocVatPham.Kieu.Doc, "Mẩu giấy dán băng dính trên nóc két, chữ mẹ.", 1,
                    new[] { "Chữ mẹ." }, new[]
                    {
                        "<i>Băng dính đã ngả vàng. Chữ mẹ, nhỏ, đều.</i>\n\nÔng hay quên mã quá.\n\nEm chép ra bốn mảnh, giấu ở bốn chỗ ông hay ngồi nhất:\n\n   — chỗ ông pha trà mỗi sáng,\n   — chỗ ông ngồi tính sổ,\n   — chỗ ông ngồi canh con Nhím mỗi lần nó ốm,\n   — còn mảnh cuối em giữ, ở chỗ em ngồi chải đầu.\n\nGhép theo thứ tự mà mở.\nĐừng viết thẳng mã ra giấy nữa, ông nhé.",
                    });
            }
            Manh(0, "Salon_BanNuoc", "mảnh giấy thứ nhất", "Mảnh giấy xé, chữ mẹ. Kẹp dưới khay trà.",
                "<i>Mảnh thứ nhất.</i>\n\n\nĐứng đầu mà lẻ loi một mình,\ngiống ông những lúc giận cả nhà.");
            Manh(1, "BanGiay_Go", "mảnh giấy thứ hai", "Mảnh giấy xé, chữ mẹ. Nằm lẫn trong giấy tờ của bố.",
                "<i>Mảnh thứ hai.</i>\n\n\nLà con số sáu\nđứng trồng cây chuối.");
            Manh(2, "BanCanhGiuong_Thuoc", "mảnh giấy thứ ba", "Mảnh giấy xé, chữ mẹ. Kẹp dưới vỉ thuốc đầu giường con bé.",
                "<i>Mảnh thứ ba.</i>\n\n\nHai cái vòng tròn chồng lên nhau,\nnhư hai thằng con trai nhà mình\nhồi bé ngủ chung một giường.");
            Manh(3, "BanTrangDiem_Bo", "mảnh giấy cuối", "Mảnh giấy xé, chữ mẹ. Dưới hộp phấn trên bàn trang điểm.",
                "<i>Mảnh cuối.</i>\n\n\nTròn như cái mâm cơm,\nmà trong mâm chẳng có gì.\n\n\n<size=19><color=#6A5A48>...Năm ấy nhà mình mừng nhất.</color></size>");
        }
        catch (System.Exception e) { Debug.LogWarning("[LocManhMa] " + e.Message); }
    }

    void Manh(int i, string tenBan, string ten, string ghiChu, string chu)
    {
        if (LocTui.Co("ket_mo")) return;
        var ban = GameObject.Find(tenBan); if (!ban) { Debug.LogWarning("[LocManhMa] không thấy " + tenBan); return; }
        var g = TaoGiay("ManhMa_" + (i + 1), ban.transform, new Vector2(0.10f, 0.07f), false);
        if (!g) return;
        var nghi = i switch
        {
            0 => new[] { "Chữ mẹ.", "...Mảnh thứ nhất. Còn ba mảnh." },
            1 => new[] { "Lại chữ mẹ." },
            2 => new[] { "Hai thằng con trai...", "Mình với anh Phong." },
            _ => new[] { "Mảnh cuối." },
        };
        var v = Mon(g, MANH_ID[i], ten, LocVatPham.Kieu.Nhat, ghiChu, 1, nghi, new[] { chu });
        if (v) v.docKhiNhat = true;   // nhặt là mở ra đọc luôn câu đố (đọc lại trong túi)
    }

    // tờ giấy mỏng nằm trên mặt đồ vật: dò mặt phẳng bằng tia từ trên xuống, ưu tiên góc (giữa bàn hay có đồ)
    GameObject TaoGiay(string ten, Transform ban, Vector2 co, bool giua)
    {
        var b = BaoV(ban);
        if (b.size == Vector3.zero) return null;
        Vector2[] uu = giua
            ? new[] { new Vector2(0, 0), new Vector2(0.2f, 0.2f), new Vector2(-0.2f, -0.2f), new Vector2(0.2f, -0.2f), new Vector2(-0.2f, 0.2f) }
            : new[] { new Vector2(0.55f, 0.45f), new Vector2(-0.55f, 0.45f), new Vector2(0.55f, -0.45f), new Vector2(-0.55f, -0.45f), new Vector2(0.3f, 0f), new Vector2(-0.3f, 0f), new Vector2(0, 0) };
        Vector3 dat = Vector3.zero; bool co1 = false;
        foreach (var o in uu)
        {
            var goc = new Vector3(b.center.x + o.x * b.extents.x, b.max.y + 0.6f, b.center.z + o.y * b.extents.z);
            var hits = Physics.RaycastAll(goc, Vector3.down, b.size.y + 1.2f, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, c) => a.distance.CompareTo(c.distance));
            foreach (var h in hits)
            {
                if (h.collider.GetComponentInParent<CharacterController>()) continue;
                if (h.normal.y < 0.85f) break;                                   // trúng đồ đặt trên bàn (nghiêng) → bỏ điểm này
                if (!h.transform.IsChildOf(ban) && h.point.y < b.max.y - 0.25f) break;   // trúng sàn / vật khác thấp hẳn → bỏ
                dat = h.point; co1 = true; break;
            }
            if (co1) break;
        }
        if (!co1) dat = new Vector3(b.center.x, b.max.y, b.center.z);

        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = ten;   // không gắn làm con của bàn (bàn có thể bị co giãn / xoay lệch → giấy méo)
        g.transform.position = dat + Vector3.up * 0.003f;
        g.transform.rotation = Quaternion.Euler(0, Random.Range(-35f, 35f) + (giua ? 0 : 90 * Random.Range(0, 4)), 0);
        g.transform.localScale = new Vector3(co.x, 0.003f, co.y);
        var mr = g.GetComponent<MeshRenderer>();
        var sh = Shader.Find("Universal Render Pipeline/Simple Lit") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(sh); var mau = new Color(0.86f, 0.80f, 0.64f);   // giấy ngả vàng
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", mau); else m.color = mau;
        mr.sharedMaterial = m;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        // hộp va chạm dày hơn tờ giấy cho dễ nhắm
        var bc = g.GetComponent<BoxCollider>(); if (bc) bc.size = new Vector3(1.4f, 20f, 1.4f);   // ~14 cm × 6 cm
        return g;
    }

    // gọi từ Dem1() mỗi khung hình
    void Dem1ManhMa()
    {
        if (!fp || !cuaHamT) return;
        var p = fp.transform.position;
        // tiếng đập cửa sắt vọng lên từ hầm — dụ người chơi tới giếng trời
        if (LocTui.Co("khoi_vo") && !LocTui.Co("go_ham") && !ThayCuaHam && !LocFirstPerson.khoa && !dangCanh)
        {
            var v = cuaHamT.position - p; float doc = Mathf.Abs(v.y); v.y = 0;
            if (doc < 2.2f && v.magnitude < 10f)
            {
                LocTui.Dat("go_ham");
                LocAm.Phat("GM_DapCua", cuaHamT.position + Vector3.down * 0.8f, 0.45f, 0.82f);
                StartCoroutine(NoiSau(2.4f, "...Tiếng gì đấy.", "Phía giếng trời."));
            }
        }
        // đủ bốn mảnh → Khôi ghép mã
        if (!LocTui.Co("ghep_ma") && !LocTui.Co("ket_mo") && SoManh() >= 4 && !LocFirstPerson.khoa && !LocTui.Mo && !LocTui.DangNoi)
        {
            LocTui.Dat("ghep_ma");
            LocTui.Noi("Một... chín... tám... không.", "Một chín tám mươi.", "...Năm anh Phong ra đời.");
        }
    }
    System.Collections.IEnumerator NoiSau(float giay, params string[] dong) { yield return new WaitForSeconds(giay); LocTui.Noi(dong); }
}
