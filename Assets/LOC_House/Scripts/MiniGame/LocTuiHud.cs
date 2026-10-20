using UnityEngine;
using UnityEngine.InputSystem;

// [21/10 gộp] Túi đồ dùng chung là LocTui (Scripts/LocTui.cs, Tab). File này chỉ còn phím B + dòng "Đang cầm".
// B: thắng ngay mini game đang mở (LocMiniGame.choBoQua — giai đoạn phát triển, chạy cả bản build).
public class LocTuiHud : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Tao() { new GameObject("LocTuiHud").AddComponent<LocTuiHud>(); }

    void Update()
    {
        var k = Keyboard.current;
        if (k != null && k.bKey.wasPressedThisFrame) LocMiniGame.BoQua();   // B = bỏ qua (F7–F12 đã là phím thử của LocNhim)
    }

    void OnGUI()
    {
        if (LocTui.DangCam == null || LocMiniGame.hienTai != null || LocTui.Mo) return;
        var st = new GUIStyle(GUI.skin.label) { fontSize = 18 };
        st.normal.textColor = Color.white;
        GUI.Label(new Rect(16, Screen.height - 40, 600, 30), "Đang cầm: " + LocTui.TenMon(LocTui.DangCam), st);
    }
}
