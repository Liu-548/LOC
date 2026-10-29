// [6/10] Hình ảnh phương án B (đã duyệt): "cũ kỹ, u tối" nhưng sạch — không vạch quét, không dải sáng trôi, viền màu rất nhẹ.
// [6/10 tối] LÀM RÕ: người chơi thấy mờ, khó nhìn đồ → tắt nhoè xa (Depth of Field), bớt méo rìa, giảm bloom/vignette/hạt,
//            tăng nhẹ độ sáng, nâng vùng tối và vùng giữa. Vẫn giữ tông ấm ở sáng, teal nhẹ ở tối, quầng đèn đỏ cam.
// [29/10] "Đầy đủ" vẫn xám đục, nhìn qua cửa sổ xe buýt gần như không thấy gì → bỏ nâng vùng đen (lift/gamma w = 0: đen là đen),
//            tăng tương phản + bão hoà nhẹ, bớt quầng/viền. Tông ấm–teal giữ ở màu của lift/gain, không ở độ nâng.
// Menu: LOC → Áp hình ảnh phương án B  (sửa thẳng Assets/LOC_House/Materials/LOC_HinhCu.asset, không cần dựng lại nhà).
// LocHouseBuilder_Ma.HinhCu() cũng gọi ApB() nên dựng lại nhà vẫn giữ hình này.
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class LocHinhAnh
{
    public const string DuongDan = "Assets/LOC_House/Materials/LOC_HinhCu.asset";

    [MenuItem("LOC/Áp hình ảnh phương án B")]
    public static void ApB()
    {
        var p = AssetDatabase.LoadAssetAtPath<VolumeProfile>(DuongDan);
        if (!p) { Debug.LogError("[LOC] Không thấy " + DuongDan); return; }
        ApB(p);
        EditorUtility.SetDirty(p);
        AssetDatabase.SaveAssets();
        Debug.Log("[LOC] Đã áp hình ảnh phương án B (bản rõ) lên " + DuongDan);
    }

    public static void ApB(VolumeProfile p)
    {
        var ca = Lay<ColorAdjustments>(p);
        ca.saturation.Override(-4f); ca.contrast.Override(24f); ca.postExposure.Override(0.35f);   // sáng hơn, tương phản rõ hơn
        ca.colorFilter.Override(new Color(1f, 0.96f, 0.90f));                                      // ấm nhẹ
        var fg = Lay<FilmGrain>(p); fg.type.Override(FilmGrainLookup.Medium4); fg.intensity.Override(0.14f); fg.response.Override(0.8f);
        var vg = Lay<Vignette>(p); vg.intensity.Override(0.24f); vg.smoothness.Override(0.7f);
        var cab = Lay<ChromaticAberration>(p); cab.intensity.Override(0.03f);
        var bl = Lay<Bloom>(p);
        bl.threshold.Override(1.0f); bl.intensity.Override(0.25f); bl.scatter.Override(0.65f);      // bớt quầng mờ phủ hình
        bl.tint.Override(new Color(1f, 0.66f, 0.46f));                                             // quầng đèn đỏ cam nhẹ
        var ld = Lay<LensDistortion>(p); ld.intensity.Override(-0.04f);                            // bớt méo/mờ ở rìa
        var dof = Lay<DepthOfField>(p); dof.active = false;                                        // TẮT nhoè xa: đồ trong nhà luôn nét
        var lg = Lay<LiftGammaGain>(p);                                                            // nâng vùng tối + vùng giữa cho nhìn rõ đồ
        lg.lift.Override(new Vector4(0.97f, 1.00f, 1.04f, 0f));
        lg.gamma.Override(new Vector4(1.00f, 1.00f, 1.00f, 0.06f));
        lg.gain.Override(new Vector4(1.04f, 1.00f, 0.95f, 0f));
    }

    static T Lay<T>(VolumeProfile p) where T : VolumeComponent
    {
        if (p.TryGet(out T c)) return c;
        c = p.Add<T>(true); c.name = typeof(T).Name; AssetDatabase.AddObjectToAsset(c, p);
        return c;
    }
}
