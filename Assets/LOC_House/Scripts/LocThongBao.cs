using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// [02/11] Thông báo QUA NGÀY MỚI — hiện sau khi Khôi bật dậy từ giấc mơ cuối Đêm 2, trước cảnh đưa tang.
//  Nền đen, chữ giữa màn hình theo kiểu thẻ đêm của LocMenu (tiêu đề to · dòng nghiêng · ngày tháng nhỏ).
//  Click chuột (hoặc Enter / Space / E) để tắt, không bấm thì tự tắt sau ~3 s. Esc tạm dừng thì đứng chờ.
public class LocThongBao : MonoBehaviour
{
    static LocThongBao I;
    public static int soLanHien;                 // tự kiểm đọc
    public static bool DangHien => I && I.dang;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset0() { I = null; soLanHien = 0; }

    string tieuDe, dong, ngay; float a, chay, giay; bool dang;
    static readonly Color NGA = new(232 / 255f, 226 / 255f, 210 / 255f), MO = new(140 / 255f, 133 / 255f, 122 / 255f), NHAT = new(100 / 255f, 95 / 255f, 88 / 255f);

    public static IEnumerator NgayMoi(string tieuDe, string dong, string ngay, float giay = 3f)
    {
        if (!I) I = new GameObject("LOC_ThongBao").AddComponent<LocThongBao>();
        var b = I; b.tieuDe = tieuDe; b.dong = dong; b.ngay = ngay; b.giay = giay; b.a = 0; b.chay = 0; b.dang = true; soLanHien++;
        LocFirstPerson.PhuDe("", 0.01f);
        for (float t = 0; t < 1; t += Time.deltaTime / 0.6f) { b.a = t; yield return null; }   // hiện dần
        b.a = 1; yield return null;
        while (b.chay < giay)
        {
            if (!LocMenu.DangChe) { if (Bam()) break; b.chay += Time.deltaTime; }
            yield return null;
        }
        for (float t = 1; t > 0; t -= Time.deltaTime / 0.45f) { b.a = t; yield return null; }  // tắt dần (nền đen vẫn giữ — màn LocFirstPerson.den đang đen)
        b.a = 0; b.dang = false;
    }
    static bool Bam()
    {
        if (LocFirstPerson.BamTiep()) return true;
        var k = Keyboard.current;
        return k != null && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame || k.eKey.wasPressedThisFrame);
    }

    void OnGUI()
    {
        if (!dang || Event.current.type != EventType.Repaint) return;
        GUI.depth = -50;   // trên lớp đen của LocFirstPerson, dưới menu (−100)
        float W = Screen.width, H = Screen.height, k = H / 1080f;
        GUI.color = Color.black; GUI.DrawTexture(new Rect(0, 0, W, H), Texture2D.whiteTexture); GUI.color = Color.white;
        Chu(H * 0.43f, tieuDe, 64, 2, NGA, a);
        Chu(H * 0.52f, dong, 28, 1, MO, a);
        Chu(H * 0.575f, ngay, 22, 0, NHAT, a);
        // góc phải dưới: "Click để tiếp tục" + vạch đếm ngược tự tắt
        var st = new GUIStyle(GUI.skin.label) { font = LocMenu.FontChu(1), fontSize = Mathf.RoundToInt(19 * k), alignment = TextAnchor.LowerRight };
        st.normal.textColor = new Color(MO.r, MO.g, MO.b, 0.75f * a);
        GUI.Label(new Rect(0, 0, W - 36 * k, H - 30 * k), "Click để tiếp tục", st);
        float w = 150 * k * (1 - Mathf.Clamp01(chay / Mathf.Max(0.1f, giay)));
        GUI.color = new Color(MO.r, MO.g, MO.b, 0.5f * a); GUI.DrawTexture(new Rect(W - 36 * k - w, H - 26 * k, w, 2 * k), Texture2D.whiteTexture); GUI.color = Color.white;
    }
    static void Chu(float y, string s, int co, int kieu, Color mau, float a)
    {
        if (string.IsNullOrEmpty(s)) return;
        float k = Screen.height / 1080f;
        var st = new GUIStyle(GUI.skin.label) { font = LocMenu.FontChu(kieu), fontSize = Mathf.RoundToInt(co * k), alignment = TextAnchor.MiddleCenter };
        st.normal.textColor = new Color(mau.r, mau.g, mau.b, a);
        GUI.Label(new Rect(0, y - co * k, Screen.width, co * 2 * k), s, st);
    }
}
