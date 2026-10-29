using UnityEngine;

// Mini game 10 — CHỈNH SÓNG RADIO. Cần BangThuAm (cuộn băng cassette). Vặn tần số, nhặt 3 đoạn âm thanh rõ ra khi dò đúng, bấm Ghi lại.
// Ghi đủ 3 đoạn thì thắng. Nội dung đoạn (giả định) sửa trong Inspector.
public class LocMG_Radio : LocMiniGame
{
    public float[] tanSo = { 90.4f, 97.1f, 104.6f };
    public string[] doan =
    {
        "…đêm nay không ai được ngủ…",
        "…chén thứ tư đã vỡ từ lâu…",
        "…đừng đếm lại số người trong nhà…"
    };
    public float cua = 0.35f;
    public string cauXong = "Cuộn băng quay. Giọng nói méo mó lặp lại ba câu.";
    float f;
    bool[] ghi;
    static readonly float[] nhieuMau = new float[48];

    protected override string TenMacDinh() { return "Radio cassette"; }
    protected override string GoiYMacDinh() { return "Vặn dò tần số. Khi tiếng nhiễu tan, ấn Ghi lại. Cần ghi đủ ba đoạn."; }

    public override bool DuDieuKien(out string cau)
    {
        cau = "";
        if (LocTui.CoMon(LocId.BangThuAm)) return true;
        cau = "Radio trống băng.";
        return false;
    }

    protected override void VaoGame() { f = 88f; ghi = new bool[tanSo.Length]; }

    int GanNhat(out float xa)
    {
        int b = -1; xa = 999f;
        for (int i = 0; i < tanSo.Length; i++) { float d = Mathf.Abs(f - tanSo[i]); if (d < xa) { xa = d; b = i; } }
        return b;
    }

    protected override void Ve(Rect r)
    {
        float xa; int g = GanNhat(out xa);
        float ro = Mathf.Clamp01(1f - xa / (cua * 3f));      // 0 = nhiễu hoàn toàn
        var band = new Rect(r.x + 30f, r.y + 10f, 540f, 90f);
        Hop(band, new Color(0.04f, 0.05f, 0.04f)); Khung(band, new Color(0.45f, 0.4f, 0.3f), 2f);
        for (int i = 0; i < 48; i++)
        {
            float h = Mathf.PerlinNoise(i * 0.7f, Time.time * 14f) * (1f - ro) * 80f + (ro > 0.2f ? 6f + 20f * ro * Mathf.Abs(Mathf.Sin(i * 0.5f + Time.time * 6f)) : 2f);
            Hop(new Rect(band.x + 6f + i * 11f, band.center.y - h / 2f, 7f, h), new Color(0.5f, 0.9f, 0.55f, 0.4f + 0.5f * ro));
        }
        f = GUI.HorizontalSlider(new Rect(r.x + 30f, r.y + 120f, 540f, 20f), f, 88f, 108f);
        GUI.Label(new Rect(r.x, r.y + 142f, r.width, 28f), f.ToString("0.0") + " MHz", KieuChu(20, TextAnchor.MiddleCenter, FontStyle.Bold, Color.white, false));
        if (GUI.Button(new Rect(r.x + 150f, r.y + 176f, 90f, 26f), "-0,1")) f = Mathf.Max(88f, Mathf.Round((f - 0.1f) * 10f) / 10f);
        if (GUI.Button(new Rect(r.x + 360f, r.y + 176f, 90f, 26f), "+0,1")) f = Mathf.Min(108f, Mathf.Round((f + 0.1f) * 10f) / 10f);

        string loi = ro > 0.6f && g >= 0 ? doan[g] : (ro > 0.25f ? "…r…rrrt… …ờ…" : "krrrrrzzz…");
        GUI.Label(new Rect(r.x + 30f, r.y + 212f, 540f, 40f), loi, KieuChu(17, TextAnchor.MiddleCenter, FontStyle.Italic, new Color(0.85f, 0.85f, 0.75f), true));
        if (GUI.Button(new Rect(r.x + 230f, r.y + 258f, 140f, 30f), "Ghi lại") )
        {
            if (ro > 0.6f && g >= 0)
            {
                if (!ghi[g]) { ghi[g] = true; thongBao = "Đã ghi."; }
                else thongBao = "Đoạn này ghi rồi.";
            }
            else Sai("Chỉ ghi được nhiễu.");
        }
        if (soLanSai >= saiThiGoiY)
        {
            string gy = "Gợi ý: sóng yếu quanh";
            for (int i = 0; i < tanSo.Length; i++) gy += " " + Mathf.Round(tanSo[i]);
            gy += " MHz";
            GUI.Label(new Rect(r.x, r.y + 316f, r.width, 20f), gy, KieuChu(12, TextAnchor.MiddleCenter, FontStyle.Italic, new Color(1f, 0.82f, 0.55f), false));
        }
        int n = 0; for (int i = 0; i < ghi.Length; i++) if (ghi[i]) n++;
        GUI.Label(new Rect(r.x, r.y + 296f, r.width, 24f), "Đã ghi " + n + "/" + ghi.Length, KieuChu(14, TextAnchor.MiddleCenter, FontStyle.Normal, new Color(0.8f, 0.78f, 0.72f), false));
        if (n == ghi.Length) Thang(cauXong);
    }
}
