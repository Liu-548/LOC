using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// [6/10] "Đi thử" cầu thang bằng CharacterController thật (cùng thông số NguoiChoi), cả lên lẫn xuống, ghi lại chỗ kẹt.
// Menu: LOC → Đi thử cầu thang · kết quả: Console + Assets/LOC_House/BaoCao_DiThu.txt
[ExecuteAlways]
public class LocDiThuHit : MonoBehaviour
{
    public static Dictionary<string, int> hits = new Dictionary<string, int>();
    void OnControllerColliderHit(ControllerColliderHit h)
    {
        string k = (h.collider.transform.parent ? h.collider.transform.parent.name + "/" : "") + h.collider.name + $" n({h.normal.x:0.0},{h.normal.y:0.0},{h.normal.z:0.0})";
        hits[k] = hits.TryGetValue(k, out var n) ? n + 1 : 1;
    }
}

public static class LocDiThu
{
    static readonly Regex Re = new Regex(@"^(.+)_Bac(\d+)$");

    [MenuItem("LOC/Đi thử cầu thang")]
    public static void Run()
    {
        Physics.SyncTransforms();
        var sb = new StringBuilder("ĐI THỬ CẦU THANG (CharacterController cao 1,65 · R 0,25 · step 0,3 · slope 45)\n");
        var flights = new Dictionary<string, List<(int idx, Collider col)>>();
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            var m = Re.Match(c.gameObject.name); if (!m.Success) continue;
            if (!flights.TryGetValue(m.Groups[1].Value, out var l)) flights[m.Groups[1].Value] = l = new List<(int, Collider)>();
            l.Add((int.Parse(m.Groups[2].Value), c));
        }
        var go = new GameObject("__DiThu"); var cc = go.AddComponent<CharacterController>(); go.AddComponent<LocDiThuHit>();
        cc.height = 1.65f; cc.radius = 0.25f; cc.center = new Vector3(0, 0.825f, 0); cc.stepOffset = 0.3f; cc.slopeLimit = 50f; cc.skinWidth = 0.02f;
        foreach (var kv in flights.OrderBy(k => k.Key))
        {
            var st = kv.Value.OrderBy(s => s.idx).ToList();
            var fb = st.First().col.bounds; var lb = st.Last().col.bounds;
            var dir = new Vector3(lb.center.x - fb.center.x, 0, lb.center.z - fb.center.z).normalized;
            float x = fb.center.x;
            // LÊN
            Vector3 start = new Vector3(x, fb.min.y + 0.12f, fb.center.z) - dir * 0.9f;
            sb.AppendLine($"\n== {kv.Key}");
            sb.AppendLine("  LÊN : " + Walk(cc, start, dir, lb.max.y, lb.center + dir * 0.9f));
            // XUỐNG
            Vector3 top = new Vector3(lb.center.x, lb.max.y + 0.05f, lb.center.z) + dir * 0.7f;
            sb.AppendLine("  XUỐNG: " + Walk(cc, top, -dir, fb.max.y, fb.center - dir * 0.9f));
        }
        Object.DestroyImmediate(go);
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_DiThu.txt", sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log("[LOC] Đi thử xong\n" + sb);
    }

    // Nâng lanh-tô ô cửa chân thang T1 (tường PhongKhach_SanhSau, ô x 1,0–1,9): đầu người đi lên bậc đụng lanh-tô cao 2,0 m.
    [MenuItem("LOC/Sửa lanh-tô chân thang")]
    public static void SuaLanhTo()
    {
        int n = 0;
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (r.name != "PhongKhach_SanhSau_1") continue;
            var b = r.bounds;
            if (b.min.y < 1.9f || b.min.y > 2.1f || b.min.x < 0.9f || b.max.x > 2.0f) continue;
            float top = b.max.y, bot = 2.7f, f = (top - bot) / b.size.y;
            Undo.RecordObject(r.transform, "Nâng lanh-tô");
            var t = r.transform; var p = t.position; p.y += (bot + top) / 2f - b.center.y; t.position = p;
            var sc = t.localScale; sc.y *= f; t.localScale = sc; n++;
        }
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (r.name != "HopThangHam_Nam_0") continue;
            var b = r.bounds; if (b.min.y > -0.1f) continue;
            float top = b.max.y, bot = 0f, f = (top - bot) / b.size.y;
            Undo.RecordObject(r.transform, "Nâng xà hầm");
            var t = r.transform; var p = t.position; p.y += (bot + top) / 2f - b.center.y; t.position = p;
            var sc = t.localScale; sc.y *= f; t.localScale = sc; n++;
        }
        foreach (var cc in Object.FindObjectsByType<CharacterController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        { Undo.RecordObject(cc, "skin"); cc.skinWidth = 0.02f; cc.stepOffset = 0.3f; cc.slopeLimit = 50f; n++; }
        Physics.SyncTransforms();
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log($"[LOC] Đã nâng {n} lanh-tô chân thang lên 2,7 m. Nhớ Ctrl+S để lưu scene.");
    }

    static string Walk(CharacterController cc, Vector3 p, Vector3 d, float yGoal, Vector3 goalPos)
    {
        cc.enabled = false; cc.transform.position = p; cc.enabled = true; LocDiThuHit.hits.Clear();
        float vy = 0; Vector3 last = p; float still = 0; float dt = 0.02f;
        for (int i = 0; i < 1200; i++)
        {
            vy = cc.isGrounded ? -1f : vy - 9.81f * dt;
            cc.Move((d * 2.0f + Vector3.up * vy) * dt);
            Physics.SyncTransforms();
            Vector3 q = cc.transform.position;
            if (Vector3.Distance(new Vector3(q.x, 0, q.z), new Vector3(goalPos.x, 0, goalPos.z)) < 0.25f) return $"OK tới (x {q.x:0.00}, y {q.y:0.00}, z {q.z:0.00})";
            still = (new Vector3(q.x, 0, q.z) - new Vector3(last.x, 0, last.z)).magnitude < 0.0005f ? still + dt : 0; last = q;
            if (still > 0.6f)
            {
                var hits = Physics.OverlapCapsule(q + Vector3.up * 0.25f, q + Vector3.up * 1.40f, 0.35f, ~0, QueryTriggerInteraction.Ignore)
                    .Select(c => c.transform.parent ? c.transform.parent.name + "/" + c.name : c.name).Distinct().Take(8);
                return $"KẸT tại (x {q.x:0.00}, y {q.y:0.00}, z {q.z:0.00}) · gần: {string.Join(" | ", hits)} · CHẠM: {string.Join(" | ", LocDiThuHit.hits.Select(k => k.Key + "×" + k.Value))}";
            }
        }
        var e = cc.transform.position; return $"HẾT GIỜ (x {e.x:0.00}, y {e.y:0.00}, z {e.z:0.00})";
    }
}
// v5
