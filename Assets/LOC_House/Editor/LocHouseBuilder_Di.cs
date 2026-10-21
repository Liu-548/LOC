using System.Linq;
using UnityEditor;
using UnityEngine;

// [21/10] Đặt NGƯỜI DÌ (người họ hàng trông nhà, chỉ Đêm 1) vào phòng khách — nhóm Tang_1/PhongKhach/Dem1 nên Đêm 2–3 tự ẩn.
//  Đứng cạnh thùng phúng điếu, phía trước–trái bàn thờ vong; nói xong đi ra cửa chính → sân → cổng → đường rồi biến mất.
public static partial class LocHouseBuilder
{
    static void Di()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LocDiImport.DIR + "Di.fbx");
        if (!prefab) { Debug.LogWarning("[LOC] thiếu Models/Di/Di.fbx — bỏ qua người dì"); return; }
        var mats = LocDiImport.VatLieu();
        var ac = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(LocDiImport.DIR + "AC_Di.controller") ?? LocDiImport.Controller();
        var clips = AssetDatabase.LoadAllAssetsAtPath(LocDiImport.DIR + "Di.fbx").OfType<AnimationClip>().ToDictionary(c => c.name, c => c);

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
        go.name = "NguoiDi"; go.transform.SetParent(G("Tang_1/PhongKhach/Dem1"), true);
        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            if (mats.TryGetValue(r.name, out var m)) r.sharedMaterial = m; else Debug.LogWarning("[Dì] không có vật liệu cho " + r.name);
            if (r is SkinnedMeshRenderer s) s.updateWhenOffscreen = true;
        }
        var an = go.GetComponent<Animator>() ?? go.AddComponent<Animator>();
        an.runtimeAnimatorController = ac; an.applyRootMotion = false; an.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        Transform T(string n) => go.GetComponentsInChildren<Transform>(true).FirstOrDefault(x => x.name == n);
        // hướng mặt khi rotation = identity: hông → mũi chân ở tư thế đứng
        go.transform.rotation = Quaternion.identity;
        clips["Dung_Cho"].SampleAnimation(go, 0f);
        var f0 = (T("Toes_L").position + T("Toes_R").position) * 0.5f - T("Hips").position; f0.y = 0;

        float San(float x, float z) => Physics.Raycast(new Vector3(x, 1.5f, z), Vector3.down, out var h, 3f) ? h.point.y : 0f;
        var di = go.AddComponent<LocDi>();
        di.anim = an; di.huong0 = f0.normalized;
        var cho = new Vector3(2.15f, 0f, 1.15f); cho.y = San(cho.x, cho.z);
        go.transform.position = cho;
        go.transform.rotation = Quaternion.Euler(0, Vector3.SignedAngle(f0, new Vector3(3.8f, 0, 0.9f) - new Vector3(cho.x, 0, cho.z), Vector3.up), 0);   // nhìn ra chỗ người chơi sẽ đứng thắp hương
        di.duongVe = new[] { new Vector3(3.2f, 0, 0.35f), new Vector3(3.8f, 0, -0.9f), new Vector3(3.8f, 0, -5.25f), new Vector3(3.8f, 0, -7.8f), new Vector3(-8f, 0, -8.2f) }
            .Select(p => new Vector3(p.x, San(p.x, p.z), p.z)).ToArray();
        di.diemCong = 2;   // tới điểm này thì mở cổng, qua khỏi thì đóng lại
        var col = go.AddComponent<CapsuleCollider>(); col.center = new Vector3(0, 0.8f, 0); col.height = 1.6f; col.radius = 0.22f;   // người chơi không đi xuyên
        Debug.Log($"[LOC] người dì đặt tại {cho:F2}, hướng gốc {f0.normalized:F2}");
    }
}
