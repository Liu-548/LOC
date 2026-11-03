#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// [03/11] Chạy NỐI TIẾP mọi bài tự kiểm bằng một lệnh: LOC › Tự kiểm TẤT CẢ (nối tiếp).
// Mỗi bài cần Play mode riêng nên KHÔNG chạy song song được trong một Editor; công cụ này bỏ thời gian chờ giữa các bài:
// bài trước tự thoát Play → đợi 3 s (ghi báo cáo/ảnh xong) → tự bấm bài sau. Bài nào treo thì bấm Stop, chuỗi tự sang bài kế.
[InitializeOnLoad]
public static class LocTuKiemTatCa
{
    const string KHOA = "LOC_TuKiemTatCa_Buoc";
    static readonly string[] DS =
    {
        "LOC/Tự kiểm gameplay (Play)",
        "LOC/Tự kiểm giấc mơ (Play)",
        "LOC/Tự kiểm cửa (Play)",
        "LOC/Tự kiểm con ma (Play + chụp)",
        "LOC/Tự kiểm Nhím (Play + chụp)",
    };
    static double cho;

    static LocTuKiemTatCa()
    {
        EditorApplication.playModeStateChanged += KhiDoiChe;
        // nạp lại domain sau khi thoát Play: đang có chuỗi dở thì đợi rồi chạy tiếp
        if (EditorPrefs.HasKey(KHOA) && !EditorApplication.isPlayingOrWillChangePlaymode) HenChay(3);
    }

    [MenuItem("LOC/Tự kiểm TẤT CẢ (nối tiếp)", false, 0)]
    static void BatDau()
    {
        if (EditorApplication.isPlaying) { Debug.LogWarning("[Tự kiểm tất cả] Đang Play — bấm Stop rồi chạy lại."); return; }
        EditorPrefs.SetInt(KHOA, 0);
        Chay();
    }

    [MenuItem("LOC/Dừng tự kiểm nối tiếp", false, 1)]
    static void Dung()
    {
        EditorPrefs.DeleteKey(KHOA); EditorApplication.update -= Doi;
        Debug.Log("[Tự kiểm tất cả] Đã dừng chuỗi (bài đang chạy, nếu có, vẫn chạy nốt).");
    }

    static void KhiDoiChe(PlayModeStateChange s)
    {
        if (s != PlayModeStateChange.EnteredEditMode || !EditorPrefs.HasKey(KHOA)) return;
        EditorPrefs.SetInt(KHOA, EditorPrefs.GetInt(KHOA, 0) + 1);
        HenChay(3);
    }

    static void HenChay(double giay)
    {
        cho = EditorApplication.timeSinceStartup + giay;
        EditorApplication.update -= Doi; EditorApplication.update += Doi;
    }

    static void Doi()
    {
        if (EditorApplication.timeSinceStartup < cho || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        EditorApplication.update -= Doi;
        Chay();
    }

    static void Chay()
    {
        if (!EditorPrefs.HasKey(KHOA) || EditorApplication.isPlayingOrWillChangePlaymode) return;   // đang Play: KhiDoiChe sẽ gọi tiếp khi thoát
        int i = EditorPrefs.GetInt(KHOA, 0);
        if (i >= DS.Length)
        {
            EditorPrefs.DeleteKey(KHOA);
            Debug.Log("[Tự kiểm tất cả] XONG cả " + DS.Length + " bài. Xem báo cáo trong thư mục LOC_KiemTra và Console.");
            EditorUtility.DisplayDialog("LỘC — Tự kiểm", "Đã chạy xong " + DS.Length + " bài tự kiểm.\nXem báo cáo trong LOC_KiemTra và Console.", "OK");
            return;
        }
        Debug.Log($"[Tự kiểm tất cả] Bài {i + 1}/{DS.Length}: {DS[i]}");
        if (!EditorApplication.ExecuteMenuItem(DS[i]))
        {
            Debug.LogWarning($"[Tự kiểm tất cả] Không thấy menu \"{DS[i]}\" — bỏ qua.");
            EditorPrefs.SetInt(KHOA, i + 1); HenChay(0.5);
        }
    }
}
#endif
