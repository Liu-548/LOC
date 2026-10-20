using System.Collections;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

// Lớp nền cho mọi mini game cận cảnh (khoá số, chỉnh đồng hồ, ghép ảnh, nối dây, trốn…).
// Mở: khoá người chơi (LocFirstPerson.khoa), báo ma đứng yên (LocMa.tamDung), nhả chuột để bấm IMGUI.
// Đóng: Esc hoặc nút Đóng; thắng thì Thang() → LocTuongTac.HoanThanh() (thưởng, mở khoá…).
// Lớp con viết: VaoGame() (đặt lại trạng thái mỗi lần mở), Ve(Rect) (vẽ + xử lý bấm trong khung giữa),
// tuỳ chọn MoiKhung(dt) (mini game thời gian thực), TenMacDinh()/GoiYMacDinh() (chữ mặc định khi Inspector để trống).
public abstract class LocMiniGame : MonoBehaviour
{
    public static LocMiniGame hienTai;
    public string tieuDe;
    [TextArea] public string goiY;
    public bool choLap;                    // chơi lại được nhiều lần (ráp chén mỗi đêm)
    public bool daXong;
    public int saiThiGoiY = 3;             // sai bấy nhiêu lần thì hiện gợi ý thêm (nếu lớp con có)

    protected string thongBao = "";
    protected int soLanSai;
    LocTuongTacMG chu;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset0() { hienTai = null; }

    protected virtual string TenMacDinh() { return "Mini game"; }
    protected virtual string GoiYMacDinh() { return ""; }
    // false → LocTuongTac không mở mini game và hiện cau thay thế
    public virtual bool DuDieuKien(out string cau) { cau = ""; return true; }
    protected abstract void VaoGame();
    protected abstract void Ve(Rect r);
    protected virtual void MoiKhung(float dt) { }
    protected virtual void KhiThang() { }
    protected virtual void RaKhoi() { }

    public void Mo(LocTuongTacMG t)
    {
        if (hienTai != null || (daXong && !choLap)) return;
        chu = t; hienTai = this; thongBao = ""; soLanSai = 0;
        LocFirstPerson.khoa = true; LocMa.tamDung = true;
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        VaoGame();
    }

    protected void Sai(string cau) { soLanSai++; thongBao = cau; }

    protected void Thang(string cau)
    {
        if (!choLap) daXong = true;
        KhiThang();
        if (!string.IsNullOrEmpty(cau)) LocFirstPerson.PhuDe(cau, 2.6f, false);
        var c = chu;
        Dong();
        if (c) c.HoanThanh();
    }

    public void Dong()
    {
        if (hienTai != this) return;
        RaKhoi();
        hienTai = null; LocFirstPerson.khoa = false; LocMa.tamDung = false;
        StartCoroutine(KhoaChuot());
    }

    // phím thử F8 (LocTuiHud): thắng ngay mini game đang mở
    public static void BoQua() { if (hienTai != null) hienTai.Thang("[thử] bỏ qua mini game"); }

    // đợi 1 khung: Esc của LocFirstPerson cũng nhả chuột trong cùng khung, khoá lại ngay sẽ bị đè
    IEnumerator KhoaChuot() { yield return null; Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked; }

    void Update()
    {
        if (hienTai != this) return;
        var k = Keyboard.current;
        if (k != null && k.escapeKey.wasPressedThisFrame) { Dong(); return; }
        MoiKhung(Time.deltaTime);
    }

    void OnDisable() { if (hienTai == this) { RaKhoi(); hienTai = null; LocFirstPerson.khoa = false; LocMa.tamDung = false; } }

    void OnGUI()
    {
        if (hienTai != this) return;
        var m0 = GUI.matrix;
        float s = Mathf.Clamp(Mathf.Min(Screen.width / 760f, Screen.height / 640f), 0.6f, 1.3f);
        GUIUtility.ScaleAroundPivot(new Vector2(s, s), Vector2.zero);
        float W = Screen.width / s, H = Screen.height / s;
        Hop(new Rect(0, 0, W, H), new Color(0, 0, 0, 0.82f));
        var r = new Rect(W / 2f - 320f, H / 2f - 270f, 640f, 540f);
        Hop(r, new Color(0.12f, 0.09f, 0.08f, 0.97f));
        string td = string.IsNullOrEmpty(tieuDe) ? TenMacDinh() : tieuDe;
        string gy = string.IsNullOrEmpty(goiY) ? GoiYMacDinh() : goiY;
        GUI.Label(new Rect(r.x, r.y + 8, r.width, 34), td, KieuChu(22, TextAnchor.MiddleCenter, FontStyle.Bold, Color.white, false));
        if (!string.IsNullOrEmpty(gy))
            GUI.Label(new Rect(r.x + 20, r.y + 44, r.width - 40, 56), gy, KieuChu(14, TextAnchor.UpperCenter, FontStyle.Italic, new Color(0.78f, 0.78f, 0.78f), true));
        Ve(new Rect(r.x + 20, r.y + 108, 600f, 330f));
        if (!string.IsNullOrEmpty(thongBao))
            GUI.Label(new Rect(r.x + 20, r.yMax - 76, r.width - 40, 30), thongBao, KieuChu(15, TextAnchor.MiddleCenter, FontStyle.Italic, new Color(1f, 0.82f, 0.55f), true));
        if (GUI.Button(new Rect(r.xMax - 120, r.yMax - 42, 100, 28), "Đóng [Esc]")) Dong();
        GUI.matrix = m0;
    }

    // ───── tiện ích vẽ IMGUI dùng chung
    protected static GUIStyle KieuChu(int co, TextAnchor can, FontStyle kieu, Color mau, bool xuongDong)
    {
        var st = new GUIStyle(GUI.skin.label) { fontSize = co, alignment = can, fontStyle = kieu, wordWrap = xuongDong };
        st.normal.textColor = mau;
        return st;
    }

    protected static void Hop(Rect r, Color c)
    {
        var o = GUI.color; GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = o;
    }

    protected static void Khung(Rect r, Color c, float day)
    {
        Hop(new Rect(r.x, r.y, r.width, day), c);
        Hop(new Rect(r.x, r.yMax - day, r.width, day), c);
        Hop(new Rect(r.x, r.y, day, r.height), c);
        Hop(new Rect(r.xMax - day, r.y, day, r.height), c);
    }

    // đoạn thẳng a→b dày day (vẽ bằng ô trắng xoay quanh a)
    protected static void VeDay(Vector2 a, Vector2 b, float day, Color c)
    {
        var d = b - a; float dai = d.magnitude; if (dai < 0.5f) return;
        var m = GUI.matrix; var o = GUI.color;
        GUIUtility.RotateAroundPivot(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, a);
        GUI.color = c;
        GUI.DrawTexture(new Rect(a.x, a.y - day / 2f, dai, day), Texture2D.whiteTexture);
        GUI.color = o; GUI.matrix = m;
    }

    protected static Texture2D TaoTexture(Color32[] px, int n)
    {
        var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp;
        t.SetPixels32(px); t.Apply();
        return t;
    }

    // bỏ dấu tiếng Việt, viết hoa, bỏ khoảng trắng thừa — để so đáp án gõ tay
    public static string BoDau(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var sb = new StringBuilder();
        foreach (char ch in s.Normalize(NormalizationForm.FormD))
        {
            var u = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (u == UnicodeCategory.NonSpacingMark) continue;
            if (ch == 'đ') sb.Append('d'); else if (ch == 'Đ') sb.Append('D'); else sb.Append(ch);
        }
        return string.Join(" ", sb.ToString().ToUpperInvariant().Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries));
    }
}
