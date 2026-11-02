using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// [02/11] HIỆU NĂNG — giảm giật khung "thỉnh thoảng" + chọn mức đồ hoạ. Tự tạo khi vào cảnh, không cần gắn vào scene.
//  1. Bóng đổ của đèn điểm/đèn rọi: mỗi đèn có bóng tốn tới 6 lượt dựng bóng (6 mặt) × mọi vật đổ bóng trong tầm. Trước đây cả 3 đèn có bóng
//     (Den_DenNgu_Nhim, Den_Rap_Trai, Den_BongDayToc_Ham) luôn bật → quay camera làm một đèn lọt vào khung hình là chi phí nhảy vọt (cảm giác "khựng nhẹ").
//     Giờ chỉ đèn nào người chơi đang ở trong tầm (range + 2,5 m) mới đổ bóng; ra xa hơn range + 5 m thì tắt bóng. Ánh sáng không đổi.
//     Tôn trọng cài đặt "Chất lượng bóng đổ" của LocMenu (Tắt / Thấp = bóng cứng / Vừa = bóng mềm như scene dựng).
//  2. Mức đồ hoạ (Cài đặt ▸ Hình ảnh ▸ Chất lượng hình ảnh): Tiết kiệm · Cân bằng · Đẹp = MSAA, tỉ lệ render, tầm bóng, số cascade, và
//     ngưỡng "vật nhỏ không đổ bóng" (cái chén, chai lọ… bỏ khỏi lượt dựng bóng — gần như không ai nhìn ra, nhưng bớt hàng trăm vật đổ bóng mỗi lượt).
//  3. Thu rác (GC) đúng lúc màn đen (LocFirstPerson.den ≥ 0,99), tối đa 1 lần / 45 s, để GC tự chạy giữa cảnh chơi ít khi xảy ra hơn.
//  4. Bảng đo khung hình: phím ` (dấu huyền, cạnh phím 1) bật/tắt. Hiện FPS, 1% khung chậm nhất, khung tệ nhất, số lần giật trong ~5 s.
public class LocHieuNang : MonoBehaviour
{
    struct Muc { public int msaa; public float tiLe, bongXa; public int tang; public float nguongBong; }
    static readonly Muc[] MUC =
    {
        new Muc { msaa = 1, tiLe = 0.85f, bongXa = 20f, tang = 1, nguongBong = 0.45f },   // Tiết kiệm: không khử răng cưa, 85% độ phân giải, bóng gần, vật < 45 cm không đổ bóng
        new Muc { msaa = 2, tiLe = 1f,    bongXa = 30f, tang = 2, nguongBong = 0.15f },   // Cân bằng (mặc định)
        new Muc { msaa = 4, tiLe = 1f,    bongXa = 40f, tang = 4, nguongBong = 0f },      // Đẹp: MSAA 4×, bóng xa, mọi vật đổ bóng như cũ
    };
    public static readonly string[] TEN_MUC = { "Tiết kiệm", "Cân bằng", "Đẹp" };

    static LocHieuNang I;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset0() { I = null; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Tao()
    {
        if (I) return;
        var g = new GameObject("_LocHieuNang"); DontDestroyOnLoad(g); I = g.AddComponent<LocHieuNang>();
    }
    // LocMenu gọi khi vào game và khi người chơi đổi mức
    public static void ApDoHoa(int muc) { if (!I) Tao(); I.DatMuc(muc); }
    // LocMenu gọi sau khi người chơi đổi "Chất lượng bóng đổ": đọc lại ngay thay vì đợi vòng đọc 1 s
    public static void DocLaiBong() { if (I) { I.lanDocPref = 0; I.tiepBong = 0; } }

    int mucHienTai = 1; float nguong;
    LocHieuNangHUD hud;

    // ───── mức đồ hoạ
    UniversalRenderPipelineAsset rp0; int msaa0, tang0; float tiLe0, bongXa0;
    readonly Dictionary<MeshRenderer, ShadowCastingMode> gocBong = new Dictionary<MeshRenderer, ShadowCastingMode>();
    Coroutine quet;

    void DatMuc(int muc)
    {
        mucHienTai = Mathf.Clamp(muc, 0, MUC.Length - 1); var m = MUC[mucHienTai]; nguong = m.nguongBong;
        var rp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (rp)
        {
            if (!rp0) { rp0 = rp; msaa0 = rp.msaaSampleCount; tiLe0 = rp.renderScale; bongXa0 = rp.shadowDistance; tang0 = rp.shadowCascadeCount; }   // nhớ giá trị gốc để trả lại khi thoát (Editor: asset không bị sửa lây)
            rp.msaaSampleCount = m.msaa; rp.renderScale = m.tiLe; rp.shadowDistance = m.bongXa; rp.shadowCascadeCount = m.tang;
        }
        if (quet != null) StopCoroutine(quet);
        quet = StartCoroutine(QuetVatDoBong());
    }

    // vật nhỏ (đường chéo hộp bao < ngưỡng) bỏ khỏi lượt dựng bóng; nhớ chế độ gốc để trả lại khi đổi mức. Rải 300 vật/khung để không tự gây khựng.
    IEnumerator QuetVatDoBong()
    {
        var chet = new List<MeshRenderer>(); foreach (var k in gocBong.Keys) if (!k) chet.Add(k); foreach (var k in chet) gocBong.Remove(k);
        var ds = FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int n = 0;
        foreach (var r in ds)
        {
            if (!r) continue;
            if (!gocBong.TryGetValue(r, out var g)) { g = r.shadowCastingMode; gocBong[r] = g; }
            bool tat = nguong > 0f && g == ShadowCastingMode.On && r.bounds.size.magnitude < nguong;
            var mong = tat ? ShadowCastingMode.Off : g;
            if (r.shadowCastingMode != mong) r.shadowCastingMode = mong;
            if (++n % 300 == 0) yield return null;
        }
        quet = null;
    }

    void Khoi()
    {
        if (rp0) { rp0.msaaSampleCount = msaa0; rp0.renderScale = tiLe0; rp0.shadowDistance = bongXa0; rp0.shadowCascadeCount = tang0; rp0 = null; }
        foreach (var kv in gocBong) if (kv.Key && kv.Key.shadowCastingMode != kv.Value) kv.Key.shadowCastingMode = kv.Value;
        gocBong.Clear();
    }
    void OnApplicationQuit() { Khoi(); }
    void OnDestroy() { Khoi(); if (I == this) I = null; }
    void OnEnable() { SceneManager.sceneLoaded += KhiVaoCanh; }
    void OnDisable() { SceneManager.sceneLoaded -= KhiVaoCanh; }
    void KhiVaoCanh(Scene s, LoadSceneMode m) { if (rp0) DatMuc(mucHienTai); lanQuetDen = 0; luot = null; }   // vào lại cảnh (chơi lại): quét lại vật & đèn

    // ───── bóng đèn điểm theo khoảng cách
    Light[] luot; float lanQuetDen, lanDocPref, tiepBong; int prefBong = 2;
    readonly HashSet<Light> coBong = new HashSet<Light>();   // đèn điểm/rọi từng có bóng
    readonly HashSet<Light> dangBong = new HashSet<Light>(); // đèn đang bật bóng vì người chơi ở gần
    public int SoDenBong => dangBong.Count;

    void CapNhatBong()
    {
        var cam = Camera.main; if (!cam) return;
        if (Time.unscaledTime >= lanQuetDen)
        {
            lanQuetDen = Time.unscaledTime + 3f; luot = FindObjectsByType<Light>(FindObjectsSortMode.None);
            coBong.RemoveWhere(x => !x); dangBong.RemoveWhere(x => !x);   // đèn đã huỷ (chơi lại cảnh)
        }
        if (Time.unscaledTime >= lanDocPref) { lanDocPref = Time.unscaledTime + 1f; prefBong = PlayerPrefs.GetInt("LOC_CD_Bong", 2); }   // cùng khoá với LocMenu
        if (luot == null) return;
        var vt = cam.transform.position;
        foreach (var l in luot)
        {
            if (!l || l.type == LightType.Directional || !l.enabled) continue;
            if (l.shadows != LightShadows.None) coBong.Add(l); else if (!coBong.Contains(l)) continue;
            float xa = Vector3.Distance(vt, l.transform.position);
            bool gan = xa < l.range + (dangBong.Contains(l) ? 5f : 2.5f);   // dải trễ 2,5 m: đứng ngay rìa không bật/tắt liên tục
            if (gan) dangBong.Add(l); else dangBong.Remove(l);
            var mong = prefBong == 0 || !gan ? LightShadows.None : prefBong == 1 ? LightShadows.Hard : LightShadows.Soft;
            if (l.shadows != mong) l.shadows = mong;
        }
    }

    // ───── thu rác lúc màn đen · đo khung hình
    float gcCuoi;
    readonly float[] ft = new float[300]; readonly float[] tmp = new float[300]; int fi, dem; float lanTinh;
    bool hienHud;

    void Awake() { hud = gameObject.AddComponent<LocHieuNangHUD>(); hud.enabled = false; }

    void Update()
    {
        ft[fi] = Time.unscaledDeltaTime; fi = (fi + 1) % ft.Length; if (dem < ft.Length) dem++;
        var k = Keyboard.current;
        if (k != null && k.backquoteKey.wasPressedThisFrame) { hienHud = !hienHud; hud.enabled = hienHud; lanTinh = 0; }
        if (Time.unscaledTime >= tiepBong) { tiepBong = Time.unscaledTime + 0.2f; if (rp0 || Time.timeSinceLevelLoad > 1.5f) CapNhatBong(); }   // đợi LocMenu.Start áp cài đặt bóng xong
        if (LocFirstPerson.den >= 0.99f && Time.unscaledTime - gcCuoi > 45f) { gcCuoi = Time.unscaledTime; System.GC.Collect(); }
        if (hienHud && Time.unscaledTime >= lanTinh) { lanTinh = Time.unscaledTime + 0.5f; TinhThongKe(); }
    }

    void TinhThongKe()
    {
        int n = dem; if (n < 10) return;
        float tong = 0, max = 0;
        for (int i = 0; i < n; i++) { float f = ft[i]; tong += f; if (f > max) max = f; tmp[i] = f; }
        float tb = tong / n; System.Array.Sort(tmp, 0, n);
        float cham1 = tmp[Mathf.Clamp(Mathf.CeilToInt(n * 0.99f) - 1, 0, n - 1)];
        float nguongGiat = Mathf.Max(0.025f, tb * 2f); int giat = 0;
        for (int i = 0; i < n; i++) if (ft[i] > nguongGiat) giat++;
        hud.chu = $"FPS {1f / tb:0}  ·  1% chậm nhất {1f / cham1:0} FPS  ·  khung tệ nhất {max * 1000f:0} ms  ·  giật (> {nguongGiat * 1000f:0} ms): {giat}/{n} khung\n" +
                  $"mức đồ hoạ: {TEN_MUC[mucHienTai]}  ·  đèn đang đổ bóng: {dangBong.Count}  ·  [`] tắt bảng này";
    }
}

// OnGUI của bảng đo tách riêng, chỉ bật khi hiện bảng (script tắt thì Unity không gọi OnGUI → không tốn gì lúc ẩn).
public class LocHieuNangHUD : MonoBehaviour
{
    public string chu = "";
    void OnGUI()
    {
        if (string.IsNullOrEmpty(chu)) return;
        float k = Mathf.Max(1f, Screen.height / 1080f);
        var st = LocKieuChu.Lay("hieunang.hud", null, Mathf.RoundToInt(15 * k), FontStyle.Normal, TextAnchor.LowerLeft, new Color(0.72f, 1f, 0.72f, 0.95f));
        GUI.Label(new Rect(12 * k, 0, Screen.width - 24 * k, Screen.height - 12 * k), chu, st);
    }
}

// Kiểu chữ IMGUI dựng một lần rồi dùng lại. `new GUIStyle(GUI.skin.label)` mỗi lần OnGUI (2–4 lần/khung) tạo rác bộ nhớ liên tục → GC thỉnh thoảng làm khựng.
// Chỉ gọi trong OnGUI (cần GUI.skin).
public static class LocKieuChu
{
    static readonly Dictionary<string, GUIStyle> kho = new Dictionary<string, GUIStyle>();
    public static GUIStyle Lay(string khoa, Font font, int co, FontStyle kieu, TextAnchor can, Color mau)
    {
        if (!kho.TryGetValue(khoa, out var st) || st == null) { st = new GUIStyle(GUI.skin.label); kho[khoa] = st; }
        if (st.font != font) st.font = font;
        if (st.fontSize != co) st.fontSize = co;
        if (st.fontStyle != kieu) st.fontStyle = kieu;
        if (st.alignment != can) st.alignment = can;
        if (st.normal.textColor != mau) st.normal.textColor = mau;
        return st;
    }
}
