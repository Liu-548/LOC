// Tự xoá mọi cờ chạy thử (EditorPrefs) khi thoát Play và khi mở Unity.
// Nếu một lần chạy thử bị dừng giữa chừng, cờ còn bật làm LocMenu tắt menu → bấm Play vào thẳng màn thử.
using UnityEditor;

[InitializeOnLoad]
public static class LocXoaCoThu
{
    static readonly string[] CO = {
        "LOC_TuKiemGamePlay", "LOC_TuKiemNhim", "LOC_TuKiemMa", "LOC_ThuCauThang",
        "LOC_ChanDoanNhay", "LOC_ThuXeBuyt", "LOC_TuKiemGiacMo", "LOC_TuKiemCua", "LOC_ChoiThu"
    };

    static LocXoaCoThu()
    {
        // Mở Unity / biên dịch lại khi đang ở Edit mode → không có lần thử nào đang chạy → xoá sạch.
        if (!EditorApplication.isPlayingOrWillChangePlaymode) Xoa();
        EditorApplication.playModeStateChanged += s => { if (s == PlayModeStateChange.EnteredEditMode) Xoa(); };
    }

    [MenuItem("LOC/Xoá cờ chạy thử (hiện lại menu)")]
    public static void Xoa()
    {
        foreach (var k in CO) EditorPrefs.DeleteKey(k);
    }
}
