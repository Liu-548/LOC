using UnityEngine;
using UnityEngine.InputSystem;

// Mini game 11 — NÍN THỞ TRONG TỦ ÁO (đêm 3). Ma đi qua trước tủ: khi nó tới gần, phải giữ SPACE để nín thở.
// Hơi thở có hạn: hết hơi thì phải thở (gây tiếng). Bị nghe thấy đủ lâu = Death Ending (LocMa.BatChet). Qua hết thời gian là sống.
public class LocMG_NinTho : LocMiniGame
{
    public float thoiGian = 14f;
    public float nguong = 0.85f;
    public string cauXong = "Tiếng chân xa dần. Nhà lại im.";
    float t, hoi, lo, tho;
    bool het;

    protected override string TenMacDinh() { return "Trốn trong tủ áo"; }
    protected override string GoiYMacDinh() { return "Giữ SPACE để nín thở khi nó đến gần. Hết hơi thì phải thở: đừng thở lúc nó ở sát cánh tủ."; }

    protected override void VaoGame() { t = 0f; hoi = 1f; lo = 0f; tho = 0f; het = false; }

    float Gan() { return Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / thoiGian)); }

    protected override void MoiKhung(float dt)
    {
        var k = Keyboard.current; var m = Mouse.current;
        bool nhan = (k != null && k.spaceKey.isPressed) || (m != null && m.leftButton.isPressed);
        if (het && hoi > 0.3f) het = false;
        bool nin = nhan && !het && hoi > 0f;
        if (nin) { hoi -= 0.14f * dt; tho = 0f; if (hoi <= 0f) { hoi = 0f; het = true; } }
        else { hoi = Mathf.Min(1f, hoi + 0.3f * dt); tho += dt; }
        t += dt;
        float g = Gan();
        bool ongThay = !nin && g > nguong;
        lo = ongThay ? lo + dt : Mathf.Max(0f, lo - 0.6f * dt);
        if (lo >= 0.45f)
        {
            Dong();
            LocMa.BatChet();
            return;
        }
        if (t >= thoiGian) Thang(cauXong);
    }

    protected override void Ve(Rect r)
    {
        float g = Gan();
        var khe = new Rect(r.x + 170f, r.y + 6f, 260f, 190f);
        Hop(khe, new Color(0.02f, 0.015f, 0.015f)); Khung(khe, new Color(0.3f, 0.2f, 0.15f), 6f);
        float x = khe.x + khe.width * (0.1f + 0.8f * (t / thoiGian));
        float cao = 40f + 130f * g;
        Hop(new Rect(x - 12f - 20f * g, khe.yMax - cao - 6f, 24f + 40f * g, cao), new Color(0.015f, 0.01f, 0.01f, 0.4f + 0.55f * g));
        if (g > nguong) Hop(new Rect(x - 6f, khe.yMax - cao + 14f, 4f, 3f), new Color(0.8f, 0.1f, 0.08f, 0.9f));
        GUI.Label(new Rect(r.x, r.y + 200f, r.width, 28f), g > nguong ? "Nó đang ở sát cánh tủ." : (g > 0.4f ? "Tiếng chân đến gần…" : "Tiếng chân ở xa."),
            KieuChu(16, TextAnchor.MiddleCenter, FontStyle.Italic, new Color(0.85f, 0.8f, 0.7f), false));
        Thanh(new Rect(r.x + 40f, r.y + 250f, 520f, 14f), hoi, het ? new Color(0.7f, 0.3f, 0.3f) : new Color(0.6f, 0.75f, 0.9f), het ? "Hơi thở (đang thở dốc)" : "Hơi thở");
        Thanh(new Rect(r.x + 40f, r.y + 292f, 520f, 14f), lo / 0.45f, new Color(0.85f, 0.25f, 0.2f), "Bị nghe thấy");
    }

    static void Thanh(Rect o, float v, Color c, string ten)
    {
        Hop(o, new Color(0.05f, 0.04f, 0.04f));
        Hop(new Rect(o.x, o.y, o.width * Mathf.Clamp01(v), o.height), c);
        Khung(o, new Color(0.5f, 0.45f, 0.4f, 0.8f), 1f);
        GUI.Label(new Rect(o.x, o.y - 18f, o.width, 18f), ten, KieuChu(12, TextAnchor.MiddleLeft, FontStyle.Normal, new Color(0.8f, 0.78f, 0.72f), false));
    }
}
