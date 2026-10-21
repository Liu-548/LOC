#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

// [21/10] Thử đi cầu thang T1 (menu LOC → Thử đi cầu thang (Play)): đẩy CharacterController của người chơi theo lộ trình
// phòng khách → vế 1 → chiếu nghỉ → vế 2 → sàn T2 rồi đi ngược lại; kẹt thì ghi vật chặn → LOC_KiemTra/CauThang/BaoCao.txt
public class LocThuCauThang : MonoBehaviour
{
    [UnityEditor.MenuItem("LOC/Thử đi cầu thang (Play)")]
    static void Menu() { UnityEditor.EditorPrefs.SetBool("LOC_ThuCauThang", true); UnityEditor.PlayerSettings.runInBackground = true; UnityEditor.EditorApplication.isPlaying = true; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bat()
    {
        if (!UnityEditor.EditorPrefs.GetBool("LOC_ThuCauThang", false)) return;
        UnityEditor.EditorPrefs.SetBool("LOC_ThuCauThang", false);
        new GameObject("_ThuCauThang").AddComponent<LocThuCauThang>();
    }

    readonly System.Text.StringBuilder sb = new();
    CharacterController cc;

    IEnumerator Start()
    {
        System.IO.Directory.CreateDirectory("LOC_KiemTra/CauThang");
        yield return new WaitForSeconds(1.0f);
        var fp = FindAnyObjectByType<LocFirstPerson>(); cc = fp.GetComponent<CharacterController>();
        sb.AppendLine($"THỬ ĐI CẦU THANG {System.DateTime.Now:dd/MM HH:mm:ss}  cc cao {cc.height} bán kính {cc.radius} bậc {cc.stepOffset} dốc {cc.slopeLimit}");
        var len = new[] { new Vector3(1.45f, 0, 6.6f), new Vector3(1.45f, 0, 7.45f), new Vector3(1.45f, 0, 10.3f), new Vector3(0.45f, 0, 10.3f), new Vector3(0.45f, 0, 7.0f) };
        cc.enabled = false; fp.transform.position = len[0] + Vector3.up * 0.05f; cc.enabled = true;
        yield return new WaitForSeconds(0.5f);
        bool okLen = true, okXuong = true;
        for (int i = 1; i < len.Length && okLen; i++) { var r = new bool[1]; yield return Di(fp.transform, len[i], "lên " + i, r); okLen = r[0]; }
        sb.AppendLine(okLen ? $"=> LÊN T2 ĐƯỢC (y = {fp.transform.position.y:0.00})" : "=> KHÔNG LÊN ĐƯỢC");
        if (okLen)
            for (int i = len.Length - 2; i >= 0 && okXuong; i--) { var r = new bool[1]; yield return Di(fp.transform, len[i], "xuống " + i, r); okXuong = r[0]; }
        sb.AppendLine(okLen && okXuong ? $"=> XUỐNG LẠI PHÒNG KHÁCH ĐƯỢC (y = {fp.transform.position.y:0.00})" : okLen ? "=> KHÔNG XUỐNG ĐƯỢC" : "");
        // cổng: đóng cả ba cánh rồi đo khe
        LocDoor Cua(string n) => GameObject.Find(n)?.GetComponent<LocDoor>();
        var ct = Cua("CanhCong_Trai (mở)"); var cp = Cua("CanhCong_Phai (mở)"); var cn = Cua("CuaNgachCong (mở)");
        foreach (var c in new[] { ct, cp, cn }) if (c && c.DangMo) c.Bam();
        yield return new WaitForSeconds(1.5f);
        if (ct && cp && cn)
        {
            Bounds B(Component c) { var rs = c.GetComponentsInChildren<Renderer>(); var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b; }
            var bt = B(ct); var bp = B(cp); var bn = B(cn);
            sb.AppendLine($"CỔNG ĐÓNG: cánh trái x {bt.min.x:0.000}→{bt.max.x:0.000}, cánh phải {bp.min.x:0.000}→{bp.max.x:0.000}, khe giữa {(bp.min.x - bt.max.x) * 100:0.0} cm");
            sb.AppendLine($"           cửa ngách x {bn.min.x:0.000}→{bn.max.x:0.000} (mặt trụ giữa 5,556 · trụ phải 6,344), hở trụ giữa {(bn.min.x - 5.556f) * 100:0.0} cm");
        }
        else sb.AppendLine("CỔNG: không tìm thấy cánh");
        fp.DatTuThe(new Vector3(3.8f, -0.15f, -2.2f), new Vector3(3.8f, 0.9f, -5.25f)); yield return new WaitForSeconds(0.5f);
        yield return Chup("Cong_Dong");
        // lan can ban công T2: đi thẳng ra mép ban công phải bị chặn
        fp.DatTuThe(new Vector3(3.0f, 3.62f, -0.6f), new Vector3(3.0f, 4.5f, -3f)); yield return new WaitForSeconds(0.5f);
        { var r = new bool[1]; yield return Di(fp.transform, new Vector3(3.0f, 0, -2.0f), "ra mép ban công (phải KẸT)", r);
          sb.AppendLine(r[0] ? "=> LAN CAN: LỖI, đi xuyên ra ngoài" : $"=> LAN CAN: chặn được, đứng ở z {fp.transform.position.z:0.00}"); }
        // phòng Khôi: bật đèn, chụp hai góc
        foreach (var c in FindObjectsByType<LocCongTac>(FindObjectsSortMode.None)) if (c.name.Contains("PhongKhoi")) c.Dat(true);
        fp.DatTuThe(new Vector3(4.0f, 3.62f, 15.9f), new Vector3(7.0f, 3.9f, 13.0f)); yield return new WaitForSeconds(0.6f);
        yield return Chup("Khoi_1");
        fp.DatTuThe(new Vector3(7.0f, 3.62f, 16.0f), new Vector3(3.6f, 4.2f, 12.8f)); yield return new WaitForSeconds(0.6f);
        yield return Chup("Khoi_2");
        System.IO.File.WriteAllText("LOC_KiemTra/CauThang/BaoCao.txt", sb.ToString());
        UnityEditor.EditorApplication.isPlaying = false;
    }

    // đi thẳng (theo mặt phẳng) tới đích; 1 s không tiến được 3 cm → kẹt
    IEnumerator Di(Transform t, Vector3 dich, string ten, bool[] ok)
    {
        float hetGio = Time.time + 12f, moc = Time.time + 1f; var p0 = t.position;
        while (true)
        {
            var d = dich - t.position; d.y = 0;
            if (d.magnitude < 0.05f) { sb.AppendLine($"  OK   {ten}: tới ({t.position.x:0.00},{t.position.y:0.00},{t.position.z:0.00})"); ok[0] = true; yield break; }
            var huong = d.normalized; t.rotation = Quaternion.LookRotation(huong);
            cc.Move(huong * Mathf.Min(1.4f * Time.deltaTime, d.magnitude));
            if (Time.time > moc)
            {
                var dd = t.position - p0; dd.y = 0;
                if (dd.magnitude < 0.03f || Time.time > hetGio)
                {
                    sb.AppendLine($"  KẸT  {ten}: đứng ở ({t.position.x:0.00},{t.position.y:0.00},{t.position.z:0.00}), cần tới ({dich.x:0.00},?,{dich.z:0.00})");
                    var a = t.position + cc.center + Vector3.up * (cc.height / 2 - cc.radius); var b = t.position + cc.center - Vector3.up * (cc.height / 2 - cc.radius);
                    foreach (var h in Physics.CapsuleCastAll(a, b, cc.radius * 0.98f, huong, 0.3f, ~0, QueryTriggerInteraction.Ignore))
                        if (h.collider != cc) sb.AppendLine($"        chặn bởi: {Duong(h.collider.transform)}  tại ({h.point.x:0.00},{h.point.y:0.00},{h.point.z:0.00})");
                    foreach (var c in Physics.OverlapCapsule(a, b, cc.radius + 0.02f, ~0, QueryTriggerInteraction.Ignore))
                        if (c != cc) sb.AppendLine($"        chạm: {Duong(c.transform)}");
                    var up = Vector3.up * 0.2f;   // nếu bước lên thêm 20 cm (lên bậc kế) thì đầu/thân đụng gì
                    foreach (var c in Physics.OverlapCapsule(a + up + huong * 0.1f, b + up + huong * 0.1f, cc.radius, ~0, QueryTriggerInteraction.Ignore))
                        if (c != cc) sb.AppendLine($"        lên thêm 20 cm thì đụng: {Duong(c.transform)}  (đỉnh vật {c.bounds.max.y:0.00}, đáy {c.bounds.min.y:0.00})");
                    ok[0] = false; yield break;
                }
                p0 = t.position; moc = Time.time + 1f;
            }
            yield return null;
        }
    }

    static IEnumerator Chup(string ten) { ScreenCapture.CaptureScreenshot($"LOC_KiemTra/CauThang/{ten}.png"); yield return null; yield return null; yield return null; }

    static string Duong(Transform t) { var s = t.name; for (int i = 0; i < 3 && t.parent; i++) { t = t.parent; s = t.name + "/" + s; } return s; }
}
#endif
