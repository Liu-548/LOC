using UnityEngine;
using UnityEngine.InputSystem;

// [21/10 gộp] Túi đồ dùng chung là LocTui (Scripts/LocTui.cs, Tab). File này chỉ còn phím B. [27/10] Dòng "Đang cầm" bỏ: thanh ô của LocTui đã đánh dấu món đang cầm.
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
}
