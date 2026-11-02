#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

// [02/11] TỰ KIỂM GIẤC MƠ (menu LOC → Tự kiểm giấc mơ (Play)): công cụ kiểm thử, KHÔNG phải gameplay.
//  Máy tự chơi giấc mơ cuối Đêm 2 bốn lần → LOC_KiemTra/GiacMo/ (log.txt + ảnh):
//   1. NÉ      — giữ W, né mọi dấu "!"      → phải THOÁT, 80–130 s, không trúng, có lúc ném đôi, luôn chừa một làn
//   2. KHÔNG NÉ — giữ W, đứng một làn        → phải BỊ BẮT vì trúng 3 lần, không hiện "CHẠY ĐI!"
//   3. ĐỨNG YÊN — không bấm gì               → phải BỊ BẮT vì ma áp sát, "CHẠY ĐI!" hiện trước khi bị bắt, chỉ khi ma đã gần
//   4. NỐI MẠCH — Đêm 2, cờ nua_tinh_2 → giường có "[E] Đi ngủ" → mơ (né) → tỉnh dậy → sang Đêm 3, cờ mo_2_xong + mo2_thoat,
//                trả điều khiển, môi trường (sương, đèn trời) trả lại như trước
//  Cả bốn lần đều kiểm: hành lang dựng được, ma không lộ mắt, dọn sạch sau khi mơ.
public class LocTuKiemGiacMo : MonoBehaviour
{
    const string KEY = "LOC_TuKiemGiacMo", DIR = "LOC_KiemTra/GiacMo/";
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bat()
    {
        if (!UnityEditor.EditorPrefs.GetBool(KEY, false)) return;
        UnityEditor.EditorPrefs.SetBool(KEY, false);
        var g = new GameObject("_TuKiemGiacMo"); DontDestroyOnLoad(g); g.AddComponent<LocTuKiemGiacMo>();
    }
    [UnityEditor.MenuItem("LOC/Tự kiểm giấc mơ (Play)")]
    static void Menu()
    {
        UnityEditor.EditorPrefs.SetBool(KEY, true);
        if (!UnityEditor.EditorPrefs.HasKey("LOC_RIB")) UnityEditor.EditorPrefs.SetBool("LOC_RIB", UnityEditor.PlayerSettings.runInBackground);
        UnityEditor.PlayerSettings.runInBackground = true;   // Unity mất tiêu điểm vẫn chạy (LocNhimImport trả lại khi thoát Play)
        UnityEditor.EditorApplication.isPlaying = true;
    }

    readonly System.Text.StringBuilder sb = new();
    int loi, anh; float t0;
    void G(string s) { sb.AppendLine($"{Time.time - t0,7:F1}s  {s}"); System.IO.File.WriteAllText(DIR + "log.txt", sb.ToString()); }
    void Kt(bool ok, string s) { G((ok ? "  OK   " : "  LỖI  ") + s); if (!ok) loi++; }
    IEnumerator Chup(string ten) { ScreenCapture.CaptureScreenshot($"{DIR}{anh++:000}_{ten}.png"); yield return null; yield return null; }

    IEnumerator Start()
    {
        System.IO.Directory.CreateDirectory(DIR);
        foreach (var f in System.IO.Directory.GetFiles(DIR)) System.IO.File.Delete(f);
        t0 = Time.time;
        float a = Time.time; while (!LocCotTruyen.I && Time.time - a < 10f) yield return null;
        yield return new WaitForSeconds(0.5f);
        LocMa.dem = 2;   // giấc mơ thuộc cuối Đêm 2 — vào thẳng Đêm 2 (LocCotTruyen.Update tự áp trạng thái nhà)
        a = Time.time; while ((LocFirstPerson.khoa || LocFirstPerson.den > 0.01f || LocXeBuyt.dangChay) && Time.time - a < 60f) yield return null;
        yield return new WaitForSeconds(1f);
        var ct = LocCotTruyen.I; var fp = FindAnyObjectByType<LocFirstPerson>();
        Kt(ct && fp, "có LocCotTruyen + người chơi"); if (!ct || !fp) { Xong(); yield break; }
        fp.DatTuThe(ct.demSauDung, ct.demSauNhin);
        LocMa.tamDung = true;
        G($"Đêm {LocMa.dem} · bắt đầu");

        // môi trường trước khi mơ (để so lúc trả lại)
        var fog0 = (RenderSettings.fog, RenderSettings.fogDensity, RenderSettings.ambientLight);
        int troi0 = 0; foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None)) if (l.type == LightType.Directional && l.enabled) troi0++;

        yield return Lan(ct, LocGiacMo.Bot.Ne, "1_ne");
        var k = LocGiacMo.cuoi;
        Kt(k.ketThoat, "NÉ: thoát được tới cửa");
        Kt(k.tgChay > 80f && k.tgChay < 130f, $"NÉ: thời lượng chạy {k.tgChay:0} s (80–130)");
        Kt(k.soTrung == 0, $"NÉ: không trúng lần nào ({k.soTrung})");
        Kt(k.soNem >= 25, $"NÉ: số lần ném {k.soNem} (≥ 25 theo bảng nhịp 4 / 3 / 2,2 s)");
        Kt(k.soNemDoi > 0, $"NÉ: có ném hai làn cùng lúc ({k.soNemDoi})");
        Kt(k.luonCoLanTrong, "NÉ: luôn chừa ít nhất một làn trống");
        Kt(!k.daCanhBao, "NÉ: không hiện \"CHẠY ĐI!\" (vẫn đang chạy)");

        yield return Lan(ct, LocGiacMo.Bot.KhongNe, "2_khongne");
        k = LocGiacMo.cuoi;
        Kt(!k.ketThoat && k.soTrung >= 3, $"KHÔNG NÉ: bị bắt vì trúng {k.soTrung} lần");
        Kt(!k.daCanhBao, "KHÔNG NÉ: không hiện \"CHẠY ĐI!\" (vẫn giữ W)");
        G($"KHÔNG NÉ: bị bắt sau {k.tgChay:0} s · ném {k.soNem}");

        yield return Lan(ct, LocGiacMo.Bot.DungYen, "3_dungyen");
        k = LocGiacMo.cuoi;
        Kt(!k.ketThoat && k.soTrung < 3, $"ĐỨNG YÊN: bị bắt vì ma áp sát (trúng {k.soTrung})");
        Kt(k.daCanhBao, "ĐỨNG YÊN: có hiện \"CHẠY ĐI!\" trước khi bị bắt");
        Kt(k.tCanhBao > 1.5f, $"ĐỨNG YÊN: cảnh báo không hiện ngay lúc đầu (hiện ở giây {k.tCanhBao:0.0})");
        Kt(k.tgChay - k.tCanhBao > 1.2f, $"ĐỨNG YÊN: từ lúc cảnh báo tới lúc bị bắt {k.tgChay - k.tCanhBao:0.0} s (> 1,2 s để kịp bấm W)");

        // 4. nối mạch Đêm 2 → giường → mơ → Đêm 3
        G("════ NỐI MẠCH");
        a = Time.time; while ((LocMa.dem != 2 || LocFirstPerson.khoa || LocFirstPerson.den > 0.01f) && Time.time - a < 15f) yield return null;
        yield return new WaitForSeconds(1f);
        Kt(LocMa.dem == 2, "đang ở Đêm 2");
        Kt(ct.ChoNgu && ct.ChoNgu.GoiY == "", "chưa nửa tỉnh: giường chưa cho ngủ");
        LocTui.Dat("giu_huong_2"); LocTui.Dat("nua_tinh_2");
        yield return null;
        Kt(ct.ChoNgu && ct.ChoNgu.GoiY == "[E] Đi ngủ", $"sau nửa tỉnh: giường có \"[E] Đi ngủ\" ({(ct.ChoNgu ? ct.ChoNgu.GoiY : "không có chỗ ngủ")})");
        if (ct.ChoNgu)
        {
            var gn = ct.ChoNgu.transform.position;
            fp.DatTuThe(new Vector3(gn.x - 1.3f, 3.65f, gn.z), gn);   // đứng giữa phòng Khôi (tầng 2), nhìn vào giường
            yield return new WaitForSeconds(0.4f); yield return Chup("4_giuong_di_ngu");
            G("gợi ý đang hiện: " + fp.GoiYDangHien);
        }
        LocGiacMo.bot = LocGiacMo.Bot.Ne;
        ct.DiNguNgay();
        a = Time.time; while (!LocGiacMo.dangMo && Time.time - a < 6f) yield return null;
        Kt(LocGiacMo.dangMo, "bấm Đi ngủ → vào mơ");
        a = Time.time; while (LocGiacMo.dangMo && Time.time - a < 180f) yield return null;
        yield return new WaitForSeconds(1.2f); yield return Chup("4_tinh_day");
        a = Time.time; while (LocMa.dem == 2 && Time.time - a < 60f) yield return null;
        Kt(LocMa.dem == 3, "tỉnh dậy → hết Đêm 2 → sang Đêm 3");
        Kt(LocTui.Co("mo_2_xong") && LocTui.Co("mo2_thoat") && !LocTui.Co("mo2_bi_bat"), "cờ: mo_2_xong + mo2_thoat");
        a = Time.time; while ((LocFirstPerson.khoa || LocFirstPerson.den > 0.01f) && Time.time - a < 30f) yield return null;
        Kt(!LocFirstPerson.khoa && LocFirstPerson.den < 0.01f, "đầu Đêm 3: trả điều khiển");
        Kt(!GameObject.Find("LOC_GiacMo"), "đã xoá hành lang giấc mơ");
        int troi1 = 0; foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None)) if (l.type == LightType.Directional && l.enabled) troi1++;
        Kt(RenderSettings.fog == fog0.Item1 && Mathf.Approximately(RenderSettings.fogDensity, fog0.Item2), "sương trả lại như trước khi mơ");
        G($"đèn trời: trước {troi0} · sau {troi1} (ban đêm có thể khác do Đêm 3 tự đặt)");
        yield return Chup("4_dau_dem3");
        LocGiacMo.bot = LocGiacMo.Bot.Khong;
        Xong();
    }

    // một lần mơ bằng phím thử (DiNguThu: mơ → tỉnh dậy → ở lại đêm hiện tại), chụp ảnh các mốc
    IEnumerator Lan(LocCotTruyen ct, LocGiacMo.Bot bot, string ten)
    {
        G($"════ LẦN {ten} (máy chơi: {bot})");
        LocGiacMo.bot = bot;
        ct.StartCoroutine((IEnumerator)typeof(LocCotTruyen).GetMethod("DiNguThu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(ct, null));
        float a = Time.time; while (!LocGiacMo.dangMo && Time.time - a < 6f) yield return null;
        Kt(LocGiacMo.dangMo, ten + ": vào mơ");
        var goc = GameObject.Find("LOC_GiacMo");
        if (goc)
        {
            int khoi = goc.GetComponentsInChildren<Renderer>().Length;
            Kt(khoi > 300, $"{ten}: hành lang dựng xong ({khoi} khối hình)");
        }
        yield return new WaitForSeconds(2.5f); yield return Chup(ten + "_mo_dau");
        yield return new WaitForSeconds(3.5f); yield return Chup(ten + "_quay_lai_thay_ma");
        bool lo = false, chupCB = false, chupNem = false; float tChup = Time.time;
        while (LocGiacMo.dangMo && Time.time - a < 170f)
        {
            if (goc && LocGiacMo.CoMatDangHien(goc.transform)) lo = true;
            var k = LocGiacMo.cuoi;
            if (!chupCB && k && k.daCanhBao) { chupCB = true; yield return Chup(ten + "_chay_di"); }
            if (!chupNem && k && k.soNem > 0 && Time.time - tChup > 1f) { chupNem = true; yield return new WaitForSeconds(0.9f); yield return Chup(ten + "_dau_cham_than"); }
            if (Time.time - tChup > 20f) { tChup = Time.time; yield return Chup($"{ten}_chay_{Time.time - a:000}"); }
            yield return null;
        }
        Kt(!lo, ten + ": ma không lộ mắt");
        Kt(!LocGiacMo.dangMo, ten + ": mơ kết thúc");
        var kq = LocGiacMo.cuoi;
        G($"{ten}: {(kq.ketThoat ? "THOÁT" : "BỊ BẮT")} · chạy {kq.tgChay:0.0} s · ném {kq.soNem} (đôi {kq.soNemDoi}) · trúng {kq.soTrung} · cảnh báo {(kq.daCanhBao ? $"ở giây {kq.tCanhBao:0.0}" : "không")}");
        a = Time.time; while ((LocFirstPerson.khoa || LocFirstPerson.den > 0.01f) && Time.time - a < 20f) yield return null;
        Kt(!LocFirstPerson.khoa, ten + ": tỉnh dậy xong, trả điều khiển");
        Kt(!GameObject.Find("LOC_GiacMo"), ten + ": đã xoá hành lang");
        LocGiacMo.bot = LocGiacMo.Bot.Khong;
        LocMa.tamDung = true;   // giữa các lần thử: ma nhà đừng dọa chen vào
        yield return new WaitForSeconds(1f);
    }

    void Xong()
    {
        G(loi == 0 ? "════ XONG — KHÔNG LỖI" : $"════ XONG — {loi} LỖI");
        Debug.Log($"[LOC] Tự kiểm giấc mơ: {loi} lỗi → {DIR}log.txt");
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
#endif
