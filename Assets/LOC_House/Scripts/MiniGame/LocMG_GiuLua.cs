using UnityEngine;
using UnityEngine.InputSystem;

// Mini game 6 — GIỮ LỬA đèn dầu. Thời gian thực: gió thổi từng đợt (có báo trước), giữ SPACE (hoặc chuột phải) để che gió.
// Che mãi sẽ mỏi tay (thanh sức tay) nên chỉ che khi gió thật sự tới. Lửa tắt thì làm lại từ đầu. Sống đủ thoiGianCan giây là thắng.
public class LocMG_GiuLua : LocMiniGame
{
    public float thoiGianCan = 20f;
    public string cauXong = "Lửa còn đó. Bóng nhà lay động rồi yên.";

    float lua, tay, tienDo, dem, giaiDoan;   // giaiDoan: 0 yên · 1 báo gió · 2 gió thổi
    bool moi;
    static Texture2D tex;

    protected override string TenMacDinh() { return "Giữ lửa"; }
    protected override string GoiYMacDinh() { return "Giữ SPACE (hoặc chuột phải) để che gió. Đừng che quá lâu, tay sẽ mỏi."; }

    protected override void VaoGame() { Lai(); }

    void Lai() { lua = 1f; tay = 1f; tienDo = 0f; moi = false; giaiDoan = 0; dem = Random.Range(1.2f, 2.4f); }

    protected override void MoiKhung(float dt)
    {
        var k = Keyboard.current; var m = Mouse.current;
        bool nhan = (k != null && k.spaceKey.isPressed) || (m != null && m.rightButton.isPressed);
        if (moi && tay > 0.3f) moi = false;
        bool che = nhan && !moi && tay > 0f;
        if (che) { tay -= 0.35f * dt; if (tay <= 0f) { tay = 0f; moi = true; che = false; } }
        else tay = Mathf.Min(1f, tay + 0.45f * dt);

        dem -= dt;
        if (dem <= 0f)
        {
            if (giaiDoan == 0) { giaiDoan = 1; dem = 0.7f; }
            else if (giaiDoan == 1) { giaiDoan = 2; dem = 0.9f; }
            else { giaiDoan = 0; dem = Random.Range(1.2f, 2.4f); }
        }
        if (giaiDoan == 2) lua -= (che ? 0.04f : 0.75f) * dt;
        else lua = Mathf.Min(1f, lua + 0.3f * dt);

        tienDo += dt;
        if (lua <= 0f) { Lai(); Sai("Lửa tắt rồi. Châm lại từ đầu."); }
        else if (tienDo >= thoiGianCan) Thang(cauXong);
    }

    static Texture2D Lua()
    {
        if (tex) return tex;
        const int n = 32;
        var px = new Color32[n * n];
        for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
        {
            float dx = (x - 15.5f) / 15.5f, dy = (y - 15.5f) / 15.5f;
            float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
            px[y * n + x] = new Color32(255, 255, 255, (byte)(a * a * 255f));
        }
        tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp; tex.SetPixels32(px); tex.Apply();
        return tex;
    }

    void Thanh(Rect o, float v, Color c, string ten)
    {
        Hop(o, new Color(0.05f, 0.04f, 0.04f, 1f));
        Hop(new Rect(o.x, o.y, o.width * Mathf.Clamp01(v), o.height), c);
        Khung(o, new Color(0.5f, 0.45f, 0.4f, 0.8f), 1f);
        GUI.Label(new Rect(o.x, o.y - 18f, o.width, 18f), ten, KieuChu(12, TextAnchor.MiddleLeft, FontStyle.Normal, new Color(0.8f, 0.78f, 0.72f), false));
    }

    protected override void Ve(Rect r)
    {
        var tam = new Vector2(r.x + r.width / 2f, r.y + 110f);
        float nhay = 0.9f + 0.1f * Mathf.PerlinNoise(Time.time * 9f, 0.4f);
        float to = (50f + 90f * Mathf.Clamp01(lua)) * nhay;
        var o = GUI.color;
        GUI.color = new Color(1f, 0.55f, 0.15f, Mathf.Clamp01(lua) * 0.9f);
        GUI.DrawTexture(new Rect(tam.x - to / 2f, tam.y - to / 2f, to, to), Lua());
        GUI.color = new Color(1f, 0.9f, 0.55f, Mathf.Clamp01(lua));
        GUI.DrawTexture(new Rect(tam.x - to / 5f, tam.y - to / 5f, to / 2.5f, to / 2.5f), Lua());
        GUI.color = o;
        Hop(new Rect(tam.x - 22f, tam.y + 42f, 44f, 50f), new Color(0.35f, 0.25f, 0.2f, 1f));   // thân đèn
        if (giaiDoan > 0)
            GUI.Label(new Rect(r.x, r.y + 10f, r.width, 40f), giaiDoan == 1 ? ">  gió sắp tới  >" : ">>>  GIÓ!  >>>",
                KieuChu(26, TextAnchor.MiddleCenter, FontStyle.Bold, new Color(0.75f, 0.85f, 1f, giaiDoan == 1 ? 0.5f : 1f), false));
        Thanh(new Rect(r.x + 40f, r.y + 220f, 520f, 14f), lua, new Color(1f, 0.6f, 0.2f), "Lửa");
        Thanh(new Rect(r.x + 40f, r.y + 262f, 520f, 14f), tay, moi ? new Color(0.7f, 0.3f, 0.3f) : new Color(0.5f, 0.75f, 0.5f), moi ? "Sức tay (mỏi, đợi hồi)" : "Sức tay");
        Thanh(new Rect(r.x + 40f, r.y + 304f, 520f, 14f), tienDo / thoiGianCan, new Color(0.8f, 0.8f, 0.85f), "Đường đi");
    }
}
