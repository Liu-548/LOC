using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Túi đồ + cờ nhiệm vụ cho các mini game (kiểu "Giá Y Giấy": nhặt đồ → dùng đúng chỗ → giải đố → mở lối).
// Trạng thái tĩnh, reset mỗi lần bấm Play (cùng kiểu với LocMa).
//  · Tab: mở/đóng danh sách đồ. Phím 1–9 (khi mở Tab): chọn món đang cầm (DangCam).
//  · Cờ (Flag/Dat): ghi nhớ việc đã làm, ví dụ "NghiThuc1".
public static class LocTui
{
    class Mon { public string id, ten; }
    static readonly List<Mon> dsDo = new List<Mon>();
    static readonly HashSet<string> cos = new HashSet<string>();
    public static string DangCam;   // id món đang cầm, hoặc null

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset0() { dsDo.Clear(); cos.Clear(); DangCam = null; }

    public static int Dem { get { return dsDo.Count; } }
    public static string IdCuaSo(int i) { return dsDo[i].id; }
    public static string TenCuaSo(int i) { return dsDo[i].ten; }

    public static bool Co(string id) { foreach (var m in dsDo) if (m.id == id) return true; return false; }
    public static bool CoTatCa(string[] ids)
    {
        if (ids == null) return true;
        foreach (var id in ids) if (!Co(id)) return false;
        return true;
    }
    public static int DemCo(string[] ids)
    {
        int n = 0;
        if (ids != null) foreach (var id in ids) if (Co(id)) n++;
        return n;
    }

    // ten = null: lấy tên trong LocId.Ten. bao = false: không hiện phụ đề "Nhặt được".
    public static void Nhan(string id, string ten = null, bool bao = true)
    {
        if (string.IsNullOrEmpty(id) || Co(id)) return;
        if (string.IsNullOrEmpty(ten)) ten = LocId.Ten(id);
        dsDo.Add(new Mon { id = id, ten = ten });
        if (bao) LocFirstPerson.PhuDe("Nhặt được: " + ten, 2f, false);
    }
    public static void Bo(string id)
    {
        for (int i = 0; i < dsDo.Count; i++) if (dsDo[i].id == id) { dsDo.RemoveAt(i); break; }
        if (DangCam == id) DangCam = null;
    }
    public static void BoTatCa(string[] ids) { if (ids != null) foreach (var id in ids) Bo(id); }

    public static bool Cam(string id) { return DangCam == id; }
    public static bool Flag(string id) { return cos.Contains(id); }
    public static void Dat(string id) { if (!string.IsNullOrEmpty(id)) cos.Add(id); }
}

// HUD túi đồ — tự tạo khi vào cảnh, không cần kéo vào scene.
// Phím thử (chỉ Editor / bản Development): F7 cho đủ đồ · F8 bỏ qua mini game đang mở.
public class LocTuiHud : MonoBehaviour
{
    bool mo;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Tao() { new GameObject("LocTuiHud").AddComponent<LocTuiHud>(); }

    void Update()
    {
        var k = Keyboard.current; if (k == null) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (k.f7Key.wasPressedThisFrame)
        {
            foreach (var id in LocId.TatCa()) LocTui.Nhan(id, null, false);
            LocFirstPerson.PhuDe("[thử] Đã cho đủ đồ trong túi", 2f, false);
        }
        if (k.f8Key.wasPressedThisFrame) LocMiniGame.BoQua();
#endif
        if (LocMiniGame.hienTai != null) { mo = false; return; }
        if (k.tabKey.wasPressedThisFrame) { mo = !mo; LocMa.tamDung = mo; }
        if (!mo) return;
        for (int i = 0; i < 9 && i < LocTui.Dem; i++)
            if (k[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame)
                LocTui.DangCam = LocTui.DangCam == LocTui.IdCuaSo(i) ? null : LocTui.IdCuaSo(i);
    }

    void OnGUI()
    {
        var st = new GUIStyle(GUI.skin.label) { fontSize = 18 };
        st.normal.textColor = Color.white;
        if (LocTui.DangCam != null && LocMiniGame.hienTai == null)
        {
            string ten = LocId.Ten(LocTui.DangCam);
            for (int i = 0; i < LocTui.Dem; i++) if (LocTui.IdCuaSo(i) == LocTui.DangCam) ten = LocTui.TenCuaSo(i);
            GUI.Label(new Rect(16, Screen.height - 40, 600, 30), "Đang cầm: " + ten, st);
        }
        if (!mo) return;
        float h = 60 + 28 * Mathf.Max(1, LocTui.Dem);
        GUI.color = new Color(0, 0, 0, 0.75f); GUI.DrawTexture(new Rect(16, 16, 380, h), Texture2D.whiteTexture); GUI.color = Color.white;
        GUI.Label(new Rect(28, 22, 360, 28), "Túi đồ  (Tab đóng · 1–9 chọn cầm)", st);
        if (LocTui.Dem == 0) GUI.Label(new Rect(28, 54, 360, 28), "Chưa có gì.", st);
        for (int i = 0; i < LocTui.Dem; i++)
            GUI.Label(new Rect(28, 54 + 28 * i, 360, 28), (i + 1) + ". " + LocTui.TenCuaSo(i) + (LocTui.Cam(LocTui.IdCuaSo(i)) ? "   <" : ""), st);
    }
}
