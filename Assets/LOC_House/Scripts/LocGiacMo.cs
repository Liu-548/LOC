using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// [02/11] GIẤC MƠ cuối Đêm 2 — "Chạy" (ke-hoach-giac-mo-02-11.md, đã chốt 02/11):
//  · chỉ một giấc mơ, cuối Đêm 2, vào bằng [E] Đi ngủ ở giường Khôi (LocCotTruyen.DiNgu) · ma là bóng đen KHÔNG MẮT
//  · giữ W để chạy (A/D đổi làn, ba làn), khúc rẽ tự rẽ theo đường, lối rẽ hai bên là lối giả
//  · không chạy thì ma áp sát và bắt — chữ "CHẠY ĐI!" CHỈ hiện khi đang đứng yên và ma sắp tới nơi
//  · đồ vật vừa chạy qua tự nhấc lên bay tới: dấu "!" đỏ ở đáy màn hình đúng làn sắp bị ném, 1,5 s trước · trúng 3 lần = bị bắt
//  · tới cửa sắt cuối hành lang = thoát (sập cửa, ma đập cửa, "Anh Khôi ơi.") · bị bắt = ngã, bàn tay túm, hét, cắt đen
//  Mơ không có thua thật: kết quả chỉ để LocCotTruyen chọn cảnh tỉnh dậy + lưu cờ.
//  [02/11 tối] Giấc mơ có CẢNH RIÊNG: Assets/Scenes/LOC_GiacMo_Dem2.unity (sương đen, ánh nền tối, không bầu trời — chỉnh trong Lighting của cảnh đó).
//  Vào mơ: nạp chồng cảnh này, đặt làm cảnh chính (ánh sáng của nó áp lên), dựng hành lang bên trong (khối + đồ nhân bản từ chính đồ trong nhà,
//  ở xa nhà: x 400, y −60) → mơ xong trả cảnh nhà làm cảnh chính rồi gỡ cảnh mơ (xoá sạch). Cảnh nhà không nặng thêm.
//  Không nạp được cảnh (chưa có trong Build Settings…) thì dựng ngay trong cảnh nhà như trước và tự đổi sương / ánh nền.
//  Âm thanh tim / gió rít / đồ rơi / thở của ma / đập cửa: AmTong (cuối file) — có file thật trong Resources/LOC_AmThanh/GM_* thì tự dùng.
public class LocGiacMo : MonoBehaviour
{
    public static bool dangMo;
    public static string loiCuoi;   // [03/11] lỗi bất ngờ gần nhất trong giấc mơ (khi đó kết quả bị tính là THOÁT) — tự kiểm ghi ra
    // giữ Enter 0,8 s để bỏ qua (như xe buýt / đoạn bác — LocFirstPerson.GiuBoQua). Giống nút B của mini game: chỉ có ở Editor / bản Development,
    //  và tắt hẳn khi bật menu LOC → Bản nộp (ký hiệu LOC_BAN_NOP)
#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && !LOC_BAN_NOP
    public static bool choBoQua = true;
#else
    public static bool choBoQua = false;
#endif
    public enum Bot { Khong, Ne, KhongNe, DungYen }
    public static Bot bot = Bot.Khong;           // tự kiểm (LocTuKiemGiacMo): máy tự chơi
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset0() { dangMo = false; bot = Bot.Khong; cuoi = null; canhNha = default; }

    [Header("Chạy")]
    public float tocChay = 4.5f, tangToc = 14f, giamToc = 11f, matCao = 1.52f, rongLan = 1.2f, tgDoiLan = 0.25f;
    [Header("Ma")]
    public float khoangMa = 8f, tocMaKhiDung = 2.2f, batKhi = 0.8f, canhBaoKhi = 5f;
    [Header("Ném")]
    public float baoTruoc = 1.5f;
    [Header("Nhạc rượt (Resources/LOC_AmThanh/GM_NhacRuot · cú đánh GM_NhacDanh)")]
    public float tgZoom = 3f;                          // [13/11] cảnh zoom vào mặt ma = đúng 3 s đầu bản nhạc (giây 3 là cú đánh)
    public float nhacLapTu = 3f, nhacLapDen = 66f;     // tới giây 66 thì quay về giây 3 (không lặp đoạn mở đầu yên tĩnh)
    public float nhipDau = 3.1f, nhipNhan = 4.5f, maNhanTheoNhip = 1.6f;   // nhịp nhấn của nhạc: ma lướt nhanh thêm lúc nhấn

    // kết quả (tự kiểm đọc)
    public bool ketThoat; public float tgChay; public int soNem, soNemDoi, soTrung; public bool daCanhBao, luonCoLanTrong = true;
    public float tCanhBao = -1f, dungTu = -1f;   // lần đầu hiện "CHẠY ĐI!" (giây, tính từ lúc bắt đầu chạy) · lúc bắt đầu đứng yên
    public static LocGiacMo cuoi;                // lần mơ gần nhất (tự kiểm đọc sau khi mơ xong)
    public const string TEN_CANH = "LOC_GiacMo_Dem2", DUONG_CANH = "Assets/Scenes/LOC_GiacMo_Dem2.unity";
    public static Scene canhNha;                 // cảnh nhà lúc đang mơ (LocMenu.NapLai: chơi lại / về menu giữa giấc mơ phải nạp cảnh nhà, không phải cảnh mơ)
    Scene canhMo; bool coCanhRieng;
    public static bool CoMatDangHien(Transform goc) { if (!goc) return false; foreach (var r in goc.GetComponentsInChildren<Renderer>()) if (r.enabled && LaMat(r.name)) return true; return false; }

    const float BUOC = 0.2f, H = 2.4f, CAO = 2.6f, R_GOC = 1.8f, S_BAT_DAU = 15f;
    static readonly Vector3 GOC = new(400f, -60f, 0f);
    static readonly float[] DAI = { 75f, 65f, 55f, 80f, 60f, 55f, 50f };   // 7 đoạn thẳng, 6 khúc rẽ (~440 m, chạy ~95 s)
    static readonly int[] RE = { 1, -1, -1, 1, 1, -1 };                   // 1 phải · −1 trái

    readonly List<Vector3> P = new(), T = new();   // mẫu đường tâm (cục bộ) cách nhau BUOC
    float dai;
    Transform goc;
    LocFirstPerson fp; Transform cam;
    Vector3 camLP0; Quaternion camLR0;
    float s, v, x, lanDich, yawLech, pitch, rung, toiChop, cham, dauBuoc, gap, tgDung;
    bool intro = true, ketThuc, canhBao, coW;
    Transform ma; Material vlMa; AudioSource amTim, amMaTho;
    Light denTruoc, denSau;
    readonly List<DoVat> doVat = new();
    readonly List<Nem> nem = new();
    readonly List<(Renderer r, float hz, float pha)> chapChon = new();
    readonly List<(Transform t, float s)> bongLoi = new();
    Transform cuaBanLe, khoaTreo;
    float tNem = 3f, tChase;
    GameObject[] nguonDo, nguonNem;

    class DoVat { public Transform t; public float s; public bool nemDuoc, daNem; }
    class Nem { public Transform t; public int lan; public float t0; public Vector3 pDat, pNhac, vBay; public Quaternion q0; public bool xong, daDat; public float tDat; public AudioSource am, keo; }
    AudioClip amKeo; bool daTimKeo; float tChapCuoi;

    // ═════════════════════════ vào mơ
    public static LocGiacMo Tao() => cuoi = new GameObject("LOC_GiacMo").AddComponent<LocGiacMo>();

    // chạy cả giấc mơ; lỗi bất ngờ ở bất kỳ đâu (vd thiếu model) → ghi lỗi, dọn dẹp, coi như thoát — không bao giờ kẹt màn đen
    public IEnumerator Chay(LocFirstPerson nguoi, System.Action<bool> xong)
    {
        bool loi = false; loiCuoi = null;
        yield return AnToan(ChayTrong(nguoi), () => loi = true);
        if (loi)
        {
            // [03/11] lỗi xảy ra SAU khi đã bị bắt (giữa cảnh ngã / lật / hét) thì kết quả vẫn là BỊ BẮT — trước đây bị tính THOÁT
            //  (tự kiểm "đứng yên": ma áp sát, cảnh bị bắt gãy giữa chừng → ghi THOÁT). Chỉ lỗi trước lúc bị bắt mới coi như thoát.
            ketThoat = !daBiBat;
            try { if (!daDon) DonDep(); } catch (System.Exception e) { Debug.LogException(e); }
        }
        LocFirstPerson.den = 1f;
        dangMo = false;
        xong?.Invoke(ketThoat);
    }
    static IEnumerator AnToan(IEnumerator e, System.Action khiLoi)
    {
        while (true)
        {
            object cur;
            try { if (!e.MoveNext()) yield break; cur = e.Current; }
            catch (System.Exception ex) { Debug.LogException(ex); loiCuoi = ex.GetType().Name + ": " + ex.Message + " @ " + (ex.StackTrace ?? "").Split('\n')[0].Trim(); khiLoi?.Invoke(); yield break; }
            if (cur is IEnumerator con) { bool hong = false; yield return AnToan(con, () => hong = true); if (hong) { khiLoi?.Invoke(); yield break; } }
            else yield return cur;
        }
    }

    // nạp chồng cảnh riêng của giấc mơ, chuyển cả cây LOC_GiacMo sang đó và đặt làm cảnh chính (sương / ánh nền của cảnh mơ tự áp lên)
    IEnumerator NapCanh()
    {
        canhNha = SceneManager.GetActiveScene();
        AsyncOperation op = null;
        try
        {
#if UNITY_EDITOR
            op = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(DUONG_CANH, new LoadSceneParameters(LoadSceneMode.Additive));
#else
            if (Application.CanStreamedLevelBeLoaded(TEN_CANH)) op = SceneManager.LoadSceneAsync(TEN_CANH, LoadSceneMode.Additive);
#endif
        }
        catch (System.Exception e) { Debug.LogWarning("[LOC] Không nạp được cảnh giấc mơ " + DUONG_CANH + ": " + e.Message); }
        if (op == null) { Debug.LogWarning("[LOC] Không có cảnh " + TEN_CANH + " — dựng giấc mơ ngay trong cảnh nhà"); yield break; }
        float t0 = Time.realtimeSinceStartup;
        while (!op.isDone && Time.realtimeSinceStartup - t0 < 20f) yield return null;
        canhMo = SceneManager.GetSceneByName(TEN_CANH);
        if (!canhMo.IsValid() || !canhMo.isLoaded) { Debug.LogWarning("[LOC] Cảnh " + TEN_CANH + " chưa nạp xong — dựng trong cảnh nhà"); yield break; }
        SceneManager.MoveGameObjectToScene(gameObject, canhMo);
        SceneManager.SetActiveScene(canhMo);
        coCanhRieng = true;
    }

    // [02/11] lần đầu trong Editor, Unity còn dịch shader (sương kiểu mới, Unlit của bóng đèn, khói của ma) → tô tạm xanh lơ.
    //  Lúc màn còn đen: xoay máy quay bốn phía cho mọi thứ được vẽ một lần, chờ dịch xong (tối đa 15 s) rồi mới mở mắt.
    IEnumerator HamNong()
    {
        foreach (float y in new[] { 0f, 90f, 180f, 270f }) { yawLech = y; DatNguoi(); yield return null; yield return null; }
        yawLech = 0; DatNguoi();
#if UNITY_EDITOR
        float t = 0;
        while (UnityEditor.ShaderUtil.anythingCompiling && t < 15f) { t += Time.unscaledDeltaTime; yield return null; }
#endif
        yield return null;
    }

    IEnumerator ChayTrong(LocFirstPerson nguoi)
    {
        dangMo = true; fp = nguoi; cam = fp.cam; goc = transform;
        camLP0 = cam.localPosition; camLR0 = cam.localRotation;
        LocFirstPerson.khoa = true; LocMa.tamDung = true;
        var cc = fp.GetComponent<CharacterController>(); if (cc) cc.enabled = false;
        LocFirstPerson.den = 1f;
        yield return NapCanh();
        DungDuong(); ChupMoiTruong(); DungHanhLang(); DungMa(); DungAm();
        s = S_BAT_DAU; gap = s - 1f; x = 0; lanDich = 0;
        DatNguoi(); DatMa();
        yield return null;
        yield return HamNong();
        yield return Intro();
        intro = false;
        while (!ketThuc)
        {
            if (LocMenu.DangChe) { yield return null; continue; }   // Esc tạm dừng: đứng hình (timeScale 0), không nhận phím đổi làn
            float dt = Time.deltaTime;
            tChase += dt; tgChay += dt;
            var k = Keyboard.current;
            // [02/11 tối] SỬA "bỏ qua ngay sau Chạy.": chỉ tính khi Enter được BẤM MỚI trong lúc chạy (Enter giữ sẵn / dính từ trước không tính)
            if (k != null && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame)) enterMoi = true;
            if (!LocFirstPerson.GiuBoQua(k)) enterMoi = false;
            if (choBoQua && enterMoi) { tBo += dt; if (tBo >= 0.8f) { ketThoat = true; break; } } else tBo = 0f;
            DieuKhien(dt);
            CapNhatNhac();
            CapNhatMa(dt);
            CapNhatNem(dt);
            CapNhatDen(dt);
            DatNguoi();
            if (gap <= batKhi || soTrung >= 3) { yield return BiBat(); break; }
            if (s >= dai - 4.5f) { yield return Thoat(); break; }
            yield return null;
        }
        LocFirstPerson.den = 1f;
        DonDep();
    }

    // ═════════════════════════ đường: 7 đoạn thẳng nối bằng cung 90°, lấy mẫu đều BUOC
    Vector3[] C; Vector3[] D;
    void DungDuong()
    {
        int n = DAI.Length; C = new Vector3[n + 1]; D = new Vector3[n];
        var d = Vector3.forward; C[0] = Vector3.zero;
        for (int i = 0; i < n; i++)
        {
            D[i] = d; C[i + 1] = C[i] + d * DAI[i];
            if (i < RE.Length) d = RE[i] > 0 ? Vector3.Cross(Vector3.up, d) : -Vector3.Cross(Vector3.up, d);
        }
        var day = new List<Vector3> { C[0] };
        void Thang(Vector3 a, Vector3 b) { float L = Vector3.Distance(a, b); int m = Mathf.Max(1, Mathf.CeilToInt(L / 0.1f)); for (int j = 1; j <= m; j++) day.Add(Vector3.Lerp(a, b, j / (float)m)); }
        var dauDoan = C[0];
        for (int i = 1; i < n; i++)
        {
            var a = C[i] - D[i - 1] * R_GOC; var b = C[i] + D[i] * R_GOC;
            Thang(dauDoan, a);
            for (int j = 1; j <= 24; j++) { float t = j / 24f; day.Add((1 - t) * (1 - t) * a + 2 * (1 - t) * t * C[i] + t * t * b); }
            dauDoan = b;
        }
        Thang(dauDoan, C[n]);
        var dd = new float[day.Count]; for (int i = 1; i < day.Count; i++) dd[i] = dd[i - 1] + Vector3.Distance(day[i - 1], day[i]);
        dai = dd[day.Count - 1];
        int k = 0;
        for (float u = 0; u <= dai; u += BUOC)
        {
            while (k < day.Count - 2 && dd[k + 1] < u) k++;
            float f = Mathf.InverseLerp(dd[k], dd[k + 1], u);
            P.Add(Vector3.Lerp(day[k], day[k + 1], f));
        }
        for (int i = 0; i < P.Count; i++)
        {
            var a = P[Mathf.Max(0, i - 3)]; var b = P[Mathf.Min(P.Count - 1, i + 3)];
            var t = b - a; t.y = 0; T.Add(t.sqrMagnitude > 1e-6f ? t.normalized : Vector3.forward);
        }
    }
    Vector3 Tam(float u) { float f = Mathf.Clamp(u / BUOC, 0, P.Count - 1.001f); int i = (int)f; return Vector3.Lerp(P[i], P[i + 1], f - i); }
    Vector3 Tiep(float u) { float f = Mathf.Clamp(u / BUOC, 0, T.Count - 1.001f); int i = (int)f; return Vector3.Slerp(T[i], T[i + 1], f - i).normalized; }
    static Vector3 Phai(Vector3 t) => Vector3.Cross(Vector3.up, t);
    Vector3 Diem(float u, float lech, float cao) { var t = Tiep(u); return GOC + Tam(u) + Phai(t) * lech + Vector3.up * cao; }
    float LanX(int lan) => lan * rongLan;
    int LanGan(float lx) => Mathf.Clamp(Mathf.RoundToInt(lx / rongLan), -1, 1);
    float SGan(Vector3 cucBo)
    {
        int tot = 0; float g = float.MaxValue;
        for (int i = 0; i < P.Count; i += 2) { float d2 = (P[i] - cucBo).sqrMagnitude; if (d2 < g) { g = d2; tot = i; } }
        return tot * BUOC;
    }

    // ═════════════════════════ môi trường: tắt sáng nhà, sương đen
    float tBo; bool enterMoi;   // đang giữ Enter để bỏ qua (chỉ tính lần bấm mới trong lúc mơ)
    readonly List<AudioSource> amNha = new();   // tiếng trong nhà đang phát (mưa trên mái, tủ lạnh, đồng hồ, radio…) — tạm dừng suốt giấc mơ
    Camera mayQuay; CameraClearFlags xoa0; Color nen0; float xa0;
    Color amb0, fog0; bool fogBat0; float fogD0; FogMode fogM0; UnityEngine.Rendering.AmbientMode ambM0; Material sky0; readonly List<Light> mat0 = new();
    void ChupMoiTruong()
    {
        if (!coCanhRieng)   // có cảnh riêng thì sương / ánh nền / bầu trời lấy từ Lighting của cảnh mơ
        {
            amb0 = RenderSettings.ambientLight; fog0 = RenderSettings.fogColor; fogBat0 = RenderSettings.fog; fogD0 = RenderSettings.fogDensity; fogM0 = RenderSettings.fogMode;
            ambM0 = RenderSettings.ambientMode; sky0 = RenderSettings.skybox;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.skybox = null;
            RenderSettings.ambientLight = new Color(0.025f, 0.024f, 0.028f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogDensity = 0.055f; RenderSettings.fogColor = new Color(0.008f, 0.008f, 0.01f);
        }
        foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None)) if (l.type == LightType.Directional && l.enabled) { mat0.Add(l); l.enabled = false; }
        foreach (var a in FindObjectsByType<AudioSource>(FindObjectsSortMode.None)) if (a.isPlaying && !a.transform.IsChildOf(transform)) { a.Pause(); amNha.Add(a); }
        // máy quay: nền đen đặc, nhìn xa 70 m — sương đen đã nuốt hết từ ~40 m, khỏi vẽ cả hành lang 240 m sau các bức tường
        mayQuay = cam ? cam.GetComponent<Camera>() : null;
        if (mayQuay) { xoa0 = mayQuay.clearFlags; nen0 = mayQuay.backgroundColor; xa0 = mayQuay.farClipPlane; mayQuay.clearFlags = CameraClearFlags.SolidColor; mayQuay.backgroundColor = RenderSettings.fogColor; mayQuay.farClipPlane = 70f; }
    }
    void TraMoiTruong()
    {
        if (coCanhRieng) { if (canhNha.IsValid() && canhNha.isLoaded) SceneManager.SetActiveScene(canhNha); }   // cảnh nhà làm cảnh chính lại → ánh sáng nhà
        else
        {
            RenderSettings.ambientMode = ambM0; RenderSettings.skybox = sky0;
            RenderSettings.ambientLight = amb0; RenderSettings.fogColor = fog0; RenderSettings.fog = fogBat0; RenderSettings.fogDensity = fogD0; RenderSettings.fogMode = fogM0;
        }
        foreach (var l in mat0) if (l) l.enabled = true;
        foreach (var a in amNha) if (a) a.UnPause();
        if (mayQuay) { mayQuay.clearFlags = xoa0; mayQuay.backgroundColor = nen0; mayQuay.farClipPlane = xa0; }
    }

    // ═════════════════════════ hành lang
    Material vlTuong, vlSan, vlTran, vlSat, vlBong, vlDay, vlDen;
    void DungHanhLang()
    {
        vlTuong = TimVL("M_TuongVoi_Tex", new Color(0.46f, 0.43f, 0.38f), new Color(0.62f, 0.6f, 0.55f));
        vlSan = TimVL("M_SanGachBong_T1_Tex", new Color(0.3f, 0.28f, 0.26f), new Color(0.55f, 0.52f, 0.48f));
        vlTran = TimVL("M_TranSan", new Color(0.36f, 0.35f, 0.33f), new Color(0.6f, 0.58f, 0.54f));
        vlSat = TimVL("M_SatSon", new Color(0.2f, 0.2f, 0.19f), Color.white);
        vlDen = TimVL(null, new Color(0.01f, 0.01f, 0.01f), Color.white);
        vlDay = TimVL(null, new Color(0.06f, 0.05f, 0.05f), Color.white);
        var unlit = Shader.Find("Universal Render Pipeline/Unlit");   // bóng đèn tự sáng, không phụ thuộc đèn — Unlit không có thì Lit + phát sáng
        if (unlit) { vlBong = Moi(new Material(unlit), "BongDen"); vlBong.SetColor("_BaseColor", new Color(1f, 0.78f, 0.5f)); }
        else
        {
            vlBong = TimVL(null, new Color(1f, 0.86f, 0.62f), Color.white);
            if (vlBong) { vlBong.EnableKeyword("_EMISSION"); vlBong.SetColor("_EmissionColor", new Color(1f, 0.62f, 0.28f) * 2.2f); }
        }
        nguonDo = TimNguon("SM_PlasticChair_Red", "SM_PlasticChair_Green", "SM_PlasticChairStack_4", "SM_PlasticChairStack_8", "GheDau_Go", "GheDauNho", "ThungGao_Nhua",
            "ThungNuoc_Nap", "ThungCarton_Chong", "BaoXiMang_Chong", "ChauNhua_Up", "ChauNhua", "XoGao_Up", "Thau_Nhua_QuanAo", "ThungRacNhua", "ThungSon_Cum2", "QuatCay_NguyenBo", "QuatBan_SatCu", "HuBinhNgamRuou", "ChauInox");
        nguonNem = TimNguon("SM_PlasticChair_Red", "SM_PlasticChair_Green", "GheDau_Go", "GheDauNho", "ChauNhua_Up", "ChauNhua", "XoGao_Up", "Thau_Nhua_QuanAo", "ThungRacNhua", "QuatBan_SatCu", "ChauInox");
        var rd = new System.Random(20021);
        float R(float a, float b) => a + (float)rd.NextDouble() * (b - a);
        int n = D.Length;
        for (int i = 0; i < n; i++)
        {
            var d = D[i]; float L = DAI[i];
            float u0 = i == 0 ? -1.2f : H, u1 = i == n - 1 ? L : L - H;   // phần thẳng không tính ô góc
            var a = C[i];
            float len = u1 - u0; var giua = a + d * ((u0 + u1) / 2f);
            Hop("San_" + i, giua + Vector3.down * 0.05f, new Vector3(2 * H + 0.4f, 0.1f, len), d, vlSan, (2 * H + 0.4f) / 0.8f, len / 0.8f);   // gạch bông 0,8 m một ô vân (như LocHouseBuilder)
            Hop("Tran_" + i, giua + Vector3.up * (CAO + 0.05f), new Vector3(2 * H + 0.4f, 0.1f, len), d, vlTran, 2f, len / 3f);
            // lối giả: 1–2 ô mỗi đoạn, không sát đầu/cuối đoạn
            var lo = new List<(float u, int ben)>();
            int soLo = L > 60 ? 2 : 1;
            for (int j = 0; j < soLo; j++) { float u = Mathf.Lerp(u0 + 8f, u1 - 8f, (j + 0.5f) / soLo) + R(-4f, 4f); if (i == 0 && u < S_BAT_DAU + 6f) u = S_BAT_DAU + 10f; lo.Add((u, rd.NextDouble() < 0.5 ? -1 : 1)); }
            foreach (int ben in new[] { -1, 1 })
            {
                float cuoi = u0;
                foreach (var (u, b) in lo) if (b == ben) { Tuong(a, d, ben, cuoi, u - 0.8f, i); LoiGia(a, d, ben, u, rd); cuoi = u + 0.8f; }
                Tuong(a, d, ben, cuoi, u1, i);
            }
            if (i == 0) Hop("Tuong_Dau", a + d * (u0 - 0.1f) + Vector3.up * (CAO / 2), new Vector3(2 * H + 0.4f, CAO, 0.2f), d, vlTuong, 2.4f, 1.2f);
            // đồ chất sát hai tường (ngoài ba làn) + bóng đèn treo
            for (float u = u0 + 1f; u < u1 - 1f; u += R(1.4f, 3.0f))
                foreach (int ben in new[] { -1, 1 })
                {
                    bool gapLo = false; foreach (var (lu, lb) in lo) if (lb == ben && Mathf.Abs(lu - u) < 1.4f) gapLo = true;
                    if (gapLo || rd.NextDouble() < 0.3) continue;
                    var p = a + d * u + Phai(d) * ben * 2.0f;
                    var g = DoTrongNha(nguonDo, rd, p, d, rd.NextDouble() < 0.5);
                    if (g && !SatTuong(g, Phai(d) * ben, 2.0f)) { Destroy(g); g = null; }
                    if (g) doVat.Add(new DoVat { t = g.transform, s = SGan(p), nemDuoc = LaDoNem(g) });
                }
            for (float u = u0 + 4f; u < u1 - 2f; u += 9f) BongDen(a + d * u, rd);
        }
        for (int i = 1; i < n; i++) OGoc(C[i], D[i - 1], D[i]);
        CuoiHanhLang(C[n], D[n - 1]);
    }

    // đẩy vật sát tường; vật lấn quá mép làn ngoài (cách tâm 1,65 m) thì bỏ — ba làn luôn trống
    static bool SatTuong(GameObject g, Vector3 ra, float lech)
    {
        var rs = g.GetComponentsInChildren<Renderer>(); if (rs.Length == 0) return false;
        var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
        float sau = Mathf.Abs(Vector3.Dot(b.extents, new Vector3(Mathf.Abs(ra.x), Mathf.Abs(ra.y), Mathf.Abs(ra.z))));
        if (H - 0.04f - 2f * sau < 1.65f) return false;
        g.transform.position += ra * (H - 0.04f - sau - lech);
        return true;
    }

    void Tuong(Vector3 a, Vector3 d, int ben, float tu, float den, int i)
    {
        float len = den - tu; if (len < 0.05f) return;
        var p = a + d * ((tu + den) / 2f) + Phai(d) * ben * (H + 0.1f) + Vector3.up * (CAO / 2f);
        Hop("Tuong_" + i, p, new Vector3(0.2f, CAO, len), d, vlTuong, len / 2f, CAO / 3.3f);   // vôi 2 × 3,3 m
        Hop("ChanTuong_" + i, p + Vector3.down * (CAO / 2f - 0.06f) - Phai(d) * ben * 0.11f, new Vector3(0.02f, 0.12f, len), d, vlDay, 1, 1);
    }

    // ô góc vuông: sàn, trần, hai vách phía ngoài khúc rẽ
    void OGoc(Vector3 c, Vector3 dVao, Vector3 dRa)
    {
        Hop("San_Goc", c + Vector3.down * 0.05f, new Vector3(2 * H + 0.4f, 0.1f, 2 * H + 0.4f), dVao, vlSan, 6.5f, 6.5f);
        Hop("Tran_Goc", c + Vector3.up * (CAO + 0.05f), new Vector3(2 * H + 0.4f, 0.1f, 2 * H + 0.4f), dVao, vlTran, 2, 2);
        Hop("Tuong_Goc1", c + dVao * (H + 0.1f) + Vector3.up * (CAO / 2), new Vector3(2 * H + 0.4f, CAO, 0.2f), dVao, vlTuong, 2.4f, 1.2f);
        Hop("Tuong_Goc2", c - dRa * (H + 0.1f) + Vector3.up * (CAO / 2), new Vector3(2 * H + 0.4f, CAO, 0.2f), dRa, vlTuong, 2.4f, 1.2f);
    }

    // lối giả: hõm tối sâu 2,4 m, đồ chất cao chặn lối, đôi khi có bóng đen đứng trong
    void LoiGia(Vector3 a, Vector3 d, int ben, float u, System.Random rd)
    {
        var r = Phai(d) * ben; var c0 = a + d * u + r * (H + 1.25f);
        Hop("LoiGia_San", c0 + Vector3.down * 0.05f, new Vector3(1.6f, 0.1f, 2.5f), r, vlSan, 2f, 3.1f);
        Hop("LoiGia_Tran", c0 + Vector3.up * (CAO + 0.05f), new Vector3(1.6f, 0.1f, 2.5f), r, vlTran, 1, 1);
        Hop("LoiGia_Day", c0 + r * 1.35f + Vector3.up * (CAO / 2), new Vector3(1.6f, CAO, 0.2f), r, vlDen, 1, 1);
        foreach (float sg in new[] { -1f, 1f }) Hop("LoiGia_Vach", a + d * (u + sg * 0.9f) + r * (H + 1.25f) + Vector3.up * (CAO / 2), new Vector3(0.2f, CAO, 2.7f), r, vlTuong, 1.35f, CAO / 3.3f);   // vách hai bên hõm: chạy theo chiều sâu
        for (int k = 0; k < 4; k++)   // đồ chất chặn
        {
            var p = a + d * (u + (float)(rd.NextDouble() - 0.5) * 0.9f) + r * (H + 0.9f + (float)rd.NextDouble() * 0.9f);
            var g = DoTrongNha(nguonDo, rd, p, d, true);
            if (g && k >= 2) g.transform.position += Vector3.up * 0.55f * (k - 1);   // chồng lên nhau
        }
        if (rd.NextDouble() < 0.4)
        {
            var bo = BongDen1m9(a + d * u + r * (H + 2.0f), -r);
            if (bo) bongLoi.Add((bo, SGan(a + d * u)));
        }
    }

    void BongDen(Vector3 p, System.Random rd)
    {
        var tren = GOC + p + Vector3.up * CAO;
        var day = Khoi(PrimitiveType.Cylinder, tren + Vector3.down * 0.15f, new Vector3(0.008f, 0.15f, 0.008f), vlDay);
        var b = Khoi(PrimitiveType.Sphere, tren + Vector3.down * 0.34f, new Vector3(0.07f, 0.09f, 0.07f), vlBong);
        if (b && rd.NextDouble() < 0.25) chapChon.Add((b.GetComponent<Renderer>(), 6f + (float)rd.NextDouble() * 10f, (float)rd.NextDouble() * 10f));
    }

    // cuối hành lang: vách có ô cửa sắt hầm (cánh lấy hình từ chính cửa sắt xuống hầm "CuaSatHam (mở)", có ổ khoá treo), cánh hé một khe;
    //  sau cửa là buồng tối nhỏ. Không thấy cửa hầm trong scene thì dựng cánh bằng khối.
    int benBanLe = -1;   // −1 bản lề bên trái (cánh nằm về bên phải bản lề) · 1 bản lề bên phải — chọn sao cho ổ khoá quay về phía người chạy tới
    void CuoiHanhLang(Vector3 c, Vector3 d)
    {
        var r = Phai(d); float W = 1.0f, HC = 2.05f;
        Transform canhThat = null, khoaThat = null; float matKhoa = 1f;
        var ng = TimNguon("CuaSatHam (mở)");
        if (ng.Length > 0)
        {
            canhThat = ng[0].transform; W = 0.8f; HC = 1.9f;
            foreach (var t in canhThat.GetComponentsInChildren<Transform>(true))
                if (t.name.StartsWith("KhoaCua_Hasp")) { khoaThat = t; matKhoa = Mathf.Sign(canhThat.InverseTransformPoint(t.position).z); break; }
            benBanLe = matKhoa < 0 ? 1 : -1;
        }
        float ben = (2 * H + 0.4f - W) / 2f;
        foreach (float sg in new[] { -1f, 1f }) Hop("TuongCuoi", c + r * sg * (W / 2 + ben / 2) + Vector3.up * (CAO / 2), new Vector3(ben, CAO, 0.2f), d, vlTuong, ben / 2f, CAO / 3.3f);
        Hop("TuongCuoi_Tren", c + Vector3.up * (HC + (CAO - HC) / 2), new Vector3(W, CAO - HC, 0.2f), d, vlTuong, 0.5f, 0.3f);
        foreach (float sg in new[] { -1f, 1f }) Hop("KhungCua", c + r * sg * (W / 2 + 0.03f) + Vector3.up * (HC / 2), new Vector3(0.06f, HC, 0.24f), d, vlSat, 1, 1);
        Hop("KhungCua_Tren", c + Vector3.up * (HC + 0.03f), new Vector3(W + 0.12f, 0.06f, 0.24f), d, vlSat, 1, 1);
        // buồng sau cửa
        var b = c + d * 1.8f;
        Hop("Buong_San", b + Vector3.down * 0.05f, new Vector3(3f, 0.1f, 3.6f), d, vlSan, 3.75f, 4.5f);
        Hop("Buong_Tran", b + Vector3.up * (CAO + 0.05f), new Vector3(3f, 0.1f, 3.6f), d, vlTran, 1, 1);
        Hop("Buong_Day", b + d * 1.8f + Vector3.up * (CAO / 2), new Vector3(3f, CAO, 0.2f), d, vlDen, 1, 1);
        foreach (float sg in new[] { -1f, 1f }) Hop("Buong_Vach", b + r * sg * 1.5f + Vector3.up * (CAO / 2), new Vector3(0.2f, CAO, 3.6f), d, vlDen, 1, 1);
        // bản lề
        var bl = new GameObject("CuaSat_BanLe").transform; bl.SetParent(goc, false);
        bl.position = GOC + c + r * (benBanLe * W / 2) + d * 0.04f; bl.rotation = Quaternion.LookRotation(d);
        if (canhThat)
        {
            // hình cánh thật: gốc model ở mép tay nắm, bản lề ở x = 0,8 (LocHouseBuilder: dHamCua.banLe)
            var sao = HinhSao(canhThat, bl, khoaThat);   // cánh + tấm móc chìa, chưa có ổ khoá
            if (khoaThat)
            {
                // ổ khoá chép riêng, giữ đúng chỗ trên cánh, để lắc lách cách lúc sập cửa
                var kh = HinhSao(khoaThat, sao.transform);
                kh.transform.localPosition = Quaternion.Inverse(canhThat.rotation) * (khoaThat.position - canhThat.position);
                kh.transform.localRotation = Quaternion.Inverse(canhThat.rotation) * khoaThat.rotation;
                khoaTreo = kh.transform;
            }
            if (benBanLe < 0) { sao.transform.localRotation = Quaternion.Euler(0, 180, 0); sao.transform.localPosition = new Vector3(0.8f, 0, 0); }
            else { sao.transform.localRotation = Quaternion.identity; sao.transform.localPosition = new Vector3(-0.8f, 0, 0); }
        }
        else
        {
            benBanLe = -1;
            var canh = Khoi(PrimitiveType.Cube, Vector3.zero, new Vector3(W - 0.02f, HC - 0.02f, 0.05f), vlSat);
            if (canh) { canh.transform.SetParent(bl, false); canh.transform.localPosition = new Vector3((W - 0.02f) / 2f, HC / 2f, 0); canh.transform.localRotation = Quaternion.identity; }
            foreach (float yy in new[] { 0.55f, 1.05f, 1.55f })
            { var nep = Khoi(PrimitiveType.Cube, Vector3.zero, new Vector3(W - 0.1f, 0.04f, 0.07f), vlDay); if (nep) { nep.transform.SetParent(bl, false); nep.transform.localPosition = new Vector3(W / 2f, yy, 0); nep.transform.localRotation = Quaternion.identity; } }
            var kh = Khoi(PrimitiveType.Cube, Vector3.zero, new Vector3(0.07f, 0.09f, 0.04f), vlSat);
            if (kh) { kh.transform.SetParent(bl, false); kh.transform.localPosition = new Vector3(W - 0.14f, 1.02f, -0.06f); kh.transform.localRotation = Quaternion.identity; khoaTreo = kh.transform; }
            var vong = Khoi(PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.045f, 0.004f, 0.045f), vlSat);
            if (vong && kh) { vong.transform.SetParent(kh.transform, false); vong.transform.localPosition = new Vector3(0, 0.75f, 0); vong.transform.localRotation = Quaternion.Euler(90, 0, 0); }
        }
        cuaBanLe = bl;
        cuaBanLe.rotation = QCua(d, 12f);   // hé một khe
    }
    // góc cánh cửa: 0 = đóng, dương = mở vào buồng (bên bản lề nào cũng vậy)
    Quaternion QCua(Vector3 d, float mo) => Quaternion.AngleAxis(benBanLe * mo, Vector3.up) * Quaternion.LookRotation(d);

    // ═════════════════════════ con ma: bóng đen cao ~1,9 m, không mắt, lướt, chân tan khói
    void DungMa()
    {
        ma = BongDen1m9(Vector3.zero, Vector3.forward, true);
        amMaTho = ma ? ma.gameObject.AddComponent<AudioSource>() : null;
        if (amMaTho) { amMaTho.clip = AmTong.MaTho(); amMaTho.loop = true; amMaTho.spatialBlend = 1f; amMaTho.minDistance = 1.5f; amMaTho.maxDistance = 18f; amMaTho.volume = 0.6f; amMaTho.Play(); }
    }

    // bóng đen đứng (dáng VanNguoi); không có model thì một khối người tối
    Transform BongDen1m9(Vector3 cucBo, Vector3 huong, bool chinh = false)
    {
        var g = HinhMa(3, chinh ? "Ma_GiacMo" : "Bong_LoiGia");
        g.position = GOC + cucBo; g.rotation = Quaternion.LookRotation(huong.sqrMagnitude > 0 ? huong : Vector3.forward);
        return g;
    }

    // hình ma từ model của LocMa (1 Tay · 3 VanNguoi — cùng một con ma nữ, khác dáng): phủ khói đen, ẩn hẳn mắt (nhãn cầu, con ngươi, tròng) và răng.
    //  Trong nhà model giữ vật liệu ma nữ (mặt, tóc); trong mơ nó chỉ là bóng đen cao — "Đen sì. Cao lắm." (Nhím, Đêm 3): mặt và mắt để dành cho Đêm 3.
    static readonly string[] AN_MA = { "Mat_", "ConNguoi", "TrongMat", "VienTrong", "Rang" };
    static bool LaMat(string ten) { foreach (var a in AN_MA) if (ten.StartsWith(a)) return true; return false; }
    Transform HinhMa(int kieu, string ten)
    {
        var lm = LocMa.I;
        var w = new GameObject(ten).transform; w.SetParent(goc, false);
        if (lm && lm.mau != null && lm.mau.Length > kieu && lm.mau[kieu] && lm.matDen)
        {
            var m = Instantiate(lm.mau[kieu], w); m.SetActive(true); m.transform.localPosition = Vector3.zero;
            // mặt trước của model: mắt nằm trước đầu (cùng cách LocMa.TaoBong) → quay để mặt nhìn theo +z của w
            var dau = Tim(m.transform, "Dau"); var mL = Tim(m.transform, "Mat_L"); var mR = Tim(m.transform, "Mat_R");
            if (dau && mL && mR && ((mL.position + mR.position) / 2 - dau.position).z < 0) m.transform.localRotation = Quaternion.Euler(0, 180, 0) * m.transform.localRotation;
            if (!vlMa) { vlMa = Moi(new Material(lm.matDen), "MaKhoi"); if (vlMa.HasProperty("_Alpha")) vlMa.SetFloat("_Alpha", 0.9f); if (vlMa.HasProperty("_Vien")) vlMa.SetFloat("_Vien", lm.vienTan); if (vlMa.HasProperty("_Nhieu")) vlMa.SetFloat("_Nhieu", lm.nhieu); }
            foreach (var r in m.GetComponentsInChildren<Renderer>(true))
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
                if (LaMat(r.name)) { r.enabled = false; continue; }   // KHÔNG MẮT
                var ms = r.sharedMaterials; for (int i = 0; i < ms.Length; i++) ms[i] = vlMa; r.sharedMaterials = ms;
            }
            DonThanhPhan(m);
        }
        else if (kieu == 3)
        {
            var than = Khoi(PrimitiveType.Capsule, Vector3.zero, new Vector3(0.5f, 0.95f, 0.32f), vlDen); if (than) { than.transform.SetParent(w, false); than.transform.localPosition = Vector3.up * 0.95f; }
            var dau = Khoi(PrimitiveType.Sphere, Vector3.zero, new Vector3(0.24f, 0.28f, 0.24f), vlDen); if (dau) { dau.name = "Dau"; dau.transform.SetParent(w, false); dau.transform.localPosition = Vector3.up * 1.78f; }
        }
        else { Destroy(w.gameObject); return null; }
        return w;
    }

    void DungAm()
    {
        amTim = cam.gameObject.AddComponent<AudioSource>();
        amTim.clip = AmTong.Tim(); amTim.loop = true; amTim.spatialBlend = 0f; amTim.volume = 0.2f; amTim.Play();
        denTruoc = DenMoi("Den_DiTheo", 9f, 1.5f, new Color(1f, 0.78f, 0.52f));
        denSau = DenMoi("Den_SauLung", 5f, 0.45f, new Color(1f, 0.7f, 0.45f));
        // bóng đèn đầu hành lang có sáng thật: lúc quay lại, bóng đen đứng ngược sáng trước bức tường cuối
        var dc = DenMoi("Den_DauHanhLang", 7f, 1.1f, new Color(1f, 0.66f, 0.38f));
        dc.transform.position = GOC + Tam(0.4f) + Vector3.up * (CAO - 0.4f);
    }
    Light DenMoi(string ten, float tam, float manh, Color mau)
    {
        var l = new GameObject(ten).AddComponent<Light>(); l.transform.SetParent(goc, false);
        l.type = LightType.Point; l.range = tam; l.intensity = manh; l.color = mau; l.shadows = LightShadows.None; return l;
    }

    // ═════════════════════════ mở đầu: tỉnh trong hành lang, nghe thở sau lưng, quay lại thấy bóng đen ở cuối, "Chạy."
    IEnumerator Intro()
    {
        if (LocNhim.I && LocNhim.I.khoiTho) AudioSource.PlayClipAtPoint(LocNhim.I.khoiTho, cam.position, 0.6f);
        for (float t = 0; t < 1; t += Time.deltaTime / 1.4f) { LocFirstPerson.den = 1 - t; DatNguoi(); DatMa(); yield return null; }
        LocFirstPerson.den = 0;
        yield return Cho(1.2f);
        if (amMaTho) amMaTho.volume = 1f;
        yield return Cho(0.6f);
        // [13/11] cảnh 1: quay người lại thật chậm, máy quay run nhẹ — chưa có nhạc, chỉ thở và tim đập nhỏ
        if (LocNhim.I && LocNhim.I.khoiTho) PhatMot(LocNhim.I.khoiTho, 0.7f);
        for (float t = 0; t < 1; t += Time.deltaTime / 1.7f)
        {
            yawLech = Mathf.Lerp(0, 175f, t * t * (3 - 2 * t)); DatNguoi(); DatMa();
            cam.localRotation *= Quaternion.Euler(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.7f, 0.7f));
            if (amTim) amTim.volume = Mathf.Lerp(0.2f, 0.3f, t);
            yield return null;
        }
        yield return Cho(0.25f);
        yield return CanhMa();
        if (amMaTho) amMaTho.volume = 0.6f;
        LocFirstPerson.PhuDe("Chạy.", 1.6f);   // không có chữ "giữ W" — lời nhắc chạy duy nhất là "CHẠY ĐI!" lúc sắp bị bắt
    }
    IEnumerator Cho(float giay) { for (float t = 0; t < giay; t += Time.deltaTime) { DatNguoi(); DatMa(); CapNhatDen(Time.deltaTime); yield return null; } }

    // ═════════════════════════ điều khiển
    float tHuongDan, tKhung; bool daHuongDan;
    void DieuKhien(float dt)
    {
        var k = Keyboard.current; var m = Mouse.current;
        coW = k != null && (k.wKey.isPressed || k.upArrowKey.isPressed);
        bool trai = k != null && (k.aKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame);
        bool phai = k != null && (k.dKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame);
        if (bot != Bot.Khong) { coW = bot != Bot.DungYen; trai = phai = false; if (bot == Bot.Ne) { int ne = LanNe(); if (ne < LanGan(lanDich)) trai = true; else if (ne > LanGan(lanDich)) phai = true; } }
        if (trai) lanDich = Mathf.Max(-rongLan, lanDich - rongLan);
        if (phai) lanDich = Mathf.Min(rongLan, lanDich + rongLan);
        x = Mathf.MoveTowards(x, lanDich, rongLan / tgDoiLan * dt);
        float muon = coW ? tocChay : 0f;
        if (tKhung > 0) { tKhung -= dt; muon *= 0.25f; }
        v = Mathf.MoveTowards(v, muon, (muon > v ? tangToc : giamToc) * dt);
        s += v * dt;
        if (m != null && m.leftButton.wasPressedThisFrame && !LocMenu.DangChe) Cursor.lockState = CursorLockMode.Locked;   // LocFirstPerson không khoá lại chuột khi đang khoa
        if (m != null && Cursor.lockState == CursorLockMode.Locked)
        {
            var d = m.delta.ReadValue() * (fp ? fp.look : 0.12f);
            yawLech = Mathf.Clamp(yawLech + d.x, -40f, 40f);
            pitch = Mathf.Clamp(pitch - (LocFirstPerson.daoTruc ? -d.y : d.y), -25f, 25f);
        }
        if (v > 1f) { dauBuoc -= dt; if (dauBuoc <= 0) { dauBuoc = Mathf.Lerp(0.55f, 0.32f, v / tocChay); LocAm.BuocChan(fp.transform.position, 0.55f); } }
        if (v < 1f) tgDung += dt; else tgDung = 0;
        if (!daHuongDan && v > 1f) { daHuongDan = true; tHuongDan = 4.5f; }   // đã biết chạy rồi mới nhắc đổi làn
        if (tHuongDan > 0) tHuongDan -= dt;
    }
    // tự kiểm: làn không bị nhắm
    int LanNe()
    {
        bool[] bi = new bool[3];
        foreach (var n in nem) if (!n.xong && Time.time - n.t0 < baoTruoc) bi[n.lan + 1] = true;
        int hien = LanGan(lanDich); if (!bi[hien + 1]) return hien;
        int tot = hien; float g = 9;
        for (int l = -1; l <= 1; l++) if (!bi[l + 1] && Mathf.Abs(l - hien) < g) { g = Mathf.Abs(l - hien); tot = l; }
        return tot;
    }

    void DatNguoi()
    {
        var t = Tiep(s);
        var chan = GOC + Tam(s) + Phai(t) * x;
        fp.transform.SetPositionAndRotation(chan, Quaternion.LookRotation(t));
        float bob = v > 0.5f ? Mathf.Abs(Mathf.Sin(s * 1.9f)) * 0.045f * (v / tocChay) * LocFirstPerson.lac : 0f;   // theo cài đặt "lắc camera" (0 = tắt)
        var lac = rung > 0 ? Random.insideUnitSphere * rung * 0.05f : Vector3.zero;
        cam.localPosition = new Vector3(0, matCao + bob - 0.02f, 0) + lac;
        cam.localRotation = Quaternion.Euler(pitch + (rung > 0 ? Random.Range(-1f, 1f) * rung * 3f : 0), yawLech, (x - lanDich) * 4f);
        if (rung > 0) rung = Mathf.Max(0, rung - Time.deltaTime * 1.6f);
        if (toiChop > 0) toiChop = Mathf.Max(0, toiChop - Time.deltaTime);
        if (denTruoc) denTruoc.transform.position = chan + t * 3.2f + Vector3.up * 2.1f;
        if (denSau) denSau.transform.position = chan - t * 1.5f + Vector3.up * 2.0f;
    }

    // ═════════════════════════ ma đuổi: giữ ~8 m (gần thêm 2 m mỗi lần trúng); đứng yên thì nó lướt tới
    void CapNhatMa(float dt)
    {
        float dich = khoangMa - 2f * soTrung;
        if (dai - s < 6f * tocChay) dich = Mathf.Max(2.5f, dich - 2f);   // 6 s cuối: thôi ném, nó áp sát hơn
        float vMa = v >= 1f ? v + Mathf.Clamp((gap - dich) * 0.8f, -0.6f, 3f) : tocMaKhiDung + Mathf.Clamp((gap - dich) * 0.5f, 0f, 2f);
        vMa += maNhanTheoNhip * NhipNhan();   // [13/11] lướt dồn theo nhịp nhấn của nhạc
        gap -= (vMa - v) * dt;
        gap = Mathf.Min(gap, 16f);
        canhBao = !coW && v < 1.5f && gap < canhBaoKhi;   // chỉ khi đứng yên (không giữ W) mà nó sắp tới nơi — lúc khựng vì trúng đồ thì không
        if (canhBao && !daCanhBao) { daCanhBao = true; tCanhBao = tChase; }
        if (v < 1f) { if (dungTu < 0) dungTu = tChase; } else dungTu = -1f;
        DatMa();
        if (amMaTho) amMaTho.volume = Mathf.Lerp(1f, 0.35f, Mathf.InverseLerp(2f, 10f, gap));
        if (amTim) { float g = Mathf.InverseLerp(10f, 1.5f, gap); amTim.volume = 0.18f + 0.6f * g; amTim.pitch = 1f + 0.9f * g; }
        foreach (var (t, sb) in bongLoi) if (t && t.gameObject.activeSelf && s > sb - 4f) t.gameObject.SetActive(false);   // bóng trong lối giả: tới gần thì không còn ở đó
    }

    void DatMa()
    {
        if (!ma) return;
        float sm = s - gap; var t = Tiep(sm);
        ma.position = GOC + Tam(sm) + Phai(t) * Mathf.Lerp(0, x, 0.5f) + Vector3.up * (0.06f + Mathf.Sin(Time.time * 2.1f) * 0.04f);
        ma.rotation = Quaternion.LookRotation(t);
    }

    // ═════════════════════════ ném đồ
    void CapNhatNem(float dt)
    {
        bool ganCua = dai - s < 6f * tocChay;
        if (!ganCua && tChase >= tNem)
        {
            float nhip = tChase < 20f ? 4f : tChase < 60f ? 3f : 2.2f;
            tNem = tChase + nhip;
            int hien = LanGan(x);
            int a = Random.value < 0.7f ? hien : Ke(hien);
            NemMoi(a);
            if (tChase >= 60f && Random.value < 0.35f)
            {
                int b; do b = Random.Range(-1, 2); while (b == a);
                NemMoi(b); soNemDoi++;
            }
            bool[] bi = new bool[3]; foreach (var n in nem) if (!n.xong && Time.time - n.t0 < baoTruoc) bi[n.lan + 1] = true;
            if (bi[0] && bi[1] && bi[2]) luonCoLanTrong = false;
        }
        for (int i = nem.Count - 1; i >= 0; i--) BayNem(nem[i], dt);
    }
    static int Ke(int lan) => lan == 0 ? (Random.value < 0.5f ? -1 : 1) : 0;

    void NemMoi(int lan)
    {
        soNem++;
        DoVat chon = null;
        foreach (var d in doVat) if (!d.daNem && d.nemDuoc && d.t && d.s > s - 10f && d.s < s - 2.5f) { chon = d; break; }
        Transform t;
        if (chon != null) { chon.daNem = true; t = chon.t; }
        else
        {
            var rd = new System.Random(soNem * 31 + 7);
            var g = DoTrongNha(nguonNem, rd, Tam(s - 6f) + Phai(Tiep(s - 6f)) * (Random.value < 0.5f ? -2f : 2f), Tiep(s - 6f), false);
            if (!g) g = Khoi(PrimitiveType.Cube, GOC + Tam(s - 6f) + Vector3.up * 0.2f, new Vector3(0.4f, 0.4f, 0.4f), vlDay);
            t = g.transform;
        }
        var n = new Nem { t = t, lan = lan, t0 = Time.time, pDat = t.position, q0 = t.rotation };
        // tiếng kéo lê lúc đồ tự nhấc lên: tiếng kéo hũ (Hu_KeoHu, LocHuCot.keo) — không có thì tiếng cửa kẽo kẹt hạ giọng
        if (!daTimKeo) { daTimKeo = true; foreach (var h in FindObjectsByType<LocHuCot>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (h.keo) { amKeo = h.keo; break; } }
        if (amKeo) { n.keo = t.gameObject.AddComponent<AudioSource>(); n.keo.clip = amKeo; n.keo.spatialBlend = 0.8f; n.keo.minDistance = 1.5f; n.keo.maxDistance = 16f; n.keo.volume = 0.7f; n.keo.pitch = Random.Range(1.1f, 1.35f); n.keo.Play(); }
        else LocAm.Phat("Cua_Ken", t.position, 0.45f, 0.55f);
        n.am = t.gameObject.AddComponent<AudioSource>(); n.am.clip = AmTong.Gio(); n.am.spatialBlend = 0.6f; n.am.minDistance = 2f; n.am.maxDistance = 20f; n.am.volume = 0.9f; n.am.Play();
        nem.Add(n);
    }

    void BayNem(Nem n, float dt)
    {
        if (!n.t) { nem.Remove(n); return; }
        float tt = Time.time - n.t0, nhac = 0.35f;
        if (!n.xong)
        {
            if (tt >= nhac && n.keo && n.keo.isPlaying) n.keo.Stop();   // nhấc khỏi sàn rồi: hết tiếng kéo, còn tiếng gió
            if (tt < nhac)
            {
                float e = tt / nhac;
                n.t.position = n.pDat + Vector3.up * 1.0f * e * e + Random.insideUnitSphere * 0.03f;
                n.pNhac = n.t.position;
                return;
            }
            float u = Mathf.Clamp01((tt - nhac) / (baoTruoc - nhac));
            var dich = Diem(s + 0.4f, LanX(n.lan), 1.05f);
            var p = Vector3.Lerp(n.pNhac, dich, u) + Vector3.up * Mathf.Sin(Mathf.PI * u) * 0.9f;
            n.vBay = (p - n.t.position) / Mathf.Max(dt, 1e-3f);
            n.t.position = p;
            n.t.rotation = Quaternion.Euler(tt * 410f, tt * 230f, 0) * n.q0;
            if (u >= 1f)
            {
                n.xong = true;
                if (Mathf.Abs(x - LanX(n.lan)) < 0.6f) Trung(n);
                else AudioSource.PlayClipAtPoint(AmTong.Roi(), n.t.position, 0.8f);
                n.vBay = Vector3.ClampMagnitude(n.vBay, 7f);
            }
            return;
        }
        if (n.daDat) { if (Time.time - n.tDat > 4f) nem.Remove(n); return; }
        n.vBay += Physics.gravity * dt;
        var q = n.t.position + n.vBay * dt;
        float san = GOC.y + 0.15f;
        if (q.y < san) { q.y = san; n.vBay = new Vector3(n.vBay.x * 0.45f, -n.vBay.y * 0.25f, n.vBay.z * 0.45f); if (n.vBay.magnitude < 0.6f) { n.daDat = true; n.tDat = Time.time; } }
        n.t.position = q;
    }

    void Trung(Nem n)
    {
        soTrung++;
        rung = 1f; toiChop = 0.28f; tKhung = 0.5f;
        AudioSource.PlayClipAtPoint(AmTong.Roi(), cam.position, 1f);
        if (LocNhim.I && LocNhim.I.khoiTho) AudioSource.PlayClipAtPoint(LocNhim.I.khoiTho, cam.position, 0.5f);
        n.vBay = n.vBay * -0.2f + Vector3.up * 1.5f;
    }

    void CapNhatDen(float dt)
    {
        foreach (var (r, hz, pha) in chapChon)
        {
            if (!r) continue;
            bool sang = Mathf.PerlinNoise(Time.time * hz * 0.3f, pha) > 0.32f;
            // bóng gần chớp sáng lại thì rè một tiếng (Dien_Chap của LocNhim), tối đa ~1 lần / giây
            if (sang && !r.enabled && fp && Time.time - tChapCuoi > 1f && LocNhim.I && LocNhim.I.dienChap && (r.transform.position - fp.transform.position).sqrMagnitude < 36f)
            { tChapCuoi = Time.time; AudioSource.PlayClipAtPoint(LocNhim.I.dienChap, r.transform.position, 0.25f); }
            r.enabled = sang;
        }
    }

    // ═════════════════════════ thoát: tới cửa sắt
    IEnumerator Thoat()
    {
        ketThuc = true; ketThoat = true;
        foreach (var n in nem) if (n.am) n.am.Stop();
        StartCoroutine(TatNhac(1.6f)); if (amU) amU.Stop();
        var cuoiS = dai - 0.9f;
        float s0 = s, x0 = x;
        for (float t = 0; t < 1; t += Time.deltaTime / 0.45f) { s = Mathf.Lerp(s0, cuoiS, t); x = Mathf.Lerp(x0, 0, t); lanDich = 0; yawLech = Mathf.Lerp(yawLech, 0, t); pitch = Mathf.Lerp(pitch, 0, t); DatNguoi(); yield return null; }
        var d = Tiep(dai); var cua = Diem(dai, -benBanLe * 0.22f, 1.1f);   // đẩy phía mép tự do
        if (LocTay.I) LocTay.I.StartCoroutine(LocTay.HaiTay(cua, 0.35f, "Xoe"));   // chạy trên LocTay: giấc mơ bị xoá cũng không kẹt tay trên màn hình
        var q0 = cuaBanLe.rotation; var qMo = QCua(d, 82f);
        for (float t = 0; t < 1; t += Time.deltaTime / 0.35f) { cuaBanLe.rotation = Quaternion.Slerp(q0, qMo, t * t); yield return null; }
        // lao vào buồng
        var p0 = fp.transform.position; var p1 = GOC + Tam(dai) + d * 1.7f;
        for (float t = 0; t < 1; t += Time.deltaTime / 0.5f) { fp.transform.position = Vector3.Lerp(p0, p1, t); cam.localPosition = new Vector3(0, matCao, 0); yield return null; }
        // quay lại, sập cửa
        var r0 = fp.transform.rotation; var r1 = Quaternion.LookRotation(-d);
        var maDich = GOC + Tam(dai - 0.7f);
        void MaLao() { if (ma) { ma.position = Vector3.MoveTowards(ma.position, maDich, Time.deltaTime * 9f); ma.rotation = Quaternion.LookRotation(d); } }   // nó lao theo tới cửa
        for (float t = 0; t < 1; t += Time.deltaTime / 0.4f) { fp.transform.rotation = Quaternion.Slerp(r0, r1, t * (2 - t)); cam.localRotation = Quaternion.identity; MaLao(); yield return null; }
        var qDong = QCua(d, 0f);
        for (float t = 0; t < 1; t += Time.deltaTime / 0.18f) { cuaBanLe.rotation = Quaternion.Slerp(qMo, qDong, t * t); MaLao(); yield return null; }
        if (ma) foreach (var r in ma.GetComponentsInChildren<Renderer>()) r.enabled = false;   // cửa đã đóng: không cần thấy nữa, tiếng thở vẫn còn sau cánh cửa
        cuaBanLe.rotation = qDong;
        LocAm.Phat("Cua_Dong", cuaBanLe.position, 1f, 0.8f); LocAm.Phat("Cong_Sat", cuaBanLe.position, 0.9f);
        StartCoroutine(Lac(0.35f, 0.6f));
        if (khoaTreo) StartCoroutine(LacKhoa());
        LocAm.Phat("OKhoa_Mo", cuaBanLe.position, 0.7f, 0.85f);
        if (amMaTho) amMaTho.volume = 1f;
        yield return new WaitForSeconds(0.9f);
        for (int i = 0; i < 2; i++)   // ma đập cửa hai cái
        {
            LocAm.Phat("Cong_Sat", cuaBanLe.position, 1f, 0.7f);
            AudioSource.PlayClipAtPoint(AmTong.Dap(), cuaBanLe.position, 1f);
            StartCoroutine(Lac(0.3f, 1f));
            var qa = cuaBanLe.rotation;
            for (float t = 0; t < 1; t += Time.deltaTime / 0.15f) { cuaBanLe.rotation = Quaternion.Slerp(qa, QCua(d, 2.5f), Mathf.Sin(t * Mathf.PI)); yield return null; }   // cánh nảy vào phía trong
            cuaBanLe.rotation = qa;
            yield return new WaitForSeconds(1.0f);
        }
        if (amMaTho) amMaTho.Stop(); if (amTim) amTim.volume = 0.12f;
        if (denTruoc) denTruoc.enabled = false; if (denSau) denSau.enabled = false;
        LocFirstPerson.den = 1f;
        yield return new WaitForSeconds(2.2f);
        // trong bóng tối, sát sau lưng (phía bên này cánh cửa): tiếng thở khò khè của Nhím rồi mới tới câu gọi — chưa có giọng đọc, chỉ phụ đề
        if (LocNhim.I && LocNhim.I.tho) AudioSource.PlayClipAtPoint(LocNhim.I.tho, cam.position - fp.transform.forward * 1.1f + Vector3.down * 0.3f, 0.55f);
        yield return new WaitForSeconds(0.8f);
        LocFirstPerson.PhuDe("NHÍM: Anh Khôi ơi.", 2.6f, false);
        yield return new WaitForSeconds(3.0f);
    }
    IEnumerator Lac(float giay, float manh) { for (float t = 0; t < giay; t += Time.deltaTime) { cam.localPosition = new Vector3(0, matCao, 0) + Random.insideUnitSphere * 0.03f * manh; yield return null; } cam.localPosition = new Vector3(0, matCao, 0); }
    IEnumerator LacKhoa() { var r0 = khoaTreo.localRotation; for (float t = 0; t < 1.2f; t += Time.deltaTime) { if (!khoaTreo) yield break; khoaTreo.localRotation = r0 * Quaternion.Euler(0, 0, Mathf.Sin(t * 30f) * 18f * (1 - t / 1.2f)); yield return null; } }

    // ═════════════════════════ [13/11] mặt ma cận cảnh (lúc quay lại nhìn + lúc bị bắt)
    //  Bản sao model ma nữ GIỮ vật liệu thật (mặt, tóc, mắt như trong nhà) thay cho bóng khói; miệng há được bằng cách bẻ hàm ngay trên lưới
    //  (đỉnh dưới đường miệng, phía trước mặt quay quanh khớp hàm). Hai tay: model Ma_Tay (tay trái vươn) + một bản lật gương chỉ để lấy tay phải.
    class Ham { public Mesh m; public Vector3[] v0, v; public float[] w; public Vector3 tam, truc; }
    readonly List<Ham> ham = new();
    readonly List<Mesh> luoiTao = new();
    readonly List<GameObject> matMaTao = new();
    Light denMat; Camera camC; float fov0 = -1f, near0 = -1f;

    Transform TaoMaMat(int kieu, string ten, bool chiTay, bool guong)
    {
        var lm = LocMa.I;
        if (!lm || lm.mau == null || lm.mau.Length <= kieu || !lm.mau[kieu]) return null;
        var w = new GameObject(ten).transform; w.SetParent(goc, false);
        var m = Instantiate(lm.mau[kieu], w); m.SetActive(true); m.transform.localPosition = Vector3.zero;
        var dau = Tim(m.transform, "Dau"); var mL = Tim(m.transform, "Mat_L"); var mR = Tim(m.transform, "Mat_R");
        if (dau && mL && mR && ((mL.position + mR.position) / 2 - dau.position).z < 0) m.transform.localRotation = Quaternion.Euler(0, 180, 0) * m.transform.localRotation;
        Material vlMat = lm.matMat ? Moi(new Material(lm.matMat), "MaMat_Mat") : null;
        foreach (var r in m.GetComponentsInChildren<Renderer>(true))
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
            bool phanDau = LaMat(r.name) || r.name.StartsWith("Mat") || r.name is "DaDau" or "Toc" or "Rang";
            if (r.name == "TayAo_R" || (chiTay && (phanDau || r.name == "Vay"))) { r.enabled = false; continue; }   // tay phải buông thõng: bỏ (bản lật gương lo tay bên kia)
            if (r.name.StartsWith("Mat_") && vlMat) r.sharedMaterial = vlMat;
        }
        DonThanhPhan(m);
        if (guong) w.localScale = new Vector3(-1f, 1f, 1f);
        matMaTao.Add(w.gameObject);
        return w;
    }

    // tâm hai mắt (thế giới) và hướng mặt của một bản mặt ma
    static bool MatVaHuong(Transform w, out Vector3 giua, out Vector3 truoc)
    {
        giua = w.position + Vector3.up * 1.6f; truoc = w.forward;
        var mL = Tim(w, "Mat_L"); var mR = Tim(w, "Mat_R"); if (!mL || !mR) return false;
        giua = (mL.position + mR.position) / 2;
        var hu = Tim(w, "MatHuong"); if (hu && (hu.position - giua).sqrMagnitude > 1e-6f) truoc = (hu.position - giua).normalized;
        return true;
    }

    // chuẩn bị bẻ hàm trên MatMa / DaDau / Rang của bản mặt w (tư thế hiện tại)
    void ChuanBiHam(Transform w)
    {
        ham.Clear();
        if (!MatVaHuong(w, out var giua, out var f)) return;
        Renderer rang = null; foreach (var r in w.GetComponentsInChildren<Renderer>(true)) if (r.name == "Rang") rang = r;
        var up = Vector3.ProjectOnPlane(w.up, f).normalized; var phai = Vector3.Cross(up, f).normalized;
        var mieng = rang ? rang.bounds.center : giua - up * 0.075f + f * 0.02f;
        var tam = mieng - f * 0.075f + up * 0.035f;   // khớp hàm: sau và trên miệng
        float sg = Vector3.Dot(Quaternion.AngleAxis(10f, phai) * f, up) < 0 ? 1f : -1f;   // chiều quay làm cằm đi xuống
        foreach (var mf in w.GetComponentsInChildren<MeshFilter>(true))
        {
            if (!(mf.name is "MatMa" or "DaDau" or "Rang") || !mf.sharedMesh || !mf.sharedMesh.isReadable) continue;
            var m = Instantiate(mf.sharedMesh); luoiTao.Add(m); mf.sharedMesh = m;
            var v0 = m.vertices; var wt = new float[v0.Length]; bool co = false;
            for (int i = 0; i < v0.Length; i++)
            {
                var p = mf.transform.TransformPoint(v0[i]);
                float d = Vector3.Dot(p - mieng, up), e = Vector3.Dot(p - tam, f);
                float a = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.006f, -0.012f, d));   // dưới đường miệng
                float b = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-0.03f, 0.025f, e));    // nửa trước của đầu
                float c = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-0.17f, -0.10f, d));    // tắt dần xuống cổ
                wt[i] = a * b * c; if (wt[i] > 0) co = true;
            }
            if (!co) continue;
            ham.Add(new Ham { m = m, v0 = v0, v = new Vector3[v0.Length], w = wt, tam = mf.transform.InverseTransformPoint(tam), truc = mf.transform.InverseTransformDirection(phai * sg).normalized });
        }
        // lòng miệng tối: khối đen sau môi, để há ra không nhìn xuyên đầu
        if (vlDen)
        {
            var k = Khoi(PrimitiveType.Sphere, mieng - f * 0.035f - up * 0.015f, new Vector3(0.075f, 0.07f, 0.06f), vlDen, "LongMieng");
            if (k) k.transform.SetParent(w, true);
        }
    }
    void HaMieng(float goc)
    {
        foreach (var h in ham)
        {
            for (int i = 0; i < h.v0.Length; i++) h.v[i] = h.w[i] <= 0 ? h.v0[i] : h.tam + Quaternion.AngleAxis(goc * h.w[i], h.truc) * (h.v0[i] - h.tam);
            h.m.vertices = h.v; h.m.RecalculateBounds();
        }
    }
    void BatHinh(Transform t, bool bat) { if (!t) return; foreach (var r in t.GetComponentsInChildren<Renderer>(true)) r.enabled = bat && !LaMat(r.name); }
    Light DenRoiMat(LightType kieu)
    {
        var l = new GameObject("Den_MatMa").AddComponent<Light>(); l.transform.SetParent(goc, false);
        l.type = kieu; l.color = new Color(0.78f, 0.84f, 1f); l.shadows = LightShadows.None; l.intensity = 0f;
        if (kieu == LightType.Spot) { l.range = 30f; l.spotAngle = 14f; } else l.range = 3f;
        return l;
    }
    void DonMatMa()
    {
        foreach (var g in matMaTao) if (g) Destroy(g);
        matMaTao.Clear(); ham.Clear();
        foreach (var m in luoiTao) if (m) Destroy(m);
        luoiTao.Clear();
        if (denMat) Destroy(denMat.gameObject);
        if (camC) { if (fov0 > 0) camC.fieldOfView = fov0; if (near0 > 0) camC.nearClipPlane = near0; }
    }
    void PhatMot(AudioClip c, float vol) { if (!c) return; var a = cam.gameObject.AddComponent<AudioSource>(); a.clip = c; a.spatialBlend = 0; a.volume = vol; a.Play(); Destroy(a, c.length + 0.1f); }

    // ═════════════════════════ [13/11] nhạc rượt: phát từ giây 0 lúc bắt đầu zoom, chạy liền mạch, tới giây 66 quay về giây 3
    AudioSource amNhac, amU; float trang;
    void BatNhac()
    {
        var c = Resources.Load<AudioClip>("LOC_AmThanh/GM_NhacRuot"); if (!c) return;
        if (!amNhac) { amNhac = goc.gameObject.AddComponent<AudioSource>(); amNhac.playOnAwake = false; amNhac.spatialBlend = 0f; amNhac.loop = false; }
        amNhac.clip = c; amNhac.volume = 0.9f; amNhac.timeSamples = 0; amNhac.Play();
    }
    void CapNhatNhac()
    {
        if (!amNhac || !amNhac.clip || !amNhac.isPlaying) return;
        int f = amNhac.clip.frequency;
        if (amNhac.timeSamples >= (int)(nhacLapDen * f)) amNhac.timeSamples = (int)(nhacLapTu * f);
    }
    // 0..1 ngay sau mỗi nhịp nhấn (mỗi ~4,5 s tính từ cú đánh)
    float NhipNhan()
    {
        if (!amNhac || !amNhac.isPlaying || amNhac.time < nhipDau) return 0f;
        float ph = (amNhac.time - nhipDau) % nhipNhan;
        return ph < 0.4f ? 1f - ph / 0.4f : 0f;
    }
    IEnumerator TatNhac(float giay) { if (!amNhac) yield break; float v0 = amNhac.volume; for (float t = 0; t < giay && amNhac; t += Time.deltaTime) { amNhac.volume = v0 * (1 - t / giay); yield return null; } if (amNhac) amNhac.Stop(); }
    // ù tai: âm sin cao ~4,1 kHz rung nhẹ, lặp (tạo một lần)
    static AudioClip clipU;
    void BatUTai()
    {
        if (!clipU)
        {
            int hz = 44100, n = hz * 2; var d = new float[n];
            for (int i = 0; i < n; i++) { float t = i / (float)hz; d[i] = Mathf.Sin(2 * Mathf.PI * 4100f * t) * (0.6f + 0.4f * Mathf.Sin(2 * Mathf.PI * 0.5f * t)) * 0.5f; }
            clipU = AudioClip.Create("GM_UTai", n, 1, hz, false); clipU.SetData(d, 0);
        }
        if (!amU) { amU = cam.gameObject.AddComponent<AudioSource>(); amU.playOnAwake = false; amU.spatialBlend = 0; amU.loop = true; }
        amU.clip = clipU; amU.volume = 0f; amU.Play();
    }

    // cảnh 1 (2–5): cắt sang góc quay con ma đứng im trong bóng tối → zoom cận 3 s theo nhạc → giây 3: nó giật quay đầu nhìn thẳng ống kính, chớp trắng
    //  → cắt về mắt Khôi, quay ngoắt về phía trước, rung mạnh 0,3 s
    IEnumerator CanhMa()
    {
        camC = cam.GetComponent<Camera>();
        var mm = (camC && ma) ? TaoMaMat(3, "Ma_GiacMo_Mat", false, false) : null;
        if (!mm) { BatNhac(); yield return Cho(tgZoom); yield break; }
        if (fov0 < 0) fov0 = camC.fieldOfView; if (near0 < 0) near0 = camC.nearClipPlane;
        foreach (var r in mm.GetComponentsInChildren<Renderer>(true)) if (LaMat(r.name) && r.name != "Rang") r.enabled = false;   // mắt trũng đen: bỏ nhãn cầu, còn hốc mắt (giữ răng)
        var up = Vector3.up;
        var veNguoi = Vector3.ProjectOnPlane(fp.transform.position - ma.position, up).normalized;
        var qNhin = Quaternion.LookRotation(veNguoi, up);
        var qQuay = qNhin * Quaternion.Euler(16f, 80f, 10f);   // đầu cúi, ngoảnh sang bên — chưa nhìn mình
        mm.SetPositionAndRotation(ma.position, qQuay);
        ChuanBiHam(mm);
        BatHinh(ma, false);
        MatVaHuong(mm, out var mat0, out _);
        denMat = DenRoiMat(LightType.Point); denMat.range = 3.2f; denMat.color = new Color(0.72f, 0.78f, 0.95f);
        var mL = Tim(mm, "Mat_L"); var mR = Tim(mm, "Mat_R");
        Vector3 Mat() => mL && mR ? (mL.position + mR.position) / 2 : mat0;
        var phai = Vector3.Cross(up, veNguoi).normalized;
        Vector3 camA = ma.position + veNguoi * 3.3f + phai * 0.55f + up * 1.45f;
        Vector3 camB = Mat() + (camA - Mat()).normalized * 1.25f;
        void RoiMat(float a) { denMat.transform.position = Mat() + veNguoi * 0.8f + up * 0.6f + phai * 0.3f; denMat.intensity = Random.value < 0.18f ? a * 0.2f : a * Random.Range(0.75f, 1.1f); }
        // 2. cắt cảnh: góc máy đứng trước mặt nó, nó đứng im, đèn mờ nhấp nháy
        cam.position = camA; cam.rotation = Quaternion.LookRotation(Mat() - camA, up); camC.fieldOfView = 42f;
        for (float t = 0; t < 0.9f; t += Time.deltaTime) { RoiMat(0.7f); cam.position = camA + new Vector3(Mathf.Sin(t * 1.3f), Mathf.Sin(t * 1.7f), 0) * 0.01f; yield return null; }
        // 3. zoom cận đúng tgZoom giây — nhạc bắt đầu từ giây 0 ngay lúc này; tim đập + ù tai to dần dưới nhạc
        BatNhac(); BatUTai();
        float tz = 0, hm = 0;
        while (tz < tgZoom)
        {
            tz = amNhac && amNhac.isPlaying ? Mathf.Min(amNhac.time, tgZoom) : tz + Time.deltaTime;
            float e = tz / tgZoom, es = e * e * (3 - 2 * e);
            cam.position = Vector3.Lerp(camA, camB, es) + Random.insideUnitSphere * 0.002f * (1 + es * 2);
            cam.rotation = Quaternion.LookRotation(Mat() - cam.position, up);
            float kc = Vector3.Distance(cam.position, Mat());
            camC.fieldOfView = Mathf.Lerp(42f, Mathf.Clamp(2f * Mathf.Atan(0.19f / kc) * Mathf.Rad2Deg, 6f, 42f), es);
            hm = Mathf.Lerp(0f, 14f, es) + Mathf.Sin(tz * 7f) * 1.5f * es; HaMieng(hm);   // miệng méo, hé dần
            if (Random.value < 0.02f) mm.rotation = qQuay * Quaternion.Euler(Random.Range(-3f, 3f), Random.Range(-3f, 3f), Random.Range(-5f, 5f)); else mm.rotation = Quaternion.Slerp(mm.rotation, qQuay, 0.2f);
            if (amTim) { amTim.volume = Mathf.Lerp(0.3f, 0.85f, es); amTim.pitch = Mathf.Lerp(1f, 1.6f, es); }
            if (amU) amU.volume = Mathf.Lerp(0f, 0.16f, es);
            RoiMat(Mathf.Lerp(0.7f, 1.3f, es));
            if (tz >= tgZoom) break;
            yield return null;
        }
        // 4. cú đánh (giây 3): nó giật quay đầu nhìn thẳng ống kính, miệng bạnh ra, máy rung, chớp trắng một khung
        var qMat = Quaternion.LookRotation(Vector3.ProjectOnPlane(cam.position - mm.position, up).normalized, up) * Quaternion.Euler(-6f, 0, 0);
        trang = 1f; rung = 1f; if (amU) amU.volume = 0.05f;
        for (float t = 0; t < 1; t += Time.deltaTime / 0.07f) { mm.rotation = Quaternion.Slerp(qQuay, qMat, t); HaMieng(Mathf.Lerp(hm, 34f, t)); yield return null; }
        mm.rotation = qMat;
        var c0 = cam.position;
        for (float t = 0; t < 0.4f; t += Time.deltaTime)
        {
            trang = t < 0.04f ? 1f : 0f;
            cam.position = c0 + Random.insideUnitSphere * 0.03f * (1 - t / 0.4f);
            cam.rotation = Quaternion.LookRotation(Mat() - cam.position, up) * Quaternion.Euler(Random.Range(-2f, 2f), Random.Range(-2f, 2f), Random.Range(-3f, 3f));
            if (Random.value < 0.08f) mm.rotation = qMat * Quaternion.Euler(0, 0, Random.Range(-8f, 8f)); else mm.rotation = Quaternion.Slerp(mm.rotation, qMat, 0.3f);
            RoiMat(2.4f);
            yield return null;
        }
        trang = 0;
        // 5. cắt về mắt Khôi: quay ngoắt về phía trước, rung mạnh ~0,3 s
        DonMatMa(); BatHinh(ma, true); camC.fieldOfView = fov0;
        if (amTim) { amTim.volume = 0.45f; amTim.pitch = 1.3f; }
        if (amU) StartCoroutine(TatAm(amU, 1.2f));
        float y0 = yawLech, p0 = pitch;
        for (float t = 0; t < 1; t += Time.deltaTime / 0.18f) { float e = 1 - (1 - t) * (1 - t); yawLech = Mathf.Lerp(y0, 0f, e); pitch = Mathf.Lerp(p0, 0f, e); rung = 1.2f; DatNguoi(); DatMa(); yield return null; }
        yawLech = 0; pitch = 0; rung = 1.2f;
        for (float t = 0; t < 0.3f; t += Time.deltaTime) { DatNguoi(); DatMa(); CapNhatNhac(); yield return null; }
    }
    IEnumerator TatAm(AudioSource a, float giay) { float v0 = a ? a.volume : 0; for (float t = 0; t < giay && a; t += Time.deltaTime) { a.volume = v0 * (1 - t / giay); yield return null; } if (a) a.Stop(); }

    // tay vồ: một bản Ma_Tay chỉ giữ cánh tay trái đang vươn (tay áo + da tay + móng), cắt bỏ phần còn lại của lưới.
    //  Trả về điểm vai và điểm bàn tay (thế giới, theo tư thế lúc tạo) để đặt tay vào rìa khung hình.
    Transform TaoTay(string ten, out Vector3 vai, out Vector3 banTay)
    {
        vai = banTay = Vector3.zero;
        var h = TaoMaMat(1, ten, true, false); if (!h) return null;
        h.gameObject.SetActive(true);
        MeshFilter mong = null, ao = null;
        foreach (var mf in h.GetComponentsInChildren<MeshFilter>(true)) { if (mf.name == "Mong") mong = mf; else if (mf.name == "TayAo_L") ao = mf; }
        if (!mong || !ao || !mong.sharedMesh || !ao.sharedMesh || !mong.sharedMesh.isReadable || !ao.sharedMesh.isReadable) { Destroy(h.gameObject); return null; }
        var truc = h.position + Vector3.up * 1.35f;   // trục thân (ngang vai)
        // bàn tay đang vươn: móng xa trục thân nhất
        var vm = mong.sharedMesh.vertices; Vector3 xa = Vector3.zero; float dx = -1;
        foreach (var v in vm) { var p = mong.transform.TransformPoint(v); float d = Vector3.ProjectOnPlane(p - truc, Vector3.up).magnitude; if (d > dx) { dx = d; xa = p; } }
        Vector3 tong = Vector3.zero; int n = 0;
        foreach (var v in vm) { var p = mong.transform.TransformPoint(v); if ((p - xa).sqrMagnitude < 0.15f * 0.15f) { tong += p; n++; } }
        banTay = n > 0 ? tong / n : xa;
        // vai: đỉnh tay áo gần trục thân nhất
        var va = ao.sharedMesh.vertices; float dv = 1e9f;
        foreach (var v in va) { var p = ao.transform.TransformPoint(v); float d = (p - truc).sqrMagnitude; if (d < dv) { dv = d; vai = p; } }
        // cắt lưới: chỉ giữ tam giác nằm quanh đoạn vai → bàn tay (kéo dài qua bàn tay 0,14 m)
        var A = vai; var B = banTay + (banTay - vai).normalized * 0.14f;
        foreach (var mf in h.GetComponentsInChildren<MeshFilter>(true))
        {
            var r = mf.GetComponent<Renderer>(); if (!r || !r.enabled) continue;
            if (!(mf.name is "Than" or "TayAo_L" or "Mong") || !mf.sharedMesh || !mf.sharedMesh.isReadable) { r.enabled = false; continue; }
            float ban = mf.name == "TayAo_L" ? 0.2f : 0.11f;
            var m = Instantiate(mf.sharedMesh); luoiTao.Add(m);
            var vv = m.vertices; var w = new Vector3[vv.Length]; for (int i = 0; i < vv.Length; i++) w[i] = mf.transform.TransformPoint(vv[i]);
            bool Gan(int i) { var ab = B - A; float t = Mathf.Clamp01(Vector3.Dot(w[i] - A, ab) / ab.sqrMagnitude); return (w[i] - (A + ab * t)).sqrMagnitude < ban * ban; }
            for (int sm = 0; sm < m.subMeshCount; sm++)
            {
                var tr = m.GetTriangles(sm); var giu = new List<int>(tr.Length);
                for (int i = 0; i + 2 < tr.Length; i += 3) if (Gan(tr[i]) && Gan(tr[i + 1]) && Gan(tr[i + 2])) { giu.Add(tr[i]); giu.Add(tr[i + 1]); giu.Add(tr[i + 2]); }
                m.SetTriangles(giu, sm);
            }
            mf.sharedMesh = m;
        }
        return h;
    }
    // đặt cánh tay h sao cho bàn tay ở 'dich', cánh tay chạy theo hướng 'huong' (từ vai tới bàn tay)
    static void DatTay(Transform h, Vector3 vai0, Vector3 tay0, Quaternion q0, Vector3 p0, Vector3 dich, Vector3 huong, float lac)
    {
        var a0 = tay0 - vai0;
        var q = Quaternion.AngleAxis(lac, huong) * Quaternion.FromToRotation(a0, huong);
        h.rotation = q * q0;
        var tayMoi = q * (tay0 - p0);   // vị trí bàn tay so với gốc sau khi xoay
        h.position = dich - tayMoi;
    }

    // cảnh 2: bị bắt → nó lao sát, mặt cận rõ; hai tay thò vào hai rìa khung hình bóp chặt đầu; há miệng rộng bất thường, ập vào — máy quay chui vào miệng
    IEnumerator VoMat()
    {
        camC = cam.GetComponent<Camera>();
        var lm = LocMa.I; var thetClip = lm ? lm.tiengThet : null; var danh = Resources.Load<AudioClip>("LOC_AmThanh/GM_NhacDanh");
        var chinh = camC ? TaoMaMat(3, "Ma_GiacMo_Vo", false, false) : null;
        if (!chinh)
        {
            if (danh) PhatMot(danh, 1f); if (thetClip) PhatMot(thetClip, 1f);
            for (float t = 0; t < 0.65f; t += Time.deltaTime) { nhieu = t / 0.65f; rung = 1.4f; yield return null; }
            yield break;
        }
        foreach (var r in chinh.GetComponentsInChildren<Renderer>(true)) if ((LaMat(r.name) && r.name != "Rang") || r.name is "Than" or "TayAo_L" or "TayAo_R") r.enabled = false;   // hốc mắt trũng đen như lúc đầu · cận mặt: bỏ thân và tay buông (tay vồ là hai bản riêng)
        if (fov0 < 0) fov0 = camC.fieldOfView; if (near0 < 0) near0 = camC.nearClipPlane;
        camC.nearClipPlane = 0.012f; camC.fieldOfView = fov0;
        // mặt úp xuống mặt mình: mặt hướng vào ống kính, đỉnh đầu theo phía trên khung hình
        var q = Quaternion.LookRotation(-cam.forward, cam.up);
        chinh.SetPositionAndRotation(Vector3.zero, q);
        MatVaHuong(chinh, out var giua0, out _);
        var lech = Quaternion.Inverse(q) * (giua0 - chinh.position);
        Vector3 Mieng() { foreach (var r in chinh.GetComponentsInChildren<Renderer>()) if (r.name == "Rang") return r.bounds.center; return chinh.position + chinh.rotation * lech - cam.up * 0.07f; }
        void Dat(float kc, Quaternion giat, float lenXuong = 0f)
        {
            var qq = q * giat; chinh.rotation = qq;
            chinh.position = cam.position + cam.forward * kc + cam.up * lenXuong - qq * lech;
        }
        Dat(1.3f, Quaternion.identity, 0.25f);
        ChuanBiHam(chinh);
        BatHinh(ma, false);
        // hai tay
        var tayT = TaoTay("Ma_GiacMo_TayTrai", out var vaiT, out var banT);
        var tayP = TaoTay("Ma_GiacMo_TayPhai", out var vaiP, out var banP);
        Quaternion qT0 = tayT ? tayT.rotation : Quaternion.identity, qP0 = tayP ? tayP.rotation : Quaternion.identity;
        Vector3 pT0 = tayT ? tayT.position : Vector3.zero, pP0 = tayP ? tayP.position : Vector3.zero;
        if (tayT) tayT.gameObject.SetActive(false); if (tayP) tayP.gameObject.SetActive(false);
        void Tay(float vao, float bop, float lac)
        {
            // vao: 0 = ngoài khung, 1 = áp vào hai bên đầu · bop: bóp vào thêm
            float ngang = Mathf.Lerp(0.34f, 0.15f - 0.012f * bop, vao);
            foreach (int sg in new[] { -1, 1 })
            {
                var h = sg < 0 ? tayT : tayP; if (!h) continue;
                var dich = cam.position + cam.right * sg * ngang + cam.forward * 0.13f + cam.up * 0.01f;   // sát hai rìa trái / phải khung hình, ngang tầm mắt
                // cánh tay: vai ở ngoài rìa khung (phía trước), bàn tay vào sát hai bên mặt mình
                if (sg < 0) DatTay(h, vaiT, banT, qT0, pT0, dich, (cam.right * 0.85f - cam.forward * 0.2f + cam.up * 0.05f).normalized, lac);
                else DatTay(h, vaiP, banP, qP0, pP0, dich, (-cam.right * 0.85f - cam.forward * 0.2f + cam.up * 0.05f).normalized, -lac);
            }
        }
        denMat = DenRoiMat(LightType.Point); denMat.range = 2.2f; denMat.color = new Color(0.85f, 0.88f, 1f);
        void Sang(float a) { denMat.transform.position = cam.position + cam.forward * 0.12f + cam.up * 0.12f; denMat.intensity = Random.value < 0.06f ? a * 0.6f : a; }
        // 2. lao sát: 1,3 m → 0,4 m, đủ sáng để thấy rõ mặt
        if (LocNhim.I && LocNhim.I.khoiTho) PhatMot(LocNhim.I.khoiTho, 1f);
        for (float t = 0; t < 1; t += Time.deltaTime / 0.22f) { float e = t * t; Dat(Mathf.Lerp(1.3f, 0.4f, e), Quaternion.identity, Mathf.Lerp(0.25f, 0f, e)); Sang(2.4f); yield return null; }
        rung = 1.0f;
        // 3. hai tay thò vào hai rìa, bóp chặt đầu — máy quay bị giữ cố định
        if (tayT) tayT.gameObject.SetActive(true); if (tayP) tayP.gameObject.SetActive(true);
        var camGiu = cam.position; var camQ = cam.rotation;
        for (float t = 0; t < 1; t += Time.deltaTime / 0.18f) { Tay(t * t, 0, 0); Dat(0.4f, Quaternion.Euler(0, 0, Mathf.Sin(t * 30f) * 2f)); Sang(2.6f); yield return null; }
        AudioSource.PlayClipAtPoint(AmTong.Roi(), cam.position, 0.5f);
        float hm = 0;
        for (float t = 0; t < 0.7f; t += Time.deltaTime)
        {
            float bop = 0.5f + 0.5f * Mathf.Sin(t * 22f);
            cam.position = camGiu + Random.insideUnitSphere * 0.002f; cam.rotation = camQ;   // không quay đi được
            Tay(1f, bop, Mathf.Sin(t * 13f) * 4f);
            var giat = Random.value < 0.07f ? Quaternion.Euler(Random.Range(-4f, 4f), Random.Range(-4f, 4f), Random.Range(-9f, 9f)) : Quaternion.Euler(0, 0, Mathf.Sin(t * 7f) * 3f);
            Dat(0.4f - t * 0.04f, giat);
            hm = Mathf.MoveTowards(hm, 18f, Time.deltaTime * 40f); HaMieng(hm);
            Sang(2.6f);
            yield return null;
        }
        // 4. há miệng rộng bất thường
        for (float t = 0; t < 1; t += Time.deltaTime / 0.55f)
        {
            hm = Mathf.Lerp(18f, 82f, t * (2 - t)); HaMieng(hm);
            cam.position = camGiu + Random.insideUnitSphere * 0.003f * (1 + t); cam.rotation = camQ;
            Tay(1f, 1f, Random.Range(-3f, 3f));
            Dat(0.36f, Quaternion.Euler(Random.Range(-1.5f, 1.5f), 0, Random.Range(-2f, 2f)));
            Sang(2.8f);
            yield return null;
        }
        // 5. ập vào: cú đánh của bản nhạc + tiếng hét, máy quay chui vào miệng, tối dần → im lặng hẳn
        if (danh) PhatMot(danh, 1f); if (thetClip) PhatMot(thetClip, 1f);
        var m0 = Mieng();
        float d0 = Vector3.Dot(m0 - cam.position, cam.forward);
        var ngang0 = (m0 - cam.position) - cam.forward * d0;
        for (float t = 0; t < 1; t += Time.deltaTime / 0.32f)
        {
            float e = t * t;
            var dich = cam.position + cam.forward * Mathf.Lerp(d0, -0.06f, e) + ngang0 * (1 - e);   // miệng trùm lên rồi vượt qua ống kính
            chinh.position += dich - Mieng();
            Tay(1f, 1f, Random.Range(-5f, 5f));
            nhieu = 0.2f + 0.6f * e; rung = 1.3f; LocFirstPerson.den = Mathf.Clamp01((t - 0.35f) / 0.65f);
            Sang(Mathf.Lerp(2.8f, 0.5f, e));
            yield return null;
        }
        LocFirstPerson.den = 1f;
        yield return new WaitForSeconds(0.35f);   // tiếng đánh + hét vang thêm một chút trong màn đen rồi cắt
        LocFirstPerson.den = 1f; nhieu = 0f; rung = 0f;
        foreach (var a in cam.GetComponents<AudioSource>()) if (a) a.Stop();   // im lặng hoàn toàn
        if (amNhac) amNhac.Stop();
    }

    // ═════════════════════════ bị bắt: ngã sấp → lật ngửa, bóng đen cúi xuống → bàn tay túm cổ áo, giật lên sát mặt → hét → cắt đen
    bool daBiBat;
    IEnumerator BiBat()
    {
        ketThuc = true; ketThoat = false; daBiBat = true;
        // [13/11] cảnh 2: khoá ngay, nhạc rượt cắt đột ngột, im lặng nửa giây; bị vồ ngã ra sau, máy quay ngửa lên nhìn nó
        foreach (var n in nem) { if (n.am) n.am.Stop(); if (n.keo) n.keo.Stop(); }
        if (amNhac) amNhac.Stop(); if (amU) amU.Stop(); if (amTim) amTim.Stop(); if (amMaTho) amMaTho.Stop();
        var lp0 = cam.localPosition; var lr0 = cam.localRotation;
        for (float t = 0; t < 1; t += Time.deltaTime / 0.5f)
        {
            float e = t * t;
            cam.localPosition = Vector3.Lerp(lp0, new Vector3(0.05f, 0.28f, -0.55f), e);
            cam.localRotation = Quaternion.Slerp(lr0, Quaternion.Euler(-78f, yawLech * (1 - e), 12f), e);
            yield return null;
        }
        rung = 0.6f;
        if (ma) BatHinh(ma, false);
        // 3–4. [13/11] nó vồ xuống: hiện rõ mặt, hai tay ôm chặt đầu, há miệng ập vào mặt → cắt đen
        yield return VoMat();
        LocFirstPerson.den = 1f; nhieu = 0;
        foreach (var a in cam.GetComponents<AudioSource>()) if (a && LocMa.I && a.clip == LocMa.I.tiengThet) a.Stop();
        if (amMaTho) amMaTho.Stop(); if (amTim) amTim.Stop();
        DonMatMa();
        yield return new WaitForSeconds(1.2f);
    }
    float nhieu;
    static Renderer TimR(Transform t, string ten) { foreach (var r in t.GetComponentsInChildren<Renderer>()) if (r.name == ten && r.enabled) return r; return null; }

    // ═════════════════════════ chữ trên màn hình: dấu "!" theo làn, "CHẠY ĐI!", viền tối nứt theo số lần trúng, nhiễu lúc hét
    static Texture2D texVien;
    void OnGUI()
    {
        GUI.depth = 1;   // vẽ dưới lớp của LocFirstPerson: phụ đề nằm trên viền tối, màn đen phủ lên dấu "!"
        if (!dangMo || Event.current.type != EventType.Repaint || LocMenu.DangChe) return;
        float W = Screen.width, Hh = Screen.height, k = Hh / 1080f;
        if (toiChop > 0) { GUI.color = new Color(0, 0, 0, toiChop / 0.28f * 0.75f); GUI.DrawTexture(new Rect(0, 0, W, Hh), Texture2D.whiteTexture); }
        if (trang > 0) { GUI.color = new Color(1, 1, 1, trang); GUI.DrawTexture(new Rect(0, 0, W, Hh), Texture2D.whiteTexture); }   // [13/11] chớp trắng một khung lúc cú đánh
        if (soTrung > 0)
        {
            if (!texVien) texVien = TaoVien();
            GUI.color = new Color(1, 1, 1, soTrung == 1 ? 0.55f : 0.9f); GUI.DrawTexture(new Rect(0, 0, W, Hh), texVien);
            GUI.color = new Color(0.02f, 0.01f, 0.01f, 0.85f);
            var rd = new System.Random(7);
            int soNut = soTrung == 1 ? 3 : 7;
            for (int i = 0; i < soNut; i++) Nut(rd, W, Hh, k);
        }
        GUI.color = Color.white;
        if (LocFirstPerson.den > 0.6f) return;
        // dấu "!" — ô trái / giữa / phải ở đáy màn hình
        foreach (var n in nem)
        {
            float tt = Time.time - n.t0; if (n.xong || tt > baoTruoc) continue;
            float u = tt / baoTruoc, xo = W * (0.5f + n.lan * 0.22f), co = (46f + 30f * u) * k;
            float nhay = 0.6f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * (10f + 14f * u)));
            var st = new GUIStyle(GUI.skin.label) { font = LocMenu.FontChu(2), fontSize = Mathf.RoundToInt(co * 1.6f), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUI.color = new Color(0, 0, 0, 0.6f * nhay); st.normal.textColor = Color.black;
            GUI.Label(new Rect(xo - co + 3 * k, Hh * 0.86f - co + 3 * k, co * 2, co * 2), "!", st);
            st.normal.textColor = new Color(0.95f, 0.12f, 0.08f, nhay); GUI.color = Color.white;
            GUI.Label(new Rect(xo - co, Hh * 0.86f - co, co * 2, co * 2), "!", st);
        }
        if (canhBao && !ketThuc)
        {
            float nhay = 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(Time.time * 9f));
            var st = new GUIStyle(GUI.skin.label) { font = LocMenu.FontChu(2), fontSize = Mathf.RoundToInt(64 * k), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            st.normal.textColor = new Color(0, 0, 0, 0.7f * nhay); GUI.Label(new Rect(4 * k, Hh * 0.38f + 4 * k, W, 90 * k), "CHẠY ĐI!", st);
            st.normal.textColor = new Color(0.92f, 0.1f, 0.06f, nhay); GUI.Label(new Rect(0, Hh * 0.38f, W, 90 * k), "CHẠY ĐI!", st);
        }
        if (tHuongDan > 0 && !ketThuc)
        {
            var st = new GUIStyle(GUI.skin.label) { font = LocMenu.FontChu(0), fontSize = Mathf.RoundToInt(22 * k), alignment = TextAnchor.MiddleCenter };
            st.normal.textColor = new Color(1, 1, 1, Mathf.Clamp01(tHuongDan) * 0.75f);
            GUI.Label(new Rect(0, Hh * 0.93f, W, 34 * k), "A / D  đổi làn  ·  thấy dấu  !  thì tránh làn đó", st);
        }
        if (choBoQua && !intro && !ketThuc)   // giống chữ bỏ qua của LocCotTruyen (góc phải dưới, vạch chạy khi giữ)
        {
            var st = new GUIStyle(GUI.skin.label) { font = LocMenu.FontChu(1), fontSize = Mathf.RoundToInt(19 * k), alignment = TextAnchor.LowerRight };
            st.normal.textColor = new Color(0.75f, 0.72f, 0.66f, 0.3f + 0.5f * Mathf.Clamp01(tBo / 0.8f));
            GUI.Label(new Rect(0, 0, W - 36 * k, Hh - 30 * k), "Giữ Enter: bỏ qua giấc mơ", st);
            if (tBo > 0) { float w = 170 * k * Mathf.Clamp01(tBo / 0.8f); GUI.color = new Color(0.75f, 0.72f, 0.66f, 0.7f); GUI.DrawTexture(new Rect(W - 36 * k - w, Hh - 26 * k, w, 2 * k), Texture2D.whiteTexture); GUI.color = Color.white; }
        }
        if (nhieu > 0)
        {
            var rd = new System.Random(Time.frameCount);
            for (int i = 0; i < 90; i++) { float g = (float)rd.NextDouble(); GUI.color = new Color(g, g, g, 0.25f + 0.5f * nhieu); GUI.DrawTexture(new Rect((float)rd.NextDouble() * W, (float)rd.NextDouble() * Hh, W * 0.3f * (float)rd.NextDouble(), 2 + 6 * (float)rd.NextDouble()), Texture2D.whiteTexture); }
            GUI.color = Color.white;
        }
    }
    static void Nut(System.Random rd, float W, float Hh, float k)
    {
        // một đường nứt gấp khúc bắt đầu từ mép màn hình
        int canh = rd.Next(4); var p = canh == 0 ? new Vector2((float)rd.NextDouble() * W, 0) : canh == 1 ? new Vector2(W, (float)rd.NextDouble() * Hh) : canh == 2 ? new Vector2((float)rd.NextDouble() * W, Hh) : new Vector2(0, (float)rd.NextDouble() * Hh);
        var huong = (new Vector2(W / 2, Hh / 2) - p).normalized;
        for (int j = 0; j < 6; j++)
        {
            var dd = Quaternion.Euler(0, 0, ((float)rd.NextDouble() - 0.5f) * 70f) * huong;
            var q = p + (Vector2)dd * (30f + 50f * (float)rd.NextDouble()) * k;
            var m = GUI.matrix; var d = q - p;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, p);
            GUI.DrawTexture(new Rect(p.x, p.y - 1.2f * k, d.magnitude, 2.4f * k), Texture2D.whiteTexture);
            GUI.matrix = m; p = q;
        }
    }
    static Texture2D TaoVien()
    {
        const int n = 64; var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++) for (int xx = 0; xx < n; xx++)
        {
            float dx = (xx + 0.5f) / n * 2 - 1, dy = (y + 0.5f) / n * 2 - 1;
            float d = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) * 0.6f + Mathf.Sqrt(dx * dx + dy * dy) * 0.4f;
            px[y * n + xx] = new Color32(8, 2, 2, (byte)(Mathf.Clamp01((d - 0.55f) / 0.45f) * 235));
        }
        t.SetPixels32(px); t.Apply(); return t;
    }

    // ═════════════════════════ dọn
    bool daDon;
    void DonDep()
    {
        daDon = true;
        DonMatMa();
        if (amNhac) amNhac.Stop(); if (amU) Destroy(amU);
        foreach (var a in cam.GetComponents<AudioSource>()) if (a && a != amTim && a.clip && (a.clip.name == "GM_NhacDanh" || a.clip.name == "Ma_XuatHien" || a.clip.name == "Khoi_Tho")) Destroy(a);
        TraMoiTruong();
        if (amTim) Destroy(amTim);
        foreach (var a in cam.GetComponents<AudioSource>()) if (a && a.clip == (LocMa.I ? LocMa.I.tiengThet : null)) Destroy(a);
        cam.localPosition = camLP0; cam.localRotation = camLR0;
        foreach (var m in vlTao) if (m) Destroy(m);   // vật liệu tự tạo không nằm trong cảnh nào — phải xoá tay (để lại thì lần mơ sau tìm nhầm, tối dần)
        vlTao.Clear();
        if (coCanhRieng && canhMo.IsValid() && canhMo.isLoaded) SceneManager.UnloadSceneAsync(canhMo);   // gỡ cảnh mơ: xoá luôn cả cây LOC_GiacMo
        else Destroy(gameObject);
    }
    void OnDestroy() { dangMo = false; }

    // ═════════════════════════ tiện ích dựng
    GameObject Hop(string ten, Vector3 cucBo, Vector3 co, Vector3 huong, Material vl, float lapX, float lapY)
    {
        var g = Khoi(PrimitiveType.Cube, GOC + cucBo, co, vl, ten);
        if (g) { g.transform.rotation = Quaternion.LookRotation(huong); LapTex(g.GetComponent<Renderer>(), lapX, lapY); }
        return g;
    }
    GameObject Khoi(PrimitiveType kieu, Vector3 tg, Vector3 co, Material vl, string ten = null)
    {
        var g = GameObject.CreatePrimitive(kieu); if (ten != null) g.name = ten;
        var col = g.GetComponent<Collider>(); if (col) DestroyImmediate(col);
        g.transform.SetParent(goc, false); g.transform.position = tg; g.transform.localScale = co;
        var r = g.GetComponent<Renderer>(); if (r) { if (vl) r.sharedMaterial = vl; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; }
        return g;
    }
    static MaterialPropertyBlock mpb;
    static void LapTex(Renderer r, float lx, float ly)
    {
        if (!r || !r.sharedMaterial || !r.sharedMaterial.HasProperty("_BaseMap") || !r.sharedMaterial.GetTexture("_BaseMap")) return;
        mpb ??= new MaterialPropertyBlock(); r.GetPropertyBlock(mpb);
        mpb.SetVector("_BaseMap_ST", new Vector4(Mathf.Max(0.2f, lx), Mathf.Max(0.2f, ly), 0, 0)); r.SetPropertyBlock(mpb);
    }
    // [02/11 tối] SỬA "tối dần sau mỗi lần mơ": bản chép mang tên có hậu tố HAU_TO, khi tìm vật liệu gốc thì bỏ qua mọi bản chép,
    //  và mọi vật liệu tự tạo được xoá lúc mơ xong (vlTao). Trước đây bản chép trùng tên gốc, không xoá → lần sau chép lại bản đã tối, tối thêm.
    const string HAU_TO = " (GiacMo)";
    readonly List<Material> vlTao = new();
    Material Moi(Material m, string ten) { m.name = ten + HAU_TO; vlTao.Add(m); return m; }
    Material TimVL(string ten, Color mau, Color nhanMau)
    {
        if (!string.IsNullOrEmpty(ten))
            foreach (var m in Resources.FindObjectsOfTypeAll<Material>())
                if (m && m.name == ten) { var k = Moi(new Material(m), ten); if (k.HasProperty("_BaseColor")) k.SetColor("_BaseColor", m.GetColor("_BaseColor") * nhanMau); return k; }
        Material goc = null;
        foreach (var r in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)) { var m = r.sharedMaterial; if (m && m.shader && !m.name.EndsWith(HAU_TO) && !m.name.EndsWith("_PBR") && !m.name.EndsWith("_phang") && m.shader.name.Contains("Universal Render Pipeline/Lit")) { goc = m; break; } }   // [03/11] bỏ vật liệu của bản "Thực tế" (_PBR/_phang, mang normal map vữa)
        Material v;
        if (goc) v = Moi(new Material(goc), "VL"); else { var sh = Shader.Find("Universal Render Pipeline/Lit"); if (!sh) return null; v = Moi(new Material(sh), "VL"); }
        if (v.HasProperty("_BaseMap")) v.SetTexture("_BaseMap", null);
        if (v.HasProperty("_BaseColor")) v.SetColor("_BaseColor", mau);
        if (v.HasProperty("_EmissionColor")) { v.SetColor("_EmissionColor", Color.black); v.DisableKeyword("_EMISSION"); }
        v.color = mau; return v;
    }
    static GameObject[] TimNguon(params string[] ten)
    {
        var l = new List<GameObject>();
        var tat = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var n in ten)
            foreach (var t in tat)
            {
                if (t.name != n || (t.parent && t.parent.name == n)) continue;
                if (!t.GetComponentInChildren<Renderer>(true)) continue;
                l.Add(t.gameObject); break;
            }
        return l.ToArray();
    }
    GameObject DoTrongNha(GameObject[] nguon, System.Random rd, Vector3 cucBo, Vector3 huong, bool xoayNgang)
    {
        if (nguon == null || nguon.Length == 0)
            return Khoi(PrimitiveType.Cube, GOC + cucBo + Vector3.up * 0.22f, new Vector3(0.42f, 0.44f, 0.42f), vlDay, "Thung_Tam");
        var src = nguon[rd.Next(nguon.Length)];
        var g = HinhSao(src.transform, goc);
        g.transform.rotation = Quaternion.LookRotation(huong) * Quaternion.Euler(0, xoayNgang ? (float)rd.NextDouble() * 360f : (float)(rd.NextDouble() - 0.5) * 40f, 0);
        g.transform.position = GOC + cucBo;
        var rs = g.GetComponentsInChildren<Renderer>();
        if (rs.Length > 0)
        {
            var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
            g.transform.position += new Vector3(GOC.x + cucBo.x - b.center.x, GOC.y - b.min.y, GOC.z + cucBo.z - b.center.z);
            if (b.size.x > 1.1f || b.size.z > 1.1f) { Destroy(g); return null; }   // vật to quá lấn vào làn chạy
        }
        return g;
    }
    // bản sao CHỈ phần hình (lưới + vật liệu): Instantiate cả vật sẽ chạy Awake của LocVatPham/LocTuongTac… trên bản sao (đăng ký trùng id)
    static GameObject HinhSao(Transform src, Transform cha, Transform boQua = null)   // boQua: nhánh con không chép (chép riêng)
    {
        var g0 = new GameObject(src.name); g0.transform.SetParent(cha, false);
        foreach (var mf in src.GetComponentsInChildren<MeshFilter>(true))
        {
            var mr = mf.GetComponent<MeshRenderer>(); if (!mf.sharedMesh || !mr || !mr.enabled) continue;
            if (boQua && mf.transform.IsChildOf(boQua)) continue;
            var g = new GameObject(mf.name); g.transform.SetParent(g0.transform, false);
            g.transform.localPosition = Quaternion.Inverse(src.rotation) * (mf.transform.position - src.position);   // g0 tỉ lệ 1
            g.transform.localRotation = Quaternion.Inverse(src.rotation) * mf.transform.rotation;
            g.transform.localScale = mf.transform.lossyScale;
            g.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
            var r = g.AddComponent<MeshRenderer>(); r.sharedMaterials = mr.sharedMaterials; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        return g0;
    }
    static bool LaDoNem(GameObject g) { var rs = g.GetComponentsInChildren<Renderer>(); if (rs.Length == 0) return false; var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b.size.y < 1.0f && b.size.magnitude < 1.3f; }
    // bỏ mọi thứ biết "làm việc" trên vật nhân bản (script, va chạm, đèn, âm, hoạt cảnh) — chỉ giữ hình
    static void DonThanhPhan(GameObject g)
    {
        foreach (var c in g.GetComponentsInChildren<MonoBehaviour>(true)) if (c) DestroyImmediate(c);
        foreach (var c in g.GetComponentsInChildren<Rigidbody>(true)) if (c) DestroyImmediate(c);
        foreach (var c in g.GetComponentsInChildren<Collider>(true)) if (c) DestroyImmediate(c);
        foreach (var c in g.GetComponentsInChildren<Light>(true)) if (c) DestroyImmediate(c);
        foreach (var c in g.GetComponentsInChildren<AudioSource>(true)) if (c) DestroyImmediate(c);
        foreach (var c in g.GetComponentsInChildren<Animator>(true)) if (c) DestroyImmediate(c);
    }
    static Transform Tim(Transform t, string ten) { foreach (var c in t.GetComponentsInChildren<Transform>(true)) if (c.name == ten) return c; return null; }
}

// Âm của giấc mơ. Có file thật thì thả vào Assets/LOC_House/Resources/LOC_AmThanh/ đúng tên dưới đây là tự dùng, không phải sửa code:
//   GM_Tim (nhịp tim, lặp) · GM_Gio (gió rít khi đồ bay, ~1,5 s, to dần) · GM_Roi_0/1/2 (đồ rơi / va: nhựa, gỗ, sắt) · GM_MaTho (ma thở, lặp) · GM_DapCua (đập cửa sắt)
//  Chưa có file thì dùng âm tổng hợp tạm (tạo một lần, giữ trong bộ nhớ qua các lần nạp lại scene).
public static class AmTong
{
    const int HZ = 22050;
    static AudioClip tim, gio, maTho, dap; static AudioClip[] roi;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset0() { tim = gio = maTho = dap = null; roi = null; }
    static AudioClip That(string ten) => Resources.Load<AudioClip>("LOC_AmThanh/" + ten);
    static AudioClip Tao(string ten, float giay, System.Func<float, System.Random, float> f, int hat)
    {
        int n = (int)(giay * HZ); var d = new float[n]; var rd = new System.Random(hat);
        for (int i = 0; i < n; i++) d[i] = Mathf.Clamp(f(i / (float)HZ, rd), -1f, 1f);
        var c = AudioClip.Create(ten, n, 1, HZ, false); c.SetData(d, 0);
        c.hideFlags = HideFlags.DontUnloadUnusedAsset;   // nạp lại scene (UnloadUnusedAssets) không xoá mất clip đang giữ trong biến tĩnh
        return c;
    }
    // so sánh kiểu Unity (!clip): clip đã bị huỷ cũng tính là chưa có
    public static AudioClip Tim()
    {
        if (!tim) tim = That("GM_Tim");
        if (!tim) tim = Tao("GM_Tim", 1.0f, (t, r) =>
        {
            float a = Mathf.Exp(-t / 0.07f) * Mathf.Sin(2 * Mathf.PI * 52 * t), b = t > 0.28f ? Mathf.Exp(-(t - 0.28f) / 0.06f) * Mathf.Sin(2 * Mathf.PI * 46 * (t - 0.28f)) * 0.7f : 0;
            return (a + b) * 0.9f;
        }, 1);
        return tim;
    }
    public static AudioClip Gio()
    {
        if (!gio) gio = That("GM_Gio");
        if (!gio) { float lp = 0; gio = Tao("GM_Gio", 1.6f, (t, r) =>
        {
            float nhieu = (float)r.NextDouble() * 2 - 1; float k = Mathf.Lerp(0.02f, 0.25f, t / 1.6f);
            lp += (nhieu - lp) * k; return lp * Mathf.Lerp(0.1f, 1.6f, t / 1.6f) * (t > 1.5f ? (1.6f - t) / 0.1f : 1f);
        }, 2); }
        return gio;
    }
    public static AudioClip Roi()
    {
        if (roi == null || !roi[0] || !roi[1] || !roi[2])
        {
            roi = new AudioClip[3];
            for (int j = 0; j < 3; j++)
            {
                roi[j] = That("GM_Roi_" + j);
                if (roi[j]) continue;
                float f0 = 70 + 35 * j; float lpr = 0;
                roi[j] = Tao("GM_Roi_" + j, 0.5f, (t, r) => { float nh = (float)r.NextDouble() * 2 - 1; lpr += (nh - lpr) * 0.35f; return Mathf.Exp(-t / 0.05f) * lpr * 1.2f + Mathf.Exp(-t / 0.12f) * Mathf.Sin(2 * Mathf.PI * f0 * t) * 0.8f; }, 10 + j);
            }
        }
        return roi[Random.Range(0, 3)];
    }
    public static AudioClip MaTho()
    {
        if (!maTho) maTho = That("GM_MaTho");
        if (!maTho) { float lp = 0; maTho = Tao("GM_MaTho", 3.2f, (t, r) =>
        {
            float nh = (float)r.NextDouble() * 2 - 1; lp += (nh - lp) * 0.06f;
            float hoi = Mathf.Pow(Mathf.Abs(Mathf.Sin(Mathf.PI * t / 1.6f)), 2.5f);
            return lp * hoi * 2.2f;
        }, 3); }
        return maTho;
    }
    public static AudioClip Dap()
    {
        if (!dap) dap = That("GM_DapCua");
        if (!dap) dap = Tao("GM_DapCua", 0.9f, (t, r) =>
            Mathf.Exp(-t / 0.15f) * Mathf.Sin(2 * Mathf.PI * 68 * t) + Mathf.Exp(-t / 0.35f) * (Mathf.Sin(2 * Mathf.PI * 410 * t) * 0.25f + Mathf.Sin(2 * Mathf.PI * 633 * t) * 0.18f + Mathf.Sin(2 * Mathf.PI * 951 * t) * 0.1f), 4);
        return dap;
    }
}
