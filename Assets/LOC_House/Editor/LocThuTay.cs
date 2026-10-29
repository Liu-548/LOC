using UnityEditor;
using UnityEngine;

// [29/10] Thử tay Khôi (không cần Play): LOC_ThuTay.txt mỗi dòng "ten viec px py pz tx ty tz" — đặt camera người chơi ở p nhìn t,
//  tay đứng yên ở giữa việc (cham · hai · xoe · chap · tra; đích = t), chụp camera người chơi → LOC_KiemTra/Tay/ten.png. viec "thay": chụp tay thầy.
public static class LocThuTay
{
    // [29/10] Nhím nằm (Nam_Ngu) đúng chỗ giường như lúc chơi, tắt chăn, chụp từ cửa phòng + từ trên xuống; ghi hướng mặt → LOC_KiemTra/Tay/nhim_*.png
    [MenuItem("LOC/Thử Nhím nằm: chụp")]
    public static void NhimNam()
    {
        const string Out = "LOC_KiemTra/Tay"; System.IO.Directory.CreateDirectory(Out);
        var n = Object.FindFirstObjectByType<LocNhim>(FindObjectsInactive.Include); if (!n) return;
        var clip = n.anim.runtimeAnimatorController.animationClips; AnimationClip ngu = null; foreach (var c in clip) if (c.name == "Nam_Ngu") ngu = c;
        n.transform.SetPositionAndRotation(n.neoGiuong, Quaternion.Euler(0, n.yawGiuong, 0));
        if (ngu) ngu.SampleAnimation(n.gameObject, 0f);
        bool chanCu = n.chan && n.chan.activeSelf; if (n.chan) n.chan.SetActive(false);
        var mat = (n.eyeL.position + n.eyeR.position) * 0.5f; var huong = mat - n.head.position; huong.y = 0;
        var cua = n.choCuaTrong; var toiCua = cua - n.head.position; toiCua.y = 0;
        System.IO.File.WriteAllText(Out + "/nhim_log.txt", $"đầu {n.head.position:F2} · mặt hướng {huong.normalized:F2} · tới cửa {toiCua.normalized:F2} · lệch {Vector3.Angle(huong, toiCua):F0}°");
        var go = new GameObject("_CamThu"); var c2 = go.AddComponent<Camera>(); c2.nearClipPlane = 0.02f; c2.fieldOfView = 60;
        var den = go.AddComponent<Light>(); den.type = LightType.Point; den.range = 6; den.intensity = 1.2f;
        var rt = new RenderTexture(1280, 720, 24); c2.targetTexture = rt;
        foreach (var (ten, p) in new[] { ("nhim_tu_cua", cua + Vector3.up * 1.52f), ("nhim_tren", n.neoGiuong + new Vector3(0, 1.6f, 0.9f)) })
        {
            go.transform.position = p; go.transform.LookAt(n.head.position + (n.neoGiuong - n.head.position) * 0.4f);
            c2.Render(); RenderTexture.active = rt;
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
            System.IO.File.WriteAllBytes($"{Out}/{ten}.png", tex.EncodeToPNG()); RenderTexture.active = null; Object.DestroyImmediate(tex);
        }
        c2.targetTexture = null; Object.DestroyImmediate(go); Object.DestroyImmediate(rt);
        if (n.chan) n.chan.SetActive(chanCu);
    }

    static (Vector3 p, Quaternion r, bool bat, LocDi di) diCu;
    [MenuItem("LOC/Thử tay: chụp")]
    public static void Run()
    {
        const string F = "LOC_ThuTay.txt", Out = "LOC_KiemTra/Tay"; const int W = 1280, H = 720;
        if (!System.IO.File.Exists(F)) return;
        System.IO.Directory.CreateDirectory(Out);
        var lt = Object.FindFirstObjectByType<LocTay>(FindObjectsInactive.Include);
        if (!lt) { System.IO.File.WriteAllText(Out + "/log.txt", "không thấy LocTay — dựng lại nhà"); return; }
        var cam = lt.transform.parent; var c = cam.GetComponent<Camera>();
        Vector3 p0 = cam.position; Quaternion r0 = cam.rotation;
        var rt = new RenderTexture(W, H, 24); var cu = c.targetTexture; c.targetTexture = rt;
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        foreach (var line in System.IO.File.ReadAllLines(F))
        {
            var p = line.Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries); if (p.Length < 8) continue;
            float f(int i) => float.Parse(p[i], ci);
            var dich = new Vector3(f(5), f(6), f(7));
            cam.position = new Vector3(f(2), f(3), f(4)); cam.LookAt(dich);
            if (p[1] == "thay" && lt.tayThay)
            {
                lt.tayThay.gameObject.SetActive(true);
                var vao = new Vector3(-0.55f, 0f, 0.83f).normalized;   // như LocCotTruyen.ThayNhanHu
                lt.tayThay.SetPositionAndRotation(dich - vao * 0.08f, Quaternion.LookRotation(vao, Vector3.down));
                int n = lt.xuongThay.Length, k = System.Array.IndexOf(lt.tenTT, "Xin");
                for (int i = 0; i < n; i++) lt.xuongThay[i].localRotation = lt.ttThay[k * n + i];
            }
            else if (p[1] == "bac")   // bác đứng cách camera 0,62 m về phía đích, nắm cẳng tay trái Khôi
            {
                var di = Object.FindFirstObjectByType<LocDi>(FindObjectsInactive.Include);
                var huong = dich - cam.position; huong.y = 0; huong.Normalize();
                var chan = new Vector3(cam.position.x, cam.position.y - 1.52f, cam.position.z) + huong * 0.62f;
                cam.LookAt(chan + Vector3.up * 1.15f);
                lt.ThuTinh("giu", dich);
                if (di)
                {
                    diCu = (di.transform.position, di.transform.rotation, di.gameObject.activeSelf, di);
                    di.gameObject.SetActive(true);
                    di.transform.SetPositionAndRotation(chan, Quaternion.Euler(0, Vector3.SignedAngle(di.huong0, -huong, Vector3.up), 0));
                    di.ThuNam(LocTay.CangTayTrai);
                    System.IO.File.AppendAllText(Out + "/log.txt", $"bác ở {chan} · nắm tới {LocTay.CangTayTrai} · {di.ThuBao()}\n");
                }
            }
            else lt.ThuTinh(p[1], dich);
            c.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(W, H, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, W, H), 0, 0); tex.Apply();
            System.IO.File.WriteAllBytes($"{Out}/{p[0]}.png", tex.EncodeToPNG());
            RenderTexture.active = null; Object.DestroyImmediate(tex);
            lt.ThuTat(); if (lt.tayThay) lt.tayThay.gameObject.SetActive(false);
            if (diCu.di) { diCu.di.transform.SetPositionAndRotation(diCu.p, diCu.r); diCu.di.ThuTha(); diCu.di.gameObject.SetActive(diCu.bat); diCu = default; }
        }
        c.targetTexture = cu; Object.DestroyImmediate(rt);
        cam.SetPositionAndRotation(p0, r0);
        System.IO.File.Delete(F);
    }
}
