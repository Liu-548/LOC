using UnityEngine;

// [14/11] ĐÊM 1 — CÂU ĐỐ MÃ KÉT (thay dòng "Ứng cho thằng Phong 1.980.000" trong sổ thu chi).
//  · Dụ: thăm em xong → tiếng đập cửa sắt vọng từ dưới hầm → người chơi tự tìm tới cửa sắt giếng trời.
//  · Thấy cửa (KiemKhamPha) → Khôi nhớ: chìa gì quan trọng bố cũng khóa vào két.
//  · Bấm két → "Bố hay quên mã. Mẹ thế nào cũng ghi lại." → đồ của mẹ: TỜ GIẤY CỦA MẸ dưới hộp phấn, bàn trang điểm (cùng phòng két).
//    Mẹ không ghi thẳng mã (người ngoài đọc được): mã = năm thằng cả ra đời; bốn số giấu ở bốn chỗ "còn giữ hơi nó" — chỉ người nhà hiểu.
//  · Bốn số, mỗi số kèm một mẩu chuyện về anh Phong (người chơi hiểu dần: nhà từng có anh cả, đã mất, bị giấu với Nhím):
//      1 kẹp bìa sổ thu chi 2002 (tủ hồ sơ phòng làm việc) — chỗ mẹ ghi khoản để riêng cho nó
//      9 hộc bàn học phòng Khôi — cái bàn hai anh em ngồi học chung (đặt trong ngăn kéo, đi theo ngăn kéo)
//      8 bàn nước phòng khách — chén trà bố rót thêm mỗi sáng
//      0 tủ đầu giường phòng Nhím — chỗ con bé ngủ, mẹ đã giấu anh nó với nó
//  · Đủ bốn số → Khôi ghép: "Một chín tám mươi. Năm anh Phong ra đời."
public partial class LocCotTruyen
{
    LocVatPham vpGiayKet;
    public static LocVatPham GiayKet => I ? I.vpGiayKet : null;
    static readonly string[] MANH_ID = { "manh_ma_1", "manh_ma_2", "manh_ma_3", "manh_ma_4" };
    // số 1 nằm trong sổ thu chi (đọc sổ = thấy) · số 2–4 là mảnh giấy nhặt được
    public static int SoManh()
    {
        int n = LocTui.Co("doc:so_thu_chi_2002") ? 1 : 0;
        for (int i = 1; i < MANH_ID.Length; i++) if (LocTui.CoMon(MANH_ID[i])) n++;
        return n;
    }

    void KhoiTaoManhMa()
    {
        if (LocMa.dem != 1) return;
        try
        {
            // tờ giấy của mẹ: dưới hộp phấn trên bàn trang điểm phòng bố mẹ (chỗ riêng của mẹ, cùng phòng két)
            var btd = GameObject.Find("BanTrangDiem_Bo");
            if (btd && !LocTui.Co("ket_mo"))
            {
                var g = TaoGiay("GiayMe_MaKet", btd.transform, new Vector2(0.14f, 0.10f), false);
                if (g) vpGiayKet = Mon(g, "giay_ket", "tờ giấy của mẹ", LocVatPham.Kieu.Doc, "Tờ giấy gấp tư dưới hộp phấn, chữ mẹ.", 1,
                    new[] { "Thằng cả...", "Anh Phong." }, new[]
                    {
                        "<i>Gấp tư, để dưới hộp phấn. Chữ mẹ, nhỏ, đều.</i>\n\nÔng hay quên mã két.\nEm không ghi thẳng ra nữa — người ngoài đọc được.\n\nMã là <b>năm thằng cả ra đời</b>.\nÔng mà quên cả năm ấy thì em giấu bốn số ở bốn chỗ còn giữ hơi nó:\n\n   — chỗ em ghi khoản để riêng cho nó,\n   — cái bàn hai anh em ngồi học chung,\n   — chỗ ông rót thêm chén trà mỗi sáng,\n   — và chỗ con Nhím ngủ, cho em nhớ mình đã giấu nó với con bé.\n\nĐọc xong thì để lại chỗ cũ.",
                    });
            }
            ManhTrongHoc(1, "BanHoc_Khoi_Bo", "mảnh giấy (số thứ hai)", "Mảnh giấy xé, chữ mẹ. Trong hộc bàn học của mình.",
                "<i>Trong hộc bàn. Chữ mẹ.</i>\n\n\n<size=30><b>Số thứ hai:  9</b></size>\n\n\nHai anh em ngồi chung cái bàn này học bài\ntới năm thằng Phong chín tuổi.\n\nThằng Khôi khóc mấy hôm\nvì phải ngồi một mình.");
            Manh(2, "Salon_BanNuoc", "mảnh giấy (số thứ ba)", "Mảnh giấy xé, chữ mẹ. Kẹp dưới khay trà.",
                "<i>Kẹp dưới khay trà. Chữ mẹ.</i>\n\n\n<size=30><b>Số thứ ba:  8</b></size>\n\n\nTám giờ sáng nào ông cũng pha trà,\nrót thêm một chén để đấy.\n\nÔng không nói. Em cũng không hỏi.");
            Manh(3, "BanCanhGiuong_Thuoc", "mảnh giấy (số cuối)", "Mảnh giấy xé, chữ mẹ. Kẹp dưới vỉ thuốc đầu giường con bé.",
                "<i>Kẹp dưới vỉ thuốc. Chữ mẹ.</i>\n\n\n<size=30><b>Số cuối:  0</b></size>\n\n\nCon Nhím hỏi ngoài anh Khôi\nnó còn anh nào nữa không.\n\nEm bảo không.\nKhông có ai cả.");
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
            1 => new[] { "Chín.", "...Mình chẳng nhớ gì chuyện ngồi chung." },
            2 => new[] { "Tám.", "Chén trà thứ hai. Hồi nào cũng thấy, chưa bao giờ nghĩ." },
            _ => new[] { "Không.", "...Không có ai cả." },
        };
        var v = Mon(g, MANH_ID[i], ten, LocVatPham.Kieu.Nhat, ghiChu, 1, nghi, new[] { chu });
        if (v) v.docKhiNhat = true;   // nhặt là mở ra đọc luôn câu đố (đọc lại trong túi)
    }

    // mảnh giấy nằm đáy ngăn kéo của một cái bàn (con của cánh ngăn kéo → kéo ra là thấy, đóng lại là khuất)
    void ManhTrongHoc(int i, string tenBan, string ten, string ghiChu, string chu)
    {
        if (LocTui.Co("ket_mo")) return;
        var ban = GameObject.Find(tenBan); if (!ban) { Debug.LogWarning("[LocManhMa] không thấy " + tenBan); return; }
        LocDoor ngan = null;
        foreach (var d in ban.GetComponentsInChildren<LocDoor>(true)) if (d.ten != null && d.ten.StartsWith("ngăn")) { ngan = d; break; }
        if (!ngan) { Manh(i, tenBan, ten, ghiChu, chu); return; }   // không có ngăn kéo → để trên mặt bàn
        var b = BaoV(ngan.transform);
        var g = TaoGiay("ManhMa_" + (i + 1), ban.transform, new Vector2(0.10f, 0.07f), true);
        if (!g) return;
        var s = g.transform.lossyScale;
        g.transform.position = new Vector3(b.center.x, b.min.y + 0.016f, b.center.z);
        g.transform.SetParent(ngan.transform, true);
        var p = ngan.transform.lossyScale;   // giữ đúng cỡ thật dù ngăn kéo có co giãn
        g.transform.localScale = new Vector3(s.x / Mathf.Max(0.001f, Mathf.Abs(p.x)), s.y / Mathf.Max(0.001f, Mathf.Abs(p.y)), s.z / Mathf.Max(0.001f, Mathf.Abs(p.z)));
        var bc = g.GetComponent<BoxCollider>(); if (bc) bc.size = new Vector3(1.4f, 12f, 1.4f);   // thấp hơn lòng ngăn (~4 cm)
        var nghi = new[] { "Chín.", "...Mình chẳng nhớ gì chuyện ngồi chung." };
        var v = Mon(g, MANH_ID[i], ten, LocVatPham.Kieu.Nhat, ghiChu, 1, nghi, new[] { chu });
        if (v) v.docKhiNhat = true;
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
        //  lần 1: ~6 s sau cảnh Khôi vỡ (đang ở tầng 2) → nghe vọng xuống, Khôi: "Tiếng gì dưới nhà?" → nhiệm vụ xuống tầng 1 tìm
        //  lần 2: xuống tầng 1, trong ~10 m quanh giếng trời mà chưa thấy cửa → đập lại, rõ hướng hơn
        if (LocTui.Co("khoi_vo") && !ThayCuaHam && !LocFirstPerson.khoa && !dangCanh && !LocTui.Mo)
        {
            if (!LocTui.Co("go_ham"))
            {
                tChoGo += Time.deltaTime;
                if (tChoGo > 6f && !LocTui.DangNoi)
                {
                    LocTui.Dat("go_ham");
                    Dap(0.75f, 0.55f);
                    StartCoroutine(NoiSau(2.6f, "...Tiếng gì dưới nhà?", "Như ai đập cửa sắt."));
                }
            }
            else if (!LocTui.Co("go_ham_2"))
            {
                var v = cuaHamT.position - p; float doc = Mathf.Abs(v.y); v.y = 0;
                if (doc < 2.2f && v.magnitude < 10f)
                {
                    LocTui.Dat("go_ham_2");
                    Dap(0.6f, 1f);
                    StartCoroutine(NoiSau(2.4f, "Lại nữa.", "Phía sau bếp."));
                }
            }
        }
        // đọc sổ thu chi: thấy số thứ nhất kẹp bìa
        if (LocTui.Co("doc:so_thu_chi_2002") && !LocTui.Co("thay_so_1") && !LocTui.Mo && !LocTui.DangNoi)
        {
            LocTui.Dat("thay_so_1");
            LocTui.Noi("Một.", "...Năm nào mẹ cũng để riêng tiền cho anh.");
        }
        // đủ bốn mảnh → Khôi ghép mã
        if (!LocTui.Co("ghep_ma") && !LocTui.Co("ket_mo") && SoManh() >= 4 && !LocFirstPerson.khoa && !LocTui.Mo && !LocTui.DangNoi)
        {
            LocTui.Dat("ghep_ma");
            LocTui.Noi("Một. Chín. Tám. Không.", "Một chín tám mươi.", "...Năm anh Phong ra đời.");
        }
    }
    float tChoGo;
    // tiếng đập trầm, như vọng từ dưới đất lên; khong = 0..1 (1 = hẳn 3D tại cửa, thấp = nghe được từ xa / tầng trên)
    void Dap(float vol, float khong)
    {
        var c = LocAm.Lay("GM_DapCua"); if (!c || !cuaHamT) return;
        var g = new GameObject("_Am_GoHam"); g.transform.position = cuaHamT.position + Vector3.down * 0.8f;
        var a = g.AddComponent<AudioSource>(); a.clip = c; a.volume = vol; a.pitch = 0.82f; a.spatialBlend = khong;
        a.minDistance = 2f; a.maxDistance = 30f; a.rolloffMode = AudioRolloffMode.Logarithmic;
        var lp = g.AddComponent<AudioLowPassFilter>(); lp.cutoffFrequency = khong < 1f ? 900f : 1800f;   // nghe đục, như cách một lớp sàn
        a.Play(); Destroy(g, c.length / 0.82f + 0.2f);
    }
    System.Collections.IEnumerator NoiSau(float giay, params string[] dong) { yield return new WaitForSeconds(giay); LocTui.Noi(dong); }
}
