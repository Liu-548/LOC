using System.Linq;
using UnityEditor;
using UnityEditor.Build;

// [02/11] BẢN NỘP — một công tắc tắt mọi nút "Bỏ qua" của giai đoạn phát triển (walkthrough 01/11, việc #9):
//  nút / phím B của mini game (LocMiniGame.choBoQua — đang bật thì Bad (b) không bao giờ xảy ra nếu bấm B)
//  và giữ Enter bỏ qua giấc mơ (LocGiacMo.choBoQua).
//  Bật = thêm ký hiệu biên dịch LOC_BAN_NOP cho Standalone (Editor cũng dùng ký hiệu của nền đang chọn → bấm Play là thấy như bản nộp).
//  Bản build thường (không Development) vốn đã tắt các nút này; công tắc này để chắc chắn cả khi build Development / chơi trong Editor.
//  Đoạn xe buýt / đoạn bác (giữ Enter) là tính năng cho người chơi, không bị tắt. Phím thử (F1–F12, 8, 9…) chỉ có trong Editor.
public static class LocBanNop
{
    const string KY_HIEU = "LOC_BAN_NOP", MENU = "LOC/Bản nộp (tắt mọi nút Bỏ qua)";

    static string[] DS() => PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone).Split(';').Where(x => x.Length > 0).ToArray();
    static bool DangBat() => DS().Contains(KY_HIEU);

    [MenuItem(MENU)]
    static void DoiTrangThai()
    {
        var ds = DS().ToList();
        if (ds.Contains(KY_HIEU)) ds.Remove(KY_HIEU); else ds.Add(KY_HIEU);
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, string.Join(";", ds));
        UnityEngine.Debug.Log(ds.Contains(KY_HIEU) ? "[LOC] BẢN NỘP: đã tắt mọi nút Bỏ qua (mini game, giấc mơ) — Unity sẽ biên dịch lại" : "[LOC] Bản phát triển: bật lại nút Bỏ qua");
    }
    [MenuItem(MENU, true)]
    static bool KiemTra() { Menu.SetChecked(MENU, DangBat()); return true; }
}
