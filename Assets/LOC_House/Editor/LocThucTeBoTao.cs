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

    static LocThucTeBoTao() { EditorApplication.delayCall += () => { if (!File.Exists(FILE)) Tao(); if (!File.Exists(BT + "/BienThe_VetBan.mat")) TaoBienThe(); }; }

    // [03/11] vật liệu "giữ biến thể shader": vật liệu Thực tế tạo lúc chạy bằng code, bản build chỉ giữ những tổ hợp keyword
    //  có vật liệu thật dùng → không có thì vết bẩn / chi tiết grunge / normal map có thể mất trong bản .exe.
    //  Các .mat này nằm trong Resources nên luôn được đóng gói; LocVetBan dùng thẳng BienThe_VetBan.
    const string BT = "Assets/LOC_House/Resources/LOC_ThucTe";
    [MenuItem("LOC/Tạo vật liệu giữ biến thể Thực tế")]
    public static void TaoBienThe()
    {
        if (!AssetDatabase.IsValidFolder(BT)) { if (!AssetDatabase.IsValidFolder(DIR)) AssetDatabase.CreateFolder("Assets/LOC_House", "Resources"); AssetDatabase.CreateFolder(DIR, "LOC_ThucTe"); }
        var lit = Shader.Find("Universal Render Pipeline/Lit"); var hat = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        var grunge = AssetDatabase.LoadAssetAtPath<Texture2D>(BT + "/ChiTiet_Grunge.png");
        var xmN = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX + "/D2_xm_N.png"); var xmM = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX + "/D2_xm_M.png");
        if (lit)
        {
            string[][] bo = { new[] { "_DETAIL_MULX2" }, new[] { "_DETAIL_MULX2", "_NORMALMAP", "_OCCLUSIONMAP" }, new[] { "_DETAIL_MULX2", "_EMISSION" },
                              new[] { "_DETAIL_MULX2", "_ALPHATEST_ON" }, new[] { "_DETAIL_MULX2", "_NORMALMAP" } };
            for (int i = 0; i < bo.Length; i++)
            {
                var m = new Material(lit);
                if (grunge) m.SetTexture("_DetailAlbedoMap", grunge);
                foreach (var k in bo[i]) m.EnableKeyword(k);
                if (System.Array.IndexOf(bo[i], "_NORMALMAP") >= 0 && xmN) m.SetTexture("_BumpMap", xmN);
                if (System.Array.IndexOf(bo[i], "_OCCLUSIONMAP") >= 0 && xmM) m.SetTexture("_OcclusionMap", xmM);
                if (System.Array.IndexOf(bo[i], "_ALPHATEST_ON") >= 0) m.SetFloat("_AlphaClip", 1);
                AssetDatabase.CreateAsset(m, $"{BT}/BienThe_Lit_{i}.mat");
            }
        }
        var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(BT + "/VetBan_Atlas.png");
        if (hat)
        {
            var v = new Material(hat) { name = "BienThe_VetBan" };
            if (atlas) { v.SetTexture("_BaseMap", atlas); v.mainTexture = atlas; }
            v.SetColor("_BaseColor", Color.white);
            v.SetFloat("_Surface", 1); v.SetFloat("_Blend", 3); v.SetFloat("_ZWrite", 0); v.SetFloat("_Cull", 0);
            v.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.DstColor); v.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
            v.SetOverrideTag("RenderType", "Transparent"); v.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); v.EnableKeyword("_ALPHAMODULATE_ON");
            v.renderQueue = 2990;
            AssetDatabase.CreateAsset(v, BT + "/BienThe_VetBan.mat");
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[LocThucTe] Đã tạo vật liệu giữ biến thể shader trong " + BT);
    }

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
