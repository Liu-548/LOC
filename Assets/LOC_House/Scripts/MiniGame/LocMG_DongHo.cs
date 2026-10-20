using UnityEngine;

// Mini game 1 — CHỈNH ĐỒNG HỒ quả lắc. Vặn kim tới đúng giờ (gioDich:phutDich) rồi bấm Xác nhận.
// Giờ đúng mặc định 2:40 — manh mối: "Kim phút chỉ về số tám. Kim giờ vừa qua số hai."
// LocTuongTac đặt trên đồng hồ yêu cầu canId = chìa lên dây (LocId.ChiaLenDay).
// Kéo LocDongHo của đồng hồ vào ô dongHo: lúc thắng kim 3D được đặt đúng giờ rồi đứng yên.
public class LocMG_DongHo : LocMiniGame
{
    public int gioDich = 2, phutDich = 40;
    public LocDongHo dongHo;
    public string cauXong = "Kim đứng đúng giờ. Có tiếng cạch nhỏ trong thân đồng hồ.";
    [TextArea] public string manhMoi = "Kim phút chỉ về số tám. Kim giờ vừa qua số hai.";
    int gio = 12, phut = 0;

    protected override string TenMacDinh() { return "Đồng hồ quả lắc"; }
    protected override string GoiYMacDinh() { return "Kim đứng im từ lâu. Vặn kim về giờ cần thiết rồi xác nhận."; }

    protected override void VaoGame() { gio = 12; phut = 0; }
    protected override void KhiThang() { if (dongHo) dongHo.DatGio(gioDich, phutDich); }

    static void Kim(Vector2 tam, float dai, float goc, float day)
    {
        var m = GUI.matrix;
        GUIUtility.RotateAroundPivot(goc, tam);
        GUI.DrawTexture(new Rect(tam.x - day / 2f, tam.y - dai, day, dai), Texture2D.whiteTexture);
        GUI.matrix = m;
    }

    protected override void Ve(Rect r)
    {
        var tam = new Vector2(r.x + r.width / 2f, r.y + 120f);
        GUI.color = new Color(0.9f, 0.85f, 0.7f, 1f);
        for (int i = 0; i < 12; i++)
        {
            var m = GUI.matrix; GUIUtility.RotateAroundPivot(i * 30f, tam);
            GUI.DrawTexture(new Rect(tam.x - 2, tam.y - 112, 4, 14), Texture2D.whiteTexture);
            GUI.matrix = m;
        }
        Kim(tam, 70f, (gio % 12) * 30f + phut * 0.5f, 6f);   // kim giờ
        Kim(tam, 100f, phut * 6f, 3f);                        // kim phút
        GUI.color = Color.white;

        float y = r.y + 250f, w = 100f, x0 = r.x + r.width / 2f - 2 * w - 10f;
        if (GUI.Button(new Rect(x0, y, w, 32), "Giờ -1")) gio = (gio + 11) % 12;
        if (GUI.Button(new Rect(x0 + w + 6, y, w, 32), "Giờ +1")) gio = (gio + 1) % 12;
        if (GUI.Button(new Rect(x0 + 2 * (w + 6), y, w, 32), "Phút -5")) phut = (phut + 55) % 60;
        if (GUI.Button(new Rect(x0 + 3 * (w + 6), y, w, 32), "Phút +5")) phut = (phut + 5) % 60;
        if (GUI.Button(new Rect(r.x + r.width / 2f - 70, y + 44, 140, 36), "Xác nhận"))
        {
            if ((gio % 12) == (gioDich % 12) && phut == phutDich) Thang(cauXong);
            else Sai("Chưa đúng giờ." + (soLanSai >= saiThiGoiY && !string.IsNullOrEmpty(manhMoi) ? "\nGợi ý: " + manhMoi : ""));
        }
    }
}
