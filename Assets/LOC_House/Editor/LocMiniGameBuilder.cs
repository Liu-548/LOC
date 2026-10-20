#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// [21/10] Dựng các mini game hợp với kịch bản v3.2 vào scene (LOC → Dựng nhà LỘC gọi DungVaoScene; menu riêng để dựng lại nhanh).
// Mỗi điểm là một cục đánh dấu nhỏ (TD_…) đặt trên mặt vật thật, không gắn thẳng lên vật — để không đè LocVatPham/LocDoor/LocHuCot sẵn có.
//  Đêm 1–3  bàn học Khôi        Xếp lại bàn học (tuỳ chọn)
//  Đêm 2–3  bàn ăn giữa bếp     Chỗ ngồi quanh bàn ăn (tuỳ chọn)
//  Đêm 2    chén trên bàn thờ hầm   Dán chén 1 + 2 (kịch bản: dán chén dưới hầm)        → cờ dan_chen
//  Đêm 3    bàn thờ hầm         Nghi lễ vòng 1 bày mâm → vòng 3 giữ ngọn nhang          → cờ nl_bay_mam, nl_giu_nhang
// Không dùng (lệch kịch bản): khoá số tủ thờ (trùng két), đồng hồ, ghép ảnh, nối dây, guitar, radio (đài là dấu hiệu của thầy),
// giải chữ gương, nín thở trốn (game không có kẻ truy sát). Script của chúng vẫn giữ trong MiniGame/.
public static class LocMiniGameBuilder
{
    const string ROOT = "LOC_MiniGame";
    static Transform root, games;
    static StringBuilder log;
    static int dem;

    [MenuItem("LOC/Mini game/Dựng hệ mini game vào scene")]
    public static void Dung()
    {
        int n = DungVaoScene();
        EditorUtility.DisplayDialog("LOC", "Đã dựng " + n + " điểm mini game. Xem Assets/LOC_House/BaoCao_MiniGame.txt.", "OK");
    }

    public static int DungVaoScene()
    {
        log = new StringBuilder("[LocMiniGameBuilder] " + System.DateTime.Now + "\n"); dem = 0;
        var cu = GameObject.Find(ROOT);
        if (cu) Undo.DestroyObjectImmediate(cu);
        foreach (var t in Object.FindObjectsByType<LocTuongTacMG>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t) Undo.DestroyObjectImmediate(t);

        root = new GameObject(ROOT).transform; Undo.RegisterCreatedObjectUndo(root.gameObject, "Dựng mini game");
        games = new GameObject("TroChoi").transform; games.SetParent(root, false);

        var ban = Diem("BanHoc_Khoi_Bo", "bàn học", Game<LocMG_XepLaiBan>("XepLaiBan_Khoi"), 1, 3, 0f);
        if (ban) ban.id = "mg_ban_hoc";

        var ghe = Diem("BanAn_Bo", "bàn ăn", Game<LocMG_XepGhe>("XepGhe"), 2, 3, 0f);
        if (ghe) ghe.id = "mg_ban_an";

        var chen = Diem("Bo_Chen_Tren_De", "Dán chén", Game<LocMG_GhepChen>("DanChen"), 2, 2, 0f);
        if (chen) chen.id = "dan_chen";

        var mam = Diem("BanThoHam", "Bày mâm", Game<LocMG_MamCung>("BayMam"), 3, 3, 0.15f);
        var lua = Game<LocMG_GiuLua>("GiuNhang");
        lua.tieuDe = "Giữ ngọn nhang"; lua.goiY = "Gió lạnh thổi từng đợt. Giữ SPACE (hoặc chuột phải) để che, đừng che quá lâu kẻo mỏi tay.";
        lua.cauXong = "Ngọn nhang vẫn cháy. Gió yếu dần.";
        var nhang = Diem("BanThoHam", "Giữ nhang", lua, 3, 3, 0.15f);
        if (mam && nhang)
        {
            mam.id = "nl_bay_mam"; nhang.id = "nl_giu_nhang";
            nhang.gameObject.SetActive(false); mam.hien = new[] { nhang.gameObject };   // vòng 3 chỉ mở sau khi bày mâm
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        log.AppendLine("Xong: " + dem + " điểm. Bỏ qua mini game: nút [Bỏ qua] hoặc phím B (LocMiniGame.choBoQua).");
        Debug.Log(log.ToString());
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_MiniGame.txt", log.ToString());
        return dem;
    }

    static T Game<T>(string ten) where T : LocMiniGame
    {
        var g = new GameObject(ten); g.transform.SetParent(games, false);
        return g.AddComponent<T>();
    }

    // cục đánh dấu 0,35 m đặt giữa mặt trên vật (dịch `lui` m theo +Z — phía người đứng ở hầm), mở mini game trong khoảng đêm
    static LocTuongTacMG Diem(string tenVat, string hienTen, LocMiniGame mg, int tu, int den, float lui)
    {
        Transform vat = null;
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (t.name == tenVat) { vat = t; break; }
        if (!vat) { log.AppendLine("! không thấy " + tenVat + " — bỏ điểm " + hienTen); return null; }
        var rs = vat.GetComponentsInChildren<Renderer>(true);
        var b = rs.Length > 0 ? rs[0].bounds : new Bounds(vat.position, Vector3.zero);
        foreach (var r in rs) b.Encapsulate(r.bounds);

        var g = new GameObject("TD_" + mg.name); g.transform.SetParent(root, false);
        g.transform.position = new Vector3(b.center.x, b.max.y + 0.12f, b.center.z + lui);
        g.AddComponent<BoxCollider>().size = Vector3.one * 0.35f;
        var tt = g.AddComponent<LocTuongTacMG>();
        tt.loai = LocTuongTacMG.Loai.MoMiniGame; tt.ten = hienTen; tt.miniGame = mg; tt.demTu = tu; tt.demDen = den;
        dem++;
        log.AppendLine($"  {hienTen,-12} Đêm {tu}–{den}  trên {tenVat}  tại {g.transform.position}");
        return tt;
    }
}
#endif
