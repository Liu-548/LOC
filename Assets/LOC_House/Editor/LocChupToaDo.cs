using System.Linq;
using UnityEditor;
using UnityEngine;

// [2/10 tối] chụp một góc nhìn bất kỳ để kiểm tra: ghi LOC_Chup.txt (mỗi dòng: ten px py pz tx ty tz [fov] [mo=TenTu]) rồi chạy menu → LOC_KiemTra/Chup/ten.png
// mo=TenTu: mở hết cánh/ngăn kéo của tủ đó trước khi chụp (trả lại sau khi chụp).
public static class LocChupToaDo
{
    [MenuItem("LOC/Chụp theo toạ độ")]
    public static void Run()
    {
        const string F = "LOC_Chup.txt", Out = "LOC_KiemTra/Chup"; const int W = 1280, H = 720;
        if (!System.IO.File.Exists(F)) { Debug.Log("[LOC] không có LOC_Chup.txt"); return; }
        System.IO.Directory.CreateDirectory(Out);
        var go = new GameObject("_CamChup"); var cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f); cam.nearClipPlane = 0.02f; cam.farClipPlane = 60;
        var den = go.AddComponent<Light>(); den.type = LightType.Point; den.range = 5; den.intensity = 0.5f;
        var rt = new RenderTexture(W, H, 24); cam.targetTexture = rt;
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        foreach (var line in System.IO.File.ReadAllLines(F))
        {
            var p = line.Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries); if (p.Length < 7) continue;
            float f(int i) => float.Parse(p[i], ci);
            cam.fieldOfView = p.Length > 7 && !p[7].StartsWith("mo=") ? f(7) : 60;
            cam.transform.position = new Vector3(f(1), f(2), f(3)); cam.transform.LookAt(new Vector3(f(4), f(5), f(6)));
            var mo = p.FirstOrDefault(x => x.StartsWith("mo="));
            var doors = new LocDoor[0]; (Vector3, Quaternion, Vector3)[] luu = null;
            if (mo != null)
            {
                var tens = mo.Substring(3).Split(',');
                doors = Object.FindObjectsByType<LocDoor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Where(d => d.GetComponentsInParent<LocProp>().Any(lp => tens.Contains(lp.name))).ToArray();
                luu = doors.Select(x => (x.transform.localPosition, x.transform.localRotation, x.transform.localScale)).ToArray();
                foreach (var d in doors)
                {
                    if (d.dungSanLaMo) continue;   // chỉ mở cánh đang đóng
                    var tr = d.transform; var hinge = tr.TransformPoint(d.banLe);
                    if (d.truot != Vector3.zero) tr.localPosition += d.truot;
                    else { var q = Quaternion.AngleAxis(d.goc, Vector3.up); tr.SetPositionAndRotation(hinge + q * (tr.position - hinge), q * tr.rotation); }
                }
            }
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(W, H, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, W, H), 0, 0); tex.Apply();
            System.IO.File.WriteAllBytes($"{Out}/{p[0]}.png", tex.EncodeToPNG());
            RenderTexture.active = null; Object.DestroyImmediate(tex);
            for (int k = 0; k < doors.Length; k++) { var tr = doors[k].transform; tr.localPosition = luu[k].Item1; tr.localRotation = luu[k].Item2; tr.localScale = luu[k].Item3; }
        }
        cam.targetTexture = null; Object.DestroyImmediate(go); Object.DestroyImmediate(rt);
        System.IO.File.Delete(F);
        Debug.Log("[LOC] chụp xong → " + Out);
    }
}
