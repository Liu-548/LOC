using UnityEngine;

// [23/10] Radio cassette phòng khách tầng 1 (RadioCassette_2Loa): nhấn E để bật / tắt.
//  · Nhạc: Resources/LOC_Radio_Nhac (bản nhạc người dùng đưa) — phát lặp, tắt rồi bật lại thì chạy tiếp chỗ cũ
//  · Âm thanh 3D: đứng gần nghe to, đi xa nhỏ dần (tắt hẳn ngoài ~20 m); cách tường thì nghe ồm (lọc bớt tiếng cao)
//  · Tiếng loa radio cũ: cắt bớt trầm/cao + rè nhẹ
//  · LocMa dùng: KeoBang() (băng kéo chậm, méo rồi tắt), Re() (xẹt nhiễu chói), khoa (ma đang “giữ” radio)
// Tự gắn vào vật tên RadioCassette_2Loa khi vào cảnh — không cần sửa scene.
public class LocRadio : MonoBehaviour
{
    public static LocRadio I;
    public string ten = "radio";
    [Range(0f, 1f)] public float amLuong = 0.85f;
    public float nghe1 = 1.5f, ngheToiDa = 20f;   // to nhất trong 1,5 m · im hẳn ở 20 m
    [System.NonSerialized] public bool khoa;     // true: không bật/tắt được (ma đang ở cạnh)

    public bool DangBat { get; private set; }
    public AudioSource Nguon => nhac;   // tự kiểm đọc
    public float BatDuoc => DangBat ? Time.time - batLuc : 0;   // đã phát liên tục bao lâu
    public Vector3 ViTri => tam;

    AudioSource nhac, re, tach; AudioLowPassFilter lpTuong; AudioClip clipTach, clipRe, clipXet;
    float batLuc, ongTuong = 1; AudioListener tai;
    Vector3 tam;

    // [25/10] gắn lại MỖI lần nạp cảnh: menu của nhóm (LocMenu) bấm "Chơi" / "Chơi lại" là nạp lại cả cảnh,
    //  radio cũ bị huỷ theo — trước đây chỉ gắn một lần lúc bấm Play nên vào game từ menu là radio câm.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void DangKy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= KhiNapCanh;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += KhiNapCanh;
    }
    static void KhiNapCanh(UnityEngine.SceneManagement.Scene sc, UnityEngine.SceneManagement.LoadSceneMode m) => TuGan();
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void TuGan()
    {
        var g = GameObject.Find("RadioCassette_2Loa");
        if (!g) foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())   // phòng khi radio đang nằm trong nhóm bị ẩn
            if (t.name == "RadioCassette_2Loa" && t.gameObject.scene.IsValid()) { g = t.gameObject; break; }
        if (g && !g.GetComponent<LocRadio>()) g.AddComponent<LocRadio>();
        Ghi(g ? "gắn radio vào " + g.name : "KHÔNG thấy RadioCassette_2Loa trong cảnh");
    }
    // nhật ký nhỏ cho người làm (chỉ trong Unity Editor): D:\LOC-main\LOC_Radio_Log.txt
    static void Ghi(string s)
    {
#if UNITY_EDITOR
        try { System.IO.File.AppendAllText("LOC_Radio_Log.txt", $"{System.DateTime.Now:HH:mm:ss} {s}\n"); } catch { }
#endif
    }

    void Awake()
    {
        I = this;
        // hộp bao (để bấm E trúng và để đặt tiếng ở giữa radio)
        var rs = GetComponentsInChildren<Renderer>(); var b = new Bounds(transform.position, Vector3.one * 0.3f);
        for (int i = 0; i < rs.Length; i++) { if (i == 0) b = rs[i].bounds; else b.Encapsulate(rs[i].bounds); }
        tam = b.center;
        if (!GetComponentInChildren<Collider>())
        {
            var bc = gameObject.AddComponent<BoxCollider>();
            bc.center = transform.InverseTransformPoint(b.center);
            var s = transform.lossyScale; bc.size = new Vector3(b.size.x / Mathf.Max(1e-4f, Mathf.Abs(s.x)), b.size.y / Mathf.Max(1e-4f, Mathf.Abs(s.y)), b.size.z / Mathf.Max(1e-4f, Mathf.Abs(s.z)));
        }
        var loa = new GameObject("LocRadio_Loa"); loa.transform.SetParent(transform, false); loa.transform.position = tam;
        nhac = loa.AddComponent<AudioSource>(); nhac.clip = Resources.Load<AudioClip>("LOC_Radio_Nhac"); Loa3D(nhac); nhac.loop = true; nhac.volume = amLuong;
        re = loa.AddComponent<AudioSource>(); Loa3D(re); re.loop = true;
        tach = loa.AddComponent<AudioSource>(); Loa3D(tach);
        // tiếng loa radio cũ: cắt trầm + cắt bớt cao, thêm rè nhẹ
        var hp = loa.AddComponent<AudioHighPassFilter>(); hp.cutoffFrequency = 160f;
        lpTuong = loa.AddComponent<AudioLowPassFilter>(); lpTuong.cutoffFrequency = 7000f;
        clipTach = TaoClip("LOC_Radio_Tach", 0.09f, (t, r) => (N(r) * 0.6f + Mathf.Sin(2 * Mathf.PI * 2200 * t) * 0.3f) * Mathf.Exp(-t / 0.01f));
        float lp = 0;
        clipRe = TaoClip("LOC_Radio_Re", 2f, (t, r) => { lp += (N(r) - lp) * 0.35f; return lp * 0.5f; });
        float lp2 = 0;
        clipXet = TaoClip("LOC_Radio_Xet", 1.1f, (t, r) => { float x = N(r); lp2 += (x - lp2) * 0.6f; float am = Mathf.Sin(2 * Mathf.PI * 60 * t) > 0 ? 1 : 0.55f; return Mathf.Clamp((x - lp2 * 0.5f) * 1.6f * am, -1, 1) * Mathf.Clamp01((1.1f - t) * 6); });
        re.clip = clipRe; re.volume = 0.05f;
    }

    void Loa3D(AudioSource a)
    {
        a.playOnAwake = false; a.spatialBlend = 1f; a.dopplerLevel = 0; a.spread = 60;
        a.rolloffMode = AudioRolloffMode.Custom; a.minDistance = nghe1; a.maxDistance = ngheToiDa;
        // to trong 1,5 m → giảm dần → ~0 ở 20 m (gần như tai người nghe radio trong nhà)
        a.SetCustomCurve(AudioSourceCurveType.CustomRolloff, new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(nghe1 / ngheToiDa, 1f), new Keyframe(0.2f, 0.62f), new Keyframe(0.4f, 0.3f),
            new Keyframe(0.65f, 0.1f), new Keyframe(1f, 0f)));
    }

    static float N(System.Random r) => (float)(r.NextDouble() * 2 - 1);
    static AudioClip TaoClip(string ten, float giay, System.Func<float, System.Random, float> f)
    {
        int sr = 44100, n = (int)(giay * sr); var d = new float[n]; var r = new System.Random(ten.GetHashCode());
        for (int i = 0; i < n; i++) d[i] = Mathf.Clamp(f(i / (float)sr, r), -1f, 1f);
        var c = AudioClip.Create(ten, n, 1, sr, false); c.SetData(d, 0); return c;
    }

    // nhìn vào radio (nó nằm trên nóc tủ tivi — tia [E] thường trúng tủ trước): đủ gần, nhìn đúng hướng, không cách tường
    public bool NhinTrung(Transform cam, float tamVoi)
    {
        var d = tam - cam.position; float kc = d.magnitude;
        if (kc > tamVoi + 0.6f || Vector3.Angle(cam.forward, d) > 14f) return false;
        // [25/10] bỏ qua: khối va chạm của chính người chơi (mắt nằm trong đầu CharacterController — bản mới của nhóm),
        //  và mọi thứ sát radio (nóc tủ tivi, lọ hoa bên cạnh) — chỉ chặn khi có vật ở giữa, cách radio > 0,8 m
        foreach (var h in Physics.RaycastAll(cam.position, d / kc, kc, ~0, QueryTriggerInteraction.Ignore))
        {
            if (h.collider is CharacterController || h.collider.transform.IsChildOf(transform)) continue;
            if (cam.root && h.collider.transform.IsChildOf(cam.root)) continue;
            if (h.distance > kc - 0.8f) continue;
            return false;
        }
        return true;
    }
    public string GoiY => khoa ? "" : "[E] " + (DangBat ? "Tắt " : "Bật ") + ten;

    // [25/10] báo cho LocMa biết NGƯỜI CHƠI vừa bấm E (true = bật, false = tắt) — dùng cho cảnh ma tự bật radio
    public static System.Action<bool> NguoiChoiBam;
    public void Toggle() { if (khoa) return; if (DangBat) Tat(); else Bat(); NguoiChoiBam?.Invoke(DangBat); Ghi($"bấm E → {(DangBat ? "bật" : "tắt")} · nhạc {(nhac && nhac.clip ? nhac.clip.name : "KHÔNG có")} · đang chạy {(nhac && nhac.isPlaying)} · âm lượng tổng {AudioListener.volume:0.00} · tạm dừng {AudioListener.pause}"); }

    public void Bat()
    {
        if (DangBat || !nhac.clip) return;
        tach.PlayOneShot(clipTach, 0.9f);
        nhac.pitch = 1; nhac.volume = amLuong;
        if (nhac.time > 0) nhac.UnPause(); else nhac.Play();
        re.Play();
        DangBat = true; batLuc = Time.time;
    }

    public void Tat(bool tiengTach = true)
    {
        if (!DangBat) return;
        if (tiengTach) tach.PlayOneShot(clipTach, 0.9f);
        nhac.Pause(); re.Stop(); nhac.pitch = 1;
        DangBat = false;
    }

    // ma tắt radio: băng kéo chậm, giọng trầm méo dần rồi tắt phụt
    public System.Collections.IEnumerator KeoBang(float giay = 1.4f)
    {
        if (!DangBat) yield break;
        for (float t = 0; t < 1; t += Time.deltaTime / giay)
        {
            float e = t * t;
            nhac.pitch = Mathf.Lerp(1f, 0.32f, e); nhac.volume = amLuong * Mathf.Lerp(1f, 0.55f, e);
            re.volume = Mathf.Lerp(0.05f, 0.35f, e);
            yield return null;
        }
        Tat(true);
    }

    // [25/10] ma tự bật radio: rè xẹt một tiếng rồi nhạc vang lên
    public void MaBat() { if (DangBat) return; Re(0.6f); Bat(); }

    // xẹt nhiễu chói (radio tự rè lên một tiếng rồi tắt)
    public void Re(float to = 1f) { tach.PlayOneShot(clipXet, to); }

    void Update()
    {
        if (!DangBat) return;
        // cách tường / qua phòng khác: nghe ồm hơn
        if (!tai) tai = FindAnyObjectByType<AudioListener>();
        if (tai)
        {
            var o = tai.transform.position; var d = tam - o; float kc = d.magnitude;
            bool chan = kc > 0.6f && Physics.Raycast(o, d / kc, kc - 0.35f, ~0, QueryTriggerInteraction.Ignore);
            ongTuong = Mathf.MoveTowards(ongTuong, chan ? 0f : 1f, Time.deltaTime * 3f);
            lpTuong.cutoffFrequency = Mathf.Lerp(900f, 7000f, ongTuong);
        }
    }
}
