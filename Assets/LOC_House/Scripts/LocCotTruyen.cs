using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// [8/10] Hệ thống đêm + cốt truyện (kịch bản v3.2 mục C1.4, C1.5, C5). Nằm trên NguoiChoi.
//  · LocMa.dem là nguồn duy nhất của "đang đêm mấy" (F1–F3 vẫn đổi được để thử). Đổi đêm → chỉ bật nhóm Dem<n> của nhà, đóng cổng
//    (Đêm 2–3 khoá: "Không được. Không phải bây giờ."), tắt đèn hầm.
//  · Vào scene (kể cả chơi lại đầu đêm sau khi chết) và khi hết đêm: Đêm 1 đứng ngoài cổng, Đêm 2–3 đứng giữa phòng khách nhìn bàn thờ vong.
//    Đổi đêm bằng F1–F3 / tự kiểm thì KHÔNG dịch người chơi.
//  · Luật đèn hầm (B3.8b): đèn hầm đang bật mà người chơi ra khỏi hầm + hộp thang → tắt ngay, không nháy, không tiếng.
//  · Đêm 1: lời mở đầu → thắp hương (E, nhìn ảnh) → thoại người họ hàng → [Nhím, két, sổ — đã có] → xuống hầm có đèn (độc thoại)
//    → đi lên ("Thôi. Mai tính.") → ra khỏi cửa hầm và đã thăm Nhím → cắt đen → Đêm 2.
//  · H: gợi ý C1.4 — bấm (chưa chốt bấm hay giữ, TL-05), mỗi lần lùi một bậc. Hiện có gợi ý 1 (mã két).
//  · Cờ cốt truyện lưu trong LocTui → chết thì "chơi lại đầu đêm" tự trả về đúng lúc đầu đêm.
[DefaultExecutionOrder(20)]
public class LocCotTruyen : MonoBehaviour
{
    public static LocCotTruyen I;
    [Header("Chỗ đứng đầu đêm")]
    public Vector3 dem1Dung = new(3.8f, 0f, -6.3f), dem1Nhin = new(3.8f, 1.4f, -3f);
    public Vector3 demSauDung = new(3.8f, 0.05f, 0.9f), demSauNhin = new(3.8f, 1.2f, 2.55f);
    [Header("Hầm + hộp thang (theo chân người chơi)")]
    public Bounds hamPhong = new(new Vector3(-1.95f, -1.4f, 13f), new Vector3(3.9f, 2.2f, 4.4f));
    public Bounds hopThang = new(new Vector3(-0.85f, -1.0f, 17.05f), new Vector3(1.5f, 3.0f, 4.5f));

    static readonly string[] tenCong = { "CanhCong_Trai (mở)", "CanhCong_Phai (mở)", "CuaNgachCong (mở)" };
    LocFirstPerson fp; LocCongTac ctHam; Transform banThoVong; readonly List<LocDoor> cong = new();
    int demDangAp = -1; bool dangCanh, dangNoiHam;
    readonly Dictionary<string, int> bacGoiY = new();

    void Awake() { I = this; fp = GetComponent<LocFirstPerson>(); }

    void Start()
    {
        ctHam = GameObject.Find("CongTac_Ham")?.GetComponent<LocCongTac>();
        banThoVong = GameObject.Find("BanThoVong_Dem1")?.transform;
        foreach (var n in tenCong) { var d = GameObject.Find(n)?.GetComponent<LocDoor>(); if (d) cong.Add(d); }
        ApDem(LocMa.dem); DauDem();
    }

    void Update()
    {
        if (LocMa.dem != demDangAp) ApDem(LocMa.dem);
        DenHam();
        if (dangCanh) return;
        var k = Keyboard.current;
        if (k != null && k.hKey.wasPressedThisFrame && !LocFirstPerson.khoa && !LocTui.Mo) GoiYH();
        if (LocMa.dem == 1) Dem1();
    }

    // ───── trạng thái nhà theo đêm
    void ApDem(int d)
    {
        demDangAp = d;
        var nha = GameObject.Find("NhaLoc");
        if (nha) foreach (var t in nha.GetComponentsInChildren<Transform>(true))
                if (t.name is "Dem1" or "Dem2" or "Dem3") t.gameObject.SetActive(t.name == "Dem" + d);
        foreach (var c in cong) { c.DatNgay(false); c.khoa = d >= 2; c.loiKhoa = "Không được. Không phải bây giờ."; }
        if (ctHam) ctHam.Dat(false);
    }

    // đặt người chơi ở chỗ đầu đêm + lời mở đầu (chỉ lúc vào scene và lúc hết đêm)
    void DauDem()
    {
        int d = LocMa.dem;
        if (fp) { if (d <= 1) fp.DatTuThe(dem1Dung, dem1Nhin); else fp.DatTuThe(demSauDung, demSauNhin); }
        if (d == 1 && !LocTui.Co("d1_mo_dau")) StartCoroutine(SauKhiSang("d1_mo_dau", 1.5f, "Hai năm rồi mới về.", "Hồi đó nói được câu tử tế thì đã không đến nỗi.", "Ai ngờ gặp lại kiểu này."));
        if (d == 2 && !LocTui.Co("d2_mo_dau")) StartCoroutine(SauKhiSang("d2_mo_dau", 1.0f, "Chắc giờ này chẳng còn ai nữa.", "Xem con bé thế nào đã."));
    }
    IEnumerator SauKhiSang(string co, float cho, params string[] dong)
    {
        while (LocFirstPerson.den > 0) yield return null;
        yield return new WaitForSeconds(cho);
        LocTui.Dat(co); LocTui.Noi(dong);
    }

    // ───── luật đèn hầm: không có ai dưới đó thì tắt
    void DenHam()
    {
        if (!ctHam || !ctHam.bat || !fp) return;
        var p = fp.transform.position;
        if (!hamPhong.Contains(p) && !hopThang.Contains(p)) ctHam.Dat(false);
    }

    // ───── Đêm 1
    void Dem1()
    {
        var p = fp.transform.position;
        bool trongHam = hamPhong.Contains(p), trongThang = hopThang.Contains(p);
        bool daXuong = LocTui.Co("d1_xuong_ham");
        if (!daXuong && trongHam && ctHam && ctHam.bat) StartCoroutine(XuongHam());
        if (daXuong && !dangNoiHam && !trongHam && trongThang && !LocTui.Co("d1_mai_tinh")) { LocTui.Dat("d1_mai_tinh"); LocTui.Noi("Thôi. Mai tính."); }
        // hết đêm: đã xuống hầm, đã thăm Nhím, đang ở ngoài hầm và ngoài phòng em, không trong cảnh nào
        bool trongPhongNhim = LocMa.I && LocMa.I.nhimPhong.Contains(p + Vector3.up * 0.9f);
        if (daXuong && LocNhim.daTham[1] && !trongHam && !trongThang && !trongPhongNhim && !LocNhim.dangCanh && !LocFirstPerson.khoa && !LocTui.Mo)
            StartCoroutine(HetDem());
    }

    IEnumerator XuongHam()
    {
        LocTui.Dat("d1_xuong_ham"); dangNoiHam = true;
        yield return new WaitForSeconds(0.8f);
        LocTui.Noi("Cái gì thế này."); yield return new WaitForSeconds(5f);   // để người chơi nhìn bàn thờ, năm chỗ chén, cái hũ
        LocTui.Noi("Bàn này thờ ai vậy trời.", "Chén thì vỡ gần hết.", "Với lại... bùa gì mà dán lắm thế."); yield return new WaitForSeconds(7.5f);
        dangNoiHam = false;
    }

    public string GoiY(string id)
    {
        if (id != "thap_huong" || LocMa.dem != 1 || LocTui.Co("thap_huong") || dangCanh) return "";
        return DungChinhDien() ? "[E] Thắp hương" : "Đứng thẳng trước bàn thờ để thắp hương";
    }
    public void Bam(LocViec v) { if (v.id == "thap_huong" && GoiY(v.id).StartsWith("[E]")) StartCoroutine(ThapHuong(v.transform)); }

    // [21/10] thắp hương phải đứng chính diện, không cho đứng chéo: ngay trước mặt bàn thờ (0,4–1,9 m tính từ tâm bàn),
    //         lệch ngang ≤ 0,25 m so với trục giữa, mặt hướng thẳng vào bàn thờ (lệch ≤ 20°)
    public bool DungChinhDien()
    {
        if (!banThoVong || !fp) return true;
        var c = banThoVong.position; c.y = 0;
        var truoc = banThoVong.forward; truoc.y = 0; truoc.Normalize();
        if (Vector3.Dot(truoc, demSauDung - c) < 0) truoc = -truoc;   // mặt bàn thờ = phía cửa chính
        var d = fp.transform.position - c; d.y = 0;
        float doc = Vector3.Dot(d, truoc), ngang = Vector3.Dot(d, Vector3.Cross(Vector3.up, truoc));
        var nhin = fp.cam.forward; nhin.y = 0;
        return doc > 0.4f && doc < 1.9f && Mathf.Abs(ngang) <= 0.25f && Vector3.Angle(nhin, -truoc) <= 20f;
    }

    IEnumerator ThapHuong(Transform ban)
    {
        dangCanh = true; LocTui.Dat("thap_huong"); LocFirstPerson.khoa = true; LocMa.tamDung = true;
        var b = Bao(ban); var anh = new Vector3(b.center.x, b.max.y - 0.3f, b.center.z);
        var tu = fp.cam.position + fp.cam.forward;
        for (float t = 0; t < 1; t += Time.deltaTime / 0.8f) { fp.DatTuThe(fp.transform.position, Vector3.Lerp(tu, anh, t * t * (3 - 2 * t))); yield return null; }
        fp.DatTuThe(fp.transform.position, anh);
        // ponytail: chưa có tay Khôi + que hương cắm vào bát (TL-10) — chỉ giữ camera trên hai tấm ảnh
        yield return new WaitForSeconds(4.5f);   // "lâu hơn mức thoải mái"; Khôi không nói gì
        LocFirstPerson.khoa = false; LocMa.tamDung = false; dangCanh = false;
        yield return new WaitForSeconds(1.2f);
        // [21/10] người họ hàng có model (người dì, LocDi); tên phụ đề vẫn để "NGƯỜI HỌ HÀNG" tới khi chốt TL-03
        yield return Thoai(("KHÔI", "Cháu cảm ơn cô mấy hôm nay đã trông nhà."),
            ("NGƯỜI HỌ HÀNG", "Ừ. Cô chia buồn với con. Cũng chẳng ai ngờ."),
            ("NGƯỜI HỌ HÀNG", "Con Nhím vẫn nằm trên gác. Tay còn bó, sốt thì chưa dứt. Bác sĩ trả về, bảo cứ theo dõi ở nhà."),
            ("NGƯỜI HỌ HÀNG", "À, hôm qua có ông nào gọi về hỏi nhà còn ai lớn không. Cô bảo còn thằng lớn sắp về. Ông ấy không xưng tên."),
            ("NGƯỜI HỌ HÀNG", "Lát con sang phòng bố mẹ xem có giấy tờ gì cần cất riêng không. Cô về đây, nhà cô còn con nhỏ."));
        if (LocDi.I) { LocDi.I.Noi(false); LocDi.I.Ve(); }   // "Cô về đây" — đi ra cổng
        LocTui.Dat("ho_hang_ve");
    }

    // thoại nói ra: chữ thẳng, có tên; nhịp 1,2 s + 0,06 s/ký tự, nghỉ 0,4 s (C1.2)
    static IEnumerator Thoai(params (string ai, string cau)[] dong)
    {
        foreach (var (ai, cau) in dong)
        {
            float t = 1.2f + 0.06f * cau.Length;
            if (LocDi.I) LocDi.I.Noi(ai == "NGƯỜI HỌ HÀNG");   // dì quay về người chơi, gật nhẹ khi tới câu của dì
            LocFirstPerson.PhuDe($"{ai}: {cau}", t, false);
            yield return new WaitForSeconds(t + 0.4f);
        }
    }

    // ───── hết đêm → cắt đen → đêm sau
    IEnumerator HetDem()
    {
        dangCanh = true;
        while (dangNoiHam || (LocTui.Co("thap_huong") && !LocTui.Co("ho_hang_ve"))) yield return null;   // không cắt ngang thoại Đêm 1
        yield return new WaitForSeconds(1.2f);   // kịp thấy đèn hầm tắt sau lưng
        LocFirstPerson.khoa = true; LocMa.tamDung = true;
        for (float t = 0; t < 1; t += Time.deltaTime / 0.6f) { LocFirstPerson.den = t; yield return null; }
        LocFirstPerson.den = 1; LocFirstPerson.PhuDe("", 0.01f);
        // ponytail: chưa có cắt cảnh ban ngày (tiếng người viếng, kèn đám, tay rót trà) — màn đen 2,5 s thay; thêm khi có âm thanh
        yield return new WaitForSeconds(2.5f);
        LocMa.dem++; ApDem(LocMa.dem); DauDem();
        yield return new WaitForSeconds(0.8f);
        for (float t = 1; t > 0; t -= Time.deltaTime / 1.2f) { LocFirstPerson.den = t; yield return null; }
        LocFirstPerson.den = 0; LocFirstPerson.khoa = false; LocMa.tamDung = false; dangCanh = false;
    }

    // ───── gợi ý H (C1.4): Khôi tự nhủ, bấm lại thì lùi một bậc, không bao giờ nói đáp án
    void GoiYH()
    {
        string id = null; string[] bac = null;
        if (LocMa.dem == 1 && LocTui.Co("thu_ket") && !LocTui.Co("ket_mo"))
        { id = "ma_ket"; bac = new[] { "Bố mẹ chắc gì nhớ được mã. Phải ghi ở đâu đó.", "Giấy tờ nhà mình toàn để bên phòng làm việc của bố.", "...Năm nay sao còn ghi tiền cho anh Phong." }; }
        if (id == null) return;
        bacGoiY.TryGetValue(id, out int i);
        LocTui.Noi(bac[Mathf.Min(i, bac.Length - 1)]);
        bacGoiY[id] = i + 1;
    }

    static Bounds Bao(Transform t)
    {
        var rs = t.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(t.position + Vector3.up, Vector3.one * 0.1f);
        var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }
}
