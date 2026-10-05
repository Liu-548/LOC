using UnityEngine;
using UnityEngine.InputSystem;

// Đi bộ góc nhìn thứ nhất để kiểm tra nhà LỘC. WASD + chuột, Shift chạy, Esc nhả chuột, click để khoá lại.
[RequireComponent(typeof(CharacterController))]
public class LocFirstPerson : MonoBehaviour
{
    public Transform cam;
    public float speed = 2.2f, look = 0.12f;
    CharacterController cc;
    float pitch, vy;
    string goiY = "";   // dòng gợi ý "[E] ..." khi đang nhìn cửa/công tắc

    // [3/10] cảnh khoá + phụ đề + màn đen (LocMa dùng)
    public static bool khoa;            // true: không nhìn/đi/tương tác
    public static float den;            // 0..1: phủ đen toàn màn hình
    public static string chuGiua;       // chữ trắng giữa màn hình (Death Ending)
    static string phuDe; static bool phuDeNghieng; static float phuDeHet;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset0() { khoa = false; den = 0; chuGiua = null; phuDe = null; }
    // nghieng = độc thoại nội tâm (chữ nghiêng, xám nhạt, không tên) — kịch bản 6.2
    public static void PhuDe(string s, float giay, bool nghieng = true) { phuDe = s; phuDeNghieng = nghieng; phuDeHet = Time.time + giay; }

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
    }

    void Update()
    {
        var k = Keyboard.current; var m = Mouse.current;
        if (k == null || m == null) return;
        if (k.escapeKey.wasPressedThisFrame) Cursor.lockState = CursorLockMode.None;
        if (m.leftButton.wasPressedThisFrame) Cursor.lockState = CursorLockMode.Locked;
        goiY = "";
        if (khoa) return;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            var d = m.delta.ReadValue() * look;
            transform.Rotate(0, d.x, 0);
            pitch = Mathf.Clamp(pitch - d.y, -85, 85);
            cam.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        // tương tác: nhìn vào cánh cửa / công tắc / hũ rồi nhấn E
        if (cam && Physics.Raycast(cam.position, cam.forward, out var hit, 2.6f, ~0, QueryTriggerInteraction.Ignore))
        {
            var d = hit.collider.GetComponentInParent<LocDoor>();
            var c = d ? null : hit.collider.GetComponentInParent<LocCongTac>();
            var h = d || c ? null : hit.collider.GetComponentInParent<LocHuCot>();
            if (h) goiY = h.GoiY;
            if (d) goiY = "[E] " + (d.DangMo ? "Đóng " : "Mở ") + d.ten;
            else if (c) goiY = "[E] " + (c.bat ? "Tắt" : "Bật") + " " + c.ten;
            if (k.eKey.wasPressedThisFrame) { if (d) d.Toggle(); else if (c) c.Toggle(); else if (h) h.Bam(); }
        }

        float f = (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0);
        float r = (k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0);
        var mv = (transform.forward * f + transform.right * r).normalized * speed * (k.leftShiftKey.isPressed ? 1.8f : 1f);
        vy = cc.isGrounded ? -1f : vy + Physics.gravity.y * Time.deltaTime;
        cc.Move((mv + Vector3.up * vy) * Time.deltaTime);
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
        if (!string.IsNullOrEmpty(chuGiua))
        {
            var sg = new GUIStyle(GUI.skin.label) { fontSize = 30, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            sg.normal.textColor = Color.white;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), chuGiua, sg);
        }
        if (phuDe != null && Time.time < phuDeHet)
        {
            var sp = new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleCenter, fontStyle = phuDeNghieng ? FontStyle.Italic : FontStyle.Normal };
            sp.normal.textColor = phuDeNghieng ? new Color(0.78f, 0.78f, 0.78f) : Color.white;
            GUI.Label(new Rect(0, Screen.height * 0.86f, Screen.width, 44), phuDe, sp);
        }
        if (string.IsNullOrEmpty(goiY)) return;
        var st = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
        st.normal.textColor = Color.white;
        GUI.Label(new Rect(0, Screen.height * 0.58f, Screen.width, 40), goiY, st);
    }
}
