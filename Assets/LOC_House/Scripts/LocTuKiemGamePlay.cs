#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

// [6/10] Tự kiểm gameplay cơ bản (menu LOC → Tự kiểm gameplay): chạy chuỗi Đêm 1 bằng code, chụp túi đồ / bàn phím két → LOC_KiemTra/GamePlay/
public class LocTuKiemGamePlay : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bat()
    {
        if (!UnityEditor.EditorPrefs.GetBool("LOC_TuKiemGamePlay", false)) return;
        UnityEditor.EditorPrefs.SetBool("LOC_TuKiemGamePlay", false);
        new GameObject("_TuKiemGamePlay").AddComponent<LocTuKiemGamePlay>();
    }

    readonly System.Text.StringBuilder sb = new();
    int loi;
    void Kt(bool ok, string s) { sb.AppendLine((ok ? "  OK   " : "  LỖI  ") + s); if (!ok) loi++; }
    static LocVatPham V(string id) { foreach (var v in FindObjectsByType<LocVatPham>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (v.id == id) return v; return null; }
    static IEnumerator Chup(string ten) { ScreenCapture.CaptureScreenshot($"LOC_KiemTra/GamePlay/{ten}.png"); yield return null; yield return null; }   // không chờ cuối khung: Unity mất tiêu điểm thì Game view không vẽ

    // [8/10] hệ thống đêm + chuỗi Đêm 1 (LocCotTruyen)
    IEnumerator CotTruyenDem1()
    {
        var ct = LocCotTruyen.I; var fp = FindAnyObjectByType<LocFirstPerson>();
        Kt(ct, "LocCotTruyen có trên người chơi"); if (!ct || !fp) yield break;
        var p0 = fp.transform.position;
        Kt(p0.z < -5.5f, $"đầu Đêm 1: đứng ngoài cổng (z = {p0.z:0.00})");
        int dong = 0; foreach (var n in new[] { "CanhCong_Trai (mở)", "CanhCong_Phai (mở)", "CuaNgachCong (mở)" }) { var d = GameObject.Find(n)?.GetComponent<LocDoor>(); if (d && !d.DangMo && !d.khoa) dong++; }
        Kt(dong == 3, $"đầu Đêm 1: 3 cánh cổng đóng, chưa khoá ({dong}/3)");
        var cham = GameObject.Find("CongTac_Ham")?.GetComponent<LocCongTac>();
        Kt(cham && !cham.bat, "đầu Đêm 1: đèn hầm tắt");
        yield return Chup("05_dau_dem1_ngoai_cong");

        LocViec viec = null; foreach (var v in FindObjectsByType<LocViec>(FindObjectsSortMode.None)) if (v.id == "thap_huong") viec = v;
        Kt(viec && viec.GetComponentInChildren<Collider>() && viec.GoiY == "[E] Thắp hương", "bàn thờ vong: [E] Thắp hương");
        if (!viec) yield break;
        fp.DatTuThe(ct.demSauDung, ct.demSauNhin); yield return new WaitForSeconds(0.3f);
        viec.Bam(); yield return new WaitForSeconds(2f); yield return Chup("06_thap_huong");
        Kt(LocFirstPerson.khoa && viec.GoiY == "", "thắp hương: khoá điều khiển, giữ camera trên ảnh, hết gợi ý");
        float t0 = Time.time; while (!LocTui.Co("ho_hang_ve") && Time.time - t0 < 60f) yield return null;
        Kt(LocTui.Co("ho_hang_ve") && !LocFirstPerson.khoa, $"thoại người họ hàng chạy hết ({Time.time - t0:0}s), trả điều khiển");

        fp.DatTuThe(new Vector3(-2.0f, -2.25f, 13.8f), new Vector3(-2.0f, -1.6f, 11.0f));
        cham.Dat(true); yield return new WaitForSeconds(1.0f);
        Kt(cham.bat && LocTui.Co("d1_xuong_ham"), "xuống hầm có đèn → độc thoại hầm, đèn vẫn sáng khi còn ở dưới");
        yield return Chup("07_ham_dem1");
        fp.DatTuThe(new Vector3(1.0f, 0.02f, 18.3f), new Vector3(3f, 1.5f, 18.3f)); yield return new WaitForSeconds(0.3f);
        Kt(!cham.bat, "bước ra khỏi cửa hầm → đèn hầm tự tắt");
        yield return new WaitForSeconds(3f);
        Kt(LocMa.dem == 1, "chưa thăm Nhím → chưa hết Đêm 1");

        LocNhim.daTham[1] = true;
        t0 = Time.time; while (LocMa.dem == 1 && Time.time - t0 < 25f) yield return null;
        Kt(LocMa.dem == 2, "đã xuống hầm + thăm Nhím → tự sang Đêm 2");
        t0 = Time.time; while ((LocFirstPerson.khoa || LocFirstPerson.den > 0) && Time.time - t0 < 10f) yield return null;
        int sai = 0; foreach (var t in GameObject.Find("NhaLoc").GetComponentsInChildren<Transform>(true))
            if (t.name is "Dem1" or "Dem2" or "Dem3" && t.gameObject.activeSelf != (t.name == "Dem2")) sai++;
        Kt(sai == 0, $"Đêm 2: chỉ bật nhóm Dem2 ({sai} nhóm sai)");
        Kt((fp.transform.position - ct.demSauDung).magnitude < 0.5f && !LocFirstPerson.khoa, "đầu Đêm 2: đứng giữa phòng khách, trả điều khiển");
        int khoa = 0; foreach (var n in new[] { "CanhCong_Trai (mở)", "CanhCong_Phai (mở)", "CuaNgachCong (mở)" }) { var d = GameObject.Find(n)?.GetComponent<LocDoor>(); if (d && !d.DangMo && d.khoa) khoa++; }
        Kt(khoa == 3, $"Đêm 2: cổng đóng + khoá ({khoa}/3)");
        yield return new WaitForSeconds(0.5f); yield return Chup("08_dau_dem2");
    }

    IEnumerator Start()
    {
        System.IO.Directory.CreateDirectory("LOC_KiemTra/GamePlay");
        yield return new WaitForSeconds(1.5f);
        LocMa.dem = 1; yield return null;
        sb.AppendLine("TỰ KIỂM GAMEPLAY " + System.DateTime.Now.ToString("dd/MM HH:mm:ss"));
        Kt(LocTui.I, "LocTui có trên người chơi");
        foreach (var id in new[] { "so_thu_chi_2002", "chia_ham", "so_cua_bo", "sach_gia_le", "bat_gao", "bat_muoi", "bo_mia" })
        { var v = V(id); Kt(v && v.GetComponent<Collider>(), $"món {id} có trong scene + có va chạm" + (v ? $" @ ({v.transform.position.x:0.00},{v.transform.position.y:0.00},{v.transform.position.z:0.00})" : "")); }
        var ks = FindAnyObjectByType<LocKetSat>(); var cuaHam = GameObject.Find("CuaSatHam (mở)")?.GetComponent<LocDoor>();
        Kt(ks && ks.canh && ks.canh.khoa, "két sắt khoá, cánh nối khoá số");
        Kt(cuaHam && cuaHam.khoa && cuaHam.canVatPham == "chia_ham", "cửa hầm khoá, cần chìa");

        cuaHam.Bam(); yield return null; Kt(cuaHam.khoa && !cuaHam.DangMo, "chưa có chìa → cửa hầm vẫn khoá");
        V("bat_gao").Bam(); Kt(!LocTui.CoMon("bat_gao") && V("bat_gao").gameObject.activeSelf, "Đêm 1 chưa lấy được bát gạo (Đêm 2)");

        var stc = V("so_thu_chi_2002"); stc.Bam(); yield return null;
        Kt(LocTui.Mo && LocTui.Co("doc:so_thu_chi_2002"), "đọc sổ thu chi tại chỗ → mở trang đọc, bật cờ đã đọc");
        yield return new WaitForSeconds(0.3f); yield return Chup("01_so_thu_chi");
        LocTui.I.DatChe(0); yield return null;   // đóng

        ks.canh.Bam(); yield return null; Kt(LocFirstPerson.khoa, "bấm E vào cánh két → mở bàn phím, khoá điều khiển");
        ks.GoMa("1234"); yield return null; yield return Chup("02_ket_sai"); Kt(ks.canh.khoa, "mã sai 1234 → vẫn khoá");
        yield return new WaitForSeconds(1.0f);
        ks.GoMa("1980"); yield return new WaitForSeconds(0.9f);
        Kt(!ks.canh.khoa && ks.canh.DangMo && !LocFirstPerson.khoa && LocTui.Co("ket_mo"), "mã 1980 → két mở, trả điều khiển");

        V("chia_ham").Bam(); yield return null;
        Kt(LocTui.CoMon("chia_ham") && !V("chia_ham").gameObject.activeSelf, "nhặt chùm chìa khoá → vào túi, ẩn khỏi két");
        V("so_cua_bo").Bam(); yield return null;
        Kt(LocTui.CoMon("so_cua_bo") && !V("so_cua_bo").DocDuoc, "nhặt sổ của bố, Đêm 1 chưa đọc được");

        cuaHam.Bam(); yield return new WaitForSeconds(0.9f);
        Kt(!cuaHam.khoa && cuaHam.DangMo, "có chìa → cửa hầm mở khoá + mở ra");

        LocTui.I.DatChe(1); yield return new WaitForSeconds(0.3f); yield return Chup("03_tui_dem1");
        LocTui.I.DatChe(0); yield return null;
        yield return CotTruyenDem1();   // [8/10] hết Đêm 1 → tự sang Đêm 2
        LocTui.I.DatChe(1); yield return new WaitForSeconds(0.3f); yield return Chup("04_tui_dem2_doc_so");
        Kt(V("so_cua_bo").DocDuoc, "Đêm 2 đọc được sổ của bố (" + V("so_cua_bo").trang.Length + " trang)");
        LocTui.I.DatChe(0); yield return null;

        V("sach_gia_le").Bam(); Kt(!LocTui.CoMon("sach_gia_le"), "Đêm 2 chưa lấy sách (đồ thờ, đừng lục)");
        LocMa.dem = 3; yield return null; V("sach_gia_le").Bam(); yield return null;
        Kt(LocTui.CoMon("sach_gia_le"), "Đêm 3 lấy được sách");

        sb.AppendLine(loi == 0 ? "KẾT QUẢ: ĐẠT" : $"KẾT QUẢ: {loi} LỖI");
        System.IO.File.WriteAllText("LOC_KiemTra/GamePlay/BaoCao.txt", sb.ToString());
        yield return new WaitForSeconds(0.5f);
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
#endif
