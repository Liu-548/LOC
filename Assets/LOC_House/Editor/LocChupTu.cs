using System.Linq;
using UnityEditor;
using UnityEngine;

// [2/10] chụp nhanh từng tủ (trước + chéo trái + chéo phải + sau lưng) và bàn thờ ông bà, ghép 1 ảnh/tủ trong LOC_KiemTra/Tu
public static class LocChupTu
{
    public static readonly string[] Ten = { "BanThoGiaTien_Set", "TuDoTho_Fix", "TuAo_Khoi", "TuQuanAo_GamHo", "TuDauGiuong", "TuNhua_Nhim", "TuThap_Sanh",
        "TuBuffet_ChenBat", "TuHoSo_Sat", "KetSat_KhoaSo", "TuDung_RuongChanMan", "TuKinh", "TuLanh_Bo", "TuTV_Dung_Bo", "BanTrangDiem_Bo",
        "Chan_BatDiaTrongChan", "BanCanhGiuong_Thuoc", "BanThoThanTai",
        "BanCoHoc_Go", "BanHoc_Nhim", "BanHoc_Khoi_Bo", "BanGiay_Go", "KeGiay_ChanCau", "KeGiayDep_Go", "KeSachHoSo_Go", "KeGoDai", "LOC_CanhQuayHang", "Salon_BanNuoc", "BanAn_Bo", "KeVLo_DoKe" };

    [MenuItem("LOC/Chụp từng tủ")]
    public static void Run()
    {
        const string Out = "LOC_KiemTra/Tu"; const int TW = 480, TH = 320;
        System.IO.Directory.CreateDirectory(Out);
        var go = new GameObject("_CamTu"); var cam = go.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);
        cam.fieldOfView = 55; cam.nearClipPlane = 0.02f; cam.farClipPlane = 30;
        var den = go.AddComponent<Light>(); den.type = LightType.Point; den.range = 4; den.intensity = 0.6f;
        var rt = new RenderTexture(TW * 4, TH * 2, 24);
        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        int i = 0;
        foreach (var ten in Ten)
            foreach (var t in all.Where(x => x.name == ten && x.GetComponent<LocProp>()))
            {
                var rs = t.GetComponentsInChildren<Renderer>(); if (rs.Length == 0) continue;
                var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
                var f = LocKiemTu.Front(t);  var right = Vector3.Cross(Vector3.up, f);
                float d = Mathf.Max(b.size.x, b.size.y, b.size.z) * 1.3f + 0.4f;
                var c = b.center;
                var views = new[] { c + f * d + Vector3.up * 0.15f, c + (f + right * 0.7f).normalized * d + Vector3.up * 0.3f,
                                    c + (f - right * 0.7f).normalized * d + Vector3.up * 0.3f, c + f * d * 0.6f + Vector3.up * (b.extents.y + 0.6f) };
                RenderTexture.active = rt; GL.Clear(true, true, Color.black); RenderTexture.active = null;
                cam.targetTexture = rt;
                var doors = t.GetComponentsInChildren<LocDoor>();
                var luu = doors.Select(x => (x.transform.localPosition, x.transform.localRotation, x.transform.localScale)).ToArray();
                for (int pose = 0; pose < 2; pose++)
                {
                    for (int k = 0; k < doors.Length; k++)
                    {
                        var dd = doors[k]; var tr = dd.transform;
                        tr.localPosition = luu[k].Item1; tr.localRotation = luu[k].Item2; tr.localScale = luu[k].Item3;
                        if ((pose == 1) != dd.dungSanLaMo)   // hàng trên: đóng, hàng dưới: mở
                        {
                            var hinge = tr.TransformPoint(dd.banLe);
                            if (dd.truot != Vector3.zero) tr.localPosition = luu[k].Item1 + dd.truot;
                            else { var q = Quaternion.AngleAxis(dd.goc, Vector3.up); tr.SetPositionAndRotation(hinge + q * (tr.position - hinge), q * tr.rotation); }
                        }
                    }
                    for (int v = 0; v < 4; v++)
                    {
                        cam.rect = new Rect(v * 0.25f, pose == 0 ? 0.5f : 0f, 0.25f, 0.5f);
                        cam.transform.position = views[v]; cam.transform.LookAt(c); cam.Render();
                    }
                }
                for (int k = 0; k < doors.Length; k++) { var tr = doors[k].transform; tr.localPosition = luu[k].Item1; tr.localRotation = luu[k].Item2; tr.localScale = luu[k].Item3; }
                cam.targetTexture = null; cam.rect = new Rect(0, 0, 1, 1);
                RenderTexture.active = rt;
                var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); tex.Apply();
                System.IO.File.WriteAllBytes($"{Out}/{i++:00}_{ten}.png", tex.EncodeToPNG());
                RenderTexture.active = null; Object.DestroyImmediate(tex);
            }
        Object.DestroyImmediate(go); Object.DestroyImmediate(rt);
        Debug.Log($"[LOC] Đã chụp {i} tủ → {Out}");
    }
}
