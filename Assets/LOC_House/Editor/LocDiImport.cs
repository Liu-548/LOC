using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// [21/10] Nạp model NGƯỜI DÌ (Models/Di/Di.fbx — dựng bằng LOC_Nguon/Di/d1…d4.py, low-poly ~1,6k tam giác, kiểu PS1):
//         Generic, 3 clip (Dung_Cho, Dung_Noi, Di_Bo — đều lặp), texture lọc Point, vật liệu URP, Animator Controller AC_Di.
public class LocDiImport : AssetPostprocessor
{
    public const string DIR = "Assets/LOC_House/Models/Di/";
    public static readonly string[] CLIP = { "Dung_Cho", "Dung_Noi", "Di_Bo" };
    bool LaDi => assetPath.Replace('\\', '/').StartsWith(DIR);

    void OnPreprocessModel()
    {
        if (!LaDi) return;
        var mi = (ModelImporter)assetImporter;
        mi.animationType = ModelImporterAnimationType.Generic;
        mi.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        mi.importBlendShapes = false; mi.importAnimation = true;
        mi.materialImportMode = ModelImporterMaterialImportMode.None;
        mi.addCollider = false; mi.isReadable = false; mi.motionNodeName = "";
    }

    void OnPreprocessAnimation()
    {
        if (!LaDi) return;
        var mi = (ModelImporter)assetImporter;
        var outp = new System.Collections.Generic.List<ModelImporterClipAnimation>();
        foreach (var c in mi.defaultClipAnimations)
        {
            var ten = c.takeName.Split('|').Last();
            if (!CLIP.Contains(ten) || outp.Any(x => x.name == ten)) continue;
            c.name = ten; c.loopTime = true; c.loopPose = false;
            c.keepOriginalPositionY = c.keepOriginalPositionXZ = c.keepOriginalOrientation = true;
            outp.Add(c);
        }
        if (outp.Count > 0) mi.clipAnimations = outp.ToArray();
    }

    void OnPreprocessTexture()
    {
        if (!LaDi) return;
        var ti = (TextureImporter)assetImporter;
        ti.sRGBTexture = true; ti.mipmapEnabled = false; ti.filterMode = FilterMode.Point; ti.maxTextureSize = 512;
        ti.wrapMode = assetPath.Contains("T_Di_Da") ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
    }

    public static Texture2D Tex(string ten) => AssetDatabase.LoadAssetAtPath<Texture2D>(DIR + "Textures/" + ten + ".png");

    static Material Mat(string ten, string tex)
    {
        System.IO.Directory.CreateDirectory(DIR + "Materials");
        string p = DIR + "Materials/" + ten + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(p);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, p); }
        m.SetTexture("_BaseMap", Tex(tex)); m.SetColor("_BaseColor", Color.white);
        m.SetFloat("_Smoothness", 0.08f); m.SetFloat("_Metallic", 0); m.SetFloat("_Cull", 0);   // hai mặt: vạt áo, ống quần, đuôi khăn nhìn được mặt trong
        EditorUtility.SetDirty(m); return m;
    }

    public static System.Collections.Generic.Dictionary<string, Material> VatLieu() => new()
    {
        ["Body"] = Mat("M_Di_Da", "T_Di_Da"), ["Ao"] = Mat("M_Di_Ao", "T_Di_Ao"), ["Quan"] = Mat("M_Di_Quan", "T_Di_Quan"),
        ["Toc"] = Mat("M_Di_Toc", "T_Di_Toc"), ["KhanTang"] = Mat("M_Di_Khan", "T_Di_Khan"), ["Dep"] = Mat("M_Di_Dep", "T_Di_Dep"),
    };

    public static AnimatorController Controller()
    {
        string p = DIR + "AC_Di.controller";
        AssetDatabase.DeleteAsset(p);
        var ac = AnimatorController.CreateAnimatorControllerAtPath(p);
        var sm = ac.layers[0].stateMachine;
        var clips = AssetDatabase.LoadAllAssetsAtPath(DIR + "Di.fbx").OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")).ToArray();
        foreach (var ten in CLIP)
        {
            var c = clips.FirstOrDefault(x => x.name == ten);
            if (!c) { Debug.LogWarning("[Dì] thiếu clip " + ten); continue; }
            var st = sm.AddState(ten); st.motion = c;
            if (ten == "Dung_Cho") sm.defaultState = st;
        }
        EditorUtility.SetDirty(ac); AssetDatabase.SaveAssets();
        return ac;
    }

    [MenuItem("LOC/Nạp lại người dì")]
    public static void NapLai()
    {
        AssetDatabase.ImportAsset(DIR + "Di.fbx", ImportAssetOptions.ForceUpdate);
        foreach (var g in AssetDatabase.FindAssets("t:Texture2D", new[] { DIR + "Textures" }))
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(g), ImportAssetOptions.ForceUpdate);
        VatLieu(); Controller(); AssetDatabase.SaveAssets();
    }
}
