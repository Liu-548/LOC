using UnityEngine;

// Mini game 9 — LÊN DÂY ĐÀN. Cần DayDan. Kéo thanh (và nút tinh chỉnh) để tiếng dây khớp nốt chuẩn.
// Âm thanh sinh bằng code (AudioClip stream), không cần file. Không có loa thì vẫn chơi được nhờ thanh lệch.
public class LocMG_LenDay : LocMiniGame
{
    public float nhacChuan = 164.8f;           // Hz (E3)
    public float dungSai = 1.2f;
    public float tuHz = 120f, denHz = 220f;
    public string cauXong = "Dây đã lên. Tiếng đàn rơi xuống sàn rất nhẹ.";
    float hz, phatHz, hetPhat; float pha;
    volatile bool dangPhat;
    float dungHz;
    AudioSource am;

    protected override string TenMacDinh() { return "Lên dây đàn"; }
    protected override string GoiYMacDinh() { return "Bấm 'Nghe nốt chuẩn', rồi chỉnh dây cho tiếng khớp. Nghe không rõ thì xem thanh lệch."; }

    public override bool DuDieuKien(out string cau)
    {
        cau = "";
        if (LocTui.Co(LocId.DayDan)) return true;
        cau = "Đàn thiếu một dây. Phải tìm dây thay.";
        return false;
    }

    protected override void VaoGame()
    {
        hz = nhacChuan + (Random.value < 0.5f ? -1f : 1f) * Random.Range(25f, 40f);
        hz = Mathf.Clamp(hz, tuHz, denHz);
        dangPhat = false; hetPhat = 0f; pha = 0f;
        if (!am)
        {
            am = gameObject.GetComponent<AudioSource>();
            if (!am) am = gameObject.AddComponent<AudioSource>();
            am.playOnAwake = false; am.spatialBlend = 0f; am.loop = true;
            am.clip = AudioClip.Create("LocDay", 44100, 1, 44100, true, Doc);
        }
    }

    void Doc(float[] data)
    {
        float f = dangPhat ? dungHz : 0f;
        for (int i = 0; i < data.Length; i++)
        {
            if (f > 0f) { pha += f / 44100f; if (pha > 1f) pha -= 1f; data[i] = Mathf.Sin(pha * 6.2831853f) * 0.25f; }
            else data[i] = 0f;
        }
    }

    void Phat(float f)
    {
        if (!am) return;
        dungHz = f; dangPhat = true; hetPhat = Time.time + 1.3f;
        if (!am.isPlaying) am.Play();
    }

    protected override void MoiKhung(float dt) { if (dangPhat && Time.time > hetPhat) { dangPhat = false; if (am) am.Stop(); } }
    protected override void RaKhoi() { dangPhat = false; if (am) am.Stop(); }

    protected override void Ve(Rect r)
    {
        float lech = hz - nhacChuan;
        var bar = new Rect(r.x + 30f, r.y + 60f, 540f, 20f);
        Hop(bar, new Color(0.05f, 0.04f, 0.04f)); Khung(bar, new Color(0.5f, 0.45f, 0.4f), 1f);
        float x = bar.x + bar.width * Mathf.InverseLerp(tuHz, denHz, hz);
        Hop(new Rect(x - 4f, bar.y - 6f, 8f, 32f), Mathf.Abs(lech) <= dungSai ? new Color(0.5f, 0.9f, 0.5f) : new Color(0.9f, 0.8f, 0.5f));
        GUI.Label(new Rect(r.x, r.y + 90f, r.width, 24f), Mathf.Abs(lech) <= dungSai ? "Khớp" : (lech > 0 ? "Dây căng quá" : "Dây chùng quá"),
            KieuChu(15, TextAnchor.MiddleCenter, FontStyle.Italic, new Color(0.8f, 0.78f, 0.72f), false));

        hz = GUI.HorizontalSlider(new Rect(r.x + 30f, r.y + 130f, 540f, 20f), hz, tuHz, denHz);
        float w = 80f, x0 = r.x + 300f - 2f * w - 10f;
        if (GUI.Button(new Rect(x0, r.y + 170f, w, 28f), "-5")) hz = Mathf.Max(tuHz, hz - 5f);
        if (GUI.Button(new Rect(x0 + w + 6f, r.y + 170f, w, 28f), "-1")) hz = Mathf.Max(tuHz, hz - 1f);
        if (GUI.Button(new Rect(x0 + 2f * (w + 6f), r.y + 170f, w, 28f), "+1")) hz = Mathf.Min(denHz, hz + 1f);
        if (GUI.Button(new Rect(x0 + 3f * (w + 6f), r.y + 170f, w, 28f), "+5")) hz = Mathf.Min(denHz, hz + 5f);
        if (GUI.Button(new Rect(r.x + 100f, r.y + 220f, 190f, 32f), "Nghe nốt chuẩn")) Phat(nhacChuan);
        if (GUI.Button(new Rect(r.x + 310f, r.y + 220f, 190f, 32f), "Gảy dây")) Phat(hz);
        if (GUI.Button(new Rect(r.x + 230f, r.y + 270f, 140f, 34f), "Xác nhận"))
        {
            if (Mathf.Abs(lech) <= dungSai) Thang(cauXong);
            else Sai(lech > 0 ? "Còn căng quá." : "Còn chùng quá.");
        }
    }
}
