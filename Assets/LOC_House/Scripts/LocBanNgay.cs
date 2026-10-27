using System.Collections;
using UnityEngine;

// [22/10] Cắt cảnh ban ngày giữa hai đêm (kịch bản v3.2, C5: mở Đêm 2 "tiếng người viếng, tiếng kèn đám, một bàn tay rót trà";
//         mở Đêm 3 "ngắn hơn nữa — ít khách, ghế đã xếp"). Thay cho màn đen 2,5 s trong LocCotTruyen.HetDem.
//  · Không thoại, không chữ, không ma (LocMa.tamDung đang bật suốt cảnh).
//  · Ánh sáng ban ngày chỉ bật trong cảnh: trời sáng, nắng xiên từ mặt tiền, môi trường sáng lên; xong trả lại đúng như cũ.
//  · Camera người chơi rời người trong cảnh rồi về lại chỗ cũ; LocCotTruyen.DauDem đặt người chơi ở chỗ đầu đêm sau đó.
[DefaultExecutionOrder(25)]
public class LocBanNgay : MonoBehaviour
{
    public AudioClip tungKinh, nguoiVieng, kenDam, xeMay, rotTra;
    [Header("Sau Đêm 1: bàn nước")]
    public float camLui = 0.80f, camCao = 0.72f;
    [Header("Sau Đêm 2: sân trước (ghế đã xếp)")]
    public Vector3 sanCam = new(7.1f, 1.5f, -0.7f), sanNhin = new(2.0f, 0.5f, -4.4f);   // trong sân, cạnh cửa chính, nhìn chéo về chồng ghế
    public static bool dangChay;
    public static bool chup;   // thử (phím 9 trong Editor): chụp khung cảnh vào LOC_KiemTra/BanNgay/
    static void Chup(string ten)
    {
        if (!chup) return;
        System.IO.Directory.CreateDirectory("LOC_KiemTra/BanNgay");
        ScreenCapture.CaptureScreenshot($"LOC_KiemTra/BanNgay/{ten}.png");
        System.IO.File.AppendAllText("LOC_KiemTra/BanNgay/log.txt", $"{System.DateTime.Now:HH:mm:ss} chụp {ten}\n");
    }

    AudioSource Nguon(AudioClip c, float vol, bool lap)
    {
        var a = gameObject.AddComponent<AudioSource>(); a.clip = c; a.volume = vol; a.loop = lap; a.spatialBlend = 0; a.playOnAwake = false;
        if (c) a.Play(); return a;
    }
    static IEnumerator Den(float tu, float toi, float giay)
    {
        for (float t = 0; t < 1; t += Time.deltaTime / giay) { LocFirstPerson.den = Mathf.Lerp(tu, toi, t); yield return null; }
        LocFirstPerson.den = toi;
    }
    static IEnumerator NhoDan(AudioSource[] ds, float giay)
    {
        var v0 = new float[ds.Length]; for (int i = 0; i < ds.Length; i++) v0[i] = ds[i] ? ds[i].volume : 0;
        for (float t = 0; t < 1; t += Time.deltaTime / giay) { for (int i = 0; i < ds.Length; i++) if (ds[i]) ds[i].volume = v0[i] * (1 - t); yield return null; }
    }

    // ───── ánh sáng ban ngày: bật / trả lại
    Color amb0, fog0, bg0; Material sky0; float fogD0; CameraClearFlags cf0; Light nang;
    void BatNgay(Camera c)
    {
        amb0 = RenderSettings.ambientLight; fog0 = RenderSettings.fogColor; fogD0 = RenderSettings.fogDensity; sky0 = RenderSettings.skybox;
        cf0 = c.clearFlags; bg0 = c.backgroundColor;
        RenderSettings.ambientLight = new Color(0.82f, 0.84f, 0.86f);
        RenderSettings.skybox = null; RenderSettings.fogColor = new Color(0.78f, 0.80f, 0.80f); RenderSettings.fogDensity = fogD0 * 0.35f;
        c.clearFlags = CameraClearFlags.SolidColor; c.backgroundColor = new Color(0.80f, 0.83f, 0.84f);
        nang = new GameObject("BanNgay_Nang").AddComponent<Light>();
        nang.type = LightType.Directional; nang.color = new Color(1f, 0.95f, 0.86f); nang.intensity = 2.2f; nang.shadows = LightShadows.Soft;
        nang.transform.rotation = Quaternion.Euler(38f, 18f, 0f);   // xiên từ phía mặt đường (z âm) vào nhà
    }
    void TraDem(Camera c)
    {
        RenderSettings.ambientLight = amb0; RenderSettings.fogColor = fog0; RenderSettings.fogDensity = fogD0; RenderSettings.skybox = sky0;
        c.clearFlags = cf0; c.backgroundColor = bg0;
        if (nang) Destroy(nang.gameObject);
    }

    static Bounds Bao(GameObject g)
    {
        var rs = g.GetComponentsInChildren<Renderer>(); var b = new Bounds(g.transform.position, Vector3.zero);
        for (int i = 0; i < rs.Length; i++) if (i == 0) b = rs[i].bounds; else b.Encapsulate(rs[i].bounds);
        return b;
    }

    // [27/10] sáng 8/8 đưa tang: (1) linh cữu giữa phòng khách, kèn đám, tụng kinh — (2) từ trong sân nhìn ra cổng mở, đường vắng, tiếng kèn xa dần.
    //  Không thoại. Phụ đề là chữ thời gian (bản tạm, chờ chốt v3.3 🗣).
    public Vector3 duaTangCam1 = new(3.8f, 1.55f, 0.9f), duaTangNhin1 = new(3.8f, 0.7f, 4.1f);
    public Vector3 duaTangCam2 = new(3.8f, 1.5f, -1.2f), duaTangNhin2 = new(3.8f, 1.1f, -7f);
    IEnumerator DuaTang(Transform cam, Camera c)
    {
        var kd = Nguon(kenDam, 0f, true); var tk = Nguon(tungKinh, 0f, true);
        cam.position = duaTangCam1; cam.rotation = Quaternion.LookRotation(duaTangNhin1 - duaTangCam1, Vector3.up);
        BatNgay(c);
        for (float t = 0; t < 1; t += Time.deltaTime / 1.0f) { kd.volume = 0.5f * t; tk.volume = 0.35f * t; yield return null; }
        yield return Den(1, 0, 1.0f);
        Chup("dem2_dua_tang_1");
        LocFirstPerson.PhuDe("Sáng 8/8. Đưa tang.", 3f, false);
        for (float t = 0; t < 3.4f; t += Time.deltaTime) { cam.position += cam.forward * 0.04f * Time.deltaTime; yield return null; }
        yield return Den(0, 1, 0.7f);
        var cong = new System.Collections.Generic.List<LocDoor>();
        foreach (var n in new[] { "CanhCong_Trai (mở)", "CanhCong_Phai (mở)" }) { var d = GameObject.Find(n)?.GetComponent<LocDoor>(); if (d) { cong.Add(d); d.DatNgay(true); } }
        cam.position = duaTangCam2; cam.rotation = Quaternion.LookRotation(duaTangNhin2 - duaTangCam2, Vector3.up);
        tk.volume = 0f; kd.volume = 0.3f;
        yield return Den(1, 0, 0.8f);
        Chup("dem2_dua_tang_2");
        for (float t = 0; t < 3.2f; t += Time.deltaTime) { kd.volume = Mathf.Lerp(0.3f, 0.04f, t / 3.2f); yield return null; }   // kèn xa dần
        yield return Den(0, 1, 0.8f);
        foreach (var d in cong) if (d) d.DatNgay(false);
        Destroy(kd); Destroy(tk);
        TraDem(c);
        yield return new WaitForSeconds(0.4f);
    }

    // màn đã đen sẵn khi gọi; apDem: dựng trạng thái nhà của đêm sau (gọi lúc còn đen). Kết thúc cũng đen.
    public IEnumerator Chay(int demSau, System.Action apDem)
    {
        dangChay = true;
        if (chup) System.IO.File.AppendAllText("LOC_KiemTra/BanNgay/log.txt", $"{System.DateTime.Now:HH:mm:ss} bắt đầu cảnh sau đêm {demSau - 1}\n");
        var fp = GetComponent<LocFirstPerson>(); var cam = fp ? fp.cam : Camera.main.transform; var c = cam.GetComponent<Camera>();
        Vector3 lp = cam.localPosition; Quaternion lr = cam.localRotation;
        if (demSau != 3) apDem?.Invoke();   // sang Đêm 3: dựng nhà Đêm 3 SAU cảnh đưa tang (lúc đó linh cữu còn trong nhà)

        if (demSau == 2)
        {
            // ── 7/8: nhà còn khách. Đen → loa tụng kinh, người viếng → sáng lên ở bàn nước → kèn đám xa → mờ về đen
            var tk = Nguon(tungKinh, 0.0f, true); var nv = Nguon(nguoiVieng, 0.0f, true);
            for (float t = 0; t < 1; t += Time.deltaTime / 1.2f) { tk.volume = 0.55f * t; nv.volume = 0.5f * t; yield return null; }
            var ban = GameObject.Find("Salon_BanNuoc");
            if (ban)
            {
                var b = Bao(ban); var tam = new Vector3(b.center.x, b.max.y, b.center.z);
                var huong = tam - new Vector3(3.8f, tam.y, tam.z); huong = huong.sqrMagnitude > 0.01f ? huong.normalized : Vector3.right;   // từ giữa phòng khách nhìn ra bàn nước
                cam.position = tam - huong * camLui + Vector3.up * camCao;      // ngồi ghế salon, cúi nhìn mặt bàn
                cam.rotation = Quaternion.LookRotation(tam + huong * 0.05f - cam.position, Vector3.up);
            }
            BatNgay(c);
            yield return Den(1, 0, 1.0f);
            var kd = Nguon(kenDam, 0.32f, true);
            Chup("dem1_sang_ban_nuoc");
            // shortcut: chưa có tay Khôi rót trà (chờ duyệt bản vẽ tay) — giữ khung bàn nước 3,2 s
            for (float t = 0; t < 3.2f; t += Time.deltaTime) { cam.rotation *= Quaternion.Euler(-0.4f * Time.deltaTime, 0.6f * Time.deltaTime, 0); yield return null; }   // trôi rất chậm
            StartCoroutine(NhoDan(new[] { tk, nv, kd }, 1.6f));
            yield return Den(0, 1, 1.2f);
            yield return new WaitForSeconds(0.5f);
            foreach (var a in new[] { tk, nv, kd }) Destroy(a);
        }
        else
        {
            // ── [27/10] VIEC_CON_LAI 1.2: cắt cảnh ĐƯA TANG (bản đơn giản: hai khung tĩnh + phụ đề), rồi mới tới sân vắng
            yield return DuaTang(cam, c);
            apDem?.Invoke();
            // ── 8/8: vắng. Sân trước, ghế nhựa đã xếp chồng, một chiếc xe máy chạy qua ngoài đường
            cam.position = sanCam; cam.rotation = Quaternion.LookRotation(sanNhin - sanCam, Vector3.up);
            BatNgay(c);
            yield return new WaitForSeconds(0.4f);
            var xm = Nguon(xeMay, 0.6f, false);
            yield return Den(1, 0, 0.8f);
            Chup("dem2_sang_san_truoc");
            for (float t = 0; t < 2.2f; t += Time.deltaTime) { cam.position += cam.right * 0.03f * Time.deltaTime; yield return null; }
            yield return Den(0, 1, 0.8f);
            yield return new WaitForSeconds(0.4f);
            Destroy(xm);
        }

        TraDem(c);
        cam.localPosition = lp; cam.localRotation = lr;
        dangChay = false; chup = false;
    }
}
