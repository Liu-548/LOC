using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// [02/11] BẢN THỬ "THỰC TẾ" — chỉ áp cho PHÒNG KHÁCH, bật/tắt trong Cài đặt › "Hình ảnh thực tế (thử)".
//  1) Hậu kỳ điện ảnh thay lớp VHS: tonemapping ACES, tông lạnh u ám, bloom nhẹ, vignette (Volume tạo bằng code, không sửa scene).
//  2) Vật liệu trong phòng khách: URP Simple Lit → URP Lit (PBR: có phản xạ, bóng loáng theo độ nhám). Giữ nguyên màu/texture.
//  3) Một Reflection Probe realtime (render 1 lần) để sàn/gỗ/kính phản chiếu căn phòng, thay vì phản chiếu skybox.
// Tắt đi là khôi phục y nguyên vật liệu gốc. Không đụng scene/asset → không tăng dung lượng, không ảnh hưởng đèn công tắc/mất điện.
[DefaultExecutionOrder(200)]
public class LocThucTe : MonoBehaviour
{
    public static LocThucTe I;
    public static bool Bat => PlayerPrefs.GetInt("LOC_CD_ThucTe", 0) == 1;

    // Hộp bao phòng khách (toạ độ thế giới) — chỉnh ở đây nếu muốn mở rộng/thu hẹp vùng thử
    static readonly Bounds VUNG = new Bounds(new Vector3(3.8f, 1.6f, 3.6f), new Vector3(7.6f, 3.6f, 7.2f));

    static VolumeProfile ho; Volume vol;
    GameObject probeGo;
    readonly Dictionary<Renderer, Material[]> goc = new();
    readonly Dictionary<Material, Material> cache = new();
    bool daQuet, dangBat;
    Volume hinhCu;

    public static void Ap()
    {
        if (!I) { I = new GameObject("LocThucTe").AddComponent<LocThucTe>(); }
        I.Dat(Bat);
    }

    void Awake() { I = this; }

    void Dat(bool b)
    {
        dangBat = b;
        if (b) { TaoHauKy(); if (vol) vol.enabled = true; StartCoroutine(BatVatLieu()); }
        else { if (vol) vol.enabled = false; KhoiPhuc(); if (probeGo) probeGo.SetActive(false); }
    }

    void Update()
    {
        if (!dangBat) return;
        // giữ lớp VHS tắt trong lúc bật (LocMenu.ApDung đặt lại mỗi khi chỉnh cài đặt khác)
        LocFirstPerson.vhs = 0f;
        if (!hinhCu) { var g = GameObject.Find("LocHinhCu"); if (g) hinhCu = g.GetComponent<Volume>(); }
        if (hinhCu) hinhCu.weight = 0f;
    }

    void TaoHauKy()
    {
        if (!ho)
        {
            ho = ScriptableObject.CreateInstance<VolumeProfile>();
            var tm = ho.Add<Tonemapping>(true); tm.mode.Override(TonemappingMode.ACES);
            var bl = ho.Add<Bloom>(true);
            bl.threshold.Override(1.0f); bl.intensity.Override(0.22f); bl.scatter.Override(0.65f);
            var ca = ho.Add<ColorAdjustments>(true);
            ca.contrast.Override(10f); ca.saturation.Override(-14f); ca.postExposure.Override(0.1f);
            var wb = ho.Add<WhiteBalance>(true); wb.temperature.Override(-9f); wb.tint.Override(4f);
            var sm = ho.Add<ShadowsMidtonesHighlights>(true);   // bóng ngả xanh lạnh, vùng sáng giữ ấm của đèn
            sm.shadows.Override(new Vector4(0.92f, 0.98f, 1.08f, -0.08f));
            sm.highlights.Override(new Vector4(1.05f, 1.0f, 0.93f, 0.02f));
            var vg = ho.Add<Vignette>(true); vg.intensity.Override(0.28f); vg.smoothness.Override(0.5f);
        }
        if (!vol)
        {
            vol = new GameObject("LocThucTeVolume").AddComponent<Volume>();
            vol.transform.SetParent(transform, false);
            vol.isGlobal = true; vol.priority = 9.5f; vol.sharedProfile = ho;   // dưới LocDoSang (10,5) và LocMatDien (11)
        }
    }

    IEnumerator BatVatLieu()
    {
        var lit = Shader.Find("Universal Render Pipeline/Lit");
        if (!lit) { Debug.LogWarning("[LocThucTe] Không tìm thấy shader URP/Lit trong bản build — bỏ qua phần vật liệu."); yield break; }
        yield return null;
        int dem = 0;
        foreach (var r in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!dangBat) yield break;
            if (!r || r is ParticleSystemRenderer || r is LineRenderer || r is TrailRenderer) continue;
            if (!VUNG.Contains(r.bounds.center)) continue;
            if (goc.ContainsKey(r)) continue;
            var cu = r.sharedMaterials; Material[] moi = null;
            for (int i = 0; i < cu.Length; i++)
            {
                var m = cu[i]; if (!m || !m.shader) continue;
                if (m.shader.name != "Universal Render Pipeline/Simple Lit") continue;
                if (m.HasProperty("_Surface") && m.GetFloat("_Surface") > 0.5f) continue;   // bỏ vật liệu trong suốt (kính, sương…)
                if (!cache.TryGetValue(m, out var n)) { n = Doi(m, lit, r); cache[m] = n; }
                if (moi == null) moi = (Material[])cu.Clone();
                moi[i] = n;
            }
            if (moi != null) { goc[r] = cu; r.sharedMaterials = moi; }
            if (++dem % 40 == 0) yield return null;   // rải ra nhiều khung, không giật
        }
        yield return null;
        TaoProbe();
    }

    static Material Doi(Material m, Shader lit, Renderer r)
    {
        var n = new Material(lit) { name = m.name + "_PBR", enableInstancing = m.enableInstancing };
        if (m.HasProperty("_BaseColor")) n.SetColor("_BaseColor", m.GetColor("_BaseColor"));
        if (m.HasProperty("_BaseMap"))
        {
            var t = m.GetTexture("_BaseMap");
            n.SetTexture("_BaseMap", t); n.SetTextureScale("_BaseMap", m.GetTextureScale("_BaseMap")); n.SetTextureOffset("_BaseMap", m.GetTextureOffset("_BaseMap"));
        }
        if (m.HasProperty("_BumpMap") && m.GetTexture("_BumpMap") && m.IsKeywordEnabled("_NORMALMAP"))
        {
            n.SetTexture("_BumpMap", m.GetTexture("_BumpMap")); n.EnableKeyword("_NORMALMAP");
            if (m.HasProperty("_BumpScale")) n.SetFloat("_BumpScale", m.GetFloat("_BumpScale"));
        }
        if (m.IsKeywordEnabled("_EMISSION") && m.HasProperty("_EmissionColor"))
        {
            n.EnableKeyword("_EMISSION"); n.SetColor("_EmissionColor", m.GetColor("_EmissionColor"));
            if (m.HasProperty("_EmissionMap")) n.SetTexture("_EmissionMap", m.GetTexture("_EmissionMap"));
            n.globalIlluminationFlags = m.globalIlluminationFlags;
        }
        if (m.HasProperty("_Cutoff") && m.IsKeywordEnabled("_ALPHATEST_ON"))
        { n.SetFloat("_AlphaClip", 1); n.EnableKeyword("_ALPHATEST_ON"); n.SetFloat("_Cutoff", m.GetFloat("_Cutoff")); n.renderQueue = m.renderQueue; }
        if (m.HasProperty("_Cull")) n.SetFloat("_Cull", m.GetFloat("_Cull"));
        n.SetFloat("_Metallic", 0f);
        n.SetFloat("_Smoothness", DoBong(m.name, r.name));
        return n;
    }

    // độ bóng theo tên vật liệu/vật thể (0 = nhám hoàn toàn, 1 = gương)
    static float DoBong(string vl, string vt)
    {
        string s = (vl + " " + vt).ToLowerInvariant();
        if (s.Contains("sangach") || s.Contains("san") || s.Contains("gach") || s.Contains("floor")) return 0.62f;   // sàn gạch bóng ẩm
        if (s.Contains("kinh") || s.Contains("glass") || s.Contains("guong")) return 0.9f;
        if (s.Contains("kimloai") || s.Contains("metal") || s.Contains("den") || s.Contains("chum")) return 0.55f;
        if (s.Contains("go") || s.Contains("wood") || s.Contains("ban") || s.Contains("ghe") || s.Contains("tu")) return 0.4f;
        if (s.Contains("tuong") || s.Contains("tran") || s.Contains("wall")) return 0.18f;   // vữa tường nhám
        if (s.Contains("vai") || s.Contains("sofa") || s.Contains("tham")) return 0.08f;
        return 0.3f;
    }

    void KhoiPhuc()
    {
        foreach (var kv in goc) if (kv.Key) kv.Key.sharedMaterials = kv.Value;
        goc.Clear();
    }

    void TaoProbe()
    {
        if (!dangBat) return;
        if (!probeGo)
        {
            probeGo = new GameObject("LocThucTeProbe"); probeGo.transform.SetParent(transform, false);
            probeGo.transform.position = VUNG.center;
            var p = probeGo.AddComponent<ReflectionProbe>();
            p.mode = ReflectionProbeMode.Realtime; p.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            p.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
            p.resolution = 128; p.hdr = true; p.boxProjection = true;
            p.size = VUNG.size; p.center = Vector3.zero; p.intensity = 1f; p.importance = 10;
            p.nearClipPlane = 0.1f; p.farClipPlane = 20f;
        }
        probeGo.SetActive(true);
        probeGo.GetComponent<ReflectionProbe>().RenderProbe();   // một lần duy nhất → gần như không tốn FPS
    }
}
