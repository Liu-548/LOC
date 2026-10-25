using UnityEngine;

// Cánh cửa / cổng / cửa sổ mở được (phục vụ tìm kiếm & giải đố).
// Tư thế dựng sẵn = trạng thái đầu. goc = số độ quay quanh trục đứng đi qua bản lề để sang trạng thái còn lại (dấu như Quaternion.Euler(0, goc, 0)).
// Gọi Toggle() / Mo() / Dong() từ script game, hoặc nhìn vào cánh và nhấn E (LocFirstPerson).
public class LocDoor : MonoBehaviour
{
    public string ten = "cửa";
    public Vector3 banLe;            // vị trí bản lề, toạ độ cục bộ của cánh
    public float goc = 90f;
    public bool dungSanLaMo = true;  // tư thế dựng sẵn là MỞ?
    public float thoiGian = 0.7f;    // giây
    public bool khoa;                // script game đặt true để khoá (Toggle không làm gì)
    // [6/10] người chơi bấm E vào cánh đang khoá (Bam): có món canVatPham trong túi → mở khoá; có khiKhoa (khoá số…) → chuyển cho nó; còn lại nói loiKhoa
    public string canVatPham;
    public string loiKhoa = "Khóa rồi.", loiMoKhoa = "";
    public LocTuongTac khiKhoa;
    public float gapX;               // > 0: cửa xếp/kéo — thay vì quay, thu chiều ngang (scale X) về tỉ lệ này quanh gốc
    public Vector3 truot;            // ≠ 0: ngăn kéo / kính lùa — tịnh tiến (toạ độ cục bộ của cha) thay vì quay
    public LocDoor[] cungCua;        // các cánh cùng ô cửa (cửa sổ 2 cánh)
    public GameObject[] anKhiMo;     // vật chắn (ô kính trà…) chỉ hiện khi tất cả cánh đều đóng

    // [23/10] tư thế gốc lưu theo toạ độ của CHA (không phải thế giới) và lưu lười (lần đầu dùng):
    //  trước đây lưu toạ độ thế giới trong Awake → cánh nằm trong nhóm đang ẩn mà bị DatNgay/Mo trước Awake thì bay về gốc (0,0,0),
    //  cha bị dời sau Awake thì cánh quay quanh bản lề cũ (mở lệch, tách khỏi khung).
    Vector3 lp0, lh, s0; Quaternion lr0; bool daLuu; float t, dich;
    void Luu()
    {
        if (daLuu) return; daLuu = true;
        lp0 = transform.localPosition; lr0 = transform.localRotation; s0 = transform.localScale;
        var h = transform.TransformPoint(banLe); lh = transform.parent ? transform.parent.InverseTransformPoint(h) : h;
    }
    void Awake() { Luu(); CapNhatChan(); }

    // [6/10 · máy bạn] cửa đôi: các cánh QUAY cùng tên, cùng cha, cách nhau ≤ 3,3 m mở/đóng cùng lúc (ngoài danh sách cungCua khai báo).
    //  Cánh trượt / xếp / ngăn kéo không ghép. Tắt hẳn: LocDoor.TuGhepCuaDoi = false.
    public static bool TuGhepCuaDoi = true;
    LocDoor[] ban;
    bool Quay => truot == Vector3.zero && gapX <= 0f;
    void Start()
    {
        if (!Quay) return;
        var l = new System.Collections.Generic.List<LocDoor>();
        if (cungCua != null) foreach (var c in cungCua) if (c && c != this && c.Quay) l.Add(c);
        if (TuGhepCuaDoi && transform.parent)
            foreach (var d in transform.parent.GetComponentsInChildren<LocDoor>())
                if (d != this && d.ten == ten && d.Quay && d.transform.parent == transform.parent && !l.Contains(d)
                    && Vector3.Distance(d.transform.position, transform.position) <= 3.3f) l.Add(d);
        ban = l.ToArray();
    }

    public bool DangMo => (dich < 0.5f) == dungSanLaMo;
    public void Bam()
    {
        if (khoa)
        {
            if (khiKhoa) { khiKhoa.Bam(); return; }
            if (string.IsNullOrEmpty(canVatPham) || !LocTui.CoMon(canVatPham)) { LocTui.Noi(loiKhoa); return; }
            khoa = false; LocTui.Noi(loiMoKhoa);
        }
        Toggle();
    }
    public void Toggle()
    {
        if (khoa) return; Luu(); dich = dich < 0.5f ? 1f : 0f; CapNhatChan();
        if (ban != null) foreach (var d in ban) if (d && !d.khoa && d.DangMo != DangMo) d.Toggle();   // cánh bạn cùng ô cửa
    }
    void CapNhatChan()
    {
        if (anKhiMo == null || anKhiMo.Length == 0) return;
        bool mo = DangMo;
        if (cungCua != null) foreach (var c in cungCua) if (c && c.DangMo) mo = true;
        foreach (var g in anKhiMo) if (g) g.SetActive(!mo);
    }
    public void Mo() { if (!DangMo) Toggle(); }
    public void Dong() { if (DangMo) Toggle(); }

    // [8/10] đặt ngay trạng thái mở/đóng, không chạy hoạt cảnh, bỏ qua khoá (đầu đêm)
    public void DatNgay(bool mo) { Luu(); dich = mo == dungSanLaMo ? 0f : 1f; t = dich; ApT(); CapNhatChan(); }

    void Update()
    {
        if (Mathf.Approximately(t, dich)) return;
        t = Mathf.MoveTowards(t, dich, Time.deltaTime / Mathf.Max(0.05f, thoiGian));
        ApT();
    }
    void ApT()
    {
        float e = Mathf.SmoothStep(0f, 1f, t);
        if (truot != Vector3.zero) { transform.localPosition = lp0 + truot * e; return; }
        if (gapX > 0f) { transform.localScale = new Vector3(Mathf.Lerp(s0.x, s0.x * gapX, e), s0.y, s0.z); return; }
        var cha = transform.parent;
        Vector3 P(Vector3 v) => cha ? cha.TransformPoint(v) : v;
        Quaternion r0 = cha ? cha.rotation * lr0 : lr0, q = Quaternion.AngleAxis(goc * e, Vector3.up);   // trục quay luôn là trục đứng thế giới
        var hinge = P(lh);
        transform.SetPositionAndRotation(hinge + q * (P(lp0) - hinge), q * r0);
    }
}
