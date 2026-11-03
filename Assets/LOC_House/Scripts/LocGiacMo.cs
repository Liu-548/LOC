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
            ketThoat = true;
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
        for (float t = 0; t < 1; t += Time.deltaTime / 0.7f) { yawLech = Mathf.Lerp(0, 175f, t * t * (3 - 2 * t)); DatNguoi(); yield return null; }
        yield return Cho(1.5f);
        for (float t = 0; t < 1; t += Time.deltaTime / 0.5f) { yawLech = Mathf.Lerp(175f, 0f, t * t * (3 - 2 * t)); DatNguoi(); yield return null; }
        yawLech = 0; if (amMaTho) amMaTho.volume = 0.6f;
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

    // ═════════════════════════ bị bắt: ngã sấp → lật ngửa, bóng đen cúi xuống → bàn tay túm cổ áo, giật lên sát mặt → hét → cắt đen
    IEnumerator BiBat()
    {
        ketThuc = true; ketThoat = false;
        foreach (var n in nem) if (n.am) n.am.Stop();
        var t0 = Tiep(s); var chan = fp.transform.position;
        // 1. ngã sấp
        AudioSource.PlayClipAtPoint(AmTong.Roi(), cam.position, 1f);
        var lp0 = cam.localPosition;
        for (float t = 0; t < 1; t += Time.deltaTime / 0.35f) { float e = t * t; cam.localPosition = Vector3.Lerp(lp0, new Vector3(0, 0.25f, 0.3f), e); cam.localRotation = Quaternion.Euler(Mathf.Lerp(pitch, 55f, e), yawLech * (1 - e), Mathf.Lerp(0, 14f, e)); yield return null; }
        rung = 0.8f; yield return new WaitForSeconds(0.35f);
        // 2. lật ngửa: bóng đen đứng sát, cúi xuống
        if (ma)
        {
            ma.position = chan - t0 * 0.55f; ma.rotation = Quaternion.LookRotation(t0);
            ma.rotation = Quaternion.AngleAxis(38f, Phai(t0)) * ma.rotation;
        }
        fp.transform.rotation = Quaternion.LookRotation(-t0);
        toiChop = 0.28f;   // chớp tối che cú lật người
        for (float t = 0; t < 1; t += Time.deltaTime / 0.45f) { cam.localRotation = Quaternion.Euler(Mathf.Lerp(20f, -72f, t * (2 - t)), 0, Mathf.Lerp(14f, 0, t)); yield return null; }
        if (amMaTho) amMaTho.volume = 1f;
        yield return new WaitForSeconds(0.5f);
        // 3. tay nó vươn xuống túm cổ áo (đổi sang dáng Ma_Tay — cùng con ma, một tay vươn ra), giật lên sát mặt nó (~0,4 m)
        var dang = HinhMa(1, "Ma_GiacMo_VuonTay");
        if (dang && ma)
        {
            dang.SetPositionAndRotation(ma.position, ma.rotation);
            foreach (var r in ma.GetComponentsInChildren<Renderer>()) r.enabled = false;
        }
        var hinh = dang ? dang : ma;
        var dauT = hinh ? Tim(hinh, "Dau") : null;
        Vector3 Dau() => dauT ? dauT.position : hinh ? hinh.position + Vector3.up * 1.5f + t0 * 1.0f : cam.position + Vector3.up * 0.8f;
        var mong = dang ? TimR(dang, "Mong") : null;   // móng tay = đầu bàn tay đang vươn
        if (mong)
        {
            var p0 = dang.position; var dich = cam.position + cam.forward * 0.2f - cam.up * 0.12f;   // cổ áo: đáy khung hình
            var lech = Vector3.ClampMagnitude(dich - mong.bounds.center, 1.6f);
            for (float t = 0; t < 1; t += Time.deltaTime / 0.3f) { dang.position = p0 + lech * (t * t); yield return null; }
            dang.position = p0 + lech;
            rung = 0.6f; yield return new WaitForSeconds(0.12f);
        }
        {
            // giật: camera bị nhấc khỏi sàn, mặt nó kéo sát tới
            var c0 = cam.position; var cDich = c0 + Vector3.up * 0.75f;
            var d0 = Dau(); var huongMat = Vector3.ProjectOnPlane(d0 - c0, Vector3.up); huongMat = huongMat.sqrMagnitude > 1e-4f ? huongMat.normalized : -t0;
            var dauDich = cDich + huongMat * 0.4f + Vector3.up * 0.05f;
            var h0 = hinh ? hinh.position : Vector3.zero;
            for (float t = 0; t < 1; t += Time.deltaTime / 0.22f)
            {
                float e = t * t;
                cam.position = Vector3.Lerp(c0, cDich, e);
                if (hinh) hinh.position = h0 + (dauDich - d0) * e;
                cam.rotation = Quaternion.LookRotation(Dau() - cam.position, Vector3.up);
                yield return null;
            }
        }
        // 4. hét: rung mạnh, mặt lao vào ống kính, nhiễu — cắt đen ngay đỉnh tiếng hét
        AudioSource thet = null;
        var clip = LocMa.I ? LocMa.I.tiengThet : null;
        if (clip) { thet = cam.gameObject.AddComponent<AudioSource>(); thet.clip = clip; thet.spatialBlend = 0; thet.volume = 1f; thet.Play(); }
        nhieu = 0f;
        for (float t = 0; t < 0.65f; t += Time.deltaTime)
        {
            nhieu = t / 0.65f; rung = 1.4f;
            if (hinh) hinh.position += (cam.position - Dau()).normalized * Time.deltaTime * 0.45f;   // mặt nó lao vào ống kính
            cam.position += Random.insideUnitSphere * 0.02f;
            yield return null;
        }
        LocFirstPerson.den = 1f; nhieu = 0;
        if (thet) thet.Stop();
        if (amMaTho) amMaTho.Stop(); if (amTim) amTim.Stop();
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
