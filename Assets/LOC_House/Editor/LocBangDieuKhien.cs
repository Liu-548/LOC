#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

// [03/11] Bảng điều khiển LOC: mọi lệnh trong menu LOC trong MỘT cửa sổ cuộn được, có ô tìm (gõ không dấu cũng được), chia nhóm.
// Menu LOC quá dài: phải bấm mũi tên cuộn, lỡ tay là menu đóng. Cửa sổ này kéo thả gắn cạnh Inspector/Console để dùng luôn.
// Mở: LOC › » Bảng điều khiển  hoặc  Window › LOC Bảng điều khiển. Không đổi tên menu nào (các lệnh cũ vẫn y nguyên trong menu LOC).
public class LocBangDieuKhien : EditorWindow
{
    [MenuItem("LOC/» Bảng điều khiển (cuộn, tìm kiếm)", false, -100)]
    static void MoTuLoc() => Mo();
    [MenuItem("Window/LOC Bảng điều khiển")]
    static void Mo() { var w = GetWindow<LocBangDieuKhien>("LOC"); w.minSize = new Vector2(260, 220); w.Show(); }

    struct Muc { public string duong, ten, timKhongDau; public int nhom, uuTien; }
    static readonly string[] NHOM = { "Tự kiểm & thử (Play)", "Chụp ảnh kiểm tra", "Kê · Đo · In · Rà soát", "Dựng · Sửa · Nạp · Tạo", "Khác" };
    // lệnh sửa scene/asset: hỏi lại trước khi chạy (bấm nhầm trong danh sách dễ hơn trong menu)
    static readonly string[] CAN_HOI = { "dựng", "sửa", "dọn", "gộp", "áp ", "xoá", "xóa", "nạp lại" };

    List<Muc> ds; string tim = ""; Vector2 cuon;
    readonly Dictionary<int, bool> mo = new();
    GUIStyle nut;

    void OnEnable() => Nap();
    void OnInspectorUpdate() => Repaint();   // cập nhật dòng trạng thái (Play / biên dịch / tự kiểm nối tiếp)

    void Nap()
    {
        ds = new List<Muc>(); var da = new HashSet<string>();
        foreach (var m in TypeCache.GetMethodsWithAttribute<MenuItem>())
            foreach (var o in m.GetCustomAttributes(typeof(MenuItem), false))
            {
                var a = (MenuItem)o;
                if (a.validate || string.IsNullOrEmpty(a.menuItem) || !a.menuItem.StartsWith("LOC/")) continue;
                string d = a.menuItem;
                int sp = d.LastIndexOf(' ');
                if (sp > 0 && sp + 1 < d.Length && "%#&_".IndexOf(d[sp + 1]) >= 0) d = d.Substring(0, sp);   // bỏ phím tắt ở cuối tên
                if (d.StartsWith("LOC/» ") || !da.Add(d)) continue;
                string ten = d.Substring(4);
                ds.Add(new Muc { duong = d, ten = ten, timKhongDau = BoDau(ten), nhom = Nhom(ten), uuTien = a.priority < 100 ? 0 : 1 });
            }
        ds.Sort((x, y) => x.uuTien != y.uuTien ? x.uuTien.CompareTo(y.uuTien) : string.Compare(x.ten, y.ten, System.StringComparison.CurrentCultureIgnoreCase));
    }

    static int Nhom(string ten)
    {
        string s = ten.ToLowerInvariant();
        if (s.Contains("tự kiểm") || s.StartsWith("thử") || s.Contains("(play")) return 0;
        if (s.Contains("chụp")) return 1;
        foreach (var k in new[] { "kê ", "đo ", "in ", "kiểm tra", "rà ", "chẩn đoán", "debug" }) if (s.StartsWith(k)) return 2;
        foreach (var k in new[] { "dựng", "sửa", "nạp", "tạo", "áp ", "dọn", "gộp", "bật", "xoá", "xóa", "mini game", "manu" }) if (s.StartsWith(k)) return 3;
        return 4;
    }

    static string BoDau(string s)
    {
        var n = s.ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD); var b = new StringBuilder(n.Length);
        foreach (var c in n) if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) b.Append(c);
        return b.ToString();
    }

    void OnGUI()
    {
        if (ds == null) Nap();
        if (nut == null) nut = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, wordWrap = true, padding = new RectOffset(8, 6, 4, 4) };

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        tim = EditorGUILayout.TextField(tim, EditorStyles.toolbarSearchField);
        if (GUILayout.Button("Xoá", EditorStyles.toolbarButton, GUILayout.Width(36))) { tim = ""; GUI.FocusControl(null); }
        if (GUILayout.Button("Làm mới", EditorStyles.toolbarButton, GUILayout.Width(58))) Nap();
        EditorGUILayout.EndHorizontal();

        string tt = EditorApplication.isCompiling ? "Đang biên dịch…" : EditorApplication.isPlaying ? "Đang Play" : "Sẵn sàng";
        if (EditorPrefs.HasKey("LOC_TuKiemTatCa_Buoc")) tt += $"  ·  Tự kiểm tất cả: bài {EditorPrefs.GetInt("LOC_TuKiemTatCa_Buoc", 0) + 1}/5";
        EditorGUILayout.HelpBox(tt, MessageType.None);

        string q = BoDau(tim ?? "").Trim(); bool loc = q.Length > 0;
        using (new EditorGUI.DisabledScope(EditorApplication.isCompiling))
        {
            cuon = EditorGUILayout.BeginScrollView(cuon);
            for (int g = 0; g < NHOM.Length; g++)
            {
                var nh = new List<Muc>();
                foreach (var x in ds) if (x.nhom == g && (!loc || x.timKhongDau.Contains(q))) nh.Add(x);
                if (nh.Count == 0) continue;
                bool m = loc || (mo.TryGetValue(g, out var v) ? v : g == 0);
                bool m2 = EditorGUILayout.Foldout(m, $"{NHOM[g]}  ({nh.Count})", true, EditorStyles.foldoutHeader);
                if (!loc) mo[g] = m2;
                if (!m2) continue;
                foreach (var x in nh)
                    if (GUILayout.Button(x.ten, nut)) Chay(x);
                GUILayout.Space(4);
            }
            EditorGUILayout.EndScrollView();
        }
    }

    static void Chay(Muc x)
    {
        string s = x.ten.ToLowerInvariant();
        foreach (var k in CAN_HOI)
            if (s.StartsWith(k) && !EditorUtility.DisplayDialog("LOC", $"Lệnh này sửa scene / asset:\n\n{x.ten}\n\nChạy thật không?", "Chạy", "Thôi")) return;
        string d = x.duong;
        EditorApplication.delayCall += () => { if (!EditorApplication.ExecuteMenuItem(d)) Debug.LogWarning("[LOC] Không chạy được menu: " + d); };
    }
}
#endif
