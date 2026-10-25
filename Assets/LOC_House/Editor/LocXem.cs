using System.Linq;
using UnityEditor;
using UnityEngine;

// [6/10 tối] Dịch chuyển người chơi (chỉ khi đang Play) tới vài điểm để xem/chụp lại lỗi người chơi báo.
public static class LocXem
{
    static void Den(Vector3 pos, Vector3 nhin)
    {
        var fp = Object.FindFirstObjectByType<LocFirstPerson>();
        if (!fp) { Debug.LogWarning("[LOC] Cần đang Play."); return; }
        fp.DatTuThe(pos, nhin);
    }
    [MenuItem("LOC/Xem/Thử mở cửa gần nhất")]
    static void Thu()
    {
        var fp = Object.FindFirstObjectByType<LocFirstPerson>(); if (!fp) return;
        LocDoor best = null; float bd = 3f;
        foreach (var d in Object.FindObjectsByType<LocDoor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        { float x = Vector3.Distance(d.transform.position, fp.transform.position); if (x < bd) { bd = x; best = d; } }
        if (best) { best.Toggle(); Debug.Log("[LOC] Thử cửa: " + best.name + " (" + best.ten + ") → " + (best.DangMo ? "mở" : "đóng")); }
        else Debug.Log("[LOC] Không có cửa trong 3 m.");
    }
    [MenuItem("LOC/Xem/Theo file xem.txt")]
    static void TheoFile()
    {
        const string f = "Assets/LOC_House/xem.txt";
        if (!System.IO.File.Exists(f)) { Debug.LogWarning("[LOC] Thiếu " + f); return; }
        var v = System.IO.File.ReadAllText(f).Split(new[] { ' ', '\n', '\r', '\t', ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        var n = new float[6]; for (int i = 0; i < 6; i++) float.TryParse(v[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out n[i]);
        var pos = new Vector3(n[0], n[1], n[2]); var nhin = new Vector3(n[3], n[4], n[5]);
        Den(pos, nhin);
        var o = pos + Vector3.up * 1.52f; var dir = (nhin - o).normalized;
        var sb = new System.Text.StringBuilder("[LOC] Xem từ " + pos + " nhìn " + nhin + ":");
        foreach (var h in Physics.RaycastAll(o, dir, 6f, ~0, QueryTriggerInteraction.Ignore).OrderBy(x => x.distance).Take(4))
            sb.Append("\n   " + h.distance.ToString("0.00") + " m · " + (h.collider.transform.parent ? h.collider.transform.parent.parent ? h.collider.transform.parent.parent.name + "/" : "" : "") + (h.collider.transform.parent ? h.collider.transform.parent.name + "/" : "") + h.collider.name);
        Debug.Log(sb);
    }
    [MenuItem("LOC/Xem/Cửa chính từ ngoài")] static void A() => Den(new Vector3(3.8f, 0, -1.4f), new Vector3(3.8f, 1.3f, -0.2f));
    [MenuItem("LOC/Xem/Cửa chính từ trong")] static void B() => Den(new Vector3(3.8f, 0, 1.6f), new Vector3(3.8f, 1.3f, -0.2f));
    [MenuItem("LOC/Xem/Góc trước trái phòng khách")] static void C() => Den(new Vector3(3.2f, 0, 3.0f), new Vector3(0.45f, 0.8f, 0.95f));
    [MenuItem("LOC/Xem/Cửa ban công T2 từ trong")] static void D() => Den(new Vector3(3.8f, 3.4f, 2.0f), new Vector3(3.8f, 4.6f, 0.03f));
    [MenuItem("LOC/Xem/Cửa phòng Nhím")] static void E() => Den(new Vector3(3.15f, 3.4f, 7.9f), new Vector3(3.15f, 4.4f, 9.4f));
}
