using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// [22/10] Giao diện bắt đầu + các menu — theo bản phác đã duyệt (Project: claude/thiet-ke-menu-22-10.md, LOC_Nguon/Menu/ban_phac 01–13).
//  · Nằm trên NguoiChoi (LocHouseBuilder_GamePlay gắn). Vẽ bằng OnGUI như phần UI còn lại, thiết kế ở 1080p, co theo chiều cao màn hình.
//  · Vào Play: cảnh báo (01) → menu chính (02) trên cảnh cổng nhà Đêm 1, camera trôi chậm. Chọn chơi → nạp lại scene ở đúng đêm.
//  · Tự lưu đầu mỗi đêm (LocCotTruyen.DauDem gọi DauDem) — một chỗ lưu, PlayerPrefs. Thẻ đêm (11) hiện lúc đó.
//  · Esc trong game = tạm dừng (09): hình đứng, mờ, rút màu; Time.timeScale = 0 nên ma, mất điện, thoại đều dừng.
//  · Death Ending (12): LocMa gọi ManChet → hai lựa chọn. Kết cục ghi bằng DatKet (số lần + ảnh chụp) cho màn Ending (13).
//  · Đang tự kiểm (cờ EditorPrefs LOC_TuKiem*, LOC_ThuCauThang, LOC_ChanDoanNhay) → menu đứng ngoài hoàn toàn.
[DefaultExecutionOrder(-100)]
public class LocMenu : MonoBehaviour
{
    public static LocMenu I;
    static bool daCanhBao, vaoGame, tat;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset0() { daCanhBao = vaoGame = tat = false; st = null; }
    public static bool DangChe => I && I.man != Man.Khong;   // menu / thẻ đêm đang phủ màn hình → lời mở đầu chờ

    enum Man { Khong, CanhBao, Chinh, ChonDem, Ending, CaiDat, DoSang, TamDung, XacNhan, TheDem, Chet }
    Man man, nenXN, cdTu;
    int chon, chonNen, tab, demThe;
    float tCB, tThe;
    bool chuotDi, dangChup, coGoc;
    Quaternion camGoc; float lookGoc = 0.12f;
    LocFirstPerson fp;

    // ───── dữ liệu
    static readonly string[] TEN_DEM = { "", "Về muộn", "Bố viết hết vào đây", "Mùng một là ngày mai" };   // tên đêm còn bàn — tạm theo kịch bản
    static readonly string[] NGAY = { "", "6/8/2002", "7/8/2002", "8/8/2002" };
    static readonly string[] NGAY_DAI = { "", "6 tháng 8 năm 2002", "7 tháng 8 năm 2002", "8 tháng 8 năm 2002" };
    static readonly string[] KET = { "good", "death", "bad", "evil" };
    static readonly string[] KET_NHOM = { "GOOD", "DEATH", "BAD", "EVIL" };
    static readonly string[] KET_TEN = { "Tiễn", "Nhà này còn một cái chén", "Mùng một", "Cháu khá hơn bố cháu" };
    static readonly string[] KET_GOI_Y =
    {
        "Tin lời mẹ hơn lời thầy, đến tận bước cuối.",
        "Trong bóng tối, đi về phía nó.",
        "Làm theo lời thầy đến bước cuối, khi chưa biết hết mọi chuyện. Hoặc để lỡ buổi lễ.",
        "Biết hết sự thật — tóc trong hũ, lời mẹ, quyển sách — mà vẫn chọn lộc.",
    };

    // ───── lưu (một chỗ)
    const string K_DEM = "LOC_Luu_Dem", K_IDS = "LOC_Luu_Ids", K_CO = "LOC_Luu_Co";
    static int DemLuu => PlayerPrefs.GetInt(K_DEM, 0);
    static string MoTaLuu => $"Đêm {DemLuu} · {NGAY[Mathf.Clamp(DemLuu, 0, 3)]}";
    static string[] Tach(string s) => string.IsNullOrEmpty(s) ? new string[0] : s.Split('|');
    static void Luu()
    {
        PlayerPrefs.SetInt(K_DEM, Mathf.Clamp(LocMa.dem, 1, 3));
        PlayerPrefs.SetString(K_IDS, LocTui.ChuoiIds); PlayerPrefs.SetString(K_CO, LocTui.ChuoiCo);
        PlayerPrefs.Save();
    }

    // ───── kết cục
    static int SoLanKet(int i) => PlayerPrefs.GetInt("LOC_Ket_" + KET[i], 0);
    static bool EndingMo => SoLanKet(0) + SoLanKet(2) + SoLanKet(3) > 0;   // hoàn thành game = đạt Good/Bad/Evil
    static string FileAnhKet(int i) => Path.Combine(Application.persistentDataPath, $"LOC_ket_{KET[i]}.png");
    readonly Texture2D[] anhKet = new Texture2D[4];
    public static void DatKet(string id)
    {
        int i = Array.IndexOf(KET, id);
        if (tat || i < 0) return;
        PlayerPrefs.SetInt("LOC_Ket_" + id, SoLanKet(i) + 1); PlayerPrefs.Save();
        if (I) I.StartCoroutine(I.ChupKet(i));
    }
    IEnumerator ChupKet(int i)
    {
        yield return new WaitForEndOfFrame();
        var t = ScreenCapture.CaptureScreenshotAsTexture();
        var n = Thu(t, 480); Destroy(t);
        try { File.WriteAllBytes(FileAnhKet(i), n.EncodeToPNG()); } catch (Exception e) { Debug.LogWarning("[LOC menu] không lưu được ảnh kết: " + e.Message); }
        Destroy(n);
    }

    // thu nhỏ ảnh chụp màn hình bằng trung bình khối (CPU, giữ nguyên giá trị màu) — dùng cho ảnh kết và nền tạm dừng
    static Texture2D Thu(Texture2D src, int w)
    {
        int f = Mathf.Max(1, src.width / w), ow = src.width / f, oh = src.height / f, n = f * f;
        var s = src.GetPixels32(); var o = new Color32[ow * oh];
        for (int y = 0; y < oh; y++)
            for (int x = 0; x < ow; x++)
            {
                int r = 0, g = 0, b = 0;
                for (int j = 0; j < f; j++) { int hang = (y * f + j) * src.width + x * f; for (int i = 0; i < f; i++) { var c = s[hang + i]; r += c.r; g += c.g; b += c.b; } }
                o[y * ow + x] = new Color32((byte)(r / n), (byte)(g / n), (byte)(b / n), 255);
            }
        var t = new Texture2D(ow, oh, TextureFormat.RGBA32, false); t.SetPixels32(o); t.Apply();
        return t;
    }

    // ───── vào game / về menu (nạp lại scene, trạng thái tĩnh đặt trước)
    static void ResetTatCa()
    {
        Time.timeScale = 1; AudioListener.pause = false;
        LocTui.Reset0(); LocMa.Reset0(); LocNhim.Reset0(); LocFirstPerson.Reset0(); LocMiniGame.Reset0();
    }
    static void Vao(int d, string[] ids, string[] co)
    {
        ResetTatCa(); LocMa.VaoDem(d);
        LocXeBuyt.choChay = d == 1 && ids == null;   // [25/10] vào Đêm 1 từ đầu → cảnh xe buýt mở đầu
        if (ids == null && d >= 2)   // chọn đêm để thử: coi như đã chơi xong các đêm trước theo đường chính
        {
            var i = new List<string> { "chia_ham", "so_cua_bo" };
            var c = new List<string> { "d1_mo_dau", "thap_huong", "ho_hang_ve", "thu_ket", "ket_mo", "d1_xuong_ham", "d1_mai_tinh" };
            LocNhim.daTham[1] = true;
            if (d >= 3) { i.AddRange(new[] { "bat_gao", "bat_muoi", "bo_mia" }); c.Add("d2_mo_dau"); LocNhim.daTham[2] = true; LocNhim.daHoangLoan = true; }
            ids = i.ToArray(); co = c.ToArray();
        }
        LocTui.Nap(ids, co);
        vaoGame = true; NapLai();
    }
    static void VeMenu() { ResetTatCa(); vaoGame = false; NapLai(); }
    static void ChoiLaiDem() { Time.timeScale = 1; AudioListener.pause = false; LocMa.ChoiLai(); }
    public static void NapLai()
    {
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(SceneManager.GetActiveScene().path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#endif
    }
    static void Thoat()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ───── móc từ các script khác
    public static void DauDem(int d)   // LocCotTruyen: vào scene + hết đêm
    {
        if (!I || tat || I.man != Man.Khong) return;
        Luu(); I.StartCoroutine(I.TheDem(d));
    }
    public static void ManChet() { if (I && !tat) { I.man = Man.Chet; I.chon = 0; } }   // LocMa: sau dòng chữ Death Ending

#if UNITY_EDITOR
    // phím thử F9/F10/F12 cũng ghi kết cục → lệnh này trả máy về như chưa chơi (giữ nguyên cài đặt)
    [UnityEditor.MenuItem("LOC/Xoá bản lưu + Ending (thử menu)")]
    static void XoaDuLieu()
    {
        PlayerPrefs.DeleteKey(K_DEM); PlayerPrefs.DeleteKey(K_IDS); PlayerPrefs.DeleteKey(K_CO);
        for (int i = 0; i < KET.Length; i++) { PlayerPrefs.DeleteKey("LOC_Ket_" + KET[i]); if (File.Exists(FileAnhKet(i))) File.Delete(FileAnhKet(i)); }
        PlayerPrefs.Save(); Debug.Log("[LOC menu] đã xoá bản lưu và Ending");
    }
#endif

    // ════════════════════════════════════════ vòng đời
    void Awake()
    {
        I = this;
#if UNITY_EDITOR
        foreach (var k in new[] { "LOC_TuKiemGamePlay", "LOC_TuKiemNhim", "LOC_TuKiemMa", "LOC_ThuCauThang", "LOC_ChanDoanNhay", "LOC_ThuXeBuyt" })
            if (UnityEditor.EditorPrefs.GetBool(k, false)) tat = true;
#endif
        if (tat) { enabled = false; return; }
        man = vaoGame ? Man.Khong : daCanhBao ? Man.Chinh : Man.CanhBao;
        chon = DemLuu > 0 ? 0 : 1;
    }

    void Start()
    {
        fp = GetComponent<LocFirstPerson>();
        if (fp) lookGoc = fp.look;
        ApDung(); ApBong();
    }

    void OnApplicationFocus(bool f)
    {
        if (tat || CD("NenAm", 0) == 1) return;
        AudioListener.pause = !f || man == Man.TamDung;
    }

    void Update()
    {
        var kb = Keyboard.current; var ms = Mouse.current;
        chuotDi = ms != null && ms.delta.ReadValue().sqrMagnitude > 0.5f;
        if (man == Man.Khong)
        {
            if (vaoGame && kb != null && kb.escapeKey.wasPressedThisFrame && !dangChup
                && !LocTui.Mo && LocMiniGame.hienTai == null && !LocFirstPerson.khoa) StartCoroutine(MoTamDung());
            return;
        }
        if (man == Man.TheDem) { LocFirstPerson.khoa = true; LocMa.tamDung = true; return; }
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        if (!vaoGame) { LocFirstPerson.khoa = true; LocMa.tamDung = true; }
        if (kb == null) return;

        bool len = kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame;
        bool xuong = kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame;
        bool trai = kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame;
        bool phai = kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;
        bool vao = kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;
        bool lui = kb.escapeKey.wasPressedThisFrame || ms != null && ms.rightButton.wasPressedThisFrame;   // chuột phải = quay lại
        int d = (xuong ? 1 : 0) - (len ? 1 : 0), n = (phai ? 1 : 0) - (trai ? 1 : 0);

        switch (man)
        {
            case Man.CanhBao:
                tCB += Time.unscaledDeltaTime;
                // shortcut: trong Editor bấm qua được ngay cho nhóm đỡ chờ; bản build phải đợi 5 giây như thiết kế
                if ((tCB >= 5f || Application.isEditor && tCB > 0.3f) && (kb.anyKey.wasPressedThisFrame || ms != null && ms.leftButton.wasPressedThisFrame))
                { daCanhBao = true; man = Man.Chinh; chon = DemLuu > 0 ? 0 : 1; }
                break;
            case Man.Chinh:
                if (d != 0) chon = Buoc(chon, d, MoChinh());
                if (vao) BamChinh(chon);
                break;
            case Man.ChonDem:
                if (d != 0) chon = (chon + d + 4) % 4;
                if (vao) BamChonDem(chon);
                if (lui) { man = Man.Chinh; chon = 2; }
                break;
            case Man.Ending:
                if (n != 0) chon = (chon + n + 4) % 4;
                if (lui) { man = Man.Chinh; chon = 3; }
                break;
            case Man.CaiDat:
                var hs = Hangs();
                if (d != 0) chon = (chon + d + hs.Count) % hs.Count;
                if (n != 0) hs[chon].doi?.Invoke(n);
                if (vao) { if (hs[chon].vao != null) hs[chon].vao(); else hs[chon].doi?.Invoke(1); }
                if (kb.qKey.wasPressedThisFrame) DoiTab(-1);
                if (kb.eKey.wasPressedThisFrame) DoiTab(1);
                if (lui) DongCaiDat();
                break;
            case Man.DoSang:
                if (trai || kb.leftArrowKey.isPressed && Time.frameCount % 4 == 0) DatCD("DoSang", Mathf.Clamp(CD("DoSang", 50) - 2, 0, 100));
                if (phai || kb.rightArrowKey.isPressed && Time.frameCount % 4 == 0) DatCD("DoSang", Mathf.Clamp(CD("DoSang", 50) + 2, 0, 100));
                ApDung();
                if (vao) { PlayerPrefs.Save(); man = Man.CaiDat; }
                if (lui) { DatCD("DoSang", sangCu); ApDung(); man = Man.CaiDat; }
                break;
            case Man.TamDung:
                if (d != 0) chon = (chon + d + 5) % 5;
                if (vao) BamTamDung(chon);
                if (lui) TiepTuc();
                break;
            case Man.XacNhan:
                if (n != 0 || d != 0) chon = 1 - chon;
                if (vao) BamXacNhan(chon);
                if (lui) BamXacNhan(1);
                break;
            case Man.Chet:
                if (n != 0 || d != 0) chon = 1 - chon;
                if (vao) BamChet(chon);
                break;
        }
    }

    // camera trôi rất chậm sau menu chính
    void LateUpdate()
    {
        if (vaoGame || man == Man.Khong || !fp || !fp.cam) return;
        if (!coGoc) { camGoc = fp.cam.localRotation; coGoc = true; }
        float t = Time.unscaledTime;
        fp.cam.localRotation = camGoc * Quaternion.Euler(Mathf.Sin(t * 0.11f) * 1.2f, Mathf.Sin(t * 0.07f) * 3.5f, 0);
    }

    // ───── hành động
    static bool[] MoChinh() => new[] { DemLuu > 0, true, true, EndingMo, true, true };
    static int Buoc(int c, int d, bool[] mo) { for (int i = 0; i < mo.Length; i++) { c = (c + d + mo.Length) % mo.Length; if (mo[c]) break; } return c; }

    void BamChinh(int i)
    {
        if (!MoChinh()[i]) return;
        switch (i)
        {
            case 0: Vao(DemLuu, Tach(PlayerPrefs.GetString(K_IDS)), Tach(PlayerPrefs.GetString(K_CO))); break;
            case 1:
                if (DemLuu > 0) XacNhan("Bắt đầu lại từ Đêm 1?", new[] { $"Tiến độ đã lưu ({MoTaLuu}) sẽ bị xoá.", "Chỉ có một chỗ lưu." }, "Bắt đầu lại", "Thôi", () => Vao(1, null, null));
                else Vao(1, null, null);
                break;
            case 2: man = Man.ChonDem; chon = 0; break;
            case 3: man = Man.Ending; chon = 0; NapAnhKet(); break;
            case 4: MoCaiDat(Man.Chinh); break;
            case 5: Thoat(); break;
        }
    }
    void BamChonDem(int i)
    {
        if (i == 3) { man = Man.Chinh; chon = 2; return; }
        int dem = i + 1;
        if (DemLuu > 0) XacNhan($"Chơi Đêm {dem}?", new[] { $"Tiến độ đã lưu ({MoTaLuu}) sẽ bị thay.", "Chỉ có một chỗ lưu." }, $"Chơi Đêm {dem}", "Thôi", () => Vao(dem, null, null));
        else Vao(dem, null, null);
    }
    void BamTamDung(int i)
    {
        int d = Mathf.Clamp(LocMa.dem, 1, 3);
        string[] mat = { "Game tự lưu ở đầu mỗi đêm.", $"Những gì làm từ đầu Đêm {d} tới giờ sẽ mất." };
        switch (i)
        {
            case 0: TiepTuc(); break;
            case 1: MoCaiDat(Man.TamDung); break;
            case 2: XacNhan($"Chơi lại từ đầu Đêm {d}?", mat, "Chơi lại", "Ở lại", ChoiLaiDem); break;
            case 3: XacNhan("Về menu chính?", mat, "Về menu", "Ở lại", VeMenu); break;
            case 4: XacNhan("Thoát game?", mat, "Thoát", "Ở lại", Thoat); break;
        }
    }
    void BamChet(int i) { if (i == 0) ChoiLaiDem(); else VeMenu(); }

    string xnTieu, xnCo, xnKhong; string[] xnDong; Action xnLam;
    void XacNhan(string tieu, string[] dong, string co, string khong, Action lam)
    { nenXN = man; chonNen = chon; man = Man.XacNhan; chon = 1; xnTieu = tieu; xnDong = dong; xnCo = co; xnKhong = khong; xnLam = lam; }
    void BamXacNhan(int i) { if (i == 0) xnLam(); else { man = nenXN; chon = chonNen; } }

    // ───── tạm dừng
    Texture2D anhDung;
    IEnumerator MoTamDung()
    {
        dangChup = true;
        yield return new WaitForEndOfFrame();
        var t = ScreenCapture.CaptureScreenshotAsTexture();
        if (anhDung) Destroy(anhDung);
        anhDung = Thu(t, Mathf.Max(64, Screen.width / 8)); Destroy(t);
        var px = anhDung.GetPixels32();   // rút màu 60% + tối đi
        for (int i = 0; i < px.Length; i++)
        {
            var c = px[i]; float l = 0.3f * c.r + 0.59f * c.g + 0.11f * c.b;
            px[i] = new Color32((byte)((c.r * 0.4f + l * 0.6f) * 0.5f), (byte)((c.g * 0.4f + l * 0.6f) * 0.5f), (byte)((c.b * 0.4f + l * 0.6f) * 0.5f), 255);
        }
        anhDung.SetPixels32(px); anhDung.filterMode = FilterMode.Bilinear; anhDung.wrapMode = TextureWrapMode.Clamp; anhDung.Apply();
        man = Man.TamDung; chon = 0; dangChup = false;
        Time.timeScale = 0; AudioListener.pause = true; LocFirstPerson.khoa = true;
    }
    void TiepTuc()
    {
        man = Man.Khong; Time.timeScale = 1; AudioListener.pause = false; LocFirstPerson.khoa = false;
        StartCoroutine(KhoaChuot());
    }
    IEnumerator KhoaChuot() { yield return null; Cursor.lockState = CursorLockMode.Locked; }   // đợi qua khung Esc của LocFirstPerson

    // ───── thẻ đêm + tự lưu
    IEnumerator TheDem(int d)
    {
        man = Man.TheDem; demThe = d; tThe = 0;
        while (tThe < 4.4f) { tThe += Time.unscaledDeltaTime; yield return null; }
        man = Man.Khong; LocFirstPerson.khoa = false; LocMa.tamDung = false;
    }

    // ════════════════════════════════════════ cài đặt
    static int CD(string k, int md) => PlayerPrefs.GetInt("LOC_CD_" + k, md);
    static void DatCD(string k, int v) => PlayerPrefs.SetInt("LOC_CD_" + k, v);
    static float EV() => (CD("DoSang", 50) - 50) / 50f * 1.2f;   // ±1,2 stop
    int sangCu;

    static VolumeProfile hoSoSang; Volume volSang;
    readonly Dictionary<Light, LightShadows> bongGoc = new();
    void ApDung()
    {
        float phim = new[] { 1f, 0.5f, 0f }[Mathf.Clamp(CD("Phim", 0), 0, 2)];
        LocFirstPerson.vhs = phim;
        var hc = GameObject.Find("LocHinhCu")?.GetComponent<Volume>(); if (hc) hc.weight = phim;
        // độ sáng: một Volume riêng trên LocHinhCu (ưu tiên 10) và dưới LocMatDien (11) — lúc mất điện vẫn tối hẳn
        if (!hoSoSang) { hoSoSang = ScriptableObject.CreateInstance<VolumeProfile>(); hoSoSang.Add<ColorAdjustments>(); }
        if (!volSang) { volSang = new GameObject("LocDoSang").AddComponent<Volume>(); volSang.isGlobal = true; volSang.priority = 10.5f; volSang.sharedProfile = hoSoSang; }
        if (hoSoSang.TryGet(out ColorAdjustments ca)) ca.postExposure.Override(-0.15f * phim + EV());
        Application.targetFrameRate = new[] { 30, 60, 120, 144, -1 }[Mathf.Clamp(CD("Fps", 1), 0, 4)];
        QualitySettings.vSyncCount = CD("VSync", 1);
        AudioListener.volume = CD("AmLuong", 80) / 100f;
        if (fp) fp.look = lookGoc * CD("Nhay", 10) / 10f;
        LocFirstPerson.daoTruc = CD("DaoTruc", 0) == 1;
        LocFirstPerson.lac = new[] { 0f, 1f, 1.8f }[Mathf.Clamp(CD("Lac", 1), 0, 2)];
        LocFirstPerson.coChu = new[] { 20, 24, 30 }[Mathf.Clamp(CD("CoChu", 1), 0, 2)];
        LocFirstPerson.nenChu = new[] { 0f, 0.45f, 0.8f }[Mathf.Clamp(CD("NenChu", 1), 0, 2)];
        LocFirstPerson.nhanChu = new[] { 1f, 1.4f, 1.8f }[Mathf.Clamp(CD("TgChu", 0), 0, 2)];
    }
    void ApBong()   // Tắt = không bóng · Thấp = bóng cứng · Vừa = như scene dựng
    {
        int b = CD("Bong", 2);
        foreach (var l in FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!bongGoc.TryGetValue(l, out var g)) bongGoc[l] = g = l.shadows;
            l.shadows = b == 0 || g == LightShadows.None ? LightShadows.None : b == 1 ? LightShadows.Hard : g;
        }
    }

    class Hang { public string ten; public Func<string> gt; public Action<int> doi; public Action vao; }
    static readonly string[] TAB = { "Hình ảnh", "Âm thanh", "Điều khiển", "Phụ đề" };
    static readonly string[] GHI_CHU =
    {
        "Độ sáng: chỉnh tới khi hình bên phải vừa khuất — game cần lúc tối phải thật tối.",
        "Hiệu ứng, tiếng nền, nhạc menu: thêm khi game có bộ trộn âm thanh.",
        "Đi WASD · Nhìn chuột · Tương tác E · Túi đồ Tab · Gợi ý H · Ngồi C · Tạm dừng Esc",
        null,
    };
    static readonly FullScreenMode[] CHE_DO = { FullScreenMode.ExclusiveFullScreen, FullScreenMode.FullScreenWindow, FullScreenMode.Windowed };
    static readonly string[] TEN_CHE_DO = { "Toàn màn hình", "Không viền", "Cửa sổ" };
    List<Hang> hangs; int hangsTab = -1;
    List<Hang> Hangs()
    {
        if (hangsTab == tab) return hangs;
        hangsTab = tab; hangs = new List<Hang>();
        Hang Chon(string ten, string kh, int md, string[] ds, Action sau = null) => new Hang
        {
            ten = ten, gt = () => ds[Mathf.Clamp(CD(kh, md), 0, ds.Length - 1)],
            doi = d => { DatCD(kh, (Mathf.Clamp(CD(kh, md), 0, ds.Length - 1) + d + ds.Length) % ds.Length); ApDung(); sau?.Invoke(); },
        };
        Hang So(string ten, string kh, int md, int min, int max, int buoc, Func<int, string> hien) => new Hang
        { ten = ten, gt = () => hien(CD(kh, md)), doi = d => { DatCD(kh, Mathf.Clamp(CD(kh, md) + d * buoc, min, max)); ApDung(); } };
        switch (tab)
        {
            case 0:
                hangs.Add(new Hang
                {
                    ten = "Chế độ màn hình", gt = () => TEN_CHE_DO[Mathf.Max(0, Array.IndexOf(CHE_DO, Screen.fullScreenMode))],
                    doi = d => { int i = Mathf.Max(0, Array.IndexOf(CHE_DO, Screen.fullScreenMode)); Screen.fullScreenMode = CHE_DO[(i + d + 3) % 3]; },
                });
                hangs.Add(new Hang
                {
                    ten = "Độ phân giải", gt = () => $"{Screen.width} × {Screen.height}",
                    doi = d =>
                    {
                        var ds = new List<Vector2Int>();
                        foreach (var r in Screen.resolutions) { var v = new Vector2Int(r.width, r.height); if (!ds.Contains(v)) ds.Add(v); }
                        if (ds.Count == 0) return;
                        int i = ds.IndexOf(new Vector2Int(Screen.width, Screen.height)); if (i < 0) i = ds.Count - 1;
                        var m = ds[(i + d + ds.Count) % ds.Count]; Screen.SetResolution(m.x, m.y, Screen.fullScreenMode);
                    },
                });
                hangs.Add(new Hang { ten = "Độ sáng", gt = () => "Chỉnh  ›", vao = () => { sangCu = CD("DoSang", 50); man = Man.DoSang; } });
                hangs.Add(Chon("Lớp phim cũ", "Phim", 0, new[] { "Đầy đủ", "Nhẹ", "Tắt" }));
                hangs.Add(Chon("Chất lượng bóng đổ", "Bong", 2, new[] { "Tắt", "Thấp", "Vừa" }, ApBong));
                hangs.Add(Chon("Giới hạn khung hình", "Fps", 1, new[] { "30", "60", "120", "144", "Không giới hạn" }));
                hangs.Add(Chon("Đồng bộ dọc", "VSync", 1, new[] { "Tắt", "Bật" }));
                break;
            case 1:
                hangs.Add(So("Âm lượng tổng", "AmLuong", 80, 0, 100, 5, v => v.ToString()));
                hangs.Add(Chon("Tiếp tục phát khi chuyển cửa sổ", "NenAm", 0, new[] { "Tắt", "Bật" }));
                break;
            case 2:
                hangs.Add(So("Độ nhạy chuột", "Nhay", 10, 2, 20, 1, v => $"{v / 2},{v % 2 * 5}"));
                hangs.Add(Chon("Đảo trục dọc", "DaoTruc", 0, new[] { "Tắt", "Bật" }));
                hangs.Add(Chon("Lắc camera khi đi", "Lac", 1, new[] { "Tắt", "Nhẹ", "Vừa" }));
                break;
            case 3:
                hangs.Add(Chon("Cỡ chữ", "CoChu", 1, new[] { "Nhỏ", "Vừa", "Lớn" }));
                hangs.Add(Chon("Nền sau chữ", "NenChu", 1, new[] { "Tắt", "Mờ", "Đậm" }));
                hangs.Add(Chon("Thời gian hiện chữ", "TgChu", 0, new[] { "Bình thường", "Chậm", "Rất chậm" }));
                break;
        }
        return hangs;
    }
    void MoCaiDat(Man tu) { cdTu = tu; man = Man.CaiDat; chon = 0; tab = 0; }
    void DoiTab(int d) { tab = (tab + d + TAB.Length) % TAB.Length; chon = 0; }
    void DongCaiDat() { PlayerPrefs.Save(); man = cdTu; chon = cdTu == Man.TamDung ? 1 : 4; }

    // ════════════════════════════════════════ vẽ
    static readonly Color NGA = C(232, 226, 210), MO = C(140, 133, 122), SON = C(176, 48, 42), MUC = C(44, 36, 28),
        KHOA = C(90, 85, 78), PHU = C(100, 95, 88), PHU_ON = C(150, 142, 128), GOI_Y = C(120, 114, 104), NHAT = C(190, 182, 166);
    static Color C(int r, int g, int b) => new(r / 255f, g / 255f, b / 255f);

    static Font fThuong, fDam, fNghieng, fTay; static Texture2D tTieuDe, tGiay, tToiTrai, tToiDung, tTron;
    static GUIStyle st;
    public static Font FontChu(int kieu)   // 0 thường · 1 nghiêng · 2 đậm — LocFirstPerson dùng cho phụ đề
    {
        if (!fThuong) { fThuong = Resources.Load<Font>("LOC_Menu/NotoSerif-Regular"); fDam = Resources.Load<Font>("LOC_Menu/NotoSerif-SemiBold"); fNghieng = Resources.Load<Font>("LOC_Menu/NotoSerif-Italic"); fTay = Resources.Load<Font>("LOC_Menu/PatrickHand-Regular"); }
        return kieu == 1 ? fNghieng : kieu == 2 ? fDam : fThuong;
    }
    static void KhoiTao()
    {
        if (st != null) return;
        FontChu(0);
        tTieuDe = Resources.Load<Texture2D>("LOC_Menu/TieuDe_Loc");
        st = new GUIStyle { wordWrap = false, richText = false, clipping = TextClipping.Overflow, padding = new RectOffset() };
        tToiTrai = Doc(u => 0.75f * Mathf.Clamp01((1 - u) * 1.5f - 0.35f));
        tToiDung = Doc(u => 0.7f * Mathf.Clamp01((1 - u) * 1.6f - 0.5f));
        tTron = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
            { float r = new Vector2(x - 63.5f, y - 63.5f).magnitude; tTron.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(63.5f - r))); }
        tTron.Apply();
        // trang sổ kẻ dòng (như sổ thu chi của mẹ): giấy ngà, kẻ ngang mỗi 62, lề đỏ
        tGiay = new Texture2D(1200, 840, TextureFormat.RGB24, false);
        var px = new Color32[1200 * 840]; var rnd = new System.Random(5);
        for (int y = 0; y < 840; y++)
        {
            bool ke = (839 - y) % 62 == 10;
            for (int x = 0; x < 1200; x++)
            {
                int n = rnd.Next(-5, 6), r = 216 + n, g = 204 + n, b = 176 + n;
                if (ke) { r -= 30; g -= 22; b -= 10; }
                if (x == 92 || x == 93) { g -= 50; b -= 50; }
                px[y * 1200 + x] = new Color32((byte)r, (byte)g, (byte)b, 255);
            }
        }
        tGiay.SetPixels32(px); tGiay.Apply();
    }
    static Texture2D Doc(Func<float, float> a)   // dải tối dần sang phải
    {
        var t = new Texture2D(256, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int i = 0; i < 256; i++) t.SetPixel(i, 0, new Color(0, 0, 0, a(i / 255f)));
        t.Apply(); return t;
    }

    float k, W;
    const float H = 1080;
    Rect R(float x, float y, float w, float h) => new(x * k, y * k, w * k, h * k);
    void O(float x, float y, float w, float h, Color c) { GUI.color = c; GUI.DrawTexture(R(x, y, w, h), Texture2D.whiteTexture); GUI.color = Color.white; }
    void Anh(Texture t, float x, float y, float w, float h, float a = 1) { if (!t) return; GUI.color = new Color(1, 1, 1, a); GUI.DrawTexture(R(x, y, w, h), t); GUI.color = Color.white; }

    // chữ: (x, y) = góc trên trái (neo 0), tâm (1), góc trên phải (2) — đơn vị thiết kế 1080p. Trả về kích thước (đơn vị thiết kế).
    Vector2 Chu(float x, float y, string s, float co, Font f, Color c, int neo = 0, bool bong = true, float a = 1)
    {
        st.font = f; st.fontSize = Mathf.Max(6, Mathf.RoundToInt(co * k));
        var nd = new GUIContent(s); var sz = st.CalcSize(nd);
        float px = x * k - (neo == 1 ? sz.x / 2 : neo == 2 ? sz.x : 0), py = y * k - (neo == 1 ? sz.y / 2 : 0);
        if (bong) { st.normal.textColor = new Color(0, 0, 0, a); GUI.Label(new Rect(px + 2 * k, py + 2 * k, sz.x, sz.y), nd, st); }
        st.normal.textColor = new Color(c.r, c.g, c.b, a); GUI.Label(new Rect(px, py, sz.x, sz.y), nd, st);
        return sz / k;
    }
    void GoiYPhim(string s, float y = H - 62, float x = -1)
    {
        st.font = null; st.fontSize = Mathf.RoundToInt(19 * k); var sz = st.CalcSize(new GUIContent(s));
        st.normal.textColor = GOI_Y; GUI.Label(new Rect(x < 0 ? (Screen.width - sz.x) / 2 : x * k, y * k, sz.x, sz.y), s, st);
    }
    bool Tro(Rect r) => chuotDi && r.Contains(Event.current.mousePosition);
    static bool Nhap(Rect r)
    {
        var e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0 && r.Contains(e.mousePosition)) { e.Use(); return true; }
        return false;
    }

    // danh sách mục kiểu menu chính — trả về mục vừa nhấp (-1 nếu không)
    int DanhSach(float x, float y, string[] ten, string[] phu, bool[] mo, int sel, bool tt, float co = 40, float gian = 64)
    {
        int bam = -1;
        for (int i = 0; i < ten.Length; i++)
        {
            bool on = i == sel, kh = mo != null && !mo[i];
            float yy = y + i * gian;
            if (on) O(x - 34, yy + co * 0.62f, 20, 4, SON);   // vệt mực son
            var sz = Chu(x, yy, ten[i], co, on ? fDam : fThuong, on ? NGA : kh ? KHOA : MO);
            if (phu != null && !string.IsNullOrEmpty(phu[i]))
                Chu(x + sz.x + 22, yy + co * 0.42f, phu[i], co * 0.55f, fNghieng, on ? PHU_ON : PHU);
            if (!tt || kh) continue;
            var r = R(x - 40, yy - 4, 640, gian);
            if (Tro(r)) chon = i;
            if (Nhap(r)) bam = i;
        }
        return bam;
    }

    void OnGUI()
    {
        if (man == Man.Khong) return;
        KhoiTao();
        GUI.depth = -100;
        k = Screen.height / H; W = Screen.width / k;
        var ve = man == Man.XacNhan ? nenXN : man;
        int chonVe = man == Man.XacNhan ? chonNen : chon;
        switch (ve)
        {
            case Man.CanhBao: VeCanhBao(); break;
            case Man.Chinh: NenChinh(0.3f); VeChinh(chonVe, man == ve); break;
            case Man.ChonDem: NenChinh(0.3f); VeChonDem(chonVe, man == ve); break;
            case Man.Ending: NenChinh(0.6f); VeEnding(); break;
            case Man.CaiDat: NenCaiDat(); VeCaiDat(); break;
            case Man.DoSang: VeDoSang(); break;
            case Man.TamDung: NenDung(); VeTamDung(chonVe, man == ve); break;
            case Man.TheDem: VeTheDem(); break;
            case Man.Chet: VeChet(); break;
        }
        if (man == Man.XacNhan) VeXacNhan();
        GUI.color = Color.white;
    }

    void NenChinh(float toi)
    {
        O(0, 0, W, H, new Color(0, 0, 0, toi));
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), tToiTrai);
    }
    void NenDung()
    {
        if (anhDung) GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), anhDung, ScaleMode.ScaleAndCrop);
        else O(0, 0, W, H, Color.black);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), tToiDung);
    }
    void NenCaiDat() { if (cdTu == Man.TamDung) NenDung(); O(0, 0, W, H, new Color(0, 0, 0, cdTu == Man.TamDung ? 0.35f : 0.55f)); }

    // 01 — cảnh báo nội dung
    void VeCanhBao()
    {
        O(0, 0, W, H, C(8, 7, 6));
        float cx = W / 2;
        Chu(cx, 330, "CẢNH BÁO", 30, fDam, SON, 1, false);
        O(cx - 40, 366, 80, 3, SON);
        Chu(cx, 440, "Game có chứa các nội dung về mê tín dị đoan, hù dọa, ám chỉ cái chết.", 32, fThuong, NGA, 1, false);
        Chu(cx, 600, "Tác phẩm hư cấu. Mọi nghi lễ, sách vở, thầy cúng, địa danh trong game đều do hư cấu.", 25, fNghieng, MO, 1, false);
        Chu(cx, 640, "Game phê phán kẻ lợi dụng nỗi sợ của người khác, không phê phán tín ngưỡng.", 25, fNghieng, MO, 1, false);
        float p = Mathf.Clamp01(tCB / 5f);
        if (p >= 1) Chu(cx, 930, "Nhấn phím bất kỳ để tiếp tục", 22, fThuong, C(110, 104, 96), 1, false, Mathf.Clamp01((tCB - 5f) / 0.6f));
        O(cx - 120, 962, 240, 2, C(40, 36, 32)); O(cx - 120, 962, 240 * p, 2, C(110, 104, 96));   // thanh mảnh = đồng hồ đếm 5 giây
    }

    // 02 — menu chính
    void VeChinh(int sel, bool tt)
    {
        if (tTieuDe) Anh(tTieuDe, 128, 100, 452f * tTieuDe.width / tTieuDe.height, 452);
        var mo = MoChinh();
        string[] phu = { DemLuu > 0 ? MoTaLuu : "chưa có bản lưu", "", "", EndingMo ? "" : "mở sau khi hoàn thành game", "", "" };
        int b = DanhSach(160, 560, new[] { "Tiếp tục", "Bắt đầu", "Chọn đêm", "Ending", "Cài đặt", "Thoát" }, phu, mo, sel, tt);
        if (b >= 0) BamChinh(b);
        Chu(W - 110, H - 140, "Nhà tôi nuôi nó mười ba năm.", 26, fNghieng, NHAT, 2);
        Chu(W - 110, H - 100, "Người mang nó tới thì chưa bao giờ đi.", 26, fNghieng, NHAT, 2);
        GoiYPhim("↑↓ / chuột  chọn     Enter / nhấp  vào", x: 160);
    }

    // chọn đêm (mở luôn để nhóm thử)
    void VeChonDem(int sel, bool tt)
    {
        if (tTieuDe) Anh(tTieuDe, 128, 100, 452f * tTieuDe.width / tTieuDe.height, 452, 0.35f);
        Chu(160, 480, "Chọn đêm", 26, fNghieng, MO);
        string[] phu = { $"{TEN_DEM[1]} · {NGAY[1]}", $"{TEN_DEM[2]} · {NGAY[2]}", $"{TEN_DEM[3]} · {NGAY[3]}", "" };
        int b = DanhSach(160, 560, new[] { "Đêm 1", "Đêm 2", "Đêm 3", "Quay lại" }, phu, null, sel, tt);
        if (b >= 0) BamChonDem(b);
        GoiYPhim("↑↓ chọn     Enter  vào     Esc  quay lại", x: 160);
    }

    // 13 — Ending
    void NapAnhKet()
    {
        for (int i = 0; i < 4; i++)
        {
            if (anhKet[i]) Destroy(anhKet[i]); anhKet[i] = null;
            if (SoLanKet(i) == 0 || !File.Exists(FileAnhKet(i))) continue;
            var t = new Texture2D(2, 2); if (t.LoadImage(File.ReadAllBytes(FileAnhKet(i)))) anhKet[i] = t; else Destroy(t);
        }
    }
    void VeEnding()
    {
        int dat = 0; for (int i = 0; i < 4; i++) if (SoLanKet(i) > 0) dat++;
        Chu(160, 110, "Ending", 52, fDam, NGA); Chu(162, 185, $"Đã đạt {dat} / 4", 24, fNghieng, MO);
        float cw = 370, chh = 640, gap = 36, x0 = Mathf.Max(40, Mathf.Min(160, (W - 4 * cw - 3 * gap) / 2)), y0 = 260;
        for (int i = 0; i < 4; i++)
        {
            float x = x0 + i * (cw + gap); bool on = i == chon, co = SoLanKet(i) > 0;
            O(x - 2, y0 - 2, cw + 4, chh + 4, on ? C(160, 150, 134) : C(60, 56, 50)); O(x, y0, cw, chh, C(20, 18, 16));
            if (co && anhKet[i]) GUI.DrawTexture(R(x + 12, y0 + 12, cw - 24, 300), anhKet[i], ScaleMode.ScaleAndCrop);
            else { O(x + 12, y0 + 12, cw - 24, 300, C(10, 9, 8)); if (!co) Chu(x + cw / 2, y0 + 162, "?", 90, fThuong, C(60, 56, 50), 1, false); }
            Chu(x + 24, y0 + 340, KET_NHOM[i], 20, fDam, co ? SON : C(110, 104, 96), 0, false);
            if (co)
            {
                Chu(x + 24, y0 + 375, KET_TEN[i], KET_TEN[i].Length < 14 ? 30 : 25, fDam, NGA, 0, false);
                Chu(x + 24, y0 + chh - 50, $"Đã đạt · {SoLanKet(i)} lần", 20, fNghieng, MO, 0, false);
            }
            else
            {
                Chu(x + 24, y0 + 375, "Chưa mở", 28, fThuong, C(120, 114, 104), 0, false);
                Chu(x + 24, y0 + 440, "Gợi ý", 18, fDam, MO, 0, false);
                float yy = y0 + 470; string dong = "";
                foreach (var tu in KET_GOI_Y[i].Split(' '))   // ngắt dòng theo bề rộng thẻ
                {
                    st.font = fNghieng; st.fontSize = Mathf.RoundToInt(22 * k);
                    if (dong.Length > 0 && st.CalcSize(new GUIContent(dong + " " + tu)).x / k > cw - 48) { Chu(x + 24, yy, dong, 22, fNghieng, NHAT, 0, false); yy += 32; dong = tu; }
                    else dong = dong.Length > 0 ? dong + " " + tu : tu;
                }
                Chu(x + 24, yy, dong, 22, fNghieng, NHAT, 0, false);
            }
            var r = R(x, y0, cw, chh); if (Tro(r)) chon = i;
        }
        GoiYPhim("←→ chọn     Esc / chuột phải  quay lại");
    }

    // 04–07 — cài đặt (trang sổ)
    void VeCaiDat()
    {
        float x0 = W / 2 - 600, y0 = 120, x1 = x0 + 1200, y1 = 960;
        for (int i = 4; i >= 1; i--) O(x0 + 14 - i * 3, y0 + 18 - i * 3, 1200 + i * 6, 840 + i * 6, new Color(0, 0, 0, 0.12f));   // bóng đổ
        Anh(tGiay, x0, y0, 1200, 840);
        Chu(x0 + 130, y0 + 40, "Cài đặt", 44, fDam, MUC, 0, false);
        float tx = x0 + 130;
        for (int t = 0; t < TAB.Length; t++)
        {
            bool on = t == tab;
            var sz = Chu(tx, y0 + 122, TAB[t], 26, on ? fDam : fThuong, on ? MUC : C(130, 118, 100), 0, false);
            if (on) O(tx, y0 + 160, sz.x, 3, SON);
            if (Nhap(R(tx - 10, y0 + 112, sz.x + 20, 56))) { tab = t; chon = 0; }
            tx += sz.x + 50;
        }
        var hs = Hangs(); float xr = x1 - 110;
        for (int i = 0; i < hs.Count; i++)
        {
            float y = y0 + 210 + i * 62; bool on = i == chon;
            var col = on ? MUC : C(70, 60, 48);
            var szT = Chu(x0 + 130, y, hs[i].ten, 28, on ? fDam : fThuong, col, 0, false);
            string gt = hs[i].gt();
            st.font = fDam; st.fontSize = Mathf.RoundToInt(28 * k); float gw = st.CalcSize(new GUIContent(gt)).x / k;
            for (float dx = x0 + 130 + szT.x + 16; dx < xr - gw - 70; dx += 12) Chu(dx, y, ".", 28, fThuong, C(150, 138, 118), 0, false);   // chấm như sổ
            Chu(xr - gw, y, gt, 28, fDam, col, 0, false);
            bool coMuiTen = hs[i].doi != null;
            if (on && coMuiTen) { Chu(xr - gw - 46, y - 2, "‹", 30, fDam, SON, 0, false); Chu(xr + 12, y - 2, "›", 30, fDam, SON, 0, false); }
            if (on) O(x0 + 100, y + 18, 14, 4, SON);
            if (Tro(R(x0 + 90, y - 6, 1010, 58))) chon = i;
            if (coMuiTen && Nhap(R(xr - gw - 60, y - 6, 44, 58))) { chon = i; hs[i].doi(-1); }
            else if (coMuiTen && Nhap(R(xr + 4, y - 6, 44, 58))) { chon = i; hs[i].doi(1); }
            else if (Nhap(R(x0 + 90, y - 6, 1010, 58))) { chon = i; if (hs[i].vao != null) hs[i].vao(); else hs[i].doi?.Invoke(1); }
        }
        if (tab == 3)   // xem trước hai kiểu phụ đề với cỡ chữ + nền đang chọn
        {
            float cx = W / 2, bw = 940;
            O(cx - bw / 2, 640, bw, 200, C(40, 36, 32));
            Chu(cx - bw / 2 + 10, 650, "xem trước", 18, fNghieng, C(150, 140, 126), 0, false);
            float co = LocFirstPerson.coChu * 1.25f;
            XemTruoc(cx, 705, "NHÍM: Anh Khôi ơi! Anh về thật à.", co, fThuong, C(240, 238, 232));
            XemTruoc(cx, 775, "Hai năm rồi mới về.", co, fNghieng, C(200, 198, 192));
        }
        string gc = GHI_CHU[tab];
        if (gc != null) Chu(x0 + 130, y1 - 75, gc, 24, fTay ? fTay : fNghieng, C(80, 70, 60), 0, false);
        GoiYPhim("↑↓ chọn     ←→ đổi     Q / E  sang mục     Esc  quay lại", y1 + 40);
    }
    void XemTruoc(float cx, float cy, string s, float co, Font f, Color c)
    {
        st.font = f; st.fontSize = Mathf.RoundToInt(co * k); var sz = st.CalcSize(new GUIContent(s)) / k;
        if (LocFirstPerson.nenChu > 0) O(cx - sz.x / 2 - 14, cy - sz.y / 2 - 4, sz.x + 28, sz.y + 8, new Color(0, 0, 0, LocFirstPerson.nenChu));
        Chu(cx, cy, s, co, f, c, 1, false);
    }

    // 08 — chỉnh độ sáng: hai khối mắt tối, mô phỏng đúng mức phơi sáng đang chỉnh (OnGUI không đi qua hậu kỳ)
    void VeDoSang()
    {
        O(0, 0, W, H, Color.black);
        float cx = W / 2;
        Chu(cx, 200, "Độ sáng", 40, fDam, NGA, 1, false);
        Chu(cx, 260, "Kéo tới khi hình bên trái vừa đủ thấy, hình bên phải vừa khuất.", 26, fThuong, MO, 1, false);
        float ev = Mathf.Pow(2, EV());
        float Sang(float v) => Mathf.Pow(Mathf.Clamp01(Mathf.Pow(v / 255f, 2.2f) * ev), 1 / 2.2f);
        foreach (var (dx, v) in new[] { (-260f, 22f), (260f, 7f) })
        {
            float s = Sang(v), sm = Sang(v + 12);
            GUI.color = new Color(s, s, s); GUI.DrawTexture(R(cx + dx - 140, 380, 280, 320), tTron);
            GUI.color = new Color(sm, sm, sm);
            foreach (var ex in new[] { -45f, 45f }) GUI.DrawTexture(R(cx + dx + ex - 18, 500, 36, 20), tTron);
            GUI.color = Color.white;
        }
        Chu(cx - 260, 740, "vừa đủ thấy", 22, fNghieng, MO, 1, false); Chu(cx + 260, 740, "vừa khuất", 22, fNghieng, MO, 1, false);
        float p = CD("DoSang", 50) / 100f, sx = cx - 300;
        O(sx, 840, 600, 6, C(60, 56, 50)); O(sx, 840, 600 * p, 6, NGA);
        GUI.color = SON; GUI.DrawTexture(R(sx + 600 * p - 11, 832, 22, 22), tTron); GUI.color = Color.white;
        var e = Event.current;   // kéo trên thanh
        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && R(sx - 20, 810, 640, 60).Contains(e.mousePosition))
        { DatCD("DoSang", Mathf.Clamp(Mathf.RoundToInt((e.mousePosition.x / k - sx) / 6f), 0, 100)); ApDung(); e.Use(); }
        GoiYPhim("←→ chỉnh     Enter  xong     Esc  huỷ", 930);
    }

    // 09 — tạm dừng
    void VeTamDung(int sel, bool tt)
    {
        int d = Mathf.Clamp(LocMa.dem, 1, 3);
        Chu(160, 300, $"ĐÊM {d}", 30, fDam, MO); Chu(160, 345, NGAY_DAI[d], 24, fNghieng, C(110, 104, 96));
        int b = DanhSach(160, 450, new[] { "Tiếp tục", "Cài đặt", "Chơi lại từ đầu đêm", "Về menu chính", "Thoát game" }, null, null, sel, tt);
        if (b >= 0) BamTamDung(b);
        GoiYPhim("E  tương tác     Tab  túi đồ     H  gợi ý     C  ngồi     Esc  tiếp tục", x: 160);
    }

    // 03, 10 — hộp xác nhận
    void VeXacNhan()
    {
        O(0, 0, W, H, new Color(0, 0, 0, 0.55f));
        float x0 = W / 2 - 400, y0 = H / 2 - 150, x1 = W / 2 + 400, y1 = H / 2 + 150;
        O(x0 - 2, y0 - 2, 804, 304, C(70, 64, 56)); O(x0, y0, 800, 300, C(18, 16, 14)); O(x0, y0, 6, 300, SON);
        Chu(x0 + 50, y0 + 45, xnTieu, 34, fDam, NGA, 0, false);
        for (int i = 0; i < xnDong.Length; i++) Chu(x0 + 50, y0 + 110 + i * 36, xnDong[i], 24, fThuong, MO, 0, false);
        string[] nut = { xnCo, xnKhong };
        for (int i = 0; i < 2; i++)
        {
            float xx = x1 - 50 - (2 - i) * 220 + 20; bool on = i == chon;
            if (on) O(xx - 16, y1 - 70 + 18, 12, 4, SON);
            var sz = Chu(xx, y1 - 70, nut[i], 28, on ? fDam : fThuong, on ? NGA : MO, 0, false);
            var r = R(xx - 20, y1 - 80, sz.x + 40, 56);
            if (Tro(r)) chon = i;
            if (Nhap(r)) { BamXacNhan(i); return; }
        }
    }

    // 11 — thẻ đêm + tự lưu
    void VeTheDem()
    {
        float nen = tThe < 3.6f ? 1 : 1 - (tThe - 3.6f) / 0.8f;
        float chu = Mathf.Clamp01((tThe - 0.3f) / 0.8f) * Mathf.Clamp01((3.5f - tThe) / 0.6f);
        var c = C(6, 5, 4); c.a = nen; O(0, 0, W, H, c);
        float cx = W / 2;
        Chu(cx, 470, $"ĐÊM {demThe}", 72, fDam, NGA, 1, false, chu);
        Chu(cx, 560, TEN_DEM[Mathf.Clamp(demThe, 1, 3)], 30, fNghieng, MO, 1, false, chu);
        Chu(cx, 620, NGAY_DAI[Mathf.Clamp(demThe, 1, 3)], 22, fThuong, C(100, 95, 88), 1, false, chu);
        GUI.color = new Color(SON.r, SON.g, SON.b, chu); GUI.DrawTexture(R(W - 150, H - 96, 12, 12), tTron); GUI.color = Color.white;
        Chu(W - 125, H - 104, "đã lưu", 20, fNghieng, C(110, 104, 96), 0, false, chu);
    }

    // 12 — Death Ending: dòng chữ do LocFirstPerson.chuGiua vẽ giữa màn hình; ở đây chỉ thêm hai lựa chọn
    void VeChet()
    {
        string[] ds = { "Chơi lại từ đầu đêm", "Về menu chính" };
        for (int i = 0; i < 2; i++)
        {
            float x = W / 2 - 260 + i * 400; bool on = i == chon;
            if (on) O(x - 34, 640 + 20, 20, 4, SON);
            var sz = Chu(x, 640, ds[i], 30, on ? fDam : fThuong, on ? NGA : MO);
            var r = R(x - 40, 630, sz.x + 60, 56);
            if (Tro(r)) chon = i;
            if (Nhap(r)) { BamChet(i); return; }
        }
    }
}
