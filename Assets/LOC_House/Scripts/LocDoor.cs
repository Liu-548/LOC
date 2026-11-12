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
        KiemGoc();
    }

    // [27/10 chơi thử] "mở/đóng cửa chính thì cánh vụt qua cột rạp ngoài sân": cánh quay hết góc goc dù giữa đường có vật.
    //  Lần đầu dùng, quét thử cánh theo từng bước 4° từ tư thế ĐÓNG về phía MỞ; gặp vật đầu tiên (không phải chính nó, không phải thứ đã
    //  chạm sẵn lúc đóng như khung/tường/ngưỡng) thì dừng góc mở trước vật đó ~4°. Cánh dựng sẵn ở tư thế mở thì dời tư thế mở về góc mới.
    //  Không chặn nếu góc còn lại < 35% (coi như đo nhầm). Tắt hẳn: LocDoor.TuChanGoc = false.
    public static bool TuChanGoc = true;
    bool daKiemGoc;
    static readonly Collider[] gocHB = new Collider[32];
    void KiemGoc()
    {
        if (daKiemGoc || !TuChanGoc || !Quay || Mathf.Abs(goc) < 20f) return;
        Luu();
        if (t > 0.001f && t < 0.999f) return;   // đang chạy hoạt cảnh: để lần sau
        daKiemGoc = true;
        var hops = GetComponentsInChildren<BoxCollider>();
        if (hops.Length == 0) return;
        var boQua = new System.Collections.Generic.HashSet<Collider>(GetComponentsInChildren<Collider>(true));
        if (ban != null) foreach (var d in ban) if (d) foreach (var c in d.GetComponentsInChildren<Collider>(true)) boQua.Add(c);
        float eDong = dungSanLaMo ? 1f : 0f, eMo = 1f - eDong;
        DatTam(eDong); Quet(hops, boQua, true);
        int n = Mathf.CeilToInt(Mathf.Abs(goc) / 4f), trung = -1;
        for (int i = 1; i <= n && trung < 0; i++) { DatTam(Mathf.Lerp(eDong, eMo, i / (float)n)); if (Quet(hops, boQua, false)) trung = i; }
        if (trung > 0)
        {
            float f = Mathf.Max(0f, (trung - 1.5f) / n);   // phần góc còn mở được
            if (f >= 0.35f)
            {
                float eMoi = Mathf.Lerp(eDong, eMo, f);
                if (dungSanLaMo) { DatTam(eMoi); lp0 = transform.localPosition; lr0 = transform.localRotation; }
                goc *= f;
            }
        }
        ApT();
    }
    // đặt cánh tạm ở phần góc e (0 = tư thế gốc, 1 = đã quay hết goc), không làm mượt
    void DatTam(float e)
    {
        var cha = transform.parent;
        Vector3 P(Vector3 v) => cha ? cha.TransformPoint(v) : v;
        Quaternion r0 = cha ? cha.rotation * lr0 : lr0, q = Quaternion.AngleAxis(goc * e, Vector3.up);
        var hinge = P(lh);
        transform.SetPositionAndRotation(hinge + q * (P(lp0) - hinge), q * r0);
    }
    // ghi = true: ghi mọi thứ đang chạm vào boQua · false: có chạm thứ gì mới không
    bool Quet(BoxCollider[] hops, System.Collections.Generic.HashSet<Collider> boQua, bool ghi)
    {
        foreach (var h in hops)
        {
            if (!h || h.isTrigger) continue;
            var s = h.transform.lossyScale;
            var nua = new Vector3(Mathf.Abs(h.size.x * s.x), Mathf.Abs(h.size.y * s.y), Mathf.Abs(h.size.z * s.z)) * 0.5f;
            nua = new Vector3(Mathf.Max(0.005f, nua.x - 0.02f), Mathf.Max(0.005f, nua.y - 0.06f), Mathf.Max(0.005f, nua.z - 0.02f));   // bỏ chạm sượt sàn/khung
            int k = Physics.OverlapBoxNonAlloc(h.transform.TransformPoint(h.center), nua, gocHB, h.transform.rotation, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < k; i++)
            {
                var c = gocHB[i];
                if (boQua.Contains(c) || c is CharacterController || c.GetComponentInParent<LocDoor>()) continue;
                if (ghi) boQua.Add(c); else return true;
            }
        }
        return false;
    }

    public bool DangMo => (dich < 0.5f) == dungSanLaMo;
    public void Bam()
    {
        if (khoa)
        {
            if (khiKhoa) { khiKhoa.Bam(); return; }
            if (string.IsNullOrEmpty(canVatPham) || !LocTui.CoMon(canVatPham)) { LocTui.Noi(loiKhoa); return; }
            if (!dangMoKhoa) StartCoroutine(MoKhoa());   // [1/11] có chìa thì cũng phải tra chìa mở ổ khoá, không bật ra ngay
            return;
        }
        Toggle();
    }

    static float amCuoi = -1f;
    // [1/11] tra chìa: nhìn vào ổ khoá → tay với tới → lách cách, bật càng → tháo ổ khoá → mở cửa
    bool dangMoKhoa;
    System.Collections.IEnumerator MoKhoa()
    {
        dangMoKhoa = true; LocFirstPerson.khoa = true;
        Transform o = null; foreach (var t in GetComponentsInChildren<Transform>(true)) if (t.name == "KhoaCua_Hasp_2") { o = t; break; }
        Vector3 diem = o ? o.position : transform.position + Vector3.up * 1.0f;
        var fp = FindAnyObjectByType<LocFirstPerson>();
        if (fp && fp.cam)
        {
            var tu = fp.cam.position + fp.cam.forward;
            for (float t = 0; t < 1; t += Time.deltaTime / 0.45f) { fp.DatTuThe(fp.transform.position, Vector3.Lerp(tu, diem, t * t * (3 - 2 * t))); yield return null; }
        }
        var tay = StartCoroutine(LocTay.MotTay(diem, 1.3f, "Nhon"));
        LocAm.Phat("OKhoa_Mo", diem, 1f);
        yield return new WaitForSeconds(0.85f);
        if (o) foreach (var t in o.parent.GetComponentsInChildren<Transform>(true)) if (t.name is "KhoaCua_Hasp_2" or "KhoaCua_Hasp_3" or "KhoaCua_Hasp_4") t.gameObject.SetActive(false);   // thân + càng ổ khoá treo
        yield return tay;
        LocTui.Noi(string.IsNullOrEmpty(loiMoKhoa) ? "Mở được rồi." : loiMoKhoa);
        khoa = false; LocFirstPerson.khoa = false; dangMoKhoa = false;
        yield return new WaitForSeconds(0.3f);
        Toggle();
    }

    // [13/11] cửa ra vào bản lề gỗ (không tính tủ, chạn, két, cửa sổ, cổng/cửa sắt): dùng CuaPhong_Mo / CuaPhong_Dong
    bool CuaPhong => ten.StartsWith("cửa phòng") || ten == "cửa WC" || ten == "cửa kho" || ten == "cửa chính" || ten == "cửa ban công" || ten == "cửa ra sân phơi" || ten == "cửa sắt kính ra giếng trời";   // [13/11] cửa sắt kính bếp: dùng tiếng cửa phòng

    // [13/11] tủ đồ, tủ bếp / chạn, ngăn kéo (bàn học…): dùng TuDo_Mo / TuDo_Dong · không tính tủ lạnh, két
    bool TuDo => !ten.Contains("tủ lạnh") && (ten.StartsWith("cửa tủ") || ten.StartsWith("tủ") || ten.StartsWith("cửa chạn") || ten.StartsWith("ngăn"));

    public void Toggle()
    {
        if (khoa) return; GhiCong("Toggle"); Luu(); KiemGoc(); dich = dich < 0.5f ? 1f : 0f; CapNhatChan();
        if (Application.isPlaying && thoiGian > 0.05f && Time.time - amCuoi > 0.08f)   // cánh bạn cùng ô cửa bật theo thì không kêu chồng   // [1/11] tiếng cửa: kẹt khi mở, bụp khi đóng · cổng/cửa sắt rít kim loại · ngăn kéo/kính lùa nhỏ
        {
            amCuoi = Time.time;
            bool sat = ten.Contains("cổng") || ten.Contains("sắt") || ten.Contains("hầm");
            var p = transform.position + Vector3.up * 1.0f;
            if (ten == "cửa sổ") LocAm.Phat(DangMo ? "CuaSo_Mo" : "CuaSo_Dong", p, 0.5f, Random.Range(0.96f, 1.04f));   // [13/11] cửa sổ (freesound window-opening)
            else if (TuDo) LocAm.Phat(DangMo ? "TuDo_Mo" : "TuDo_Dong", p, DangMo ? 0.5f : 0.4f, Random.Range(0.95f, 1.05f));   // [13/11] tủ đồ, tủ bếp/chạn, ngăn kéo bàn học (freesound door-opening-and-closing)
            else if (truot != Vector3.zero || gapX > 0f) LocAm.Phat("Cua_Dong", p, 0.25f, 1.6f);
            else if (CuaPhong) LocAm.Phat(DangMo ? "CuaPhong_Mo" : "CuaPhong_Dong", p, DangMo ? 0.6f : 0.65f, Random.Range(0.96f, 1.04f));   // [13/11] cửa phòng: tiếng mở / đóng thật (freesound main-door-opening-closing)
            else if (sat) LocAm.Phat("Cong_Sat", p, DangMo ? 0.55f : 0.45f, DangMo ? 1f : 1.15f);
            else LocAm.Phat(DangMo ? "Cua_Ken" : "Cua_Dong", p, DangMo ? 0.45f : 0.5f, Random.Range(0.9f, 1.08f));
        }
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
    public void DatNgay(bool mo) { GhiCong(mo ? "DatNgay(mở)" : "DatNgay(đóng)"); Luu(); KiemGoc(); dich = mo == dungSanLaMo ? 0f : 1f; t = dich; ApT(); CapNhatChan(); }
    // [03/11] tự kiểm báo "đầu Đêm 1: 2 cánh cổng lớn đang mở" mà chưa rõ ai mở → trong Editor, mọi lần cổng đổi trạng thái đều ghi Console
    //  (Console kèm stack trace = biết script nào gọi). Bản build không ghi gì.
    void GhiCong(string viec)
    {
#if UNITY_EDITOR
        if (name.StartsWith("CanhCong") || name.StartsWith("CuaNgachCong")) Debug.Log($"[Cổng] {name}: {viec} · đêm {LocMa.dem} · t={Time.time:0.0}s");
#endif
    }

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
