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
public partial class LocCotTruyen : MonoBehaviour   // [27/10] phần v3.3 còn thiếu (cuộc gọi thầy, dán chén, kiện hàng, nghi lễ, chỗ rẽ, các kết) ở LocCotTruyen_V34.cs
{
    public static LocCotTruyen I;
    [Header("Chỗ đứng đầu đêm")]
    public Vector3 dem1Dung = new(3.8f, 0f, -6.3f), dem1Nhin = new(3.8f, 1.4f, -3f);
    public Vector3 demSauDung = new(3.8f, 0.05f, 0.9f), demSauNhin = new(3.8f, 1.2f, 2.55f);
    [Header("Hầm + hộp thang (theo chân người chơi)")]
    public Bounds hamPhong = new(new Vector3(-1.95f, -1.4f, 13f), new Vector3(3.9f, 2.2f, 4.4f));
    public Bounds hopThang = new(new Vector3(-0.85f, -1.0f, 17.05f), new Vector3(1.5f, 3.0f, 4.5f));

    static readonly string[] tenCong = { "CanhCong_Trai (mở)", "CanhCong_Phai (mở)", "CuaNgachCong (mở)" };
    [Header("v3.3")]
    public float phutMoiTieng = 6f;          // Đêm 3: đồng hồ quả lắc gõ thêm một giờ sau chừng này phút chơi (TL-24, chốt sau chơi thử)
    public int gioBatDau = 9, gioHet = 12;   // gõ đủ 12 tiếng = sang mùng một → Bad (a)
    LocFirstPerson fp; LocCongTac ctHam; Transform banThoVong, banThoT3, dienThoai, dongHo, cuaHamT; readonly List<GameObject> linhCuu = new(); readonly List<LocDoor> cong = new();
    AudioSource amReo, amGo; AudioClip cReo, cGo, cRe, cTut; bool dangReo, nhacMay; float tDongHo; int gio;
    int demDangAp = -1; bool dangCanh, dangNoiHam;
    readonly Dictionary<string, int> bacGoiY = new();

    void Awake() { I = this; fp = GetComponent<LocFirstPerson>(); }

    void Start()
    {
        ctHam = GameObject.Find("CongTac_Ham")?.GetComponent<LocCongTac>();
        banThoVong = GameObject.Find("BanThoVong_Dem1")?.transform;
        banThoT3 = GameObject.Find("KhungAnhOngBa")?.transform;
        dienThoai = GameObject.Find("LOC_DienThoai_TronBo")?.transform;
        dongHo = GameObject.Find("DongHoQuaLac")?.transform;
        cuaHamT = GameObject.Find("CuaSatHam (mở)")?.transform;
        foreach (var n in new[] { "QuanTai_LOC", "VaiPhuQuanTai_A", "VaiPhuQuanTai_B", "DenDau_GamQuanTai", "Den_DenDau_GamQuanTai" })
        { var g = GameObject.Find(n); if (g) linhCuu.Add(g); }
        cReo = Resources.Load<AudioClip>("LOC_V33/DienThoai_Reo"); cGo = Resources.Load<AudioClip>("LOC_V33/DongHo_Go");
        cRe = Resources.Load<AudioClip>("LOC_V33/DienThoai_Re"); cTut = Resources.Load<AudioClip>("LOC_V33/DienThoai_Tut");
        amReo = AmTai(dienThoai, 14f); amGo = AmTai(dongHo, 40f);
        var bang = Resources.Load<Texture2D>("LOC_V33/T_BangChuVongHoa");   // băng vòng hoa trong .glb chỉ có chữ giữ chỗ
        if (bang) foreach (var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            foreach (var m in r.sharedMaterials) if (m && m.name.StartsWith("Bang_Chu_Vong_Hoa")) { foreach (var mm in r.materials) if (mm.name.StartsWith("Bang_Chu_Vong_Hoa")) foreach (var p in new[] { "_BaseMap", "_MainTex", "baseColorTexture" }) if (mm.HasProperty(p)) mm.SetTexture(p, bang); break; }
        // các kết: xong cảnh thì màn đen + một dòng chữ rồi về menu
        LocNhim.SauGood = () => StartCoroutine(KetThuc("Mười ba năm, người mang nó tới không cho nó đi."));
        LocNhim.SauBad = () => StartCoroutine(KetThuc("Anh đã ở trong căn nhà này bốn năm."));
        LocNhim.SauEvil = () => StartCoroutine(KetThuc("Anh không đếm lại bao giờ."));
        foreach (var n in tenCong) { var d = GameObject.Find(n)?.GetComponent<LocDoor>(); if (d) cong.Add(d); }
        KhoiTaoV34();
        ApDem(LocMa.dem); DauDem();
    }

    void Update()
    {
        if (LocMa.dem != demDangAp) ApDem(LocMa.dem);
        DenHam();
        if (dangKet) { LocFirstPerson.khoa = true; LocMa.tamDung = true; }   // [27/10] phần kết: cảnh khác (LocNhim) có nhả khoá cũng giữ lại
        if (dangCanh) return;
        var k = Keyboard.current;
        if (k != null && k.hKey.wasPressedThisFrame && !LocFirstPerson.khoa && !LocTui.Mo) GoiYH();
#if UNITY_EDITOR
        if (k != null && k.digit9Key.wasPressedThisFrame && LocMa.dem < 3 && !LocFirstPerson.khoa && !LocMenu.DangChe) StartCoroutine(HetDemThu());   // [22/10] thử: phím 9 = hết đêm ngay (xem cảnh ban ngày)
#endif
        if (LocMa.dem == 1) Dem1();
        if (LocMa.dem == 2) Dem2();
        if (LocMa.dem == 3) Dem3();
    }

    AudioSource AmTai(Transform t, float xa)
    {
        var g = t ? t.gameObject : gameObject;
        var a = g.AddComponent<AudioSource>(); a.playOnAwake = false; a.spatialBlend = t ? 1f : 0f; a.minDistance = 1.5f; a.maxDistance = xa; a.rolloffMode = AudioRolloffMode.Linear;
        return a;
    }
    float KC(Transform t) => t && fp ? Vector3.Distance(fp.transform.position, t.position) : 999f;

    // ───── trạng thái nhà theo đêm
    void ApDem(int d)
    {
        demDangAp = d;
        var nha = GameObject.Find("NhaLoc");
        if (nha) foreach (var t in nha.GetComponentsInChildren<Transform>(true))
                if (t.name is "Dem1" or "Dem2" or "Dem3") t.gameObject.SetActive(t.name == "Dem" + d);
        foreach (var g in linhCuu) if (g) g.SetActive(d < 3);
        // nhà có tang: từ Đêm 2 các cửa phòng để mở (mép cửa là chỗ cái bóng lấp ló — cửa đóng thì không có chỗ)
        if (d >= 2 && LocMa.I && LocMa.I.cuaRef != null) foreach (var c in LocMa.I.cuaRef) if (c && !c.khoa) c.DatNgay(true);
        foreach (var c in cong) { c.DatNgay(false); c.khoa = d >= 2; c.loiKhoa = "Không được. Không phải bây giờ."; }
        if (ctHam) ctHam.Dat(false);
        ApDemV34(d);
    }

    // đặt người chơi ở chỗ đầu đêm + lời mở đầu (chỉ lúc vào scene và lúc hết đêm)
    void DauDem()
    {
        int d = LocMa.dem;
        LocMenu.DauDem(d);   // [22/10] tự lưu + thẻ đêm
        if (fp) { if (d <= 1) fp.DatTuThe(dem1Dung, dem1Nhin); else fp.DatTuThe(demSauDung, demSauNhin); }
        if (d == 3) NapDiemLuu();
        if (d == 1 && LocXeBuyt.choChay && LocXeBuyt.I && fp) StartCoroutine(LocXeBuyt.I.Chay(fp));   // [25/10] mở đầu trên xe buýt → xuống xe đúng chỗ dem1Dung
        if (d == 1 && !LocTui.Co("d1_mo_dau")) StartCoroutine(SauKhiSang("d1_mo_dau", 1.5f, "Hai năm rồi mới về.", "Gọi về một cuộc cũng không."));
        if (d == 2 && !LocTui.Co("d2_mo_dau")) StartCoroutine(SauKhiSang("d2_mo_dau", 1.0f, "Chắc giờ này chẳng còn ai nữa.", "Xem con bé thế nào đã."));
    }
    IEnumerator SauKhiSang(string co, float cho, params string[] dong)
    {
        while (LocFirstPerson.den > 0 || LocMenu.DangChe || LocXeBuyt.dangChay) yield return null;
        yield return new WaitForSeconds(cho);
        LocTui.Dat(co); LocTui.Noi(dong);
    }

    // ───── luật đèn hầm: không có ai dưới đó thì tắt
    void DenHam()
    {
        if (!ctHam || !ctHam.bat || !fp) return;
        var p = fp.transform.position;
        if (hamPhong.Contains(p) || hopThang.Contains(p)) return;
        ctHam.Dat(false);
        if (!LocTui.Co("den_ham_tat_1")) { LocTui.Dat("den_ham_tat_1"); StartCoroutine(GiatMinh(hopThang.center)); }
    }

    // camera khựng, quay nhanh về một điểm (0,3 s), tiếng thở hắt — không chữ
    IEnumerator GiatMinh(Vector3 diem)
    {
        if (LocNhim.I && LocNhim.I.khoiTho) AudioSource.PlayClipAtPoint(LocNhim.I.khoiTho, fp.cam.position, 0.7f);
        LocFirstPerson.khoa = true;
        var tu = fp.cam.position + fp.cam.forward * 2f;
        for (float t = 0; t < 1; t += Time.deltaTime / 0.3f) { fp.DatTuThe(fp.transform.position, Vector3.Lerp(tu, diem, t * (2 - t))); yield return null; }
        yield return new WaitForSeconds(0.5f);
        LocFirstPerson.khoa = false;
    }

    // ───── Đêm 1
    void Dem1()
    {
        var p = fp.transform.position;
        if (LocTui.Co("d1_mo_dau") && !LocTui.Co("d1_liem") && KC(banThoVong) < 4.5f) { LocTui.Dat("d1_liem"); LocTui.Noi("Về tới nơi thì liệm rồi.", "Không kịp nhìn mặt."); }
        if (LocTui.Co("chia_ham") || LocTui.CoMon("chia_ham"))
            if (!LocTui.Co("d1_cay_ham") && KC(cuaHamT) < 1.8f) { LocTui.Dat("d1_cay_ham"); LocTui.Noi("Hồi lớp mười cạy thử một lần. Bị bố tát cho."); }
        // cuộc gọi thứ nhất: vừa lên khỏi hầm, điện thoại phòng khách reo, lại gần thì tắt (C5 Đêm 1)
        if (LocTui.Co("d1_mai_tinh") && !LocTui.Co("goi_1") && !hopThang.Contains(p) && !hamPhong.Contains(p)) { LocTui.Dat("goi_1"); StartCoroutine(Reo(6, false)); }
        bool trongHam = hamPhong.Contains(p), trongThang = hopThang.Contains(p);
        bool daXuong = LocTui.Co("d1_xuong_ham");
        if (!daXuong && trongHam && ctHam && ctHam.bat) StartCoroutine(XuongHam());
        if (daXuong && !dangNoiHam && !trongHam && trongThang && !LocTui.Co("d1_mai_tinh")) { LocTui.Dat("d1_mai_tinh"); LocTui.Noi("Thôi. Mai tính."); }
        // hết đêm: đã xuống hầm, đã thăm Nhím, đang ở ngoài hầm và ngoài phòng em, không trong cảnh nào
        bool trongPhongNhim = LocMa.I && LocMa.I.nhimPhong.Contains(p + Vector3.up * 0.9f);
        if (daXuong && LocNhim.daTham[1] && LocTui.Co("giu_huong_1") && !trongHam && !trongThang && !trongPhongNhim && !LocNhim.dangCanh && !LocFirstPerson.khoa && !LocTui.Mo)
            StartCoroutine(HetDem());
    }

    IEnumerator XuongHam()
    {
        LocTui.Dat("d1_xuong_ham"); dangNoiHam = true;
        yield return new WaitForSeconds(0.8f);
        LocTui.Noi("Cái gì thế này."); yield return new WaitForSeconds(5f);   // để người chơi nhìn bàn thờ, năm chỗ chén, cái hũ
        LocTui.Noi("Thì ra ông thầy xuống đây làm cái này.", "Thờ cái gì đây.", "Chén thì vỡ gần hết.", "Bùa đâu mà dán lắm thế này.", "Bố mẹ tin mấy thứ này đến thế à.");
        yield return new WaitForSeconds(13f);
        dangNoiHam = false;
    }

    // giữ hương (v3.3): Đêm 1 sau khi xuống hầm + thăm em, Đêm 2 sau đoạn hoảng loạn — thắp nén mới là việc cuối của đêm
    bool ChoGiuHuong(int d) => d == 1 ? LocTui.Co("d1_xuong_ham") && LocNhim.daTham[1] : d == 2 && LocNhim.daHoangLoan && LocNhim.daTham[2] && Dem2DuViec();
    public string GoiY(string id)
    {
        if (dangCanh) return "";
        int d = LocMa.dem;
        if (id == "dien_thoai") return dangReo && nhacMay ? "[E] Nhấc máy" : "";
        if (id == "thap_huong_t3")
            return d == 3 && LocMa.huDaHe && !LocTui.Co("thap_huong_t3") ? (DungChinhDien(banThoT3) ? "[E] Thắp hương" : "Đứng thẳng trước bàn thờ để thắp hương") : "";
        if (id != "thap_huong") return "";
        string viec = d == 1 && !LocTui.Co("thap_huong") ? "[E] Thắp hương"
                    : d <= 2 && ChoGiuHuong(d) && !LocTui.Co("giu_huong_" + d) ? "[E] Thắp nén mới" : null;
        if (viec == null) return "";
        return DungChinhDien() ? viec : "Đứng thẳng trước bàn thờ để thắp hương";
    }
    public void Bam(LocViec v)
    {
        if (!GoiY(v.id).StartsWith("[E]")) return;
        if (v.id == "dien_thoai") { nhacMay = false; dangReo = false; return; }
        if (v.id == "thap_huong_t3") { StartCoroutine(GiuCamera(v.transform, "thap_huong_t3")); return; }
        if (LocMa.dem == 1 && !LocTui.Co("thap_huong")) StartCoroutine(ThapHuong(v.transform));
        else StartCoroutine(GiuCamera(v.transform, "giu_huong_" + LocMa.dem));
    }
    IEnumerator GiuCamera(Transform ban, string co)
    {
        dangCanh = true; LocFirstPerson.khoa = true; LocMa.tamDung = true;
        var b = Bao(ban); var anh = new Vector3(b.center.x, b.max.y - 0.3f, b.center.z);
        var tu = fp.cam.position + fp.cam.forward;
        for (float t = 0; t < 1; t += Time.deltaTime / 0.8f) { fp.DatTuThe(fp.transform.position, Vector3.Lerp(tu, anh, t * t * (3 - 2 * t))); yield return null; }
        StartCoroutine(LocTay.ThapHuong(2.3f));   // [29/10] chắp tay kẹp nén hương, vái
        yield return new WaitForSeconds(2.5f);
        LocTui.Dat(co);
        LocFirstPerson.khoa = false; LocMa.tamDung = false; dangCanh = false;
    }

    // [21/10] thắp hương phải đứng chính diện, không cho đứng chéo: ngay trước mặt bàn thờ (0,4–1,9 m tính từ tâm bàn),
    //         lệch ngang ≤ 0,25 m so với trục giữa, mặt hướng thẳng vào bàn thờ (lệch ≤ 20°)
    public bool DungChinhDien() => DungChinhDien(banThoVong);
    public bool DungChinhDien(Transform ban)
    {
        if (!ban || !fp) return true;
        var c = ban.position; c.y = 0;
        var truoc = ban.forward; truoc.y = 0; truoc.Normalize();
        var mat = ban == banThoVong ? demSauDung : fp.transform.position;   // bàn thờ vong: mặt = phía cửa chính · bàn thờ ông bà: phía người đứng (sau lưng là tường)
        if (Vector3.Dot(truoc, mat - c) < 0) truoc = -truoc;
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
        StartCoroutine(LocTay.ThapHuong(3.9f));   // [29/10] tay Khôi chắp ba nén hương, vái ba lần — camera vẫn giữ trên hai tấm ảnh
        yield return new WaitForSeconds(4.5f);   // "lâu hơn mức thoải mái"; Khôi không nói gì
        LocFirstPerson.khoa = false; LocMa.tamDung = false; dangCanh = false;
        yield return new WaitForSeconds(1.2f);
        // [29/10] cả đoạn bác (bước tới, nắm tay, 15 câu thoại) bỏ qua được: giữ Space 0,8 s như đoạn xe buýt
        bool buong = false;
        IEnumerator DoanBac()
        {
            // [29/10 v3.3] bác bước tới, nắm lấy cánh tay Khôi (IK tay phải bác lên giữa cẳng tay trái Khôi) — giữ qua hai câu chào rồi buông
            if (LocDi.I && LocDi.I.gameObject.activeInHierarchy)
            {
                LocFirstPerson.khoa = true;
                var dau = LocDi.I.transform.position + Vector3.up * 1.4f;
                fp.DatTuThe(fp.transform.position, dau);
                yield return LocDi.I.BuocToi(fp.transform);
                fp.DatTuThe(fp.transform.position, LocDi.I.transform.position + Vector3.up * 1.35f);
                StartCoroutine(LocTay.GiuTayTrai(() => buong));
                yield return new WaitForSeconds(0.35f);
                LocDi.I.NamTay(() => LocTay.CangTayTrai);
            }
            // [v3.3] chị gái của mẹ — "bác" (TL-22), giọng Bắc, không nói câu của khách. Tên riêng còn chờ TL-03.
            yield return Thoai((BAC, "Khôi đấy à… Khổ thân con. Về muộn quá."),
                ("KHÔI", "Bác. Mấy hôm nay vất vả bác quá."));
            if (LocDi.I) LocDi.I.NamTay(null);
            yield return new WaitForSeconds(0.5f);
            buong = true; LocFirstPerson.khoa = false;
            yield return Thoai(
                (BAC, "Đang yên đang lành, thế mà…"),
                (BAC, "Mẹ con dạo này cứ thấp thỏm. Hỏi thì không nói."));
            yield return new WaitForSeconds(2f);
            yield return Thoai((BAC, "Con Nhím vẫn nằm trên gác. Tay còn bó, sốt thì chưa dứt."),
                (BAC, "Bố con nhất định không cho nó đi viện. Gọi ông bác sĩ đầu phố sang bó tận giường."),
                (BAC, "Thuốc bác để trên tủ, sáng tối nửa viên."),
                (BAC, "Nó biết rồi. Khóc mấy hôm, giờ mệt quá chỉ nằm."),
                (BAC, "À, hôm qua có ông nào gọi về hỏi nhà còn ai lớn không."),
                (BAC, "Bác bảo còn thằng lớn sắp về. Ông ấy không xưng tên."),
                (BAC, "Chắc lại người đòi tiền hàng. Dạo này bố con nợ nhiều chỗ lắm."),
                (BAC, "Lát con sang phòng bố mẹ xem có giấy tờ gì cần cất riêng không."),
                (BAC, "Người ta bảo nhà mình phạm trùng tang. Hai người một ngày, lại sắp sang tháng bảy."),
                (BAC, "Tối không ai dám ở lại đâu con."),
                (BAC, "Bác cũng phải về, nhà còn đứa cháu nhỏ. Nhớ giữ hương trên linh cữu, đừng để tắt."));
        }
        yield return BoQuaDuoc(DoanBac(), () =>
        {
            buong = true; LocFirstPerson.khoa = false;
            if (LocDi.I) { LocDi.I.NamTay(null); LocDi.I.DungLai(); }
        });
        if (LocDi.I) { LocDi.I.Noi(false); LocDi.I.Ve(); }   // bác về — đi ra cổng
        LocTui.Dat("ho_hang_ve");
    }

    // ───── [29/10] đoạn dài bỏ qua được (bác nói chuyện…): giữ Space 0,8 s — cùng cách với đoạn xe buýt (LocXeBuyt).
    //  Bỏ qua thì dừng ngay đoạn đang chạy, xoá phụ đề, gọi donDep để trả trạng thái (mở khoá, buông tay…), rồi chạy tiếp phần sau đoạn.
    float giuSpace; bool coBoQua;
    IEnumerator BoQuaDuoc(IEnumerator doan, System.Action donDep)
    {
        bool xong = false;
        IEnumerator Chay() { yield return doan; xong = true; }
        var c = StartCoroutine(Chay());
        coBoQua = true; giuSpace = 0;
        while (!xong)
        {
            var k = Keyboard.current;
            giuSpace = k != null && k.spaceKey.isPressed && !LocMenu.DangChe ? giuSpace + Time.deltaTime : 0;
            if (giuSpace >= 0.8f) { StopCoroutine(c); LocFirstPerson.PhuDe("", 0.01f); donDep?.Invoke(); break; }
            yield return null;
        }
        coBoQua = false; giuSpace = 0;
    }
    void OnGUI()
    {
        if (!coBoQua || LocMenu.DangChe) return;
        float k = Screen.height / 1080f;
        var st = new GUIStyle(GUI.skin.label) { font = LocMenu.FontChu(1), fontSize = Mathf.RoundToInt(19 * k), alignment = TextAnchor.LowerRight };
        st.normal.textColor = new Color(0.75f, 0.72f, 0.66f, 0.38f + 0.5f * Mathf.Clamp01(giuSpace / 0.8f));
        GUI.Label(new Rect(0, 0, Screen.width - 36 * k, Screen.height - 30 * k), "Giữ Space để bỏ qua", st);
        if (giuSpace > 0) { float w = 170 * k * Mathf.Clamp01(giuSpace / 0.8f); GUI.color = new Color(0.75f, 0.72f, 0.66f, 0.7f); GUI.DrawTexture(new Rect(Screen.width - 36 * k - w, Screen.height - 26 * k, w, 2 * k), Texture2D.whiteTexture); GUI.color = Color.white; }
    }

    const string BAC = "BÁC";
    // thoại nói ra: chữ thẳng, có tên; nhịp 1,2 s + 0,06 s/ký tự, nghỉ 0,4 s (C1.2)
    static IEnumerator Thoai(params (string ai, string cau)[] dong)
    {
        foreach (var (ai, cau) in dong)
        {
            float t = 1.2f + 0.06f * cau.Length;
            if (LocDi.I) LocDi.I.Noi(ai == BAC);   // dì quay về người chơi, gật nhẹ khi tới câu của dì
            LocFirstPerson.PhuDe($"{ai}: {cau}", t, false);
            yield return new WaitForSeconds(t * LocFirstPerson.nhanChu + 0.4f);
        }
    }

    // ───── hết đêm → cắt đen → đêm sau
    IEnumerator HetDemThu() { dangNoiHam = false; LocBanNgay.chup = true; yield return HetDem(); }
    IEnumerator HetDem()
    {
        dangCanh = true;
        while (dangNoiHam || (LocTui.Co("thap_huong") && !LocTui.Co("ho_hang_ve"))) yield return null;   // không cắt ngang thoại Đêm 1
        yield return new WaitForSeconds(1.2f);   // kịp thấy đèn hầm tắt sau lưng
        bool daNuaTinh = false;
        if (LocMa.dem == 2 && LocNhim.I && !LocBanNgay.chup)   // [27/10] cảnh nửa tỉnh cuối Đêm 2 (đã có sẵn, nay nối vào): đi ngang cửa phòng em đang hé
        {
            LocNhim.I.CanhNuaTinh(true);
            yield return null;
            while (LocNhim.dangCanh) yield return null;
            daNuaTinh = true;   // cảnh tự cắt đen, màn vẫn đen
        }
        LocFirstPerson.khoa = true; LocMa.tamDung = true;
        if (!daNuaTinh) for (float t = 0; t < 1; t += Time.deltaTime / 0.6f) { LocFirstPerson.den = t; yield return null; }
        LocFirstPerson.den = 1; LocFirstPerson.PhuDe("", 0.01f);
        // [22/10] cắt cảnh ban ngày (LocBanNgay) thay màn đen; nhà đổi sang trạng thái đêm sau ngay lúc còn đen
        var bn = GetComponent<LocBanNgay>(); int sau = LocMa.dem + 1;
        if (bn) yield return bn.Chay(sau, () => ApDem(sau)); else yield return new WaitForSeconds(2.5f);
        LocMa.dem = sau; ApDem(LocMa.dem); DauDem();
        yield return new WaitForSeconds(0.8f);
        for (float t = 1; t > 0; t -= Time.deltaTime / 1.2f) { LocFirstPerson.den = t; yield return null; }
        LocFirstPerson.den = 0; LocFirstPerson.khoa = false; LocMa.tamDung = false; dangCanh = false;
    }

    // ───── gợi ý H (C1.4): Khôi tự nhủ, bấm lại thì lùi một bậc, không bao giờ nói đáp án
    void GoiYH()
    {
        string id = null; string[] bac = null;
        if (LocMa.dem == 1 && LocTui.Co("thu_ket") && !LocTui.Co("ket_mo"))
        { id = "ma_ket"; bac = new[] { "Bố mẹ chắc gì nhớ được mã. Phải ghi ở đâu đó.", "Giấy tờ nhà mình toàn để bên phòng làm việc của bố.", "...Sao lại có dòng ứng tiền cho anh Phong." }; }
        if (id == null) GoiYHV34(ref id, ref bac);
        if (id == null) return;
        bacGoiY.TryGetValue(id, out int i);
        LocTui.Noi(bac[Mathf.Min(i, bac.Length - 1)]);
        bacGoiY[id] = i + 1;
    }

    // ───── Đêm 2: cuộc gọi không lời → đọc sổ → hoảng loạn → giữ hương → hết đêm
    void Dem2()
    {
        Dem2V34();
        if (LocNhim.daTham[2] && !LocTui.Co("goi_2") && KC(dienThoai) < 6f) { LocTui.Dat("goi_2"); StartCoroutine(Reo(8, true)); }
        if (LocTui.Co("so_bo_het") && !LocNhim.daHoangLoan && !LocTui.Mo && !LocNhim.dangCanh && !LocFirstPerson.khoa && LocNhim.I)
        { LocNhim.I.CanhHoangLoan(); if (LocMa.I) LocMa.I.LoSom(); }
        if (LocTui.Co("giu_huong_2") && !LocNhim.dangCanh && !LocFirstPerson.khoa && !LocTui.Mo) StartCoroutine(HetDem());
    }

    // chuông điện thoại bàn: lan hồi, lại gần (khi không cho nhấc) thì tắt; cho nhấc thì [E] Nhấc máy → tiếng rè, cái đài, không ai nói
    IEnumerator Reo(int hoi, bool choNhac, System.Func<IEnumerator> loi = null)
    {
        if (!cReo || !amReo) yield break;
        dangReo = true; nhacMay = choNhac;
        for (int i = 0; i < hoi && dangReo; i++)
        {
            amReo.PlayOneShot(cReo, 0.9f);
            for (float t = 0; t < cReo.length && dangReo; t += Time.deltaTime) { if (!choNhac && KC(dienThoai) < 3.5f) dangReo = false; yield return null; }
        }
        amReo.Stop();
        bool daNhac = choNhac && !nhacMay;   // Bam(dien_thoai) đặt nhacMay = false
        dangReo = false; nhacMay = false;
        if (!choNhac) { LocTui.Noi("Giờ này còn ai gọi."); yield break; }
        if (!daNhac) yield break;   // không nhấc: tắt, không hậu quả
        if (loi != null) { dangCanh = true; LocMa.tamDung = true; yield return loi(); LocMa.tamDung = false; dangCanh = false; yield break; }   // [27/10] cuộc gọi có lời (thầy Đồng)
        dangCanh = true; LocMa.tamDung = true;
        yield return Thoai(("KHÔI", "A lô."));
        if (cRe) { amReo.PlayOneShot(cRe, 1f); yield return new WaitForSeconds(3f); }
        yield return Thoai(("KHÔI", "A lô. Ai đấy ạ."));
        if (cTut) { amReo.PlayOneShot(cTut, 0.8f); yield return new WaitForSeconds(cTut.length); }
        LocMa.tamDung = false; dangCanh = false;
    }

    // ───── Đêm 3: đồng hồ quả lắc đo hạn (Bad a)
    void Dem3()
    {
        Dem3V34();
        if (LocTui.Co("doc:sach_gia_le") && !LocTui.Co("diem_luu_2") && !LocTui.Mo) DiemLuu("diem_luu_2");
        if (gio == 0) { gio = 9; for (int g = 10; g <= gioHet; g++) if (LocTui.Co("gio" + g)) gio = g; if (gio == 9) StartCoroutine(Go(9)); }
        if (LocFirstPerson.khoa || LocMenu.DangChe || LocNhim.dangCanh || gio >= gioHet) return;
        if ((tDongHo += Time.deltaTime) < phutMoiTieng * 60f) return;
        tDongHo = 0; gio++; LocTui.Dat("gio" + gio); StartCoroutine(Go(gio));
        if (gio >= gioHet) StartCoroutine(BadA());
    }
    IEnumerator Go(int n)
    {
        yield return new WaitForSeconds(2f);
        for (int i = 0; i < n && cGo && amGo; i++) { amGo.PlayOneShot(cGo, 1f); yield return new WaitForSeconds(1.6f); }
    }
    IEnumerator BadA()
    {
        yield return new WaitForSeconds(12 * 1.6f + 3f);   // tiếng thứ mười hai, rồi im
        if (LocNhim.I && !LocNhim.dangCanh) LocNhim.I.CanhBad();
    }

    // shortcut: cảnh kết thay tạm (C6 chưa dựng: bốn lọn tóc, chén 5 vỡ, cuộc gọi thầy…) — chỉ màn đen + chữ cuối rồi về menu
    IEnumerator KetThuc(string chu)
    {
        yield return new WaitForSeconds(2.5f);
        dangCanh = true; LocFirstPerson.khoa = true; LocMa.tamDung = true;
        for (float t = LocFirstPerson.den; t < 1; t += Time.deltaTime / 1.2f) { LocFirstPerson.den = t; yield return null; }
        LocFirstPerson.den = 1; LocFirstPerson.khoa = true; LocFirstPerson.chuGiua = chu;
        yield return new WaitForSeconds(6f);
        LocFirstPerson.chuGiua = null; LocMenu.VeMenuChinh();
    }

    // ───── điểm lưu ngầm Đêm 3 (C3): chết thì chơi lại từ đây, không phải từ đầu đêm
    public static void DiemLuu(string ten)
    {
        if (!I || !I.fp) return;
        LocTui.Dat(ten);
        var p = I.fp.transform.position; var n = I.fp.cam.position + I.fp.cam.forward * 2f;
        LocTui.BoCo("vt:"); LocTui.Dat(string.Format(System.Globalization.CultureInfo.InvariantCulture, "vt:{0};{1};{2};{3};{4};{5}", p.x, p.y, p.z, n.x, n.y, n.z));
        LocTui.ChupDiemLuu(); LocMenu.LuuNgay();
    }
    void NapDiemLuu()
    {
        if (!LocTui.Co("he_hu")) return;
        LocMa.huDaHe = LocMa.maTucGian = true;
        var hu = FindAnyObjectByType<LocHuCot>(); if (hu) hu.DatDaHe();
        foreach (var c in LocTui.ChuoiCo.Split('|'))
            if (c.StartsWith("vt:"))
            {
                var s = c.Substring(3).Split(';'); var f = new float[6];
                for (int i = 0; i < 6; i++) float.TryParse(s[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out f[i]);
                fp.DatTuThe(new Vector3(f[0], f[1], f[2]), new Vector3(f[3], f[4], f[5]));
            }
    }

    static Bounds Bao(Transform t)
    {
        var rs = t.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(t.position + Vector3.up, Vector3.one * 0.1f);
        var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }
}
