using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// [5/10] NHÍM — em gái (kịch bản v3.1 + đặc tả claude/dac-ta-nhim-05-10.md).
//  · Nằm giường phòng Nhím suốt 3 đêm, tay trái bó bột, sốt nặng dần theo đêm (texture da/mắt, nhịp thở, độ bóng mồ hôi).
//  · Lần thăm bắt buộc mỗi đêm (vào phòng + nhìn về phía giường) → thoại phụ đề nguyên văn v3.1; Đêm 1–2 có bước [E] Thay khăn ở chậu nước.
//  · Quay lại phòng sau lần thăm: mỗi đêm một câu, chỉ một lần (Đêm 3 không nói, chỉ thở nhanh).
//  · Cảnh: hoảng loạn Đêm 2 · nửa tỉnh cuối Đêm 2 · (phòng Nhím sau tiếng thét do LocMa chạy — em chỉ ngủ) · Good (sáng hôm sau, ngồi dậy ở mép giường) · Bad (A ngồi quỳ đầu gập → B đứng góc tường) · Evil (gọi từ trên gác).
//  · [5/10] Nhím KHÔNG bao giờ ra khỏi phòng (cả 3 đêm lẫn mọi cảnh kết). Nhím và Khôi đều biết nhà có hầm, chỉ không biết dưới đó có gì và bị cấm mở cửa, đi xuống.
//  · Phím thử: F7 thăm đêm hiện tại · F8 nửa tỉnh · F9 Good · F10 Bad · F11 hoảng loạn · F12 Evil.
//  · Cờ cho hệ thống cốt truyện: daTham[đêm], daThayKhan[đêm], daHoangLoan. Trong lúc thoại/cảnh: LocMa.tamDung = true (không lấp ló).
public class LocNhim : MonoBehaviour
{
    public static LocNhim I;
    public static bool[] daTham = new bool[4], daThayKhan = new bool[4], daQuayLai = new bool[4];
    public static bool daHoangLoan, dangCanh;
    public static System.Action SauGood, SauBad;   // cảnh kết của hệ thống cốt truyện nối tiếp ở đây
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset0() { daTham = new bool[4]; daThayKhan = new bool[4]; daQuayLai = new bool[4]; daHoangLoan = dangCanh = false; SauGood = SauBad = null; }

    [Header("Model (LocHouseBuilder_Nhim gán)")]
    public Animator anim;
    public SkinnedMeshRenderer da;
    public Renderer[] mat;
    public Transform head, eyeL, eyeR;
    public GameObject chan, chanVo, khanKho, khanUot, dayDeo;
    public Texture2D[] texDa = new Texture2D[4], texMat = new Texture2D[4];
    public AudioClip tho, ho, sotSoat, vatKhan, dienChap, khoiTho;
    [Header("Chỗ đặt (gốc model)")]
    public Vector3 neoGiuong; public float yawGiuong;
    public Vector3 neoGhe; public float yawGhe;
    public Vector3 neoGoc, neoGoc2; public float yawGoc;
    [Header("Phòng")]
    public Bounds phong;
    public LocDoor cua;
    public Light denNgu;
    public Renderer chau; public Material matChauKho; public int chauNuoc = 1;
    public Vector3 choThayKhan, choCuaTrong, choCuaNgoai, choBanAn, huongCauThang;
    [Header("Thông số")]
    public float nhipGoc = 20f;                    // Nam_Ngu dài 3 s = 20 lần/phút
    public float[] nhipDem = { 20, 22, 26, 30 };   // lần thở/phút theo đêm (0 = khỏe)
    public float[] sotDem = { 0.05f, 0.35f, 0.6f, 0.85f };
    public float gocNhin = 50f;
    public bool hienDebug = true;

    Transform cam, player; LocFirstPerson fp;
    AudioSource amTho, am;
    int demDangAp = -1;
    bool thuc, dangThoai, trongPhong, choThay, chauKhoDangDung;
    float nhinW, chopHet, tiepChop = 2f, noiHet, hTro, hHo;
    float bsNham, bsHe, bsMieng, bsCuoi, bsNhau, bsChat;   // giá trị hiện tại
    float tNham, tHe, tCuoi, tNhau, tChat;                 // mục tiêu
    Vector3 faceLocal, eyeLocalL, eyeLocalR;
    int iNham = -1, iHe = -1, iMieng = -1, iCuoi = -1, iNhau = -1, iChat = -1;
    Material[] chauGoc;
    string dbg = ""; float dbgHet;

    void Awake()
    {
        I = this;
        // hướng mặt (khung cục bộ của xương đầu/mắt) — đo ở tư thế nghỉ, trước khi Animator chạy
        if (head && eyeL && eyeR)
        {
            var mid = (eyeL.position + eyeR.position) * 0.5f;
            var f = mid - head.position; var up = head.position - (head.parent ? head.parent.position : transform.position);
            f = Vector3.ProjectOnPlane(f, up.normalized).normalized;
            faceLocal = head.InverseTransformDirection(f);
            eyeLocalL = eyeL.InverseTransformDirection(f); eyeLocalR = eyeR.InverseTransformDirection(f);
        }
        if (da && da.sharedMesh)
        {
            var m = da.sharedMesh;
            iNham = m.GetBlendShapeIndex("MatNham"); iHe = m.GetBlendShapeIndex("MatHe"); iMieng = m.GetBlendShapeIndex("MiengMo");
            iCuoi = m.GetBlendShapeIndex("Cuoi"); iNhau = m.GetBlendShapeIndex("MayNhau"); iChat = m.GetBlendShapeIndex("NhamChat");
        }
        amTho = gameObject.AddComponent<AudioSource>(); amTho.clip = tho; amTho.loop = true; amTho.spatialBlend = 1; amTho.minDistance = 0.4f; amTho.maxDistance = 5f; amTho.rolloffMode = AudioRolloffMode.Linear;
        am = gameObject.AddComponent<AudioSource>(); am.spatialBlend = 1; am.minDistance = 0.5f; am.maxDistance = 8f; am.rolloffMode = AudioRolloffMode.Linear;
        if (chau) chauGoc = chau.sharedMaterials;
    }

    void Start()
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorPrefs.GetBool("LOC_TuKiemNhim", false)) { UnityEditor.EditorPrefs.SetBool("LOC_TuKiemNhim", false); StartCoroutine(TuKiem()); }
#endif
        fp = FindAnyObjectByType<LocFirstPerson>();
        player = fp ? fp.transform : null;
        cam = fp && fp.cam ? fp.cam : Camera.main ? Camera.main.transform : null;
        if (anim) anim.applyRootMotion = false;
    }

    // ───────── chỗ đặt + trạng thái theo đêm
    void Dat(Vector3 p, float yaw) { transform.SetPositionAndRotation(p, Quaternion.Euler(0, yaw, 0)); }
    void Clip(string ten, float fade = 0.35f, float toc = 1f) { if (!anim) return; anim.speed = toc; anim.CrossFadeInFixedTime(ten, fade); }
    int Muc => Mathf.Clamp(LocMa.dem, 1, 3);
    float TocTho => nhipDem[Muc] / nhipGoc;

    void ApDem(int d)
    {
        demDangAp = d; d = Mathf.Clamp(d, 1, 3);
        Dat(neoGiuong, yawGiuong);
        DatSot(d);
        if (chan) chan.SetActive(true); if (chanVo) chanVo.SetActive(false); if (dayDeo) dayDeo.SetActive(false);
        if (khanKho) khanKho.SetActive(true); if (khanUot) khanUot.SetActive(false);
        if (chau && chauGoc != null) chau.sharedMaterials = chauGoc;
        Ngu(0f);
        amTho.pitch = 0.9f + 0.1f * d; amTho.volume = 0.35f + 0.15f * d; if (tho && !amTho.isPlaying) amTho.Play();
        hTro = Random.Range(40f, 90f); hHo = Random.Range(30f, 70f);
    }

    void DatSot(int lv)
    {
        if (da) { var m = da.material; if (texDa[lv]) m.SetTexture("_BaseMap", texDa[lv]); m.SetFloat("_Smoothness", 0.30f + 0.22f * sotDem[lv]); }
        if (mat != null) foreach (var r in mat) if (r && texMat[lv]) r.material.SetTexture("_BaseMap", texMat[lv]);
    }

    void Ngu(float fade = 0.8f) { thuc = false; tNham = 100; tHe = 0; tCuoi = 0; tNhau = Muc >= 2 ? 25 : 0; tChat = 0; Clip("Nam_Ngu", fade, TocTho); }
    void Thuc(float he = 0) { thuc = true; tNham = 0; tHe = he; Clip("Nam_Thuc", 0.6f, TocTho); }

    bool NguoiTrongPhong() => player && phong.Contains(player.position + Vector3.up * 0.9f);

    // ───────── vòng chính
    void Update()
    {
        PhimThu();
        if (LocMa.dem != demDangAp && !dangCanh) ApDem(LocMa.dem);
        bool vao = NguoiTrongPhong();
        if (!dangCanh && !dangThoai)
        {
            int d = Mathf.Clamp(LocMa.dem, 1, 3);
            if (vao && !daTham[d] && !(d == 3 && LocMa.huDaHe) && DangNhinEm(3.4f, 40f)) coTham = StartCoroutine(Tham(d));
            else if (vao && !trongPhong && daTham[d] && !daQuayLai[d]) StartCoroutine(QuayLai(d));
            // ngủ: trở mình, ho (Đêm 2–3)
            if (!thuc && d >= 2)
            {
                if ((hTro -= Time.deltaTime) <= 0) { hTro = Random.Range(40f, 90f); Clip("Nam_TroMinh", 0.4f); if (sotSoat) am.PlayOneShot(sotSoat, 0.5f); StartCoroutine(VeNgu(3.1f)); }
                if ((hHo -= Time.deltaTime) <= 0) { hHo = Random.Range(40f, 90f); if (ho) am.PlayOneShot(ho, 0.35f + 0.1f * d); }
            }
        }
        trongPhong = vao;
        if (amTho) amTho.mute = dangCanhKhongTho;
        // không lấp ló trong lúc thoại/cảnh (kịch bản 6.3)
        if (dangThoai || dangCanh) { LocMa.tamDung = true; toiDat = true; } else if (toiDat) { LocMa.tamDung = false; toiDat = false; }
    }
    bool dangCanhKhongTho, toiDat;
    Coroutine coTham;
    IEnumerator VeNgu(float sau) { yield return new WaitForSeconds(sau); if (!thuc && !dangCanh && !dangThoai) Clip("Nam_Ngu", 0.6f, TocTho); }

    bool DangNhinEm(float xa, float goc)
    {
        if (!cam || !head) return false;
        var d = head.position - cam.position;
        return d.magnitude < xa && Vector3.Angle(cam.forward, d) < goc;
    }

    void LateUpdate()
    {
        float dt = Time.deltaTime;
        // chớp mắt khi thức
        float chop = 0;
        if (thuc) { if (Time.time > tiepChop) { chopHet = Time.time + 0.13f; tiepChop = Time.time + Random.Range(2f, 6f); } if (Time.time < chopHet) chop = 100; }
        float k = 1 - Mathf.Exp(-9f * dt);
        bsNham = Mathf.Lerp(bsNham, Mathf.Max(tNham, chop), chop > 0 ? 1 : k * 0.6f);
        bsHe = Mathf.Lerp(bsHe, tHe, k); bsCuoi = Mathf.Lerp(bsCuoi, tCuoi, k); bsNhau = Mathf.Lerp(bsNhau, tNhau, k); bsChat = Mathf.Lerp(bsChat, tChat, k);
        float mieng = Time.time < noiHet ? 14f + 22f * Mathf.Abs(Mathf.Sin(Time.time * 10.5f)) * Mathf.PerlinNoise(Time.time * 3f, 0.3f) : 0f;
        bsMieng = Mathf.Lerp(bsMieng, mieng, 1 - Mathf.Exp(-20f * dt));
        if (da)
        {
            void S(int i, float v) { if (i >= 0) da.SetBlendShapeWeight(i, v); }
            S(iNham, bsNham); S(iHe, bsHe); S(iMieng, bsMieng); S(iCuoi, bsCuoi); S(iNhau, bsNhau); S(iChat, bsChat);
        }
        // đầu + mắt nhìn Khôi khi thức
        nhinW = Mathf.MoveTowards(nhinW, thuc && cam ? 1f : 0f, dt * 1.5f);
        if (nhinW > 0.001f && cam && head)
        {
            var f = head.TransformDirection(faceLocal); var to = (cam.position - head.position).normalized;
            var q = Quaternion.FromToRotation(f, to); q.ToAngleAxis(out float a, out Vector3 ax);
            if (a > 180) a -= 360;
            a = Mathf.Clamp(a, -gocNhin, gocNhin);
            float gat = Time.time < noiHet ? Mathf.Sin(Time.time * 6.3f) * 2.2f : 0f;   // gật khẽ khi nói
            head.rotation = Quaternion.AngleAxis(a * nhinW * Mathf.SmoothStep(0, 1, nhinW), ax) * head.rotation;
            if (gat != 0) head.rotation = Quaternion.AngleAxis(gat * nhinW, Vector3.Cross(Vector3.up, f).normalized) * head.rotation;
            foreach (var (e, loc) in new[] { (eyeL, eyeLocalL), (eyeR, eyeLocalR) })
            {
                if (!e) continue;
                var ef = e.TransformDirection(loc); var et = (cam.position - e.position).normalized;
                var qe = Quaternion.FromToRotation(ef, et); qe.ToAngleAxis(out float ae, out Vector3 axe); if (ae > 180) ae -= 360;
                e.rotation = Quaternion.AngleAxis(Mathf.Clamp(ae, -22, 22) * nhinW, axe) * e.rotation;
            }
        }
    }

    // ───────── thoại
    float ThoiGian(string s, float he) => (1.2f + 0.06f * s.Length) * he;
    IEnumerator Noi(string ai, string cau, float he = 1f, float nghiSau = 0.4f, bool nghieng = false)
    {
        string s = ai == null ? cau : $"{ai}: {cau}";
        float d = ThoiGian(cau, he);
        while (true)
        {
            while (!NguoiTrongPhong() && !dangCanh) yield return null;
            LocFirstPerson.PhuDe(s, d, nghieng); Ghi("thoại  " + s);
            if (ai == "NHÍM") noiHet = Time.time + d * 0.8f;
            float t = 0; bool dut = false;
            while (t < d) { if (!dangCanh && !NguoiTrongPhong()) { dut = true; break; } t += Time.deltaTime; yield return null; }
            if (!dut) break;
            LocFirstPerson.PhuDe("", 0.01f); noiHet = 0;   // đi ra giữa chừng: dừng, vào lại thì nói lại dòng này
        }
        yield return new WaitForSeconds(nghiSau);
    }
    IEnumerator Cho(float giay) { for (float t = 0; t < giay; t += Time.deltaTime) { while (!dangCanh && !NguoiTrongPhong()) yield return null; yield return null; } }

    IEnumerator Tham(int d)
    {
        dangThoai = true;
        if (d == 1)
        {
            Thuc(); Clip("Nam_NgocDau", 0.5f); tCuoi = 45;
            yield return Noi("NHÍM", "Anh Khôi ơi!", 1f, 0.3f);
            yield return Noi("NHÍM", "Anh về thật à. Em tưởng anh không về.");
            tCuoi = 0;
            yield return Noi("KHÔI", "Về rồi. Tay em sao thế này.");
            yield return Noi("NHÍM", "Em ngã. Ở cầu thang dưới hầm ấy.");
            yield return Noi("KHÔI", "...Em xuống hầm à? Bố cấm cả nhà xuống đấy cơ mà.");
            tNhau = 60; Clip("Nam_Thuc", 1.4f, TocTho);
            yield return Noi("NHÍM", "Cửa không khóa. Em chỉ muốn xem dưới đấy có gì. Tối lắm. Bố mắng em.", 1f, 0.8f);
            tNhau = 20;
            yield return ChoThayKhan();
            tHe = 50;
            yield return Noi("NHÍM", "Anh ngồi đây tí nữa nhé.", 1.1f, 0.2f);
            Ngu(2.5f);
        }
        else if (d == 2)
        {
            Thuc(35); Clip("Nam_NgheDau", 0.6f); tNhau = 55;
            yield return Noi("NHÍM", "Anh Khôi ơi. Nóng quá.", 1.1f, 0.6f);
            Clip("Nam_Thuc", 1.2f, TocTho);
            yield return ChoThayKhan();
            yield return Noi("NHÍM", "Sao em nằm mãi mà không đỡ nhỉ.", 1.1f);
            yield return Noi("KHÔI", "Mai anh đưa em đi viện lại. Cần gì cứ gọi anh, nghe chưa.", 1f, 0.6f);
            Ngu(2f);
        }
        else
        {
            const float CHAM = 1.3f;
            thuc = true; tNham = 0; tHe = 55; tNhau = 70; Clip("Nam_Thuc", 0.8f, TocTho);
            yield return Noi("NHÍM", "Anh Khôi ơi.", CHAM);
            yield return Noi("NHÍM", "Đêm qua có ai đứng ở cửa nhìn em.", CHAM);
            yield return Noi("KHÔI", "Ai.");
            yield return Noi("NHÍM", "Em không biết. Đen sì. Cao lắm. Nó không vào, nó chỉ đứng nhìn thôi.", CHAM);
            tChat = 100; tNhau = 0; thuc = false; Clip("Nam_So", 0.6f);
            yield return Noi("NHÍM", "Em nhắm mắt lại. Em sợ.", CHAM, 0.8f);
            tChat = 0; thuc = true; tNham = 0; tHe = 65; tNhau = 60;
            yield return Noi("NHÍM", "Anh Khôi ơi. Mai anh đưa em đi viện thật không.", CHAM, 0f);
            yield return Cho(4f);
            yield return Noi("KHÔI", "Ừ.", 1f, 1f);
            Ngu(2.5f);
        }
        daTham[d] = true; dangThoai = false;
    }

    IEnumerator QuayLai(int d)
    {
        daQuayLai[d] = true;
        if (d == 3) yield break;   // Đêm 3: không nói, chỉ thở nhanh
        dangThoai = true;
        yield return new WaitForSeconds(1.4f);
        if (NguoiTrongPhong())
        {
            if (d == 1) { tHe = 40; yield return Noi("NHÍM", "Anh chưa đi ngủ à.", 1.2f); }
            else yield return Noi("NHÍM", "...Anh Khôi ơi. Bật đèn cho em.", 1.3f);
            tHe = 0;
        }
        dangThoai = false;
    }

    // ───────── thay khăn
    public bool ChoThay => choThay;
    IEnumerator ChoThayKhan()
    {
        choThay = true;
        while (choThay) yield return null;
        while (dangThayKhan) yield return null;
    }
    bool dangThayKhan;
    public void ThayKhan() { if (choThay && !dangThayKhan) StartCoroutine(CanhThayKhan()); }
    IEnumerator CanhThayKhan()
    {
        dangThayKhan = true; LocFirstPerson.khoa = true;
        yield return Den(0.35f, 1);
        if (fp && head) fp.DatTuThe(choThayKhan, head.position + Vector3.up * 0.04f);
        yield return Den(0.35f, 0);
        if (vatKhan) am.PlayOneShot(vatKhan, 0.8f);
        yield return new WaitForSeconds(1.2f);
        yield return Den(0.25f, 1);
        if (khanKho) khanKho.SetActive(false); if (khanUot) khanUot.SetActive(true);
        yield return Den(0.25f, 0);
        Clip("Nam_ThayKhan", 0.3f); tHe = 70;
        yield return new WaitForSeconds(1.6f);
        Clip("Nam_Thuc", 0.8f, TocTho); tHe = 30;
        daThayKhan[Muc] = true; choThay = false;
        LocFirstPerson.khoa = false; dangThayKhan = false;
    }

    IEnumerator Den(float giay, float toi)
    {
        float a = LocFirstPerson.den;
        for (float t = 0; t < 1; t += Time.deltaTime / Mathf.Max(giay, 0.01f)) { LocFirstPerson.den = Mathf.Lerp(a, toi, t); yield return null; }
        LocFirstPerson.den = toi;
    }

    // ───────── đoạn hoảng loạn Đêm 2 (sau khi đọc xong sổ)
    public void CanhHoangLoan() { if (!dangCanh) StartCoroutine(HoangLoan()); }
    IEnumerator HoangLoan()
    {
        BatCanh();
        if (khoiTho) am.PlayOneShot(khoiTho, 1f);
        var rung = StartCoroutine(Rung(1.6f, 1.4f));
        LocFirstPerson.PhuDe("Mùng một tháng bảy.", 1.8f); yield return new WaitForSeconds(1.9f);
        LocFirstPerson.PhuDe("Mùng một tháng bảy là ngày kia.", 2.2f); yield return new WaitForSeconds(2.0f);
        yield return Den(0.3f, 1);
        if (cua) cua.Mo();
        Ngu(0f);
        if (fp && head) fp.DatTuThe(choCuaTrong, head.position);
        yield return new WaitForSeconds(0.5f);
        yield return Den(0.4f, 0);
        yield return new WaitForSeconds(4f);
        LocFirstPerson.PhuDe("Không sao. Nó ngủ.", 2f); yield return new WaitForSeconds(2.1f);
        LocFirstPerson.PhuDe("Bình tĩnh cái đã.", 2f); yield return new WaitForSeconds(2.4f);
        if (fp) fp.DatTuThe(choCuaTrong, choCuaTrong + Vector3.up * 1.4f + huongCauThang * 3f);
        LocFirstPerson.PhuDe("Bế nó đi. Đi viện, đi đâu cũng được.", 2.6f); yield return new WaitForSeconds(2.6f + 3f);
        foreach (var c in new[] { "...Bố mẹ cũng đi.", "Đi buổi sáng, chiều thì người ta gọi về.", "Sổ bố viết. Ra khỏi cổng là hết.", "Ở đây thì còn được. Ở đây, với nó.", "Đến mùng một thôi." })
        { LocFirstPerson.PhuDe(c, 2.4f); yield return new WaitForSeconds(2.6f); }
        daHoangLoan = true;
        TatCanh(false);
    }
    IEnumerator Rung(float giay, float manh)
    {
        if (!cam) yield break;
        var r0 = cam.localRotation;
        for (float t = 0; t < giay; t += Time.deltaTime)
        { cam.localRotation = r0 * Quaternion.Euler(Random.Range(-manh, manh), Random.Range(-manh, manh), 0); yield return null; }
        cam.localRotation = r0;
    }

    // ───────── cuối Đêm 2: đi ngang cửa phòng đang hé
    public void CanhNuaTinh() { if (!dangCanh) StartCoroutine(NuaTinh()); }
    IEnumerator NuaTinh()
    {
        BatCanh();
        yield return Den(0.4f, 1);
        Ngu(0f);
        if (cua) cua.Mo();
        if (fp && head) fp.DatTuThe(choCuaNgoai, head.position);
        yield return Den(0.5f, 0);
        yield return new WaitForSeconds(1.2f);
        Clip("Nam_NuaTinh", 0.5f); thuc = true; tNham = 0; tHe = 70;
        yield return new WaitForSeconds(1.0f);
        yield return Noi("NHÍM", "Anh Khôi ơi... mai anh còn ở nhà không.", 1.4f, 0.3f);
        yield return Noi("KHÔI", "Còn. Ngủ đi.", 1f, 0.6f);
        Ngu(2f);
        yield return new WaitForSeconds(2.2f);
        yield return Den(0.8f, 1);              // cắt đen — Đêm 2 hết
        yield return new WaitForSeconds(1.5f);
        TatCanh(true);
    }

    // ───────── Good: sáng hôm sau — em ngồi dậy được, ở mép giường trong phòng mình (em không rời phòng)
    public void CanhGood() { if (!dangCanh) StartCoroutine(Good()); }
    IEnumerator Good()
    {
        BatCanh(); dangCanhKhongTho = true;
        yield return Den(0.6f, 1);
        Dat(neoGhe, yawGhe); DatSot(0);
        if (chan) chan.SetActive(false); if (chanVo) chanVo.SetActive(false); if (khanKho) khanKho.SetActive(false); if (khanUot) khanUot.SetActive(false);
        if (dayDeo) dayDeo.SetActive(true);
        if (cua) cua.Mo();
        thuc = true; tNham = 0; tHe = 0; tNhau = 0; tChat = 0; tCuoi = 85; Clip("Ngoi_NoiChuyen", 0f);
        if (fp && head) fp.DatTuThe(choBanAn, head.position);
        yield return new WaitForSeconds(0.4f);
        yield return Den(0.8f, 0);
        yield return new WaitForSeconds(0.6f);
        yield return Noi("NHÍM", "Anh Khôi ơi!", 1.2f, 3.5f);
        SauGood?.Invoke();   // điện thoại reo… (cảnh kết)
        yield return new WaitForSeconds(2f);
        yield return Den(0.8f, 1); yield return new WaitForSeconds(1f);
        TatCanh(true);
    }

    // ───────── Bad (đã chốt: A nối B) — không thoại, không thấy mặt
    public void CanhBad() { if (!dangCanh) StartCoroutine(Bad()); }
    IEnumerator Bad()
    {
        BatCanh(); dangCanhKhongTho = true;
        yield return Den(0.6f, 1);
        Dat(neoGiuong, yawGiuong); DatSot(3);
        thuc = false; tNham = 100; tHe = 0; tNhau = 0; tCuoi = 0; tChat = 0; nhinW = 0;
        Clip("Ngoi_DauGap", 0f);
        if (chan) chan.SetActive(false); if (chanVo) chanVo.SetActive(true);
        if (khanKho) khanKho.SetActive(false); if (khanUot) khanUot.SetActive(false);
        if (chau && matChauKho && chauGoc != null)   // chậu nước cạn khô: mặt nước → màu lòng chậu
        {
            var ms = (Material[])chauGoc.Clone();
            if (chauNuoc < ms.Length) ms[chauNuoc] = matChauKho;
            chau.sharedMaterials = ms;
        }
        if (cua) cua.Mo();
        if (denNgu) denNgu.enabled = true;
        if (fp) fp.DatTuThe(choCuaTrong, transform.position + Vector3.up * 0.55f);
        yield return new WaitForSeconds(0.6f);
        yield return Den(1.0f, 0);
        yield return new WaitForSeconds(2.2f);
        yield return NhayDen(2);
        yield return new WaitForSeconds(2.5f);
        // cắt đen nửa giây, điện chập
        LocFirstPerson.den = 1; if (dienChap) am.PlayOneShot(dienChap, 0.9f);
        yield return new WaitForSeconds(0.5f);
        Dat(neoGoc, yawGoc); Clip("Dung_DauGap", 0f);
        if (fp) fp.DatTuThe(choCuaTrong, neoGoc + Vector3.up * 1.0f);
        yield return null;
        LocFirstPerson.den = 0;
        yield return new WaitForSeconds(1.6f);
        // đèn ngủ nháy — trong lúc tối, em đã đứng sát tường hơn
        if (denNgu) denNgu.enabled = false; LocFirstPerson.den = 0.75f;
        yield return new WaitForSeconds(0.18f);
        Dat(neoGoc2, yawGoc);
        yield return new WaitForSeconds(0.12f);
        if (denNgu) denNgu.enabled = true; LocFirstPerson.den = 0;
        yield return new WaitForSeconds(0.08f);
        if (denNgu) denNgu.enabled = false; yield return new WaitForSeconds(0.1f); if (denNgu) denNgu.enabled = true;
        yield return new WaitForSeconds(2.4f);
        LocFirstPerson.den = 1;
        SauBad?.Invoke();    // → cảnh hầm: chén số 5 vỡ (hệ thống kết)
        yield return new WaitForSeconds(1.5f);
        TatCanh(true);
    }
    IEnumerator NhayDen(int lan)
    {
        if (!denNgu) yield break;
        for (int i = 0; i < lan; i++) { denNgu.enabled = false; yield return new WaitForSeconds(0.09f); denNgu.enabled = true; yield return new WaitForSeconds(0.14f); }
    }

    // ───────── Evil: tiếng gọi từ trên gác, hai lần
    public void GoiTuTrenGac() { StartCoroutine(Evil()); }
    IEnumerator Evil()
    {
        LocFirstPerson.PhuDe("NHÍM: Anh Khôi ơi...", 2.6f, false); yield return new WaitForSeconds(6f);
        LocFirstPerson.PhuDe("NHÍM: Anh Khôi ơi...", 2.6f, false);
    }

    // ───────── khoá/mở cảnh
    void BatCanh() { dangCanh = true; LocFirstPerson.khoa = true; if (coTham != null) StopCoroutine(coTham); dangThoai = false; choThay = false; }
    void TatCanh(bool veDem)
    {
        if (veDem) { dangCanhKhongTho = false; ApDem(LocMa.dem); }
        LocFirstPerson.den = 0; LocFirstPerson.khoa = false; dangCanh = false;
    }

    // ───────── phím thử
    void PhimThu()
    {
        var k = Keyboard.current; if (k == null || dangCanh) return;
        if (k.f7Key.wasPressedThisFrame)
        {
            int d = Mathf.Clamp(LocMa.dem, 1, 3); daTham[d] = false; daQuayLai[d] = false; StopAllCoroutines(); dangThoai = false; choThay = false;
            ApDem(LocMa.dem);
            if (fp && head) fp.DatTuThe(choCuaTrong, head.position);
            Bao($"thăm Nhím — Đêm {d}");
        }
        if (k.f8Key.wasPressedThisFrame) { Bao("cảnh nửa tỉnh cuối Đêm 2"); CanhNuaTinh(); }
        if (k.f9Key.wasPressedThisFrame) { Bao("Good — mép giường"); CanhGood(); }
        if (k.f10Key.wasPressedThisFrame) { Bao("Bad — A rồi B"); CanhBad(); }
        if (k.f11Key.wasPressedThisFrame) { Bao("hoảng loạn Đêm 2"); CanhHoangLoan(); }
        if (k.f12Key.wasPressedThisFrame) { Bao("Evil — gọi từ trên gác"); GoiTuTrenGac(); }
    }
    void Bao(string s) { dbg = s; dbgHet = Time.time + 3f; }
    void OnGUI()
    {
        if (!hienDebug) return;
        var st = new GUIStyle(GUI.skin.label) { fontSize = 13 }; st.normal.textColor = new Color(1, 1, 1, 0.55f);
        int d = Mathf.Clamp(LocMa.dem, 1, 3);
        string s = $"[thử Nhím] F7 thăm · F8 nửa tỉnh · F9 Good · F10 Bad · F11 hoảng loạn · F12 Evil · đã thăm Đ{d}: {(daTham[d] ? "rồi" : "chưa")}";
        if (Time.time < dbgHet) s += "  → " + dbg;
        GUI.Label(new Rect(10, 44, 1000, 20), s, st);
    }

    // ───────── tự kiểm (menu LOC → Tự kiểm Nhím): chạy lần lượt mọi cảnh, chụp từng nhịp → LOC_KiemTra/Nhim/ + BaoCao_TuKiem.txt
    static System.Text.StringBuilder nk;
    static void Ghi(string s) { if (nk != null) nk.AppendLine($"{Time.time,7:F1}s  {s}"); }
#if UNITY_EDITOR
    int soAnh;
    void Chup(string ten)
    {
        var c = cam ? cam.GetComponent<Camera>() : Camera.main; if (!c) return;
        const int W = 960, H = 540;
        var rt = new RenderTexture(W, H, 24); var old = c.targetTexture; c.targetTexture = rt; c.Render();
        RenderTexture.active = rt; var t = new Texture2D(W, H, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, W, H), 0, 0);
        // phủ màn đen như LocFirstPerson (để thấy cắt đen trong ảnh)
        if (LocFirstPerson.den > 0.01f) { var px = t.GetPixels(); for (int i = 0; i < px.Length; i++) px[i] *= 1 - LocFirstPerson.den; t.SetPixels(px); }
        t.Apply();
        System.IO.File.WriteAllBytes($"LOC_KiemTra/Nhim/{soAnh++:00}_{ten}.png", t.EncodeToPNG());
        RenderTexture.active = null; c.targetTexture = old; Destroy(t); rt.Release();
        Ghi($"ảnh {soAnh - 1:00}_{ten} · clip {TenClip()} · nhắm {bsNham:F0} hé {bsHe:F0} miệng {bsMieng:F0} cười {bsCuoi:F0} · nhìn {nhinW:F2}");
    }
    string TenClip()
    {
        if (!anim) return "-";
        var ci = anim.GetCurrentAnimatorClipInfo(0); return ci.Length > 0 ? ci[0].clip.name : "-";
    }
    IEnumerator ChupLien(string ten, float giay, float buoc)
    { for (float t = 0; t < giay; t += buoc) { Chup(ten); yield return new WaitForSeconds(buoc); } }

    IEnumerator TuKiem()
    {
        nk = new System.Text.StringBuilder($"TỰ KIỂM NHÍM {System.DateTime.Now:dd/MM HH:mm:ss}\n");
        System.IO.Directory.CreateDirectory("LOC_KiemTra/Nhim");
        foreach (var f in System.IO.Directory.GetFiles("LOC_KiemTra/Nhim", "*.png")) try { System.IO.File.Delete(f); } catch { }
        yield return new WaitForSeconds(1f);
        Ghi($"blendshape: nhắm {iNham} hé {iHe} miệng {iMieng} cười {iCuoi} nhíu {iNhau} chặt {iChat} · mặt cục bộ {faceLocal}");
        for (int d = 1; d <= 3; d++)
        {
            LocMa.dem = d; LocMa.huDaHe = false; LocMa.maTucGian = false;
            yield return null; yield return null;
            Ghi($"── ĐÊM {d}");
            if (fp) fp.DatTuThe(choCuaTrong, neoGiuong + Vector3.up * 0.35f);
            yield return new WaitForSeconds(1.0f);
            Chup($"d{d}_cua");
            if (fp) fp.DatTuThe(choThayKhan + Vector3.left * 0.5f, head.position);
            float t0 = Time.time;
            while (!daTham[d] && Time.time - t0 < 60f)
            {
                if (choThay && !dangThayKhan) { Chup($"d{d}_truocThay"); ThayKhan(); yield return new WaitForSeconds(1.0f); Chup($"d{d}_dangThay"); }
                if ((int)((Time.time - t0) * 1.0f) % 3 == 0) Chup($"d{d}_tham");
                yield return new WaitForSeconds(1f);
            }
            Ghi(daTham[d] ? $"đêm {d}: thăm xong ({Time.time - t0:F0}s), thay khăn: {daThayKhan[d]}" : $"đêm {d}: LỖI — thăm không kết thúc");
            if (fp) fp.DatTuThe(choThayKhan + new Vector3(-0.4f, 0, 0.6f), head.position); yield return new WaitForSeconds(1.5f); Chup($"d{d}_ngu");
            // quay lại phòng
            if (fp) fp.DatTuThe(choCuaNgoai, head.position); yield return new WaitForSeconds(1.0f);
            if (fp) fp.DatTuThe(choCuaTrong, head.position); yield return new WaitForSeconds(5f); Chup($"d{d}_quayLai");
        }
        LocMa.dem = 2; yield return null; yield return null;
        Ghi("── HOẢNG LOẠN"); CanhHoangLoan(); yield return null; while (dangCanh) { yield return ChupLien("hoangloan", 3f, 1.5f); }
        Ghi("── NỬA TỈNH"); CanhNuaTinh(); yield return null; while (dangCanh) { yield return ChupLien("nuatinh", 2f, 1f); }
        LocMa.dem = 3; yield return null; yield return null;
        Ghi("── GOOD"); CanhGood(); yield return null; while (dangCanh) { yield return ChupLien("good", 2f, 1f); }
        Ghi("── BAD"); CanhBad(); yield return null; while (dangCanh) { yield return ChupLien("bad", 1f, 0.5f); }
        Ghi("── EVIL"); GoiTuTrenGac(); yield return new WaitForSeconds(7f);
        // các clip: chụp cận từng tư thế
        if (fp) fp.DatTuThe(choThayKhan, head.position);
        foreach (var c in new[] { "Nam_Ngu", "Nam_Thuc", "Nam_NgocDau", "Nam_NgheDau", "Nam_So", "Nam_NuaTinh", "Nam_TroMinh", "Nam_ThayKhan" })
        { Clip(c, 0f, 1f); yield return new WaitForSeconds(1.6f); Chup("clip_" + c); }
        System.IO.File.WriteAllText("LOC_KiemTra/Nhim/BaoCao_TuKiem.txt", nk.ToString()); nk = null;
        UnityEditor.EditorApplication.isPlaying = false;
    }
#endif
}
