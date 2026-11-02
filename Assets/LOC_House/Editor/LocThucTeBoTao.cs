#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

// [02/11] Tự tạo Resources/LocThucTeBo.asset (tham chiếu tới D2_*_{A,N,M}.png) khi mở project; thiếu mới tạo.
[InitializeOnLoad]
public static class LocThucTeBoTao
{
    const string DIR = "Assets/LOC_House/Resources", FILE = DIR + "/LocThucTeBo.asset", TEX = "Assets/LOC_House/Textures/PBR";
    static readonly string[] LOP = { "xm", "gach", "go", "vai", "da", "son" };

    static LocThucTeBoTao() { EditorApplication.delayCall += () => { if (!File.Exists(FILE)) Tao(); }; }

    [MenuItem("LOC/Tạo bộ PBR thực tế")]
    public static void Tao()
    {
        if (!AssetDatabase.IsValidFolder(DIR)) AssetDatabase.CreateFolder("Assets/LOC_House", "Resources");   // CreateAsset cần thư mục đã có trong AssetDatabase
        var bo = ScriptableObject.CreateInstance<LocThucTeBo>();
        var ds = new System.Collections.Generic.List<string>();
        var A = new System.Collections.Generic.List<Texture2D>(); var N = new System.Collections.Generic.List<Texture2D>(); var M = new System.Collections.Generic.List<Texture2D>();
        foreach (var l in LOP)
        {
            var a = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TEX}/D2_{l}_A.png"); if (!a) continue;
            ds.Add(l); A.Add(a);
            N.Add(AssetDatabase.LoadAssetAtPath<Texture2D>($"{TEX}/D2_{l}_N.png"));
            M.Add(AssetDatabase.LoadAssetAtPath<Texture2D>($"{TEX}/D2_{l}_M.png"));
        }
        bo.lop = ds.ToArray(); bo.a = A.ToArray(); bo.n = N.ToArray(); bo.m = M.ToArray();
        AssetDatabase.CreateAsset(bo, FILE); AssetDatabase.SaveAssets();
        Debug.Log($"[LocThucTe] Đã tạo {FILE} với {ds.Count} lớp PBR.");
    }
}
#endif
