using UnityEngine;
using UnityEngine.InputSystem;

// Đi bộ góc nhìn thứ nhất để kiểm tra nhà LỘC. WASD + chuột, Shift chạy, C/Ctrl ngồi, Esc nhả chuột, click để khoá lại.
[RequireComponent(typeof(CharacterController))]
public class LocFirstPerson : MonoBehaviour
{
    public Transform cam;
    public float speed = 2.2f, look = 0.12f;
    CharacterController cc;
    float pitch, vy;
    // [21/10] ngồi xổm (C hoặc Ctrl, bấm bật/tắt): mắt 1,52 → 0,95 m, đi chậm một nửa — để nhìn/bấm đồ dưới thấp (gầm bàn thờ, ngăn tủ dưới)
    public static bool ngoi;
    public float matNgoi = 0.95f;
    float matDung, camY, ccCao, camX; Vector3 ccTam;
    // [22/10] lắc camera theo bước chân (kịch bản D1: "lắc rất nhẹ"). 0 = tắt · 1 = nhẹ · 1,8 = vừa — đặt từ Cài đặt
    public static float lac = 1f;
    float pha, lacW;
    string goiY = "";   // dòng gợi ý "[E] ..." khi đang nhìn cửa/công tắc
    public string GoiYDangHien => goiY;   // tự kiểm đọc
    // [21/10] tia từ mắt, bỏ qua va chạm của chính người chơi (mắt 1,52 nằm trong đầu khối CharacterController nên tia nhìn xuống trúng chính mình)
    static readonly RaycastHit[] tiaHB = new RaycastHit[16];
    public bool Tia(Vector3 huong, float xa, out RaycastHit hit)
    {
        hit = default; float gan = float.MaxValue; bool co = false;
        int n = Physics.RaycastNonAlloc(cam.position, huong, tiaHB, xa, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < n; i++)
            if (tiaHB[i].collider != cc && tiaHB[i].distance < gan) { gan = tiaHB[i].distance; hit = tiaHB[i]; co = true; }
        return co;
    }

    // [3/10] cảnh khoá + phụ đề + màn đen (LocMa dùng)
    public static bool khoa;            // true: không nhìn/đi/tương tác
    public static float den;            // 0..1: phủ đen toàn màn hình
    public static string chuGiua;       // chữ trắng giữa màn hình (Death Ending)
    static string phuDe; static bool phuDeNghieng; static float phuDeHet;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reset0() { khoa = false; den = 0; chuGiua = null; phuDe = null; ngoi = false; }
    // nghieng = độc thoại nội tâm (chữ nghiêng, xám nhạt, không tên) — kịch bản 6.2
    public static void PhuDe(string s, float giay, bool nghieng = true) { phuDe = s; phuDeNghieng = nghieng; phuDeHet = Time.time + giay * nhanChu; }
    // [22/10] cài đặt (LocMenu): đảo trục dọc · cỡ chữ phụ đề (ở 1080p) · độ đậm nền sau chữ · hệ số thời gian hiện chữ
    public static bool daoTruc; public static int coChu = 24; public static float nenChu = 0.45f, nhanChu = 1f;

    // dịch chuyển tức thời + quay nhìn về điểm nhin
    public void DatTuThe(Vector3 pos, Vector3 nhin)
    {
        if (!cc) cc = GetComponent<CharacterController>();
        cc.enabled = false; transform.position = pos; cc.enabled = true;
        var d = nhin - (pos + Vector3.up * (cam ? cam.localPosition.y : 1.52f));
        var dp = new Vector3(d.x, 0, d.z);
        if (dp.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(dp, Vector3.up);
        pitch = Mathf.Clamp(-Mathf.Atan2(d.y, dp.magnitude) * Mathf.Rad2Deg, -85, 85);
        if (cam) cam.localEulerAngles = new Vector3(pitch, 0, 0);
        vy = 0;
    }

    void Start()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        matDung = camY = cam ? cam.localPosition.y : 1.52f; ccCao = cc.height; ccTam = cc.center; camX = cam ? cam.localPosition.x : 0;
    }

    void Update()
    {
        var k = Keyboard.current; var m = Mouse.current;
        if (k == null || m == null) return;
        if (k.escapeKey.wasPressedThisFrame && !LocTui.Mo) Cursor.lockState = CursorLockMode.None;
        if (m.leftButton.wasPressedThisFrame && !khoa) Cursor.lockState = CursorLockMode.Locked;   // mini game cần chuột tự do
        goiY = "";
        if (khoa || LocTui.Mo || LocXeBuyt.dangChay) return;   // [25/10] trên xe buýt mở đầu: LocXeBuyt tự lo nhìn

        if (k.cKey.wasPressedThisFrame || k.leftCtrlKey.wasPressedThisFrame)
        {
            if (!ngoi) ngoi = true;
            else if (!Tia(Vector3.up, matDung - camY + 0.12f, out _)) ngoi = false;   // trên đầu vướng thì chưa đứng được
        }
        float cao = ngoi ? matNgoi + 0.1f : ccCao;
        cc.height = cao; cc.center = new Vector3(ccTam.x, ccTam.y - (ccCao - cao) / 2f, ccTam.z);   // giữ nguyên đáy
        camY = Mathf.MoveTowards(camY, ngoi ? matNgoi : matDung, Time.deltaTime * 2.6f);
        cam.localPosition = new Vector3(cam.localPosition.x, camY, cam.localPosition.z);

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            var d = m.delta.ReadValue() * look;
            transform.Rotate(0, d.x, 0);
            pitch = Mathf.Clamp(pitch - (daoTruc ? -d.y : d.y), -85, 85);
            cam.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        // tương tác: nhìn vào cánh cửa / công tắc / hũ rồi nhấn E
        if (cam && Tia(cam.forward, 2.6f, out var hit))
        {
            var d = hit.collider.GetComponentInParent<LocDoor>();
            var tt = hit.collider.GetComponentInParent<LocTuongTac>();   // [6/10] món đồ, két sắt…
            if (tt && tt.GoiY.Length == 0) tt = null;
            if (d && tt) { if (tt.transform.IsChildOf(d.transform)) d = null; else tt = null; }   // món nằm trong ngăn kéo/cánh tủ → món; cánh két nằm trong két → cánh
            var c = d || tt ? null : hit.collider.GetComponentInParent<LocCongTac>();
            var h = d || tt || c ? null : hit.collider.GetComponentInParent<LocHuCot>();
            var nc = d || tt || c || h ? null : hit.collider.GetComponentInParent<LocNhimChau>();   // [5/10] chậu nước: [E] Thay khăn
            if (tt) goiY = tt.GoiY;
            if (h) goiY = h.GoiY;
            if (nc) goiY = nc.GoiY;
            if (d) goiY = d.khoa && d.khiKhoa ? d.khiKhoa.GoiY : "[E] " + (d.DangMo ? "Đóng " : "Mở ") + d.ten;
            else if (c) goiY = "[E] " + (c.bat ? "Tắt" : "Bật") + " " + c.ten;
            if (k.eKey.wasPressedThisFrame) { if (d) d.Bam(); else if (c) c.Toggle(); else if (h) h.Bam(); else if (nc && nc.GoiY.Length > 0) nc.Bam(); else if (tt) tt.Bam(); }
        }

        float f = (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0);
        float r = (k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0);
        var mv = (transform.forward * f + transform.right * r).normalized * speed * (ngoi ? 0.5f : k.leftShiftKey.isPressed ? 1.8f : 1f);
        vy = cc.isGrounded ? -1f : vy + Physics.gravity.y * Time.deltaTime;
        cc.Move((mv + Vector3.up * vy) * Time.deltaTime);

        // lắc: pha chạy theo quãng đường thật đi được (đâm tường thì không lắc), ~1,9 bước/giây khi đi thường
        float v = new Vector2(cc.velocity.x, cc.velocity.z).magnitude;
        lacW = Mathf.MoveTowards(lacW, cc.isGrounded && v > 0.3f ? 1 : 0, Time.deltaTime * 4f);
        pha += v * Time.deltaTime * 2.7f;
        float a = lac * lacW * (ngoi ? 0.5f : 1f);
        cam.localPosition = new Vector3(camX + Mathf.Sin(pha) * 0.006f * a, camY + Mathf.Abs(Mathf.Sin(pha)) * 0.014f * a - 0.007f * a, cam.localPosition.z);
    }

    // [3/10] lớp VHS: vạch quét ngang mờ + một dải sáng trôi chậm (đi cùng hậu kỳ LOC_HinhCu: hạt phim, rút màu, nhoè xa, méo rìa)
    public static float vhs = 1f;
    static Texture2D scan;
    static void VHS()
    {
        if (vhs <= 0) return;
        if (!scan)
        {
            scan = new Texture2D(1, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
            scan.SetPixels(new[] { new Color(0, 0, 0, 0.16f), new Color(0, 0, 0, 0.05f), Color.clear, Color.clear }); scan.Apply();
        }
        float W = Screen.width, H = Screen.height;
        GUI.color = new Color(1, 1, 1, vhs);
        GUI.DrawTextureWithTexCoords(new Rect(0, 0, W, H), scan, new Rect(0, 0, 1, H / 4f));
        float y = (Time.time * 38f) % (H + 160f) - 80f;
        GUI.color = new Color(1, 1, 1, 0.03f * vhs); GUI.DrawTexture(new Rect(0, y, W, 70), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    void OnGUI()
    {
        if (Event.current.type == EventType.Repaint) VHS();
        if (den > 0) { GUI.color = new Color(0, 0, 0, den); GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white; }
        float k = Screen.height / 1080f;   // [22/10] chữ co theo màn hình, font Noto Serif của menu
        if (!string.IsNullOrEmpty(chuGiua))
        {
            var fd = LocMenu.FontChu(2);
            var sg = new GUIStyle(GUI.skin.label) { font = fd, fontSize = Mathf.RoundToInt(44 * k), alignment = TextAnchor.MiddleCenter, fontStyle = fd ? FontStyle.Normal : FontStyle.Bold };
            sg.normal.textColor = Color.white;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), chuGiua, sg);
        }
        if (phuDe != null && Time.time < phuDeHet)
        {
            var fc = LocMenu.FontChu(phuDeNghieng ? 1 : 0);
            var sp = new GUIStyle(GUI.skin.label) { font = fc, fontSize = Mathf.RoundToInt(coChu * k), alignment = TextAnchor.MiddleCenter, fontStyle = phuDeNghieng && !fc ? FontStyle.Italic : FontStyle.Normal };
            sp.normal.textColor = phuDeNghieng ? new Color(0.78f, 0.78f, 0.78f) : Color.white;
            var nd = new GUIContent(phuDe); var sz = sp.CalcSize(nd); float cy = Screen.height * 0.86f + 22 * k;
            if (nenChu > 0 && phuDe.Length > 0) { GUI.color = new Color(0, 0, 0, nenChu); GUI.DrawTexture(new Rect((Screen.width - sz.x) / 2 - 14 * k, cy - sz.y / 2 - 4 * k, sz.x + 28 * k, sz.y + 8 * k), Texture2D.whiteTexture); GUI.color = Color.white; }
            GUI.Label(new Rect(0, cy - sz.y / 2, Screen.width, sz.y), nd, sp);
        }
        if (string.IsNullOrEmpty(goiY)) return;
        var st = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
        st.normal.textColor = Color.white;
        GUI.Label(new Rect(0, Screen.height * 0.58f, Screen.width, 40), goiY, st);
    }
}
