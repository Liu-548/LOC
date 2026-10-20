using UnityEngine;
using UnityEngine.InputSystem;

// [21/10 gộp] Túi đồ dùng chung là LocTui (Scripts/LocTui.cs, Tab). File này chỉ còn phím thử + dòng "Đang cầm".
// Phím thử (chỉ Editor / bản Development): F7 cho đủ đồ mini game · F8 thắng ngay mini game đang mở.
public class LocTuiHud : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Tao() { new GameObject("LocTuiHud").AddComponent<LocTuiHud>(); }

    void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        var k = Keyboard.current; if (k == null) return;
        if (k.f7Key.wasPressedThisFrame)
        {
            foreach (var id in LocId.TatCa()) LocTui.Nhan(id, null, false);
            LocFirstPerson.PhuDe("[thử] Đã cho đủ đồ trong túi", 2f, false);
        }
        if (k.f8Key.wasPressedThisFrame) LocMiniGame.BoQua();
#endif
    }

    void OnGUI()
    {
        if (LocTui.DangCam == null || LocMiniGame.hienTai != null || LocTui.Mo) return;
        var st = new GUIStyle(GUI.skin.label) { fontSize = 18 };
        st.normal.textColor = Color.white;
        GUI.Label(new Rect(16, Screen.height - 40, 600, 30), "Đang cầm: " + LocTui.TenMon(LocTui.DangCam), st);
    }
}
