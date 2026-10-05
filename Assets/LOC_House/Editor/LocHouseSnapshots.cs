// Chụp ảnh kiểm tra nhà LỘC: mặt bằng từng tầng (nhìn từ trên, sáng đều) + các góc 1,65 m theo mục 8.2.
// Menu: LOC → Chụp ảnh kiểm tra. Ảnh lưu ở <project>/LOC_KiemTra/
using UnityEditor;
using UnityEngine;

public static class LocHouseSnapshots
{
    const string Out = "LOC_KiemTra";

    [MenuItem("LOC/Chụp ảnh kiểm tra")]
    public static void Shoot()
    {
        System.IO.Directory.CreateDirectory(Out);
        var go = new GameObject("_CamKiemTra");
        var cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);

        // mặt bằng: ánh sáng đều để thấy bố trí
        var amb = RenderSettings.ambientLight;
        RenderSettings.ambientLight = new Color(0.85f, 0.85f, 0.85f);
        cam.orthographic = true; cam.orthographicSize = 6.8f;
        Plan(cam, "MB_Ham", -2.3f, 2.05f);
        Plan(cam, "MB_T1", 0f, 3.1f);
        Plan(cam, "MB_T2", 3.6f, 2.9f);
        Plan(cam, "MB_T3", 7.0f, 2.9f);
        RenderSettings.ambientLight = amb;

        // góc mắt 1,65 — ánh sáng thật
        cam.orthographic = false; cam.fieldOfView = 70; cam.nearClipPlane = 0.05f; cam.farClipPlane = 60;
        View(cam, "00_NgoaiDuong", 3.8f, 1.5f, -10f, 3.0f, 2.5f, 0f);
        View(cam, "01_TrongCong_NhinVao", 3.8f, 1.5f, -4.8f, 3.8f, 1.3f, 2f);
        View(cam, "02_Hien_SangTiem", 3.8f, 1.5f, -0.8f, -1.5f, 1.2f, -1.2f);
        View(cam, "03_TrongTiem_Quay", -0.9f, 1.65f, -1.2f, -3.3f, 0.9f, -2.3f);
        View(cam, "04_CuaChinh_PhongKhach", 3.8f, 1.65f, 0.3f, 1.5f, 1.0f, 4.5f);
        View(cam, "05_TruocCuaHam", 5.6f, 1.65f, 6.8f, 6.4f, 1.2f, 6.8f);
        View(cam, "06_Bep_GoodEnding", 3.4f, 1.65f, 11.2f, 4.25f, 0.9f, 15.5f);
        View(cam, "07_Ham", 7.1f, -0.7f, 10.2f, 6.4f, -1.6f, 13.4f);
        View(cam, "08_PhongBoMe_GamTu", 1.2f, 5.05f, 2.85f, 1.2f, 3.55f, 5.1f);
        View(cam, "09_CuaPhongNhim", 2.5f, 5.05f, 9.4f, 5.3f, 3.9f, 9.0f);
        View(cam, "10_HanhLang_TuCuaKhoi", 2.5f, 5.05f, 14.0f, 2.5f, 4.8f, 8.0f);
        View(cam, "11_PhongKhoi", 3.6f, 5.05f, 13.8f, 5.7f, 4.2f, 16.3f);
        View(cam, "12_CuaPhongTho", 6.2f, 8.25f, 6.8f, 3.8f, 7.4f, 5.9f);
        View(cam, "13_TruocTuTho_NhinSanPhoi", 3.8f, 8.25f, 5.5f, 3.8f, 7.9f, 1.5f);
        View(cam, "14_SanPhoi", 3.8f, 8.25f, 2.7f, 3.8f, 7.6f, -0.2f);
        View(cam, "15_PhongKhach_TuSanhSau", 3.5f, 1.65f, 7.8f, 2.5f, 1.0f, 2.0f);
        View(cam, "16_TuLanh", 4.6f, 1.4f, 12.9f, 7.4f, 0.8f, 11.5f);
        View(cam, "17_XeDap_GocGamCauThang", 3.6f, 1.4f, 8.4f, 1.0f, 0.5f, 10.4f);
        View(cam, "18_Bep_BenTrai", 3.4f, 1.5f, 12.4f, 0.4f, 0.8f, 15.4f);
        View(cam, "19_Bep_BenPhai", 3.2f, 1.5f, 13.2f, 7.2f, 1.0f, 14.4f);
        View(cam, "20_Cong_TuDuong", 3.8f, 1.3f, -9.0f, 3.0f, 1.0f, -5.4f);
        View(cam, "21_Bat_NhinTuDuoiLen", 3.8f, 0.9f, -2.9f, 3.8f, 2.6f, -1.6f);
        View(cam, "22_GuongBoMe", 2.0f, 4.6f, 2.45f, 0.25f, 4.6f, 2.45f);
        View(cam, "23_GuongWC_T2", 6.05f, 4.6f, 4.1f, 7.6f, 4.55f, 4.1f);
        View(cam, "24_GuongSanh_T2", 1.9f, 4.9f, 6.2f, 0.0f, 4.9f, 6.22f);
        View(cam, "25_WC_T1", 4.9f, 1.9f, 19.4f, 7.0f, 0.9f, 21.4f);
        View(cam, "26_Bep_TuongSau", 4.0f, 1.6f, 12.0f, 3.5f, 1.5f, 16.4f);
        View(cam, "27_HanhLang_T2", 2.8f, 4.9f, 11.0f, 0.5f, 4.4f, 15.0f);
        View(cam, "28_PhongTho_TuongTrai", 5.5f, 8.3f, 5.0f, 0.0f, 8.0f, 5.4f);
        View(cam, "29_SanhT3", 2.5f, 8.3f, 7.3f, 7.0f, 7.5f, 7.6f);
        View(cam, "30_SanhT2_GiaPhoi", 2.6f, 5.0f, 5.9f, 4.2f, 4.0f, 6.9f);
        View(cam, "32_Cong_BenPhai", 8.0f, 1.3f, -9.0f, 7.2f, 1.0f, -5.4f);
        View(cam, "31_BoMe_GocQuat", 4.0f, 4.9f, 2.0f, 1.0f, 4.2f, 4.2f);
        // [29/9 tối] kiểm tra đợt sửa: vải quan tài · cổng (vòng hoa, cáo phó) · khoá cửa hầm · công tắc
        View(cam, "40_VaiQuanTai_TuChan", 5.8f, 1.9f, 3.0f, 3.8f, 0.9f, 4.12f);
        View(cam, "41_VaiQuanTai_TuBen", 1.5f, 1.9f, 6.0f, 3.8f, 1.0f, 4.12f);
        View(cam, "42_Cong_BenTrai", 1.2f, 1.5f, -8.6f, 1.2f, 0.7f, -5.5f);
        View(cam, "43_Cong_BenPhai", 6.6f, 1.5f, -8.6f, 6.4f, 0.7f, -5.5f);
        View(cam, "44_CuaHam_MatNgoai", 4.7f, 1.65f, 6.8f, 6.35f, 1.1f, 6.8f);
        View(cam, "45_CongTac_PhongKhach", 5.3f, 1.65f, 5.4f, 5.3f, 1.45f, 7.4f);
        View(cam, "46_CongTac_CuaChinh", 2.1f, 1.65f, 1.6f, 2.1f, 1.45f, 0f);
        View(cam, "47_CongTac_PhongBoMe", 3.25f, 5.05f, 3.6f, 3.25f, 4.85f, 5.4f);
        View(cam, "48_CongTac_HanhLang", 2.4f, 5.05f, 12.4f, 3.1f, 4.85f, 10.4f);
        View(cam, "49_CongTac_Bep", 4.75f, 1.65f, 12.6f, 4.75f, 1.45f, 11.0f);
        View(cam, "50_Bep_ThanhTreoDungCu", 3.4f, 1.65f, 13.4f, 3.4f, 1.4f, 16.4f);
        View(cam, "51_CuaHam_GocPhongKhach", 4.6f, 1.65f, 4.4f, 6.6f, 1.0f, 6.6f);
        // [29/9 khuya] tủ áo · quạt cây · mắc áo · giỏ quần áo · cửa WC · góc làm việc · công tắc hầm · cửa sân phơi
        View(cam, "60_TuQuanAo_BoMe", 2.6f, 5.05f, 3.2f, 1.2f, 4.6f, 5.2f);
        View(cam, "61_QuatCay_MacAo", 2.8f, 5.05f, 3.4f, 0.4f, 4.5f, 4.0f);
        View(cam, "62_GioQuanAo", 4.9f, 5.05f, 2.8f, 3.85f, 3.8f, 5.0f);
        View(cam, "63_CuaWC_BoMe", 4.5f, 5.0f, 4.3f, 5.75f, 4.6f, 4.25f);
        View(cam, "64_CuaLamViec_TuHanhLang", 1.9f, 5.05f, 6.6f, 4.5f, 4.6f, 6.6f);
        View(cam, "65_LamViec_BenTrong", 7.0f, 5.05f, 8.0f, 3.3f, 4.6f, 6.6f);
        View(cam, "66_CongTac_Ham_DoiDienCua", 6.55f, 1.65f, 6.15f, 7.6f, 1.45f, 6.75f);
        View(cam, "67_CuaSanPhoi_TuPhongTho", 3.8f, 8.25f, 5.3f, 3.8f, 7.9f, 3.0f);
        View(cam, "68_CuaSanPhoi_TuSanPhoi", 3.8f, 8.25f, 0.6f, 3.8f, 7.9f, 3.05f);
        View(cam, "69_TuAo_Khoi", 5.2f, 5.05f, 12.6f, 3.68f, 4.4f, 12.9f);
        View(cam, "70_CuaBoMe_CuaWC_TuNgoai", 3.6f, 5.05f, 3.6f, 3.0f, 4.7f, 5.4f);

        Object.DestroyImmediate(go);
        Debug.Log("[LOC] Đã chụp ảnh kiểm tra vào thư mục " + System.IO.Path.GetFullPath(Out));
    }

    static void Plan(Camera cam, string name, float floorY, float cut)
    {
        cam.transform.SetPositionAndRotation(new Vector3(1.75f, floorY + cut, 9.7f), Quaternion.LookRotation(Vector3.down, Vector3.left));
        cam.nearClipPlane = 0.01f; cam.farClipPlane = cut + 0.3f;
        Save(cam, name, 2000, 866);
    }

    [MenuItem("LOC/Chụp kiểm tra sửa 30-9")]
    public static void Shoot3009()
    {
        System.IO.Directory.CreateDirectory(Out);
        var go = new GameObject("_CamKiemTra");
        var cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);
        cam.fieldOfView = 70; cam.nearClipPlane = 0.03f; cam.farClipPlane = 60;
        View(cam, "s01_CuaHam_Khoa", 4.9f, 1.3f, 6.8f, 6.35f, 1.0f, 6.8f);
        View(cam, "s02_WC_Khan", 4.9f, 1.7f, 20.0f, 5.3f, 1.3f, 21.8f);
        View(cam, "s03_WC_Khan_GanCua", 5.2f, 1.6f, 20.4f, 4.7f, 1.2f, 21.0f);
        View(cam, "s04_BeGiat", 5.4f, 1.5f, 22.6f, 7.2f, 0.7f, 24.3f);
        View(cam, "s05_TuLanh_TayNam", 5.9f, 1.25f, 11.0f, 7.25f, 0.85f, 11.5f);
        View(cam, "s06_RoRaThit", 3.0f, 1.5f, 12.3f, 1.3f, 0.4f, 13.5f);
        View(cam, "s07_ChoiLau", 4.6f, 1.4f, 10.3f, 6.1f, 0.5f, 10.6f);
        View(cam, "s08_Ham_VetAm", 6.9f, -1.0f, 12.3f, 5.2f, -1.2f, 12.0f);
        View(cam, "s09_Ham_ChanThang", 6.0f, -0.8f, 12.0f, 7.15f, -2.2f, 10.2f);
        View(cam, "s10_Ham_DauThang", 7.15f, -0.3f, 9.4f, 7.15f, -2.0f, 10.3f);
        View(cam, "s11_T3_LoiRaCauThang", 1.0f, 8.1f, 8.6f, 1.0f, 7.3f, 6.0f);
        View(cam, "s12_T2_CauThang", 1.3f, 4.9f, 6.9f, 1.9f, 4.5f, 10.0f);
        View(cam, "s13_T2_ChieuNghi", 3.0f, 5.0f, 10.0f, 1.0f, 4.6f, 10.5f);
        View(cam, "s14_WC_Khan_TrongWC", 6.7f, 1.55f, 20.1f, 5.05f, 1.25f, 21.8f);
        View(cam, "s15_WC_Khan_GanHon", 5.9f, 1.55f, 20.9f, 5.05f, 1.3f, 21.8f);
        View(cam, "s16_CuaChinh_BanLe", 3.4f, 1.3f, 0.6f, 2.9f, 1.2f, 0.0f);
        Object.DestroyImmediate(go);
        Debug.Log("[LOC] Đã chụp kiểm tra sửa 30-9 → " + Out);
    }

    [MenuItem("LOC/Chụp kiểm tra 1-10")]
    public static void Shoot0110()
    {
        System.IO.Directory.CreateDirectory(Out);
        var go = new GameObject("_CamKiemTra");
        var cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);
        cam.fieldOfView = 70; cam.nearClipPlane = 0.03f; cam.farClipPlane = 60;
        const float t2 = 3.6f;
        // gầm giường bố mẹ (đầu giường tường phải +X, giường x 5,46–7,46 · z 0,7–2,7)
        View(cam, "t01_GamGiuong_ChanGiuong", 4.4f, t2 + 0.30f, 1.7f, 6.6f, t2 + 0.12f, 1.7f);
        View(cam, "t02_GamGiuong_BenTrai", 6.0f, t2 + 0.30f, 0.45f, 6.4f, t2 + 0.15f, 1.7f);
        View(cam, "t03_GamGiuong_BenPhai", 6.0f, t2 + 0.30f, 2.95f, 6.4f, t2 + 0.15f, 1.7f);
        View(cam, "t04_GamGiuong_Tren", 4.6f, t2 + 1.65f, 1.7f, 6.4f, t2 + 0.5f, 1.7f);
        // cánh cửa phòng làm việc (hai cánh mở vào phòng, bản lề z 6,0 / 7,2 trên tường x 3,1)
        View(cam, "t05_CuaLamViec_TuHanhLang", 1.9f, t2 + 1.65f, 6.6f, 4.5f, t2 + 1.2f, 6.6f);
        View(cam, "t06_CuaLamViec_TuTrongVao", 5.0f, t2 + 1.65f, 6.6f, 3.2f, t2 + 1.2f, 6.6f);
        View(cam, "t07_CuaLamViec_CanhTrai", 4.2f, t2 + 1.4f, 6.5f, 3.3f, t2 + 1.1f, 6.1f);
        View(cam, "t08_CuaLamViec_CanhPhai", 4.2f, t2 + 1.4f, 6.7f, 3.3f, t2 + 1.1f, 7.1f);
        // cầu thang
        View(cam, "u01_T1_Ve1_TuSanh", 3.2f, 1.5f, 8.0f, 1.3f, 1.1f, 8.9f);
        View(cam, "u02_T1_GamThang", 3.2f, 0.5f, 9.4f, 1.2f, 1.0f, 8.6f);
        View(cam, "u03_T1_ChieuNghi_Xuong", 0.45f, 2.7f, 10.6f, 0.45f, 1.7f, 8.2f);
        View(cam, "u04_T1_Ve2_Len", 0.45f, 1.0f, 8.0f, 0.45f, 2.2f, 10.0f);
        View(cam, "u05_T2_Ve1_TuSanh", 3.3f, t2 + 1.5f, 8.0f, 1.3f, t2 + 1.1f, 8.9f);
        View(cam, "u06_T2_ChieuNghi", 0.45f, t2 + 3.2f, 10.6f, 0.45f, t2 + 1.8f, 8.0f);
        View(cam, "u07_ThangHam", 7.3f, 0.9f, 7.5f, 7.3f, -1.5f, 9.8f);
        View(cam, "u08_T1_LanCan_ChieuNghi", 3.8f, 1.5f, 10.2f, 1.8f, 1.6f, 9.9f);
        Object.DestroyImmediate(go);
        Debug.Log("[LOC] Đã chụp kiểm tra 1-10 → " + Out);
    }

    [MenuItem("LOC/Chụp kiểm tra thang 2-10")]
    public static void Shoot0210()
    {
        System.IO.Directory.CreateDirectory(Out);
        var go = new GameObject("_CamKiemTra");
        var cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);
        cam.fieldOfView = 70; cam.nearClipPlane = 0.03f; cam.farClipPlane = 60;
        const float t2 = 3.6f, t3 = 7.0f;
        View(cam, "w01_T1_SanhSau_TuongThang", 4.6f, 1.55f, 9.2f, 1.95f, 1.5f, 9.2f);
        View(cam, "w02_T1_SanhSau_Rong", 5.5f, 1.65f, 7.7f, 2.0f, 1.5f, 10.2f);
        View(cam, "w03_T1_TuPhongKhach_VaoThang", 1.45f, 1.6f, 4.5f, 1.45f, 1.7f, 8.8f);
        View(cam, "w04_T1_Ve1_TrongThang", 1.45f, 1.5f, 7.65f, 1.45f, 2.2f, 9.6f);
        View(cam, "w05_T1_ChieuNghi", 0.45f, 2.7f, 10.6f, 0.45f, 1.7f, 8.2f);
        View(cam, "w06_T2_HanhLang_DocTuong", 2.5f, t2 + 1.5f, 7.7f, 2.5f, t2 + 1.4f, 10.6f);
        View(cam, "w07_T2_ChieuNghi", 0.45f, t2 + 3.2f, 10.6f, 0.45f, t2 + 1.8f, 8.0f);
        View(cam, "w08_T3_SanhGocKho", 3.8f, t3 + 1.5f, 7.8f, 2.0f, t3 + 1.4f, 9.6f);
        View(cam, "w09_T3_DauThang", 3.5f, t3 + 1.6f, 6.0f, 1.0f, t3 + 0.5f, 8.3f);
        Object.DestroyImmediate(go);
        Debug.Log("[LOC] Đã chụp kiểm tra thang 2-10 → " + Out);
    }

    [MenuItem("LOC/Chụp kiểm tra nâng nhà 2-10")]
    public static void Shoot0210b()
    {
        System.IO.Directory.CreateDirectory(Out);
        var go = new GameObject("_CamKiemTra");
        var cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);
        cam.fieldOfView = 70; cam.nearClipPlane = 0.03f; cam.farClipPlane = 80;
        const float t2 = 3.6f, t3 = 7.0f;
        View(cam, "x01_NgoaiDuong", 3.8f, 1.5f, -12f, 3.8f, 4.5f, 0f);
        View(cam, "x02_PhongKhach_Tran", 3.8f, 1.5f, 0.9f, 3.8f, 2.9f, 5.5f);
        View(cam, "x03_PhongKhach_Chum", 3.8f, 1.5f, 6.8f, 3.8f, 2.9f, 4.1f);
        View(cam, "x04_SanhSau_Bep", 4.0f, 1.5f, 8.0f, 4.0f, 1.8f, 13.0f);
        View(cam, "x05_T2_BoMe", 6.0f, t2 + 1.5f, 0.6f, 2.0f, t2 + 1.6f, 4.0f);
        View(cam, "x06_T2_CuaKhoi", 2.0f, t2 + 1.55f, 12.4f, 3.3f, t2 + 1.0f, 13.9f);
        View(cam, "x07_T2_CuaBoMe", 2.6f, t2 + 1.55f, 7.0f, 2.6f, t2 + 1.0f, 5.4f);
        View(cam, "x08_T3_PhongTho", 3.8f, t3 + 1.5f, 3.4f, 3.8f, t3 + 2.2f, 6.2f);
        View(cam, "x09_T3_CuaPhongTho", 5.5f, t3 + 1.55f, 7.8f, 6.3f, t3 + 1.0f, 6.45f);
        View(cam, "x10_T3_SanPhoi_Mai", 3.8f, t3 + 1.5f, 1.0f, 3.8f, t3 + 3.4f, 5.0f);
        Object.DestroyImmediate(go);
        Debug.Log("[LOC] Đã chụp kiểm tra nâng nhà 2-10 → " + Out);
    }

    static void View(Camera cam, string name, float x, float y, float z, float tx, float ty, float tz)
    {
        cam.transform.position = new Vector3(x, y, z);
        cam.transform.LookAt(new Vector3(tx, ty, tz));
        Save(cam, name, 1280, 720);
    }

    static void Save(Camera cam, string name, int w, int h)
    {
        var rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt; cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply();
        System.IO.File.WriteAllBytes($"{Out}/{name}.png", tex.EncodeToPNG());
        cam.targetTexture = null; RenderTexture.active = null;
        Object.DestroyImmediate(rt); Object.DestroyImmediate(tex);
    }
}
