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

    // [21/10] mini game: đứng cách điểm ~1,1 m, tia nhìn trúng điểm, có gợi ý → bấm E mở → bỏ qua → cờ bật, trả điều khiển
    static LocTuongTacMG MG(string id) { foreach (var t in FindObjectsByType<LocTuongTacMG>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (t.id == id) return t; return null; }
    IEnumerator ThuMiniGame(string id, string anh)
    {
        var t = MG(id); var fp = FindAnyObjectByType<LocFirstPerson>();
        Kt(t && t.gameObject.activeInHierarchy, $"mini game {id} có trong scene, đang bật"); if (!t || !fp || !t.gameObject.activeInHierarchy) yield break;
        yield return null;   // Update bật lại va chạm theo đêm
        var m = t.transform.position; bool toi = false;
        foreach (var dir in new[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right })
        {
            if (!Physics.Raycast(m + dir * 1.1f + Vector3.up * 0.3f, Vector3.down, out var san, 4f, ~0, QueryTriggerInteraction.Ignore) || san.point.y > m.y - 0.5f) continue;
            var mat = san.point + Vector3.up * 1.52f;
            if (Physics.Raycast(mat, (m - mat).normalized, out var h, 2.6f, ~0, QueryTriggerInteraction.Ignore) && h.collider.transform == t.transform)
            { fp.DatTuThe(san.point + Vector3.up * 0.05f, m); toi = true; break; }
        }
        Kt(toi, $"{id}: đứng cách ~1,1 m nhìn trúng điểm @ ({m.x:0.00},{m.y:0.00},{m.z:0.00})");
        yield return new WaitForSeconds(0.4f);
        string trung = fp.Tia(fp.cam.forward, 2.6f, out var hc) ? hc.collider.name : "(không trúng gì)";
        Kt(t.GoiY.Length > 0 && fp.GoiYDangHien == t.GoiY, $"{id}: màn hình hiện gợi ý \"{fp.GoiYDangHien}\" (cần \"{t.GoiY}\"; tia nhìn trúng {trung}, khoa={LocFirstPerson.khoa}, túi mở={LocTui.Mo})");
        yield return Chup(anh + "_a_goi_y");
        t.Bam(); yield return new WaitForSeconds(0.3f);
        Kt(LocMiniGame.hienTai == t.miniGame && LocFirstPerson.khoa, $"{id}: E mở mini game, khoá điều khiển");
        yield return Chup(anh + "_b_mo");
        LocMiniGame.BoQua(); yield return new WaitForSeconds(0.3f);
        Kt(LocMiniGame.hienTai == null && !LocFirstPerson.khoa && t.xong && LocTui.Co(id) && t.GoiY == "", $"{id}: Bỏ qua → xong, bật cờ, trả điều khiển, hết gợi ý");
    }

    IEnumerator Start()
    {
        System.IO.Directory.CreateDirectory("LOC_KiemTra/GamePlay");
        yield return new WaitForSeconds(1.5f);
        LocMa.dem = 1; yield return null;
        sb.AppendLine("TỰ KIỂM GAMEPLAY " + System.DateTime.Now.ToString("dd/MM HH:mm:ss"));
        Kt(LocTui.I, "LocTui có trên người chơi");
        LocFirstPerson.ngoi = true; yield return new WaitForSeconds(0.6f);
        var fp0 = FindAnyObjectByType<LocFirstPerson>();
        Kt(fp0 && Mathf.Abs(fp0.cam.localPosition.y - fp0.matNgoi) < 0.02f, $"ngồi: mắt hạ xuống {fp0.cam.localPosition.y:0.00} m");
        Kt(!fp0.Tia(Vector3.up, 0.7f, out _), "ngồi giữa sân: trên đầu trống (không tự vướng vào mình)");
        LocFirstPerson.ngoi = false; yield return new WaitForSeconds(0.6f);
        Kt(fp0.cam.localPosition.y > 1.5f, "đứng dậy: mắt về 1,52 m");
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
        yield return ThuMiniGame("mg_ban_hoc", "10_ban_hoc");   // sau phần Đêm 1 (đầu Đêm 1 phải còn đứng ngoài cổng)
        yield return ThuMiniGame("mg_ban_an", "11_ban_an");
        yield return ThuMiniGame("dan_chen", "12_dan_chen");
        LocTui.I.DatChe(1); yield return new WaitForSeconds(0.3f); yield return Chup("04_tui_dem2_doc_so");
        Kt(V("so_cua_bo").DocDuoc, "Đêm 2 đọc được sổ của bố (" + V("so_cua_bo").trang.Length + " trang)");
        LocTui.I.DatChe(0); yield return null;

        V("sach_gia_le").Bam(); Kt(!LocTui.CoMon("sach_gia_le"), "Đêm 2 chưa lấy sách (đồ thờ, đừng lục)");
        LocMa.dem = 3; yield return null; V("sach_gia_le").Bam(); yield return null;
        Kt(LocTui.CoMon("sach_gia_le"), "Đêm 3 lấy được sách");
        Kt(!MG("dan_chen").HienDuoc, "Đêm 3 không còn dán chén");
        Kt(MG("nl_giu_nhang") && !MG("nl_giu_nhang").gameObject.activeSelf, "giữ nhang ẩn tới khi bày mâm xong");
        yield return ThuMiniGame("nl_bay_mam", "13_bay_mam");
        yield return ThuMiniGame("nl_giu_nhang", "14_giu_nhang");

        sb.AppendLine(loi == 0 ? "KẾT QUẢ: ĐẠT" : $"KẾT QUẢ: {loi} LỖI");
        System.IO.File.WriteAllText("LOC_KiemTra/GamePlay/BaoCao.txt", sb.ToString());
        yield return new WaitForSeconds(0.5f);
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
#endif
