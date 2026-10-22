#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

// [23/10] Tự kiểm cửa lúc Play (menu LOC → Tự kiểm cửa (Play)): mở rồi đóng MỌI LocDoor (bỏ khoá), kiểm
//  bản lề đứng yên · quay đúng góc / trượt đúng quãng · đóng lại về đúng chỗ cũ · đồ con (tay nắm, ổ khoá…) đi theo cánh.
//  → LOC_KiemTra/Cua_Play.txt rồi tự thoát Play.
public class LocTuKiemCua : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bat()
    {
        if (!UnityEditor.EditorPrefs.GetBool("LOC_TuKiemCua", false)) return;
        UnityEditor.EditorPrefs.SetBool("LOC_TuKiemCua", false);
        new GameObject("_TuKiemCua").AddComponent<LocTuKiemCua>();
    }

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        var sb = new System.Text.StringBuilder(); int loi = 0, n = 0;
        var ds = FindObjectsByType<LocDoor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var d in ds)
        {
            var t = d.transform; d.khoa = false; n++;
            var p0 = t.position; var r0 = t.rotation; var h0 = t.TransformPoint(d.banLe);
            var con = t.GetComponentsInChildren<Transform>(); var lc = new Vector3[con.Length];
            for (int i = 0; i < con.Length; i++) lc[i] = t.InverseTransformPoint(con[i].position);
            d.Toggle(); yield return new WaitForSeconds(d.thoiGian + 0.15f);
            string ghi = "";
            if (d.truot != Vector3.zero) { if (Vector3.Distance(t.localPosition - d.truot, t.parent ? t.parent.InverseTransformPoint(p0) : p0) > 0.005f) ghi += " trượt sai quãng"; }
            else if (d.gapX <= 0)
            {
                float goc = Quaternion.Angle(r0, t.rotation), h = Vector3.Distance(h0, t.TransformPoint(d.banLe));
                if (Mathf.Abs(goc - Mathf.Abs(d.goc)) > 1.5f) ghi += $" quay {goc:0}° (cần {Mathf.Abs(d.goc):0}°)";
                if (h > 0.01f) ghi += $" bản lề trôi {h * 100:0.0} cm";
            }
            for (int i = 0; i < con.Length; i++) if (con[i] && Vector3.Distance(t.InverseTransformPoint(con[i].position), lc[i]) > 0.002f) { ghi += $" «{con[i].name}» không theo cánh"; break; }
            d.Toggle(); yield return new WaitForSeconds(d.thoiGian + 0.15f);
            if (Vector3.Distance(t.position, p0) > 0.002f || Quaternion.Angle(t.rotation, r0) > 0.3f) ghi += " đóng lại lệch chỗ cũ";
            if (ghi.Length > 0) loi++;
            sb.AppendLine($"{(ghi.Length > 0 ? "LỖI" : "OK ")}  {Ten(t)} «{d.ten}»{ghi}");
        }
        sb.Insert(0, $"TỰ KIỂM CỬA (Play) — {System.DateTime.Now:dd/MM HH:mm}: {n} cánh, {loi} lỗi\n");
        System.IO.Directory.CreateDirectory("LOC_KiemTra");
        System.IO.File.WriteAllText("LOC_KiemTra/Cua_Play.txt", sb.ToString());
        UnityEditor.EditorApplication.isPlaying = false;
    }
    static string Ten(Transform t) { var s = t.name; while (t.parent && t.parent.name != "NhaLoc") { t = t.parent; s = t.name + "/" + s; } return s; }
}

public static class LocTuKiemCuaMenu
{
    [UnityEditor.MenuItem("LOC/Tự kiểm cửa (Play)")]
    static void Chay()
    {
        UnityEditor.EditorPrefs.SetBool("LOC_TuKiemCua", true);
        if (!UnityEditor.EditorPrefs.HasKey("LOC_RIB")) UnityEditor.EditorPrefs.SetBool("LOC_RIB", UnityEditor.PlayerSettings.runInBackground);
        UnityEditor.PlayerSettings.runInBackground = true;   // LocNhimImport.TraLaiRIB trả lại khi thoát Play
        UnityEditor.EditorApplication.isPlaying = true;
    }
}
#endif
