using System.Linq;
using UnityEditor;
using UnityEngine;

// [5/10] Đặt NHÍM vào nhà (gọi cuối Build(), sau Ma()): giường phòng Nhím (tư thế nằm), ghế ăn tầng 1 (Good), góc phòng (Bad B).
//  Hướng model được đo bằng cách lấy mẫu clip (đầu phải về phía đầu giường, mặt hướng ra phòng / về phía bàn / vào tường).
//  Báo cáo: Assets/LOC_House/BaoCao_Nhim.txt
public static partial class LocHouseBuilder
{
    static void Nhim()
    {
        var log = new System.Text.StringBuilder($"NHÍM — LocHouseBuilder_Nhim {System.DateTime.Now:dd/MM HH:mm}\n");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LocNhimImport.DIR + "Nhim.fbx");
        if (!prefab) { System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_Nhim.txt", "THIẾU Models/Nhim/Nhim.fbx"); return; }
        var mats = LocNhimImport.VatLieu();
        var ac = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(LocNhimImport.DIR + "AC_Nhim.controller") ?? LocNhimImport.Controller();
        var clips = AssetDatabase.LoadAllAssetsAtPath(LocNhimImport.DIR + "Nhim.fbx").OfType<AnimationClip>().ToDictionary(c => c.name, c => c);

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
        go.name = "Nhim"; go.transform.SetParent(root, true);
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (mats.TryGetValue(r.name, out var m)) r.sharedMaterial = m; else log.AppendLine("không có vật liệu cho " + r.name);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            if (r is SkinnedMeshRenderer s) { s.updateWhenOffscreen = true; }
        }
        var an = go.GetComponent<Animator>() ?? go.AddComponent<Animator>();
        an.runtimeAnimatorController = ac; an.applyRootMotion = false; an.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        Transform T(string n) => go.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == n);
        Transform R(string n) => root.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == n || x.name.StartsWith(n + " ") || x.name.StartsWith(n + "("));
        Bounds B(Transform t) { var rs = t.GetComponentsInChildren<Renderer>(); var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b; }
        var n = go.AddComponent<LocNhim>();
        n.anim = an; n.head = T("Head"); n.eyeL = T("Eye_L"); n.eyeR = T("Eye_R");
        n.da = T("Body").GetComponent<SkinnedMeshRenderer>();
        n.mat = new[] { T("Eye_L").GetComponent<Renderer>(), T("Eye_R").GetComponent<Renderer>() };
        foreach (var (ten, set) in new (string, System.Action<GameObject>)[] { ("Chan", g => n.chan = g), ("ChanVo", g => n.chanVo = g), ("Khan_Kho", g => n.khanKho = g), ("Khan_Uot", g => n.khanUot = g), ("DayDeo", g => n.dayDeo = g) })
        { var t = T(ten); if (t) set(t.gameObject); else log.AppendLine("THIẾU " + ten); }
        GameObject[] Bo(params string[] ts) => ts.Select(T).Where(t => t).Select(t => t.gameObject).ToArray();
        n.boDem1 = Bo("Ao", "Quan"); n.boDem2 = Bo("Ao_D2", "Quan_D2", "DaVai_D2"); n.boDem3 = Bo("Ao_D3", "Quan_D3", "InTho");   // [29/10]
        n.daVai = T("DaVai_D2")?.GetComponent<Renderer>();
        if (n.boDem2.Length < 3 || n.boDem3.Length < 3) log.AppendLine("THIẾU bộ quần áo Đêm 2/3 trong Nhim.fbx (b8)");
        for (int i = 0; i < 4; i++) { n.texDa[i] = LocNhimImport.Tex("T_Nhim_Da_" + i); n.texMat[i] = LocNhimImport.Tex("T_Nhim_Mat_" + i); }
        AudioClip A(string t) => AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/LOC_House/Audio/{t}.wav");
        n.tho = A("Nhim_Tho"); n.ho = A("Nhim_Ho"); n.sotSoat = A("Chan_SotSoat"); n.vatKhan = A("Khan_Vat"); n.dienChap = A("Dien_Chap"); n.khoiTho = A("Khoi_Tho");

        // hướng mặt / đầu trong một tư thế (lấy mẫu clip ở khung 0)
        (Vector3 head, Vector3 face, Vector3 hips) Mau(string clip)
        {
            clips[clip].SampleAnimation(go, 0f);
            var h = n.head.position; var mid = (n.eyeL.position + n.eyeR.position) * 0.5f;
            return (h, (mid - h), T("Hips").position);
        }
        float Yaw(string clip, Vector3 muon)   // xoay quanh trục đứng để thân (hông → mũi chân) trùng hướng muốn
        {
            go.transform.rotation = Quaternion.identity; Mau(clip);
            var f = (T("Toes_L").position + T("Toes_R").position) * 0.5f - T("Hips").position; f.y = 0; muon.y = 0;
            return Vector3.SignedAngle(f, muon, Vector3.up);
        }
        float SanTai(float x, float z, float tu) => Physics.Raycast(new Vector3(x, tu + 1.5f, z), Vector3.down, out var hh, 3f) ? hh.point.y : tu;

        // ── giường: gốc = tâm mặt chăn có sẵn của giường (đáy giường + 0,334)
        var giuong = R("GiuongNhim_ChieuChan");
        var bg = B(giuong);
        n.neoGiuong = new Vector3(bg.center.x, bg.min.y + 0.334f, bg.center.z);
        go.transform.position = n.neoGiuong;
        // đầu về phía đầu giường (+X), rồi kiểm mặt ra phía phòng (+Z)
        go.transform.rotation = Quaternion.identity;
        { var m0 = Mau("Nam_Ngu"); var truc = m0.head - m0.hips; truc.y = 0; n.yawGiuong = Mathf.Round(Vector3.SignedAngle(truc, Vector3.right, Vector3.up) / 90f) * 90f; }   // khớp đúng khung giường (chăn mô phỏng theo khung này)
        go.transform.rotation = Quaternion.Euler(0, n.yawGiuong, 0);
        var s0 = Mau("Nam_Ngu");
        log.AppendLine($"giường {bg.center:F2} · gốc {n.neoGiuong:F3} · yaw {n.yawGiuong:F0}° · đầu {s0.head:F2} hông {s0.hips:F2} · đầu {(s0.head.x > s0.hips.x ? "về đầu giường ✔" : "NGƯỢC ✘")} · mặt {(s0.face.z > 0 ? "ra phía phòng ✔" : "VÀO TƯỜNG ✘")}");

        // ── phòng, cửa, đèn từ LocMa
        var ma = root.GetComponentInChildren<LocMa>(true);
        if (ma) { n.phong = ma.nhimPhong; n.cua = ma.nhimCua; n.denNgu = ma.nhimDen; n.choCuaNgoai = ma.nhimDung; }
        float san2 = SanTai(5.4f, 10.4f, Y2);
        n.choCuaTrong = new Vector3(3.62f, san2, 9.45f);
        n.huongCauThang = Vector3.left;
        n.choThayKhan = new Vector3(n.neoGiuong.x + 0.40f, san2, bg.max.z + 0.40f);

        // ── Good: Nhím KHÔNG rời phòng — sáng hôm sau ngồi dậy ở mép giường (phía phòng), chân thả xuống sàn.
        //    Clip Ngoi_NoiChuyen có mông ở cao 0,50 trên gốc → hạ gốc để mông chạm mặt giường.
        float matGiuong = n.neoGiuong.y;
        n.neoGhe = new Vector3(bg.center.x - 0.15f, matGiuong - 0.50f, bg.max.z - 0.16f);
        go.transform.position = n.neoGhe;
        n.yawGhe = Mathf.Round(Yaw("Ngoi_NoiChuyen", Vector3.forward) / 90f) * 90f;
        n.choBanAn = new Vector3(n.neoGhe.x + 0.2f, san2, bg.max.z + 1.35f);
        log.AppendLine($"Good (mép giường): gốc {n.neoGhe:F2} · yaw {n.yawGhe:F0}° · chỗ đứng nhìn {n.choBanAn:F2}");

        // ── góc phòng (Bad B): sát tường phải, chỗ con vong từng đứng
        float zg = 10.30f;
        float tuong = Physics.Raycast(new Vector3(6.4f, san2 + 1.0f, zg), Vector3.right, out var ht, 3f) ? ht.point.x : 7.8f;
        n.neoGoc = new Vector3(tuong - 0.45f, san2, zg); n.neoGoc2 = new Vector3(tuong - 0.31f, san2, zg);
        go.transform.position = n.neoGoc;
        n.yawGoc = Mathf.Round(Yaw("Dung_DauGap", Vector3.right) / 90f) * 90f;
        log.AppendLine($"góc: tường x={tuong:F2} · chỗ đứng {n.neoGoc:F2} → {n.neoGoc2:F2} · yaw {n.yawGoc:F0}°");

        // ── chậu nước: [E] Thay khăn + vật liệu "khô" cho cảnh Bad
        var chau = R("ChauNuoc_KhanUot");
        if (chau)
        {
            if (!chau.GetComponentInChildren<Collider>()) { var bx = chau.gameObject.AddComponent<BoxCollider>(); var bcau = B(chau); bx.center = chau.InverseTransformPoint(bcau.center); bx.size = new Vector3(0.4f, 0.12f, 0.4f); }
            chau.gameObject.AddComponent<LocNhimChau>();
            n.chau = chau.GetComponentInChildren<Renderer>();
            var cm = n.chau.sharedMaterials;   // ChauNuoc_KhanUot.glb: 0 = chậu nhựa · 1 = mặt nước · 2 = khăn
            n.matChauKho = cm.Length > 1 ? cm[0] : null; n.chauNuoc = 1;
            log.AppendLine($"chậu nước: vật liệu {string.Join(", ", n.chau.sharedMaterials.Select(m => m ? m.name : "null"))}");
        }
        else log.AppendLine("THIẾU chậu nước");

        // về tư thế đầu game: nằm trên giường
        go.transform.SetPositionAndRotation(n.neoGiuong, Quaternion.Euler(0, n.yawGiuong, 0));
        clips["Nam_Ngu"].SampleAnimation(go, 0f);
        if (n.chanVo) n.chanVo.SetActive(false); if (n.khanUot) n.khanUot.SetActive(false); if (n.dayDeo) n.dayDeo.SetActive(false);

        // kiểm xuyên: đỉnh lưới người dưới mặt đệm / trong tường
        Physics.SyncTransforms();
        int duoi = 0, tong = 0;
        foreach (var smr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (smr.name is "Chan" or "ChanVo" or "Khan_Uot" or "DayDeo") continue;
            var mesh = new Mesh(); smr.BakeMesh(mesh, true);
            foreach (var v in mesh.vertices) { var w = smr.transform.TransformPoint(v); tong++; if (w.y < n.neoGiuong.y - 0.02f) duoi++; }
        }
        log.AppendLine($"đỉnh lưới thấp hơn mặt chăn giường >2 cm: {duoi}/{tong}");
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_Nhim.txt", log.ToString());
    }
}
