using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// [29/10] Kê nguồn asset (file model/prefab) của mọi LocProp, gom theo nhóm tầng → BaoCao_NguonTheoTang.txt
public static class LocKeNguon
{
    [MenuItem("LOC/Kê nguồn asset theo tầng")]
    public static void Ke()
    {
        var hidden = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(t => (t.name is "Dem2" or "Dem3") && !t.gameObject.activeSelf).ToList();
        foreach (var h in hidden) h.gameObject.SetActive(true);
        var rows = new System.Collections.Generic.List<string>();
        foreach (var p in Object.FindObjectsByType<LocProp>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (p.GetComponentsInParent<LocProp>(true).Length > 1) continue;
            var path = p.transform.GetHierarchyPath();
            var goc = path.Split('/')[0];
            var src = PrefabUtility.GetCorrespondingObjectFromOriginalSource(p.gameObject);
            string nguon = src ? AssetDatabase.GetAssetPath(src) : "";
            var meshes = p.GetComponentsInChildren<MeshFilter>(true).Select(m => m.sharedMesh).Concat(p.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(s => s.sharedMesh))
                .Where(m => m).Select(m => AssetDatabase.GetAssetPath(m)).Select(a => a == "" ? "(lưới dựng bằng code)" : a).Distinct().ToList();
            int tris = p.GetComponentsInChildren<MeshFilter>(true).Where(m => m.sharedMesh).Sum(m => (int)(m.sharedMesh.triangles.Length / 3));
            rows.Add($"{goc}\t{path}\t{nguon}\t{string.Join(" ; ", meshes)}\t{tris}");
        }
        foreach (var h in hidden) h.gameObject.SetActive(false);
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_NguonTheoTang.txt", "gốc\tđường dẫn\tprefab nguồn\tfile lưới\tsố tam giác\n" + string.Join("\n", rows.OrderBy(r => r)));
    }

    // [29/10] mọi lưới trong scene (kể cả đang ẩn): đường dẫn \t tên lưới (bỏ hậu tố bản sao) → BaoCao_LuoiScene.txt
    [MenuItem("LOC/Kê lưới trong scene")]
    public static void KeLuoi()
    {
        var sb = new StringBuilder();
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Mesh m = r is SkinnedMeshRenderer s ? s.sharedMesh : r.GetComponent<MeshFilter>()?.sharedMesh;
            if (!m) continue;
            var ten = m.name; foreach (var h in new[] { "_uv", "_tg", "(Clone)", "_V35", "_nhay", "_tumo", "_boGuong" }) ten = ten.Replace(h, "");
            sb.AppendLine($"{r.transform.GetHierarchyPath()}\t{ten}\t{AssetDatabase.GetAssetPath(m)}");
        }
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_LuoiScene.txt", sb.ToString());
    }

    // [29/10] LOC_VL.txt: mỗi dòng một tên object → in renderer con + tên lưới + vật liệu + shader + bounds → LOC_VL_KetQua.txt
    [MenuItem("LOC/In vật liệu theo tên")]
    public static void InVL()
    {
        if (!System.IO.File.Exists("LOC_VL.txt")) return;
        var sb = new StringBuilder();
        foreach (var ten in System.IO.File.ReadAllLines("LOC_VL.txt"))
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(x => x.name == ten.Trim()))
                foreach (var r in t.GetComponentsInChildren<Renderer>(true))
                {
                    var mf = r.GetComponent<MeshFilter>();
                    sb.AppendLine($"{r.transform.GetHierarchyPath()} | {(mf && mf.sharedMesh ? mf.sharedMesh.name : "-")} | b={r.bounds.center:F3} {r.bounds.size:F3} | rot={r.transform.rotation.eulerAngles:F0} | " + string.Join(", ", r.sharedMaterials.Select(m => m ? $"{m.name}[{m.shader.name}]" : "null")));
                }
        System.IO.File.WriteAllText("LOC_VL_KetQua.txt", sb.ToString());
    }
    static string GetHierarchyPath(this Transform t) => t.parent ? t.parent.GetHierarchyPath() + "/" + t.name : t.name;
}
