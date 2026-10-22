using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// [5/10] Nạp model Nhím (Models/Nhim/Nhim.fbx — dựng bằng LOC_Nguon/Nhim/*.py): Generic, blendshape, 11 clip đổi tên + đặt lặp,
//        texture, vật liệu URP, Animator Controller AC_Nhim. Menu LOC → Nạp lại Nhím.
public class LocNhimImport : AssetPostprocessor
{
    public const string DIR = "Assets/LOC_House/Models/Nhim/";
    public static readonly string[] CLIP = { "Nam_Ngu", "Nam_Thuc", "Nam_TroMinh", "Nam_NgocDau", "Nam_NgheDau", "Nam_ThayKhan", "Nam_So", "Nam_NuaTinh", "Ngoi_NoiChuyen", "Ngoi_DauGap", "Dung_DauGap" };
    static readonly string[] LAP = { "Nam_Ngu", "Nam_Thuc", "Ngoi_NoiChuyen" };

    bool LaNhim => assetPath.Replace('\\', '/').StartsWith(DIR);

    void OnPreprocessModel()
    {
        if (!LaNhim) return;
        var mi = (ModelImporter)assetImporter;
        mi.animationType = ModelImporterAnimationType.Generic;
        mi.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        mi.importBlendShapes = true; mi.importAnimation = true;
        mi.materialImportMode = ModelImporterMaterialImportMode.None;
        mi.addCollider = false; mi.isReadable = false;
        mi.motionNodeName = "";
        mi.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
    }

    void OnPreprocessAnimation()
    {
        if (!LaNhim) return;
        var mi = (ModelImporter)assetImporter;
        var src = mi.defaultClipAnimations;
        var outp = new System.Collections.Generic.List<ModelImporterClipAnimation>();
        foreach (var c in src)
        {
            var ten = c.takeName.Split('|').Last();
            if (!CLIP.Contains(ten) || outp.Any(x => x.name == ten)) continue;
            c.name = ten; c.loopTime = LAP.Contains(ten); c.loopPose = false;
            c.keepOriginalPositionY = c.keepOriginalPositionXZ = c.keepOriginalOrientation = true;
            outp.Add(c);
        }
        if (outp.Count > 0) mi.clipAnimations = outp.ToArray();
    }

    void OnPreprocessTexture()
    {
        if (!LaNhim) return;
        var ti = (TextureImporter)assetImporter;
        ti.sRGBTexture = true; ti.mipmapEnabled = true; ti.maxTextureSize = 1024;
        ti.alphaIsTransparency = assetPath.Contains("Toc");
        ti.wrapMode = assetPath.Contains("_Ao") || assetPath.Contains("T_Chan") ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
    }

    [MenuItem("LOC/Nạp lại Nhím")]
    public static void NapLai()
    {
        AssetDatabase.ImportAsset(DIR + "Nhim.fbx", ImportAssetOptions.ForceUpdate);
        foreach (var g in AssetDatabase.FindAssets("t:Texture2D", new[] { DIR + "Textures" }))
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(g), ImportAssetOptions.ForceUpdate);
        VatLieu(); Controller();
        AssetDatabase.SaveAssets();
    }

    [MenuItem("LOC/Tự kiểm Nhím (Play + chụp)")]
    static void TuKiem()
    {
        try { System.Type.GetType("UnityEditor.LogEntries,UnityEditor").GetMethod("SetConsoleFlag").Invoke(null, new object[] { 4, false }); } catch { }
        EditorPrefs.SetBool("LOC_TuKiemNhim", true);
        // Unity không có tiêu điểm thì Play đứng hình (runInBackground = false) → bật tạm trong lúc kiểm, tự trả lại khi thoát Play
        if (!EditorPrefs.HasKey("LOC_RIB")) EditorPrefs.SetBool("LOC_RIB", PlayerSettings.runInBackground);
        PlayerSettings.runInBackground = true;
        EditorApplication.isPlaying = true;
    }
    [InitializeOnLoadMethod]
    static void TraLaiRIB()
    {
        EditorApplication.playModeStateChanged += s =>
        {
            if (s != PlayModeStateChange.EnteredEditMode || !EditorPrefs.HasKey("LOC_RIB")) return;
            PlayerSettings.runInBackground = EditorPrefs.GetBool("LOC_RIB"); EditorPrefs.DeleteKey("LOC_RIB");
        };
    }

    public static Texture2D Tex(string ten) => AssetDatabase.LoadAssetAtPath<Texture2D>(DIR + "Textures/" + ten + ".png");

    static Material Mat(string ten, string tex, float smooth, bool haiMat = false, bool cat = false, Color? mau = null)
    {
        System.IO.Directory.CreateDirectory(DIR + "Materials");
        string p = DIR + "Materials/" + ten + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(p);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, p); }
        m.shader = Shader.Find("Universal Render Pipeline/Lit");
        m.SetTexture("_BaseMap", tex == null ? null : Tex(tex)); m.SetColor("_BaseColor", mau ?? Color.white);
        m.SetFloat("_Smoothness", smooth); m.SetFloat("_Metallic", 0); m.SetFloat("_SpecularHighlights", 1);
        m.SetFloat("_Cull", haiMat ? 0 : 2); m.doubleSidedGI = haiMat;
        if (cat) { m.SetFloat("_AlphaClip", 1); m.SetFloat("_Cutoff", 0.5f); m.EnableKeyword("_ALPHATEST_ON"); m.renderQueue = 2450; }
        else { m.SetFloat("_AlphaClip", 0); m.DisableKeyword("_ALPHATEST_ON"); m.renderQueue = -1; }
        EditorUtility.SetDirty(m); return m;
    }

    public static System.Collections.Generic.Dictionary<string, Material> VatLieu()
    {
        var da = Mat("M_Nhim_Da", "T_Nhim_Da_1", 0.38f);
        var mat = Mat("M_Nhim_Mat", "T_Nhim_Mat_1", 0.85f);
        var rang = Mat("M_Nhim_Rang", null, 0.4f, mau: new Color(0.30f, 0.13f, 0.12f));   // [22/10] lòng miệng tối (trước là màu răng → hé miệng thấy trắng toát)
        var toc = Mat("M_Nhim_Toc", "T_Nhim_Toc", 0.18f, true, true);
        var ao = Mat("M_Nhim_Ao", "T_Nhim_Ao", 0.05f, true);
        var bot = Mat("M_Nhim_Bot", "T_Nhim_Bot", 0.1f, true);
        var chan = Mat("M_Chan", "T_Chan", 0.05f, true);
        var khan = Mat("M_Khan", "T_Khan", 0.2f, true);
        var day = Mat("M_DayDeo", "T_DayDeo", 0.05f, true);
        return new System.Collections.Generic.Dictionary<string, Material>
        {
            ["Body"] = da, ["Eye_L"] = mat, ["Eye_R"] = mat, ["MouthIn"] = rang, ["Toc"] = toc, ["Ao"] = ao, ["Quan"] = ao, ["Bot"] = bot,
            ["Chan"] = chan, ["ChanVo"] = chan, ["Khan_Kho"] = khan, ["Khan_Uot"] = khan, ["DayDeo"] = day,
        };
    }

    public static AnimatorController Controller()
    {
        string p = DIR + "AC_Nhim.controller";
        AssetDatabase.DeleteAsset(p);
        var ac = AnimatorController.CreateAnimatorControllerAtPath(p);
        var sm = ac.layers[0].stateMachine;
        var clips = AssetDatabase.LoadAllAssetsAtPath(DIR + "Nhim.fbx").OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview")).ToArray();
        foreach (var ten in CLIP)
        {
            var c = clips.FirstOrDefault(x => x.name == ten);
            if (!c) { Debug.LogWarning("[Nhím] thiếu clip " + ten); continue; }
            var st = sm.AddState(ten); st.motion = c;
            if (ten == "Nam_Ngu") sm.defaultState = st;
        }
        EditorUtility.SetDirty(ac); AssetDatabase.SaveAssets();
        return ac;
    }
}
