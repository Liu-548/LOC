#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// [21/10] Chẩn đoán chớp nháy khi xoay camera (menu LOC → Chẩn đoán nháy). Tắt sẵn mọi thứ cố ý thay đổi theo thời gian
//  (hậu kỳ hạt phim, lớp VHS, đèn dầu nhấp nháy, SSAO nhiễu xanh). Xoay 1°/khung qua cùng một đường HAI LẦN, so từng khung cùng góc:
//  hình dựng đúng thì hai lần giống hệt → độ lệch = nháy thật. Lặp lại khi tắt lần lượt từng nghi phạm. Kết quả → LOC_KiemTra/Nhay/BaoCao.txt
public class LocChanDoanNhay : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bat()
    {
        if (!UnityEditor.EditorPrefs.GetBool("LOC_ChanDoanNhay", false)) return;
        UnityEditor.EditorPrefs.SetBool("LOC_ChanDoanNhay", false);
        new GameObject("_ChanDoanNhay").AddComponent<LocChanDoanNhay>();
    }

    const int GX = 64, GY = 36, N = 60;
    UniversalRendererData rd; RenderingMode rm0; UniversalRenderPipelineAsset ua; bool ssao0;
    void OnDestroy() { if (rd) { rd.renderingMode = rm0; rd.rendererFeatures[0].SetActive(ssao0); UnityEditor.EditorUtility.SetDirty(rd); } }   // trả renderer như cũ dù dừng giữa chừng

    IEnumerator Start()
    {
        System.IO.Directory.CreateDirectory("LOC_KiemTra/Nhay");
        yield return new WaitForSeconds(2.5f);
        var fp = FindAnyObjectByType<LocFirstPerson>(); var cam = Camera.main;
        LocMa.tamDung = true;
        rd = UnityEditor.AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PC_Renderer.asset"); rm0 = rd.renderingMode; ssao0 = rd.rendererFeatures[0].isActive;
        var cd = cam.GetUniversalAdditionalCameraData();
        cd.renderPostProcessing = false; LocFirstPerson.vhs = 0;
        foreach (var b in FindObjectsByType<LocLuaNhapNhay>(FindObjectsSortMode.None)) b.enabled = false;
        rd.rendererFeatures[0].SetActive(false);
        var bong = new List<(Light l, LightShadows s)>();
        foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None)) if (l.shadows != LightShadows.None) bong.Add((l, l.shadows));
        var sb = new System.Text.StringBuilder($"CHẨN ĐOÁN NHÁY v2 {System.DateTime.Now:dd/MM HH:mm} · {SystemInfo.graphicsDeviceName} · {SystemInfo.graphicsDeviceType} · {Screen.width}×{Screen.height} · đường dựng {rm0}\n");
        sb.AppendLine("đèn có bóng: " + string.Join(", ", bong.ConvertAll(b => $"{b.l.name}({b.l.type})")));
        sb.AppendLine("Độ lệch giữa 2 lần xoay giống hệt nhau (0 = không nháy). Đơn vị: 1/1000 độ sáng.\n");

        System.Action<bool> bongDiem = on => { foreach (var b in bong) if (b.l.type != LightType.Directional) b.l.shadows = on ? b.s : LightShadows.None; };
        System.Action<bool> bongMat = on => { foreach (var b in bong) if (b.l.type == LightType.Directional) b.l.shadows = on ? b.s : LightShadows.None; };
        var cauHinh = new (string ten, System.Action bat, System.Action tat)[]
        {
            ("Forward+ (hiện tại)", null, null),
            ("Forward+ · tắt bóng đèn điểm", () => bongDiem(false), () => bongDiem(true)),
            ("Forward+ · tắt bóng mặt trời", () => bongMat(false), () => bongMat(true)),
            ("Forward+ · tắt mọi bóng", () => { bongDiem(false); bongMat(false); }, () => { bongDiem(true); bongMat(true); }),
            ("Forward", () => rd.renderingMode = RenderingMode.Forward, () => rd.renderingMode = rm0),
            ("Deferred", () => rd.renderingMode = RenderingMode.Deferred, () => rd.renderingMode = rm0),
            ("Forward+ · bật lại SSAO", () => rd.rendererFeatures[0].SetActive(true), () => rd.rendererFeatures[0].SetActive(false)),
        };
        var dong = new[] { (new Vector3(3.8f, 0.05f, 0.9f), -30f), (new Vector3(3.2f, 0.05f, 6.6f), 150f) };
        int so = 0;
        foreach (var (ten, bat, tat) in cauHinh)
        {
            bat?.Invoke();
            for (int k = 0; k < 20; k++) yield return null;   // cho renderer dựng lại sau khi đổi
            float tong = 0, max = 0; int dem = 0, khungLech = 0; Texture2D anhA = null, anhB = null;
            for (int v = 0; v < dong.Length; v++)
            {
                var luot = new List<float[]>[2] { new(), new() }; var anh = new List<Texture2D>[2] { new(), new() };   // ảnh thu nhỏ 640×360
                for (int p = 0; p < 2; p++)
                {
                    LocFirstPerson.khoa = true; fp.DatTuThe(dong[v].Item1, dong[v].Item1 + Quaternion.Euler(0, dong[v].Item2, 0) * Vector3.forward + Vector3.up * 1.45f);
                    for (int k = 0; k < 6; k++) yield return null;
                    for (int i = 0; i < N; i++)
                    {
                        fp.transform.rotation = Quaternion.Euler(0, dong[v].Item2 + i * 1f, 0);
                        yield return new WaitForEndOfFrame();
                        var t = ScreenCapture.CaptureScreenshotAsTexture();
                        luot[p].Add(Luoi(t)); anh[p].Add(Nho(t)); Destroy(t);
                    }
                }
                for (int i = 0; i < N; i++)
                {
                    float s = 0, m = 0; var a = luot[0][i]; var b = luot[1][i];
                    for (int j = 0; j < a.Length; j++) { float d = Mathf.Abs(a[j] - b[j]); s += d; m = Mathf.Max(m, d); }
                    tong += s / a.Length; dem++; if (m > 0.03f) khungLech++;
                    if (m > max) { max = m; anhA = anh[0][i]; anhB = anh[1][i]; }
                }
                if (anhA) { System.IO.File.WriteAllBytes($"LOC_KiemTra/Nhay/v2_{so}_a.png", anhA.EncodeToPNG()); System.IO.File.WriteAllBytes($"LOC_KiemTra/Nhay/v2_{so}_b.png", anhB.EncodeToPNG()); anhA = anhB = null; }
                foreach (var l in anh) foreach (var t in l) Destroy(t);
            }
            sb.AppendLine($"{ten,-32} lệch TB {tong / dem * 1000:0.00} · ô lệch nhất {max * 1000:0} · khung nháy (ô lệch > 3%) {khungLech}/{dem}");
            so++;
            tat?.Invoke();
            for (int k = 0; k < 10; k++) yield return null;
        }
        System.IO.File.WriteAllText("LOC_KiemTra/Nhay/BaoCao.txt", sb.ToString());
        UnityEditor.EditorApplication.isPlaying = false;
    }

    static Texture2D Nho(Texture2D t)
    {
        var src = t.GetPixels32(); int W = t.width, H = t.height, w = 640, h = 360; var o = new Color32[w * h];
        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) o[y * w + x] = src[(y * H / h) * W + x * W / w];
        var n = new Texture2D(w, h, TextureFormat.RGBA32, false); n.SetPixels32(o); n.Apply(false); return n;
    }

    static float[] Luoi(Texture2D t)
    {
        var px = t.GetPixels32(); int W = t.width, H = t.height; var o = new float[GX * GY];
        int bx = W / GX, by = H / GY, st = 3;
        for (int gy = 0; gy < GY; gy++) for (int gx = 0; gx < GX; gx++)
        {
            float s = 0; int n = 0;
            for (int y = gy * by; y < (gy + 1) * by; y += st) for (int x = gx * bx; x < (gx + 1) * bx; x += st)
            { var c = px[y * W + x]; s += (0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b) / 255f; n++; }
            o[gy * GX + gx] = s / n;
        }
        return o;
    }
}
#endif
