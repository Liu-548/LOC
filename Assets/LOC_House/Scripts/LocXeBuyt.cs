using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// [25/10] Đoạn mở đầu trên xe buýt — sau thẻ "ĐÊM 1", trước khi Khôi đứng ở cổng (thay chỗ dịch chuyển thẳng ra cổng của LocCotTruyen.DauDem).
//  · Chỉ chạy khi vào Đêm 1 từ đầu (Bắt đầu / Chọn đêm 1). "Tiếp tục" và chơi lại đầu đêm sau khi chết thì không chạy.
//  · Khôi ngồi ghế cạnh cửa sổ bên phải (phía cửa xuống), bị khoá trên ghế, chỉ quay đầu nhìn (chuột, giới hạn góc).
//  · Tuyến: phố → đường ven sông có bến → rẽ phải vào phố nhà → dừng ngay trước cổng (cửa giữa đúng x 3,8). Đường + tốc độ + mốc thoại do LocKhuPho_TuyenXe tính sẵn.
//  · Xuống xe góc nhìn thứ nhất: phanh hơi → mở cửa → bác tài hỏi → đứng dậy → ra cửa → hai bậc → vỉa hè, nhìn cổng → trả điều khiển. Xe đóng cửa, chạy đi.
//  · Giữ Space để bỏ qua (giai đoạn phát triển, giống nút skip của mini game).
//  · Độc thoại theo C1.2: chữ nghiêng, không tên. Thoại nói ra (bác tài, Khôi): chữ thẳng, có tên.
public class LocXeBuyt : MonoBehaviour
{
    public static bool choChay, dangChay;
    public static LocXeBuyt I;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset0() { choChay = dangChay = false; }

    [Header("Đường chạy (thế giới) — LocKhuPho_TuyenXe tính")]
    public Vector3[] duong;
    public float[] toc;      // tốc độ đích ở từng điểm (m/s)
    public float[] moc;      // quãng đường (m) tới các mốc thoại
    public int iDung;        // điểm dừng: cửa giữa ngay trước cổng
    [Header("Xe")]
    public Transform ghe;                    // chỗ chân Khôi khi ngồi (cục bộ của xe)
    public Transform canhTruoc, canhSau;     // hai cánh cửa giữa, xoay quanh bản lề
    public Light[] denTrong;
    public AudioClip dongCo, cuaMo, cuaDong, phanh, coTrung, buoc;
    [Header("Xuống xe")]
    public Vector3 xuongDung = new(3.8f, -0.15f, -6.3f), xuongNhin = new(3.8f, 1.4f, -3f);   // = LocCotTruyen.dem1Dung/dem1Nhin (cốt vỉa hè −0,15)

    const float MAT_NGOI = 1.2f;   // mắt khi ngồi, tính từ sàn xe
    static readonly string[][] DOC_THOAI =
    {
        new[] { "Chuyến cuối.", "Cả xe có mỗi mình." },
        new[] { "Phố vẫn thế. Chẳng khác gì." },
        new[] { "Mẹ gọi lên ký túc mấy lần.", "Toàn bảo bận.", "...Bận cái gì đâu." },
        new[] { "Bến sông." },
        new[] { "Anh Phong đi tắm ở khúc này.", "...Thôi. Không nghĩ nữa." },
        new[] { "Lúc đi còn cãi nhau với bố một trận.", "Câu cuối nói với bố là gì nhỉ.", "...Không nhớ nổi." },
        new[] { "Con Nhím chắc lớn lắm rồi." },
        new[] { "Rạp dựng rồi." },
    };

    float[] sAt; float s, v, yaw, pitch, camY, chongNgu, rung, nghieng, ngoc, giuSpace; bool boQua;
    AudioSource amMay, amDem, amCua;
    LocFirstPerson fp; Transform cha0;
    float[] denGoc;

    void Awake()
    {
        I = this;
        sAt = new float[duong.Length];
        for (int i = 1; i < duong.Length; i++) sAt[i] = sAt[i - 1] + Vector3.Distance(duong[i - 1], duong[i]);
        denGoc = new float[denTrong.Length]; for (int i = 0; i < denTrong.Length; i++) denGoc[i] = denTrong[i].intensity;
        DatXe(0, 0, 0);
#if UNITY_EDITOR
        if (UnityEditor.EditorPrefs.GetBool("LOC_ThuXeBuyt", false)) { choChay = true; thu = true; }
#endif
    }
    void Start() { if (!choChay) gameObject.SetActive(false); }   // không có cảnh mở đầu: xe không có mặt (menu, Tiếp tục, chơi lại đêm)

    // ───── vị trí trên đường theo quãng đường s
    Vector3 P(float x)
    {
        x = Mathf.Clamp(x, 0, sAt[^1]);
        int lo = 0, hi = sAt.Length - 1;
        while (hi - lo > 1) { int m = (lo + hi) / 2; if (sAt[m] <= x) lo = m; else hi = m; }
        float d = sAt[hi] - sAt[lo];
        return Vector3.Lerp(duong[lo], duong[hi], d > 1e-4f ? (x - sAt[lo]) / d : 0);
    }
    float TocDich(float x)
    {
        int i = 0; while (i < sAt.Length - 1 && sAt[i + 1] < x) i++;
        return toc[Mathf.Min(i, toc.Length - 1)];
    }
    Quaternion Huong(float x) { var d = P(x + 2.5f) - P(x - 2.5f); d.y = 0; return d.sqrMagnitude > 1e-4f ? Quaternion.LookRotation(d) : transform.rotation; }
    void DatXe(float x, float chuiDau, float lac)   // chuiDau: phanh thì chúi mũi; lac: vào cua thì nghiêng ra ngoài
    {
        transform.SetPositionAndRotation(P(x), Huong(x) * Quaternion.Euler(chuiDau, 0, lac));
    }

    // ───── cảnh chính (LocCotTruyen.DauDem gọi khi Đêm 1 bắt đầu và choChay)
    public IEnumerator Chay(LocFirstPerson f)
    {
        choChay = false; dangChay = true; fp = f;
        var p = fp.transform; var cam = fp.cam;
        var cc = p.GetComponent<CharacterController>(); if (cc) cc.enabled = false;
        cha0 = p.parent;
        p.SetParent(transform, false); p.localPosition = ghe.localPosition; p.localRotation = Quaternion.identity;
        yaw = 22; pitch = -3; camY = MAT_NGOI; s = 0; v = toc[0];
        gameObject.SetActive(true); DatXe(0, 0, 0); Cam();
        amMay = Nguon(dongCo, 0.5f, true, false); amDem = Nguon(coTrung, 0f, true, false); amCua = Nguon(null, 0.8f, false, false);

        while (LocMenu.DangChe) { Khoa(); Nhin(); Cam(); yield return null; }   // thẻ "ĐÊM 1" che màn: xe đứng chờ ở đầu tuyến
        Chup("00_batdau_nhin_truoc", 0.6f);

        // ── chạy
        float sDung = sAt[iDung]; int mocKe = 0; float t0 = Time.time, vTruoc = v, gocTruoc = transform.eulerAngles.y;
        while (s < sDung - 0.02f)
        {
            if (DaBoQua()) yield break;
            Khoa();
            float dt = Time.deltaTime;
            float dich = Mathf.Min(TocDich(s), Mathf.Sqrt(2f * 1.3f * Mathf.Max(0, sDung - s)) + 0.25f);
            v = Mathf.MoveTowards(v, dich, (dich > v ? 1.1f : 1.8f) * dt);
            s = Mathf.Min(s + v * dt, sDung);
            float gia = dt > 0 ? (v - vTruoc) / dt : 0; vTruoc = v;
            float goc = transform.eulerAngles.y, quay = dt > 0 ? Mathf.DeltaAngle(gocTruoc, goc) / dt : 0; gocTruoc = goc;
            chongNgu = Mathf.Lerp(chongNgu, Mathf.Clamp(-gia * 0.55f, -1.4f, 1.6f), dt * 3f);
            nghieng = Mathf.Lerp(nghieng, Mathf.Clamp(quay * v * 0.012f, -2.4f, 2.4f), dt * 3f);
            DatXe(s, chongNgu, nghieng);
            while (mocKe < moc.Length && s >= moc[mocKe]) { if (mocKe < DOC_THOAI.Length) LocTui.Noi(DOC_THOAI[mocKe]); ChupMoc(mocKe); mocKe++; }
            if (amMay) amMay.pitch = 0.72f + 0.42f * Mathf.Clamp01(v / 10f) + Mathf.Max(0, gia) * 0.04f;
            if (amDem) amDem.volume = Mathf.MoveTowards(amDem.volume, GanSong() ? 0.32f : 0.06f, dt * 0.25f);
            NhayDen();
            Nhin(); Rung(dt); Cam();
            yield return null;
        }
        s = sDung; v = 0;
        if (thu) Log($"chạy {Time.time - t0:0.0} s, {sDung:0} m");

        // ── dừng: phanh hơi, mũi xe nhún lại
        Choi(phanh, 0.7f);
        for (float t = 0; t < 0.7f; t += Time.deltaTime)
        {
            if (DaBoQua()) yield break;
            Khoa(); chongNgu = Mathf.Lerp(1.3f, 0, t / 0.7f) * Mathf.Cos(t * 9f); DatXe(s, chongNgu, 0);
            if (amMay) amMay.pitch = Mathf.Lerp(amMay.pitch, 0.66f, Time.deltaTime * 3);
            Nhin(); Rung(Time.deltaTime); Cam(); yield return null;
        }
        DatXe(s, 0, 0);
        float tXuong = Time.time;
        StartCoroutine(Cua(true));
        yield return Doi(0.9f, true);
        Chup("04_dung_cua_mo", 0f);

        // ── bác tài hỏi, mắt tự quay về phía cửa
        var thoai = new (string ai, string cau)[] { ("BÁC TÀI", "Nhà có việc hả cháu?"), ("KHÔI", "Dạ."), ("BÁC TÀI", "Ừ. Xuống cẩn thận.") };
        float yaw0 = yaw, pitch0 = pitch; int k = 0; float tc = 0, hetCau = 0;
        for (float t = 0; k < thoai.Length || t < hetCau + 0.2f; t += Time.deltaTime)
        {
            if (DaBoQua()) yield break;
            Khoa();
            float a = Mathf.SmoothStep(0, 1, t / 1.4f);
            yaw = Mathf.LerpAngle(yaw0, 28f, a); pitch = Mathf.Lerp(pitch0, -9f, a);
            if (k < thoai.Length && t >= tc)
            {
                float d = 1.2f + 0.06f * thoai[k].cau.Length;
                LocFirstPerson.PhuDe($"{thoai[k].ai}: {thoai[k].cau}", d, false);
                tc = t + d * LocFirstPerson.nhanChu + 0.4f + (k == 0 ? 0.5f : 0); hetCau = tc; k++;
            }
            Rung(Time.deltaTime * 0.2f); Cam(); yield return null;
        }

        // ── xuống xe (toạ độ cục bộ của xe: sàn 0,75 · bậc 0,52 · bậc 0,30 · vỉa hè 0,15). Mắt đứng 1,52 như LocFirstPerson.
        var cuoi = transform.InverseTransformPoint(xuongDung);
        var nhinCuoi = transform.InverseTransformDirection(xuongNhin - (xuongDung + Vector3.up * 1.52f));
        float yawCuoi = Mathf.Atan2(nhinCuoi.x, nhinCuoi.z) * Mathf.Rad2Deg, pitchCuoi = -Mathf.Atan2(nhinCuoi.y, new Vector2(nhinCuoi.x, nhinCuoi.z).magnitude) * Mathf.Rad2Deg;
        var buocDi = new (Vector3 chan, float mat, float yaw, float pitch, float giay, bool buoc)[]
        {
            (new Vector3(0.88f, 0.75f, -1.0f), 1.52f, 25f, -14f, 1.15f, false),   // đứng dậy khỏi ghế
            (new Vector3(0.40f, 0.75f, -0.25f), 1.52f, 78f, -20f, 0.95f, true),   // ra mép cửa
            (new Vector3(0.78f, 0.52f, 0.0f), 1.52f, 90f, -16f, 0.55f, true),     // bậc trên
            (new Vector3(1.06f, 0.30f, 0.0f), 1.52f, 90f, -8f, 0.5f, true),       // bậc dưới
            (new Vector3(1.75f, cuoi.y, 0.0f), 1.52f, yawCuoi, -3f, 0.6f, true),  // xuống vỉa hè
            (new Vector3(cuoi.x, cuoi.y, cuoi.z), 1.52f, yawCuoi, pitchCuoi, 0.75f, true),
        };
        for (int i = 0; i < buocDi.Length; i++)
        {
            var (chan, mat, yw, pt, giay, coBuoc) = buocDi[i];
            Vector3 c0 = p.localPosition; float m0 = camY, y0 = yaw, q0 = pitch;
            if (coBuoc) Choi(buoc, 0.55f, Random.Range(0.9f, 1.08f));
            for (float t = 0; t < 1; t += Time.deltaTime / giay)
            {
                if (DaBoQua()) yield break;
                Khoa();
                float a = Mathf.SmoothStep(0, 1, t);
                p.localPosition = Vector3.Lerp(c0, chan, a);
                camY = Mathf.Lerp(m0, mat, a) - (coBuoc ? Mathf.Sin(t * Mathf.PI) * 0.045f : 0);
                yaw = Mathf.LerpAngle(y0, yw, a); pitch = Mathf.Lerp(q0, pt, a);
                Cam(); yield return null;
            }
            if (i == 3) Chup("05_buoc_xuong", 0f);
        }
        p.localPosition = cuoi; camY = 1.52f; yaw = yawCuoi; pitch = pitchCuoi; Cam();
        if (thu) Log($"xuống xe {Time.time - tXuong:0.0} s");
        TraNguoi();
        StartCoroutine(ChayDi());
        if (thu) StartCoroutine(ThuNhinXe());
    }

    // trả người chơi về đúng tư thế LocFirstPerson đang giữ — không giật hình
    void TraNguoi()
    {
        var p = fp.transform;
        p.SetParent(cha0, true);
        fp.cam.localPosition = new Vector3(0, 1.52f, 0);
        fp.DatTuThe(xuongDung, xuongNhin);
        LocFirstPerson.khoa = false; LocMa.tamDung = false; dangChay = false;
        if (amMay) { amMay.spatialBlend = 1; amMay.minDistance = 3; amMay.maxDistance = 70; amMay.rolloffMode = AudioRolloffMode.Linear; }
        if (amDem) StartCoroutine(Nho(amDem, 2.5f));
    }

    IEnumerator ChayDi()
    {
        yield return new WaitForSeconds(0.9f);
        yield return Cua(false);
        yield return new WaitForSeconds(0.35f);
        while (s < sAt[^1] - 0.5f)
        {
            v = Mathf.MoveTowards(v, toc[^1], 1.0f * Time.deltaTime);
            s += v * Time.deltaTime;
            DatXe(s, Mathf.Lerp(-0.8f, 0, v / 3f), 0);
            if (amMay) amMay.pitch = 0.7f + 0.4f * Mathf.Clamp01(v / 8f);
            yield return null;
        }
        gameObject.SetActive(false);
    }

    // ───── cửa giữa: hai cánh xoay vào trong 85°
    IEnumerator Cua(bool mo)
    {
        Choi(mo ? cuaMo : cuaDong, 0.75f);
        for (float t = 0; t < 1; t += Time.deltaTime / 0.9f)
        {
            float a = Mathf.SmoothStep(0, 1, mo ? t : 1 - t) * 85f;
            if (canhTruoc) canhTruoc.localRotation = Quaternion.Euler(0, a, 0);
            if (canhSau) canhSau.localRotation = Quaternion.Euler(0, -a, 0);
            yield return null;
        }
        if (canhTruoc) canhTruoc.localRotation = Quaternion.Euler(0, mo ? 85 : 0, 0);
        if (canhSau) canhSau.localRotation = Quaternion.Euler(0, mo ? -85 : 0, 0);
    }

    // ───── điều khiển trong lúc ngồi
    void Khoa()
    {
        LocFirstPerson.khoa = true; LocMa.tamDung = true;
        var ms = Mouse.current;
        if (ms != null && ms.leftButton.wasPressedThisFrame) Cursor.lockState = CursorLockMode.Locked;
        if (!LocMenu.DangChe && Cursor.lockState == CursorLockMode.None && !daKhoaChuot) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; daKhoaChuot = true; }
    }
    bool daKhoaChuot;

    void Nhin()
    {
#if UNITY_EDITOR
        if (thu) { NhinThu(); return; }
#endif
        var ms = Mouse.current;
        if (ms == null || Cursor.lockState != CursorLockMode.Locked || LocMenu.DangChe) return;
        var d = ms.delta.ReadValue() * fp.look;
        yaw = Mathf.Clamp(yaw + d.x, -100f, 125f);
        pitch = Mathf.Clamp(pitch - (LocFirstPerson.daoTruc ? -d.y : d.y), -45f, 32f);
    }

    // xe rung theo tốc độ + thỉnh thoảng một cú xóc ổ gà
    void Rung(float dt)
    {
        float t = Time.time, a = 0.0035f + 0.0045f * Mathf.Clamp01(v / 9f);
        if (v > 3f && Random.value < dt * 0.25f) ngoc = 0.028f;
        ngoc = Mathf.MoveTowards(ngoc, 0, dt * 0.12f);
        rung = (Mathf.PerlinNoise(t * 9f, 0.3f) - 0.5f) * 2f * a - ngoc * Mathf.Abs(Mathf.Sin(t * 40f));
    }
    void Cam()
    {
        fp.transform.localRotation = Quaternion.Euler(0, yaw, 0);
        fp.cam.localPosition = new Vector3(0, camY + rung, 0);
        fp.cam.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    bool GanSong() => moc.Length > 5 && s > moc[3] - 50f && s < moc[5] + 10f;

    // một bóng tuýp thỉnh thoảng chập chờn
    float tNhay;
    void NhayDen()
    {
        if (denTrong.Length < 2) return;
        if (tNhay <= 0 && Random.value < Time.deltaTime * 0.12f) tNhay = Random.Range(0.25f, 0.7f);
        tNhay -= Time.deltaTime;
        denTrong[1].intensity = denGoc[1] * (tNhay > 0 ? (Random.value < 0.5f ? 0.15f : 0.9f) : 1f);
    }

    // ───── bỏ qua: giữ Space 0,8 s
    bool DaBoQua()
    {
        if (boQua) return true;
        var k = Keyboard.current;
        giuSpace = k != null && k.spaceKey.isPressed && !LocMenu.DangChe ? giuSpace + Time.deltaTime : 0;
        if (giuSpace < 0.8f) return false;
        boQua = true; StartCoroutine(BoQua());
        return true;
    }
    IEnumerator BoQua()
    {
        for (float t = 0; t < 1; t += Time.deltaTime / 0.35f) { LocFirstPerson.den = t; yield return null; }
        LocFirstPerson.den = 1; LocFirstPerson.PhuDe("", 0.01f); LocTui.Noi("");
        if (canhTruoc) canhTruoc.localRotation = Quaternion.identity;
        if (canhSau) canhSau.localRotation = Quaternion.identity;
        TraNguoi();
        LocFirstPerson.khoa = true;
        if (amMay) amMay.Stop();
        gameObject.SetActive(false);   // coroutine này đang chạy trên xe → phần còn lại để LocFirstPerson (bật lại) lo
    }
    void OnDisable()
    {
        if (!boQua || !fp) return;
        fp.StartCoroutine(SangLai());
        boQua = false;
    }
    static IEnumerator SangLai()
    {
        yield return new WaitForSeconds(0.25f);
        for (float t = 1; t > 0; t -= Time.deltaTime / 0.6f) { LocFirstPerson.den = t; yield return null; }
        LocFirstPerson.den = 0; LocFirstPerson.khoa = false;
    }

    void OnGUI()
    {
        if (!dangChay || LocMenu.DangChe || boQua) return;
        float k = Screen.height / 1080f;
        var st = new GUIStyle(GUI.skin.label) { font = LocMenu.FontChu(1), fontSize = Mathf.RoundToInt(19 * k), alignment = TextAnchor.LowerRight };
        st.normal.textColor = new Color(0.75f, 0.72f, 0.66f, 0.38f + 0.5f * Mathf.Clamp01(giuSpace / 0.8f));
        var r = new Rect(0, 0, Screen.width - 36 * k, Screen.height - 30 * k);
        GUI.Label(r, "Giữ Space để bỏ qua", st);
        if (giuSpace > 0) { GUI.color = new Color(0.75f, 0.72f, 0.66f, 0.7f); GUI.DrawTexture(new Rect(Screen.width - 36 * k - 170 * k * Mathf.Clamp01(giuSpace / 0.8f), Screen.height - 26 * k, 170 * k * Mathf.Clamp01(giuSpace / 0.8f), 2 * k), Texture2D.whiteTexture); GUI.color = Color.white; }
    }

    // ───── âm thanh
    AudioSource Nguon(AudioClip c, float vol, bool lap, bool khong)
    {
        var a = gameObject.AddComponent<AudioSource>(); a.clip = c; a.volume = vol; a.loop = lap; a.playOnAwake = false;
        a.spatialBlend = khong ? 1 : 0; a.dopplerLevel = 0;
        if (c && lap) a.Play(); return a;
    }
    void Choi(AudioClip c, float vol, float cao = 1f) { if (!c || !amCua) return; amCua.pitch = cao; amCua.PlayOneShot(c, vol); }
    static IEnumerator Nho(AudioSource a, float giay)
    {
        float v0 = a.volume;
        for (float t = 0; t < 1 && a; t += Time.deltaTime / giay) { a.volume = v0 * (1 - t); yield return null; }
        if (a) a.Stop();
    }
    IEnumerator Doi(float giay, bool rungXe)
    {
        for (float t = 0; t < giay; t += Time.deltaTime) { if (DaBoQua()) yield break; Khoa(); Nhin(); if (rungXe) Rung(Time.deltaTime * 0.3f); Cam(); yield return null; }
    }

    // ───── thử (Editor): LOC → Thử xe buýt mở đầu — tự quay đầu theo kịch bản, chụp ảnh vào LOC_KiemTra/XeBuyt/, xong tự thoát Play
    bool thu;
    void Log(string s) { System.IO.Directory.CreateDirectory("LOC_KiemTra/XeBuyt"); System.IO.File.AppendAllText("LOC_KiemTra/XeBuyt/log.txt", $"{System.DateTime.Now:HH:mm:ss} {s}\n"); }
    void Chup(string ten, float tre) { if (thu) StartCoroutine(ChupSau(ten, tre)); }
    IEnumerator ChupSau(string ten, float tre)
    {
        yield return new WaitForSeconds(tre);
        System.IO.Directory.CreateDirectory("LOC_KiemTra/XeBuyt");
        ScreenCapture.CaptureScreenshot($"LOC_KiemTra/XeBuyt/{ten}.png");
        Log($"chụp {ten}  s={s:0} v={v:0.0} xe=({transform.position.x:0.0},{transform.position.z:0.0})");
    }
    static readonly string[] TEN_MOC = { "01_pho", "01b_pho_cuaso", "02_pho_xa", "03_ben_song", "03b_qua_ben", "03c_cuoi_song", "06_re_pho_nha", "07_thay_rap" };
    void ChupMoc(int i) { if (thu && i < TEN_MOC.Length) Chup(TEN_MOC[i], i == 3 ? 2.6f : 0.8f); }
#if UNITY_EDITOR
    // hướng nhìn khi thử: nhìn trước lúc đầu, nhìn cửa sổ phải ở phố/bến/rạp, nhìn ra trước ở đoạn rẽ
    void NhinThu()
    {
        float yd = 0, pd = -3;
        if (moc.Length > 7)
        {
            if (s > moc[0] + 15 && s < moc[2]) yd = 75;
            else if (s >= moc[3] - 20 && s < moc[5]) yd = 82;
            else if (s >= moc[7] - 25) yd = 70;
        }
        yaw = Mathf.MoveTowardsAngle(yaw, yd, Time.deltaTime * 55f); pitch = Mathf.MoveTowards(pitch, pd, Time.deltaTime * 20f);
    }
    IEnumerator ThuNhinXe()
    {
        yield return new WaitForSeconds(0.5f);
        Chup("08_ve_cong", 0f);
        yield return new WaitForSeconds(1.3f);
        if (fp) fp.DatTuThe(fp.transform.position, transform.position + Vector3.up * 1.4f);   // quay lại nhìn xe đóng cửa, chạy đi
        Chup("09_xe_chay_di", 0.4f);
        yield return new WaitForSeconds(2.2f);
        Chup("10_xe_xa_dan", 0f);
        yield return new WaitForSeconds(1.0f);
        Log("xong");
        UnityEditor.EditorPrefs.SetBool("LOC_ThuXeBuyt", false);
        UnityEditor.EditorApplication.isPlaying = false;
    }
#else
    void NhinThu() { }
    IEnumerator ThuNhinXe() { yield break; }
#endif
}
