using UnityEngine;
using UnityEngine.InputSystem;

// [6/10] Két sắt đứng phòng bố mẹ — câu đố duy nhất của game (kịch bản v3.1 mục 2, Đêm 1). Mã 1980 = năm sinh Phong,
// giấu trong sổ thu chi 2002 thành dòng "Ứng cho thằng Phong — 1.980.000". Gõ số trên bàn phím (hàng số hoặc numpad).
public class LocKetSat : LocTuongTac
{
    public string ma = "1980";
    public LocDoor canh;
    string nhap = ""; bool dangNhap, daNoi; float sai;

    void Start() { if (LocTui.Co("ket_mo") && canh) canh.khoa = false; }

    public override string GoiY => canh && canh.khoa ? "[E] Nhập mã két" : "";
    public override void Bam()
    {
        if (!canh || !canh.khoa) return;
        if (!daNoi) { daNoi = true; LocTui.Noi("Khóa số.", "Mã gì đây."); }
        LocTui.Dat("thu_ket");   // [8/10] mở gợi ý H mã két
        nhap = ""; dangNhap = true; LocFirstPerson.khoa = true; LocMa.tamDung = true;
    }

    static readonly Key[] so = { Key.Digit0, Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9 };
    static readonly Key[] soNp = { Key.Numpad0, Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4, Key.Numpad5, Key.Numpad6, Key.Numpad7, Key.Numpad8, Key.Numpad9 };

    void Dong() { dangNhap = false; LocFirstPerson.khoa = false; LocMa.tamDung = false; }

    void Update()
    {
        if (!dangNhap) return;
        var k = Keyboard.current; if (k == null) return;
        if (k.escapeKey.wasPressedThisFrame) { Dong(); return; }
        if (sai > 0) { sai -= Time.deltaTime; if (sai <= 0) nhap = ""; return; }
        for (int i = 0; i < 10; i++) if ((k[so[i]].wasPressedThisFrame || k[soNp[i]].wasPressedThisFrame) && nhap.Length < 4) nhap += i;
        if (k.backspaceKey.wasPressedThisFrame && nhap.Length > 0) nhap = nhap[..^1];
        Kiem();
    }
    public void GoMa(string s) { nhap = s; Kiem(); }   // tự kiểm
    void Kiem()
    {
        if (nhap.Length < 4) return;
        if (nhap == ma) { Dong(); canh.khoa = false; canh.Mo(); LocTui.Dat("ket_mo"); }
        else sai = 0.9f;
    }

    void OnGUI()
    {
        if (!dangNhap) return;
        float sk = Screen.height / 1080f; GUI.matrix = Matrix4x4.Scale(new Vector3(sk, sk, 1));
        float W = Screen.width / sk, H = 1080f, w = 260, h = 340, x = (W - w) / 2, y = (H - h) / 2;
        GUI.color = new Color(0.12f, 0.13f, 0.13f, 0.96f); GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
        GUI.color = new Color(0.03f, 0.05f, 0.03f, 1f); GUI.DrawTexture(new Rect(x + 30, y + 24, w - 60, 54), Texture2D.whiteTexture); GUI.color = Color.white;
        var led = new GUIStyle(GUI.skin.label) { fontSize = 34, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        led.normal.textColor = sai > 0 ? new Color(0.85f, 0.25f, 0.2f) : new Color(0.44f, 0.66f, 0.42f);
        string hien = ""; for (int i = 0; i < 4; i++) hien += (i < nhap.Length ? nhap[i] : '-') + (i < 3 ? " " : "");
        GUI.Label(new Rect(x + 30, y + 24, w - 60, 54), sai > 0 ? "S A I" : hien, led);
        var nut = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter }; nut.normal.textColor = new Color(0.86f, 0.86f, 0.83f);
        string[] phim = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "*", "0", "#" };
        for (int i = 0; i < 12; i++)
        {
            var r = new Rect(x + 40 + (i % 3) * 62, y + 100 + (i / 3) * 52, 52, 42);
            GUI.color = new Color(0.25f, 0.27f, 0.27f); GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = Color.white;
            GUI.Label(r, phim[i], nut);
        }
        var g = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter }; g.normal.textColor = new Color(0.7f, 0.7f, 0.68f);
        GUI.Label(new Rect(0, y + h + 10, W, 26), "Gõ 4 số    [Backspace] xoá    [Esc] thôi", g);
        GUI.matrix = Matrix4x4.identity;
    }
}
