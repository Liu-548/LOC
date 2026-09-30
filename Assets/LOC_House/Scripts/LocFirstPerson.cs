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

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            var d = m.delta.ReadValue() * look;
            transform.Rotate(0, d.x, 0);
            pitch = Mathf.Clamp(pitch - d.y, -85, 85);
            cam.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        // tương tác: nhìn vào cánh cửa / công tắc rồi nhấn E
        goiY = "";
        if (cam && Physics.Raycast(cam.position, cam.forward, out var hit, 2.6f, ~0, QueryTriggerInteraction.Ignore))
        {
            var d = hit.collider.GetComponentInParent<LocDoor>();
            var c = d ? null : hit.collider.GetComponentInParent<LocCongTac>();
            if (d) goiY = "[E] " + (d.DangMo ? "Đóng " : "Mở ") + d.ten;
            else if (c) goiY = "[E] " + (c.bat ? "Tắt" : "Bật") + " " + c.ten;
            if (k.eKey.wasPressedThisFrame) { if (d) d.Toggle(); else if (c) c.Toggle(); }
        }

        float f = (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0);
        float r = (k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0);
        var mv = (transform.forward * f + transform.right * r).normalized * speed * (k.leftShiftKey.isPressed ? 1.8f : 1f);
        vy = cc.isGrounded ? -1f : vy + Physics.gravity.y * Time.deltaTime;
        cc.Move((mv + Vector3.up * vy) * Time.deltaTime);
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(goiY)) return;
        var st = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
        st.normal.textColor = Color.white;
        GUI.Label(new Rect(0, Screen.height * 0.58f, Screen.width, 40), goiY, st);
    }
}
