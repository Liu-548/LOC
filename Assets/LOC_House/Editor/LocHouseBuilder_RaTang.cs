using System.Linq;
using UnityEngine;

// [23/10] Rà lại bố trí theo hoàn cảnh nhà có tang (gia đình ở lâu năm: bừa ở kho/góc phụ, gọn ở bàn thờ, linh cữu, chỗ tiếp khách)
//  · che màn hình TV phòng khách như đã che gương (nhà có tang che vật phản chiếu)
//  · kho sau T1 có đồ dồn của nhà ở lâu năm (trước chỉ 3 chồng thùng)
//  · dép khách cởi ngoài cửa chính
//  · đồ trong tủ thò xuyên qua cánh đóng → mở cánh thì món đó "lơ lửng" ở chỗ cũ: đẩy lùi vào trong lòng tủ
public static partial class LocHouseBuilder
{
    static void VaiPhuTV(GameObject tuTv)
    {
        var tv = tuTv ? tuTv.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "TV_CRT") : null;
        if (!tv) return;
        var b = LocSceneAudit.LocalBounds(tv, tv); if (b.size == Vector3.zero) return;
        var g = new GameObject("VaiChePhuTV").transform; g.SetParent(tv, false);
        var vai = M("M_VaiPhu_TV", "#E6E0D0");
        Cb(g, "vai_noc", new Vector3(b.center.x, b.max.y + 0.004f, b.center.z + 0.015f), new Vector3(b.size.x + 0.05f, 0.008f, b.size.z + 0.03f), vai);
        Cb(g, "vai_truoc", new Vector3(b.center.x, b.max.y - b.size.y * 0.36f, b.max.z + 0.013f), new Vector3(b.size.x + 0.05f, b.size.y * 0.74f, 0.008f), vai);   // rủ trùm kín màn hình
        Cb(g, "nep_gap", new Vector3(b.center.x - b.size.x * 0.18f, b.max.y - b.size.y * 0.36f, b.max.z + 0.018f), new Vector3(0.03f, b.size.y * 0.7f, 0.004f), M("M_VaiPhu_TV_Gap", "#CFC8B6"));
    }

    // kho sau (x 0…3,60 · z 19,20…24,60, cửa x 1,40…2,20 mở vào, cánh nằm dọc x 2,20 khi mở): chừa lối giữa x 1,30…2,40 từ cửa vào tới chồng thùng cuối
    static void KhoSauDoDon()
    {
        A("GocKho_DoGio_Fix", 0.24f, 21.15f, 0, 90);           // nồi đồng úp, thùng nhựa, chiếu cói gấp — dựa tường trái
        A("ChongGhe", 0.3f, 22.55f, 0, 90);                    // ghế gỗ cũ xếp chồng
        A("ThungSon_Cum2", 3.15f, 20.75f, 0, -90);             // sơn tồn của tiệm
        A("BaoXiMang_Chong", 3.2f, 21.85f, 0, -90);            // bao xi măng dở
        A("SM_PlasticChairStack_8", 3.3f, 22.45f, 0, -80);     // ghế nhựa dư của đám dồn vào kho
        A("ThungNuoc_Nap", 0.35f, 20.0f, 0);                   // can nước đầy (mất nước thì dùng)
        A("ChauNhua_Up", 2.95f, 19.65f, 0, 30);
    }

    // dép khách viếng cởi ngoài hiên (sân trước y −0,15); đêm 1 đông khách, đêm 2 thưa
    static void DepKhachHien(float y)
    {
        var old = cur;
        cur = G("SanTruoc/Dem1");
        A("Dep_Khach", 3.05f, -0.55f, y, 15); A("Dep_Khach", 4.45f, -0.5f, y, -25); A("Dep_Khach", 5.55f, -0.7f, y, 170);
        cur = G("SanTruoc/Dem2");
        A("Dep_Khach", 4.2f, -0.6f, y, 5);
        cur = old;
    }

    // đồ trong lòng tủ (nhóm DoTu_TrongTu) lấn qua mặt trong của cánh đang đóng → lùi vào trong 8 mm sau mặt cánh
    static string DayDoTrongTuVaoTrong(Transform goc)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var d in goc.GetComponentsInChildren<LocDoor>(true))
        {
            if (d.dungSanLaMo || d.truot != Vector3.zero || d.gapX > 0) continue;   // chỉ cánh quay đang đóng
            var leaf = d.transform; var tu = leaf.parent; while (tu && !tu.GetComponent<LocProp>()) tu = tu.parent;
            if (!tu) continue;
            var lb = LocSceneAudit.LocalBounds(leaf, leaf); if (lb.size == Vector3.zero) continue;
            var tamTu = leaf.InverseTransformPoint(LocSceneAudit.WB(tu.gameObject).center);
            float vao = tamTu.z < lb.center.z ? -1f : 1f;   // hướng vào lòng tủ theo trục z của cánh
            foreach (var nhom in tu.GetComponentsInChildren<Transform>(true).Where(x => x.name == "DoTu_TrongTu"))
                foreach (Transform mon in nhom)
                {
                    if (mon.GetComponentInParent<LocDoor>()) continue;
                    float lan = 0;
                    foreach (var r in mon.GetComponentsInChildren<Renderer>())
                    {
                        var mf = r.GetComponent<MeshFilter>(); if (!mf || !mf.sharedMesh) continue;
                        var m = leaf.worldToLocalMatrix * r.transform.localToWorldMatrix; var mb = mf.sharedMesh.bounds; var ib = new Bounds(m.MultiplyPoint3x4(mb.center), Vector3.zero);
                        for (int i = 0; i < 8; i++) ib.Encapsulate(m.MultiplyPoint3x4(mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
                        if (ib.max.x < lb.min.x || ib.min.x > lb.max.x || ib.max.y < lb.min.y || ib.min.y > lb.max.y) continue;
                        float matTrong = vao < 0 ? lb.min.z : lb.max.z;                          // mặt cánh quay vào lòng tủ
                        float thoRa = vao < 0 ? ib.max.z - (matTrong - 0.008f) : (matTrong + 0.008f) - ib.min.z;
                        lan = Mathf.Max(lan, thoRa);
                    }
                    if (lan <= 0 || lan > 0.12f) continue;
                    mon.position += leaf.TransformVector(new Vector3(0, 0, vao * lan));
                    sb.AppendLine($"  {tu.name}/{mon.name}: lùi {lan * 100:0.0} cm vào lòng tủ (cánh {leaf.name})");
                }
        }
        return sb.ToString();
    }

    // lớp mỏng (vết tay, nhãn dán…) nằm sát mặt cánh nhưng là nút riêng của model tủ → cánh trượt/quay đi thì vết vẫn treo giữa không trung: gắn làm con của cánh
    static string GanLopMongVaoCanh(Transform goc)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var d in goc.GetComponentsInChildren<LocDoor>(true))
        {
            var leaf = d.transform; var tu = leaf.parent; while (tu && !tu.GetComponent<LocProp>()) tu = tu.parent;
            if (!tu) continue;
            var lb = LocSceneAudit.LocalBounds(leaf, leaf); if (lb.size == Vector3.zero) continue;
            foreach (var r in tu.GetComponentsInChildren<Renderer>(true))
            {
                if (r.transform.IsChildOf(leaf) || r.GetComponentInParent<LocDoor>()) continue;
                var mf = r.GetComponent<MeshFilter>(); if (!mf || !mf.sharedMesh) continue;
                var m = leaf.worldToLocalMatrix * r.transform.localToWorldMatrix; var mb = mf.sharedMesh.bounds; var ib = new Bounds(m.MultiplyPoint3x4(mb.center), Vector3.zero);
                for (int i = 0; i < 8; i++) ib.Encapsulate(m.MultiplyPoint3x4(mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
                if (ib.size.z > 0.004f) continue;   // chỉ lớp mỏng song song mặt cánh
                float ox = Mathf.Min(ib.max.x, lb.max.x) - Mathf.Max(ib.min.x, lb.min.x), oy = Mathf.Min(ib.max.y, lb.max.y) - Mathf.Max(ib.min.y, lb.min.y);
                if (ox < ib.size.x * 0.8f || oy < ib.size.y * 0.8f) continue;
                if (ib.center.z < lb.min.z - 0.015f || ib.center.z > lb.max.z + 0.015f) continue;
                var pr = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(r.gameObject);   // nút trong prefab instance không đổi cha được → gỡ prefab trước
                if (pr) UnityEditor.PrefabUtility.UnpackPrefabInstance(pr, UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
                r.transform.SetParent(leaf, true);
                if (r.transform.parent != leaf) { sb.AppendLine($"  ✗ {tu.name}/{r.name}: không gắn được vào {leaf.name}"); continue; }
                sb.AppendLine($"  {tu.name}/{r.name} → con của cánh {leaf.name}");
            }
        }
        return sb.ToString();
    }

    // nồi nhôm to nấu cỗ (Ø 0,46 · cao 0,36) — chưa có .glb nên dựng khối; up=false: úp miệng phơi
    static void NoiTo(float x, float z, float y, bool nap, bool up = true)
    {
        var g = new GameObject("NoiNhom_To"); g.transform.SetParent(cur, false); g.transform.position = new Vector3(x, y, z);
        var p = new GameObject("Than").transform; p.SetParent(g.transform, false);
        if (!up) { p.localRotation = Quaternion.Euler(180, 0, 0); p.localPosition = new Vector3(0, 0.36f, 0); }
        Material nhom = M("M_NhomNoi", "#B9BCBD"), sam = M("M_NhomNoi_Sam", "#6E6A63"), den = M("M_NoiDay_Am", "#2E2B27");
        Cb(p, "than", new Vector3(0, 0.18f, 0), new Vector3(0.46f, 0.18f, 0.46f), nhom, PrimitiveType.Cylinder);
        Cb(p, "day_am_khoi", new Vector3(0, 0.03f, 0), new Vector3(0.462f, 0.03f, 0.462f), den, PrimitiveType.Cylinder);   // đáy ám đen
        Cb(p, "mieng", new Vector3(0, 0.355f, 0), new Vector3(0.49f, 0.01f, 0.49f), sam, PrimitiveType.Cylinder);
        foreach (var sx in new[] { -0.255f, 0.255f }) Cb(p, "quai", new Vector3(sx, 0.31f, 0), new Vector3(0.05f, 0.025f, 0.07f), sam);
        if (nap && up)
        {
            Cb(p, "nap", new Vector3(0, 0.37f, 0), new Vector3(0.47f, 0.008f, 0.47f), nhom, PrimitiveType.Cylinder);
            Cb(p, "num_nap", new Vector3(0, 0.395f, 0), new Vector3(0.05f, 0.015f, 0.05f), sam, PrimitiveType.Cylinder);
        }
        Mark(g, LocProp.Kieu.San);
    }

    // [23/10] bếp ngày có đám — Đêm 1: họ hàng bắc bếp kiềng nấu cho khách · Đêm 2: bừa nhất, bát đĩa mượn chất đống chưa rửa
    //  · Đêm 3: thu dọn dở, ghế + két nước chờ trả. Lối sảnh → bếp → cửa giếng trời (x 3,8–4,7) và hai lối quanh bàn ăn để trống.
    static void BepCoDam()
    {
        var old = cur;
        cur = G("Tang_1/Bep/Dem1");
        A("BepGas_Kieng_BinhGas", 5.95f, 15.8f, 0, 180); NoiTo(5.95f, 15.8f, 0.6f, true);
        A("GheDauNho", 5.4f, 15.25f, 0, 30); A("GheDauNho", 6.55f, 15.15f, 0, -40);
        A("ChauInox", 5.45f, 14.75f, 0);
        A("BatDia_TrongChau", 0.95f, 14.95f, 0, 20); A("BatDia_TrongChau", 1.05f, 15.55f, 0, -15);
        A("RauQua_Ro", 1.75f, 14.6f, 0, 25);
        A("ThungNuocNgot_Khach", 5.5f, 11.35f, 0, 10); A("ThungNuocNgot_Khach", 5.5f, 11.35f, 0.4f, -5);
        A("SM_PlasticChairStack_4", 5.95f, 11.4f, 0, 0);   // [29/10] dời khỏi vùng quét cánh tủ lạnh (bản lề x≈6,92, cánh 0,58 m)
        A("KhayTra_LyThuyTinh", 6.95f, 12.6f, 0.6f, 0); A("DiaKeoBanh_HatDua", 6.95f, 13.6f, 0.6f, 0);   // soạn sẵn trên chõng để bưng ra rạp

        cur = G("Tang_1/Bep/Dem2");
        A("BepGas_Kieng_BinhGas", 5.95f, 15.8f, 0, 170); NoiTo(5.95f, 15.8f, 0.6f, false);
        A("NapNoi", 6.5f, 15.5f, 0, 40);
        A("GheDauNho", 5.35f, 15.3f, 0, 75);
        A("BatDia_TrongChau", 0.95f, 14.95f, 0, 20); A("BatDia_TrongChau", 1.05f, 15.55f, 0, -15); A("BatDia_TrongChau", 1.65f, 15.25f, 0, 60);
        A("ChauInox", 5.45f, 14.75f, 0); A("ChauNhua", 6.35f, 14.8f, 0, 15);
        A("ThungNuocNgot_Khach", 5.5f, 11.35f, 0, 10); A("ThungNuocNgot_Khach", 5.5f, 11.35f, 0.4f, -5); A("ThungNuocNgot_Khach", 6.0f, 11.3f, 0, 30);
        A("SM_PlasticChairStack_4", 6.05f, 11.85f, 0, 15);   // [29/10] như Đêm 1
        A("ThungRacNhua", 5.3f, 16.1f, 0, 0);
        A("SM_PlasticChair_Red", 6.1f, 14.3f, 0, 40); A("SM_PlasticChair_Green", 1.9f, 12.3f, 0, -70);   // ghế khách bưng vào ngồi rửa bát, bỏ lại
        A("BaoTaiRong", 1.95f, 14.95f, 0, 20);
        A("KhayTra_LyThuyTinh", 4.3f, 13.35f, 0.8f, 30); A("DiaKeoBanh_HatDua", 3.3f, 14.05f, 0.8f, -20);   // khay ly bẩn, đĩa hạt dưa bưng từ rạp vào để dồn lên bàn ăn
        A("PorridgeBowl_Tron", 6.9f, 12.55f, 0.6f); A("BatChao_An", 7.05f, 13.6f, 0.6f); A("KhoaiLuoc_Dia", 6.85f, 13.9f, 0.6f);   // ăn dở để trên chõng

        cur = G("Tang_1/Bep/Dem3");
        A("BepGas_Kieng_BinhGas", 6.55f, 16.0f, 0, 150); NoiTo(5.55f, 15.85f, 0, false, false);   // kiềng dồn vào góc, nồi úp phơi
        A("SM_PlasticChairStack_8", 5.5f, 11.35f, 0, 0);       // ghế mượn xếp lại chờ trả
        A("ThungNuocNgot_Khach", 6.15f, 11.35f, 0, 0);
        A("BatDia_TrongChau", 0.95f, 14.95f, 0, 20);
        A("ThungRacNhua", 6.4f, 15.15f, 0, 0);
        A("ChauInox", 6.95f, 12.6f, 0.6f);                      // bát mượn đã rửa gom vào chậu
        cur = old;
    }
}
