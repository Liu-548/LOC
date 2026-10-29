using System.Linq;
using UnityEditor;
using UnityEngine;

// [29/10] Tay Khôi (hai tay, con của camera người chơi) + tay thầy Đồng (trong thế giới, ẩn) — model Models/Tay (LOC_Nguon/Tay/tay_xuat.py).
//  Mỗi .fbx có 7 clip một khung (tư thế co ngón trong tay.py). Ở đây lấy mẫu từng clip → lưu góc xương vào LocTay; lúc chơi không cần Animator.
//  Khung tay: gốc ở cổ tay, +Z = hướng ngón, +Y = mu bàn tay — tự canh theo xương (khỏi phụ thuộc trục khi xuất FBX).
public static partial class LocHouseBuilder
{
    static readonly string[] TayTT = { "Tha", "Nam", "Cam", "Nhon", "Xoe", "Xin", "Chap" };
    static readonly string[] TayXuong = { "Hand", "Thumb1", "Thumb2", "Thumb3", "Index1", "Index2", "Index3", "Middle1", "Middle2", "Middle3", "Ring1", "Ring2", "Ring3", "Pinky1", "Pinky2", "Pinky3" };

    static void Tay()
    {
        var log = new System.Text.StringBuilder($"TAY — LocHouseBuilder_Tay {System.DateTime.Now:dd/MM HH:mm}\n");
        var fp = root.GetComponentInChildren<LocFirstPerson>(true);
        if (!fp || !fp.cam) { System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_Tay.txt", "TAY: không thấy camera người chơi"); return; }
        var g = new GameObject("TayKhoi"); g.transform.SetParent(fp.cam, false);
        var lt = g.AddComponent<LocTay>(); lt.tenTT = TayTT;
        (lt.phai, lt.xuongPhai, lt.ttPhai) = DungTay("TayKhoi_R", g.transform, false, log);
        (lt.trai, lt.xuongTrai, lt.ttTrai) = DungTay("TayKhoi_L", g.transform, true, log);
        var th = new GameObject("TayThayDong").transform; th.SetParent(root, false);
        (lt.tayThay, lt.xuongThay, lt.ttThay) = DungTay("TayThay_R", th, false, log);
        if (lt.tayThay)   // tay áo kéo dài sau khuỷu: cánh tay thò vào từ sau trụ cổng, không lộ chỗ cắt ở khuỷu
        {
            var ong = GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(ong.GetComponent<Collider>());
            ong.name = "TayAo_Dai"; ong.transform.SetParent(lt.tayThay, false);
            ong.transform.localRotation = Quaternion.Euler(90, 0, 0); ong.transform.localPosition = new Vector3(0, 0, -0.55f);
            ong.transform.localScale = new Vector3(0.10f, 0.28f, 0.088f);
            var rd = ong.GetComponent<Renderer>(); rd.sharedMaterial = M("M_TayThay_Ao", "#7A6A4A"); rd.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // ba nén hương kẹp giữa hai lòng bàn tay (thắp hương): chân đỏ, thân nâu, đầu than hồng
        if (lt.phai)
        {
            var n = new GameObject("NhangCam").transform; n.SetParent(lt.phai, false);
            n.localPosition = new Vector3(-0.021f, 0.004f, 0.05f);   // khe giữa hai lòng bàn tay
            for (int i = 0; i < 3; i++)
            {
                var goc = Quaternion.Euler(-4f + i * 4f, 0, -6f + i * 6f);
                var gc = new GameObject("Nen_" + i).transform; gc.SetParent(n, false); gc.localRotation = goc;
                Que(gc, "Chan", 0.00f, 0.09f, 0.0022f, "#9E2A22");
                Que(gc, "Than", 0.09f, 0.33f, 0.0026f, "#6B4A32");
                var dau = GameObject.CreatePrimitive(PrimitiveType.Sphere); Object.DestroyImmediate(dau.GetComponent<Collider>());
                dau.name = "Dau"; dau.transform.SetParent(gc, false); dau.transform.localPosition = new Vector3(0, 0, 0.332f); dau.transform.localScale = Vector3.one * 0.006f;
                var m = M("M_Tay_DauNhang", "#FF7A3A"); m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", new Color(1f, 0.35f, 0.1f) * 2.2f);
                EditorUtility.SetDirty(m); dau.GetComponent<Renderer>().sharedMaterial = m; dau.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            lt.nhang = n.gameObject;
        }
        // ấm cầm tay ở cảnh rót trà (dùng model ấm của mini game dọn bàn nước)
        var am = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/LOC_House/Models/MiniGame/MG_AmTra.fbx");
        if (am && lt.phai)
        {
            var a = (GameObject)Object.Instantiate(am, lt.phai); a.name = "AmTra_Cam";
            foreach (var c in a.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
            var rs = a.GetComponentsInChildren<Renderer>(); var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
            a.transform.localPosition += new Vector3(0, -0.06f, 0.07f) - lt.phai.InverseTransformPoint(b.center);   // tâm ấm ngay dưới lòng bàn tay (bàn tay úp cầm quai)
            foreach (var r in rs) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lt.amTra = a;
        }
        else log.AppendLine("TAY: thiếu Models/MiniGame/MG_AmTra.fbx (cảnh rót trà chỉ có tay)");
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_Tay.txt", log.ToString());
    }

    static void Que(Transform cha, string ten, float z0, float z1, float r, string hex)
    {
        var q = GameObject.CreatePrimitive(PrimitiveType.Cylinder); Object.DestroyImmediate(q.GetComponent<Collider>());
        q.name = ten; q.transform.SetParent(cha, false);
        q.transform.localRotation = Quaternion.Euler(90, 0, 0); q.transform.localPosition = new Vector3(0, 0, (z0 + z1) / 2);
        q.transform.localScale = new Vector3(r * 2, (z1 - z0) / 2, r * 2);
        var rd = q.GetComponent<Renderer>(); rd.sharedMaterial = M("M_Tay_Nhang_" + ten, hex); rd.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    static (Transform, Transform[], Quaternion[]) DungTay(string ten, Transform cha, bool laTrai, System.Text.StringBuilder log)
    {
        string path = "Assets/LOC_House/Models/Tay/" + ten + ".fbx";
        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (!fbx) { log.AppendLine("TAY: THIẾU " + path); return (null, new Transform[0], new Quaternion[0]); }
        var giu = new GameObject(laTrai ? "TayTrai" : cha.name == "TayKhoi" ? "TayPhai" : "TayThay").transform; giu.SetParent(cha, false);
        var m = (GameObject)PrefabUtility.InstantiatePrefab(fbx, giu);
        PrefabUtility.UnpackPrefabInstance(m, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        var anim = m.GetComponent<Animator>(); if (anim) Object.DestroyImmediate(anim);
        Transform B(string n) => m.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == n);
        var xuong = TayXuong.Select(B).ToArray();
        if (xuong.Any(x => !x) || !B("Arm")) { log.AppendLine("TAY: " + ten + " thiếu xương"); return (giu, xuong, new Quaternion[0]); }

        // lấy mẫu tư thế (clip "TayKhoi_R|Cam"…)
        var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")).ToArray();
        var tt = new Quaternion[TayTT.Length * xuong.Length];
        for (int p = 0; p < TayTT.Length; p++)
        {
            var c = clips.FirstOrDefault(x => x.name.EndsWith("|" + TayTT[p]) || x.name == TayTT[p]);
            if (!c) { log.AppendLine($"TAY: {ten} thiếu clip {TayTT[p]}"); c = clips.FirstOrDefault(); }
            if (c) c.SampleAnimation(m, 0f);
            for (int i = 0; i < xuong.Length; i++) tt[p * xuong.Length + i] = xuong[i].localRotation;
        }
        for (int i = 0; i < xuong.Length; i++) xuong[i].localRotation = tt[i];   // về tư thế "Tha"

        // canh khung: khuỷu→cổ tay = +Z, mu bàn tay = +Y (ngón trỏ→ngón út chỉ sang phải với tay phải, sang trái với tay trái)
        var doc = giu.InverseTransformDirection(B("Hand").position - B("Arm").position).normalized;
        var ngang = giu.InverseTransformDirection(B("Pinky1").position - B("Index1").position).normalized;
        if (laTrai) ngang = -ngang;
        var q = Quaternion.LookRotation(doc, Vector3.Cross(doc, ngang));
        m.transform.localRotation = Quaternion.Inverse(q) * m.transform.localRotation;
        m.transform.localPosition -= giu.InverseTransformPoint(B("Hand").position);

        foreach (var r in m.GetComponentsInChildren<Renderer>(true))
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = true;
            if (r is SkinnedMeshRenderer s) s.updateWhenOffscreen = true;
            var phan = r.name.Substring(r.name.IndexOf('_') + 1);   // Khoi_Da · Khoi_Mong · Khoi_Ao · Thay_Nhan · Thay_Da_Nhan
            bool thay = ten.StartsWith("TayThay");
            string hex = phan switch
            {
                "Da" => thay ? "#9A6A4E" : "#C49274", "Mong" => thay ? "#C9A486" : "#D9AE9A", "Ao" => thay ? "#7A6A4A" : "#4C5560",
                "Nhan" => "#C9A64A", "Da_Nhan" => "#2E7A55", _ => "#C49274",
            };
            var mt = M("M_" + ten.Replace("_R", "").Replace("_L", "") + "_" + phan, hex);
            mt.SetFloat("_Smoothness", phan is "Nhan" or "Da_Nhan" or "Mong" ? 0.55f : phan == "Ao" ? 0.1f : 0.25f);
            EditorUtility.SetDirty(mt); r.sharedMaterial = mt;
        }
        foreach (var c in m.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
        log.AppendLine($"TAY: {ten} — {clips.Length} clip, {xuong.Length} xương");
        return (giu, xuong, tt);
    }
}
