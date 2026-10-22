using UnityEngine;
using UnityEngine.Events;

// [21/10 gộp] Đổi tên từ LocTuongTac → LocTuongTacMG, kế thừa LocTuongTac (lớp nền chung với LocVatPham, LocKetSat…).
// Vật tương tác tổng quát: gắn lên vật có Collider, nhìn vào rồi nhấn E (LocFirstPerson gọi Bam()).
//  Loai.Nhat       nhặt vào túi (id / tenMon); giuVat = false thì vật biến mất, true thì vẫn đứng đó (móc chìa khoá, gương…)
//  Loai.Dung       cần đúng món (canId có trong túi — tự cầm lên khi dùng — và/hoặc canCo đủ trong túi) → nhận thưởng, mở khoá, hiện/ẩn vật
//  Loai.MoMiniGame như Dung nhưng phải thắng mini game trước
// Chỉ hiện từ đêm demTu đến demDen (LocMa.dem). Xong rồi thì tắt gợi ý (trừ khi lapLai).
// Kết quả khi xong (HoanThanh): tiêu hao đồ cần, thêm thưởng vào túi, mở khoá cửa, hiện/ẩn vật, bật cờ, gọi khiXong.
public class LocTuongTacMG : LocTuongTac
{
    public enum Loai { Nhat, Dung, MoMiniGame }
    public Loai loai = Loai.Nhat;
    public string ten = "vật";             // hiện trong gợi ý "[E] ..."
    public string id;                      // Nhat: id món nhận được · khác: tên cờ bật khi xong (tuỳ chọn)
    public string tenMon;                  // Nhat: tên hiển thị trong túi (rỗng = LocId.Ten)
    public bool giuVat;                    // Nhat: vật vẫn còn đó sau khi nhặt

    [Header("Điều kiện")]
    public string canId;                   // món phải có trong túi, tự cầm lên khi dùng (rỗng = không cần)
    public string[] canCo;                 // các món phải CÓ ĐỦ trong túi (không cần cầm)
    public bool tieuHao = true;            // bỏ món canId sau khi xong
    public bool tieuHaoCo = true;          // bỏ các món canCo sau khi xong
    public int demTu = 1, demDen = 3;      // chỉ tương tác được trong khoảng đêm này
    public string cauThieu = "Chưa dùng được.";
    public LocMiniGame miniGame;

    [Header("Khi xong")]
    public string cauXong = "";
    public string thuongId, thuongTen;     // thưởng thêm vào túi
    public LocDoor[] moKhoa;               // đặt khoa = false (và mở luôn nếu moLuon)
    public bool moLuon;
    public bool khoaLucDau;                // khoá moKhoa ngay từ đầu (chỉ khi đêm hiện tại >= demTu) cho tới khi xong
    public GameObject[] hien, an;
    public UnityEvent khiXong;
    public bool lapLai;                    // làm được nhiều lần (ráp chén mỗi đêm)
    public bool xong;

    [Header("[22/10] Điều kiện cờ + cảnh hé lộ")]
    public string sauCo;                   // chỉ hiện khi đã có cờ này (vd "ho_hang_ve": dọn bàn nước sau khi cô về)
    public Transform nhinVao;              // xong thì camera từ từ quay về vật này …
    [TextArea] public string[] cauPhatHien;   // … và Khôi nghĩ mấy dòng này (chữ nghiêng). Khoá điều khiển tới khi nói xong.
    public static bool dangPhatHien;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset0() { dangPhatHien = false; }

    public bool HienDuoc { get { return (lapLai || !xong) && LocMa.dem >= demTu && LocMa.dem <= demDen && (string.IsNullOrEmpty(sauCo) || LocTui.Co(sauCo) || LocMa.dem > demTu); } }   // sauCo chỉ chặn trong đêm đầu (F2/F3 nhảy đêm vẫn chơi được)
    public override string GoiY { get { return !HienDuoc ? "" : loai == Loai.Nhat ? "[E] Nhặt " + ten : "[E] " + ten; } }

    // [21/10] chưa tới đêm / đã xong thì tắt va chạm để tia nhìn không bị cục đánh dấu chặn
    Collider vc;
    void Update() { if (vc) vc.enabled = HienDuoc; }

    void Start()
    {
        vc = GetComponent<Collider>();
        if (loai == Loai.Nhat && LocTui.CoMon(id)) { xong = true; if (!giuVat) gameObject.SetActive(false); return; }
        if (!lapLai && !string.IsNullOrEmpty(id) && LocTui.Co(id)) xong = true;
        if (xong && hien != null) foreach (var g in hien) if (g) g.SetActive(true);   // chơi lại đầu đêm: vòng sau đã mở thì vẫn mở
        if (xong && an != null) foreach (var g in an) if (g) g.SetActive(false);
        if (!khoaLucDau || moKhoa == null || xong || LocMa.dem < demTu) return;
        foreach (var d in moKhoa) if (d) d.khoa = true;
    }

    public override void Bam()
    {
        if (!HienDuoc) return;
        if (loai == Loai.Nhat) { LocTui.Nhan(id, string.IsNullOrEmpty(tenMon) ? null : tenMon); HoanThanh(); return; }
        if (!string.IsNullOrEmpty(canId))
        {
            if (!LocTui.CoMon(canId)) { LocFirstPerson.PhuDe(cauThieu, 2.4f); return; }
            LocTui.DangCam = canId;   // có trong túi thì tự cầm lên, khỏi mở Tab
        }
        if (canCo != null && canCo.Length > 0 && !LocTui.CoTatCa(canCo))
        {
            LocFirstPerson.PhuDe(cauThieu + " (" + LocTui.DemCo(canCo) + "/" + canCo.Length + ")", 2.4f);
            return;
        }
        if (loai == Loai.Dung) { HoanThanh(); return; }
        if (!miniGame) { HoanThanh(); return; }
        string cau;
        if (!miniGame.DuDieuKien(out cau)) { LocFirstPerson.PhuDe(cau, 2.6f); return; }
        miniGame.Mo(this);
    }

    public void HoanThanh()
    {
        if (!lapLai) xong = true;
        if (loai != Loai.Nhat)
        {
            if (tieuHao && !string.IsNullOrEmpty(canId)) LocTui.Bo(canId);
            if (tieuHaoCo) LocTui.BoTatCa(canCo);
        }
        if (!string.IsNullOrEmpty(id) && loai != Loai.Nhat) LocTui.Dat(id);
        bool nhanThuong = !string.IsNullOrEmpty(thuongId) && !LocTui.CoMon(thuongId);
        if (nhanThuong) LocTui.Nhan(thuongId, string.IsNullOrEmpty(thuongTen) ? null : thuongTen, false);
        if (moKhoa != null) foreach (var d in moKhoa) if (d) { d.khoa = false; if (moLuon) d.Mo(); }
        if (hien != null) foreach (var g in hien) if (g) g.SetActive(true);
        if (an != null) foreach (var g in an) if (g) g.SetActive(false);
        if (khiXong != null) khiXong.Invoke();
        string cau = cauXong ?? "";
        if (nhanThuong) cau += (cau.Length > 0 ? " — " : "") + "Nhặt được: " + (string.IsNullOrEmpty(thuongTen) ? LocId.Ten(thuongId) : thuongTen);
        if (cau.Length > 0 && loai != Loai.Nhat) LocFirstPerson.PhuDe(cau, 3f, false);
        if (loai == Loai.Nhat && !giuVat) gameObject.SetActive(false);
        if (nhinVao && cauPhatHien != null && cauPhatHien.Length > 0 && gameObject.activeInHierarchy) StartCoroutine(PhatHien());
    }

    // [22/10] cảnh hé lộ sau mini game: không bật bảng chữ — camera tự quay về món đồ, Khôi nghĩ vài dòng rồi trả điều khiển
    System.Collections.IEnumerator PhatHien()
    {
        dangPhatHien = true; LocFirstPerson.khoa = true; LocMa.tamDung = true;
        yield return null;   // đợi bảng mini game đóng hẳn
        var fp = FindAnyObjectByType<LocFirstPerson>();
        if (fp && fp.cam)
        {
            Vector3 tu = fp.cam.position + fp.cam.forward, den = nhinVao.position;
            for (float t = 0; t < 1; t += Time.deltaTime / 0.9f) { fp.DatTuThe(fp.transform.position, Vector3.Lerp(tu, den, t * t * (3 - 2 * t))); yield return null; }
            fp.DatTuThe(fp.transform.position, den);
        }
        yield return new WaitForSeconds(0.4f);
        LocTui.Noi(cauPhatHien);
        float tong = 0; foreach (var d in cauPhatHien) tong += Mathf.Max(1.8f, d.Length * 0.07f) + 0.15f;   // nhịp của LocTui.Noi
        yield return new WaitForSeconds(tong + 0.4f);
        LocFirstPerson.khoa = false; LocMa.tamDung = false; dangPhatHien = false;
    }
}
