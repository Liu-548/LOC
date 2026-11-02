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
    public static int Muc => PlayerPrefs.GetInt("LOC_CD_ThucTe", 0);   // 0 tắt · 1 phòng khách · 2 cả nhà
    public static bool Bat => Muc > 0;

    // Hộp bao phòng khách (toạ độ thế giới) — chỉnh ở đây nếu muốn mở rộng/thu hẹp vùng thử
    static readonly Bounds PK = new Bounds(new Vector3(3.8f, 1.6f, 3.6f), new Vector3(7.6f, 3.6f, 7.2f));
    static Bounds VUNG = PK;
    readonly List<GameObject> probes = new();

    static VolumeProfile ho; Volume vol;
    ParticleSystem bui;
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
        dangBat = b; StopAllCoroutines(); KhoiPhuc();
        if (b) { VUNG = Muc == 2 ? TinhNha() : PK; TaoHauKy(); if (vol) vol.enabled = true; StartCoroutine(BatVatLieu()); }
        else { if (vol) vol.enabled = false; KhoiPhuc(); foreach (var g in probes) if (g) g.SetActive(false); if (bui) bui.gameObject.SetActive(false); }
    }

    void Update()
    {
        if (!dangBat) return;
        if (bui && Camera.main) bui.transform.position = Camera.main.transform.position;
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
            // [03/11] bóng / tuýp / chao do LocCongTac + LocMa bật tắt bằng cách tráo vật liệu: đổi sang PBR ở đây thì khi tắt "Thực tế"
            //  KhoiPhuc trả vật liệu sáng về dù đèn đang tắt (hoặc ngược lại trả bản tắt khi đèn đã bật lại) → để nguyên các vật này
            bool doDien = false; foreach (var m0 in cu) if (LocDien.DoDien(m0) || LocDien.LaBanTat(m0)) { doDien = true; break; }
            if (doDien) continue;
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
        StartCoroutine(TaoProbes());
        TaoBui();
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
        ThemChiTiet(n, m, r);
        return n;
    }

    static LocThucTeBo bo; static bool thuBo;

    // Tường/trần: thêm normal map + AO vữa (D2_xm) lên vật liệu sẵn có. Tường đã có texture riêng nên giữ nguyên albedo, dùng cùng tỉ lệ lặp;
    // vật liệu trơn thì dùng luôn albedo D2 nhân màu gốc. Sàn gạch KHÔNG thêm normal (vạch ron của D2 sẽ lệch ron thật).
    static void ThemChiTiet(Material n, Material goc, Renderer r)
    {
        if (!thuBo) { thuBo = true; bo = Resources.Load<LocThucTeBo>("LocThucTeBo"); }
        if (!bo) return;
        string t = r.name;
        bool tuong = t.StartsWith("Tuong") || (t.StartsWith("Vach") && !t.StartsWith("VachSan")) || (t.StartsWith("Tran") && !t.StartsWith("Tranh"));
        if (!tuong) return;
        if (!bo.Lay("xm", out var A, out var N, out var M)) return;
        bool coTex = goc.HasProperty("_BaseMap") && goc.GetTexture("_BaseMap");
        Vector2 sc = coTex ? goc.GetTextureScale("_BaseMap") : new Vector2(1f, 1f);
        if (!coTex && A) { n.SetTexture("_BaseMap", A); n.SetTextureScale("_BaseMap", new Vector2(1f / 1.0f, 1f / 1.0f)); sc = n.GetTextureScale("_BaseMap"); }
        if (N) { n.SetTexture("_BumpMap", N); n.SetFloat("_BumpScale", 0.8f); n.EnableKeyword("_NORMALMAP"); n.SetTextureScale("_BumpMap", sc); }
        if (M) { n.SetTexture("_OcclusionMap", M); n.SetFloat("_OcclusionStrength", 0.5f); n.EnableKeyword("_OCCLUSIONMAP"); n.SetTextureScale("_OcclusionMap", sc); }
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

    // Bụi lơ lửng trong luồng sáng: ~70 hạt nhỏ, trôi rất chậm, mờ — rẻ (một draw call), tạo chiều sâu cho không khí u ám.
    void TaoBui()
    {
        if (!dangBat) return;
        if (!bui)
        {
            var sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (!sh) return;
            var go = new GameObject("LocThucTeBui"); go.transform.SetParent(transform, false); go.transform.position = Vector3.zero;
            bui = go.AddComponent<ParticleSystem>();
            var main = bui.main; main.loop = true; main.startLifetime = 14f; main.startSpeed = 0.03f; main.startSize = new ParticleSystem.MinMaxCurve(0.008f, 0.02f);
            main.maxParticles = 70; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(0.8f, 0.82f, 0.9f, 0.22f);
            var em = bui.emission; em.rateOverTime = 5f;
            var sp = bui.shape; sp.shapeType = ParticleSystemShapeType.Box; sp.scale = new Vector3(8f, 3.2f, 8f);
            var vel = bui.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f); vel.y = new ParticleSystem.MinMaxCurve(-0.01f, 0.03f); vel.z = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
            var col = bui.colorOverLifetime; col.enabled = true;
            var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.2f), new GradientAlphaKey(1, 0.8f), new GradientAlphaKey(0, 1) });
            col.color = g;
            var pr = go.GetComponent<ParticleSystemRenderer>();
            var mat = new Material(sh) { name = "LocThucTe_Bui" };
            mat.SetFloat("_Surface", 1); mat.SetFloat("_Blend", 0); mat.SetFloat("_ZWrite", 0);
            mat.SetOverrideTag("RenderType", "Transparent"); mat.renderQueue = 3000;
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha); mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.mainTexture = ChamTron();
            pr.sharedMaterial = mat; pr.shadowCastingMode = ShadowCastingMode.Off; pr.receiveShadows = false;
            bui.Simulate(20f, true, true); bui.Play();
        }
        bui.gameObject.SetActive(true);
    }

    static Texture2D ChamTron()
    {
        var t = new Texture2D(32, 32, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
        { float d = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f)) / 16f; float a = Mathf.Clamp01(1f - d); t.SetPixel(x, y, new Color(1, 1, 1, a * a)); }
        t.Apply(); return t;
    }

    // Cả nhà: gom khối bao của mọi renderer nhỏ hơn 40 m (bỏ nền trời/mặt đất khổng lồ)
    static Bounds TinhNha()
    {
        Bounds b = PK; bool dau = true;
        foreach (var r in FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!r || r is ParticleSystemRenderer) continue;
            var s = r.bounds.size; if (s.x > 40f || s.z > 40f || s.y > 12f) continue;
            if (dau) { b = r.bounds; dau = false; } else b.Encapsulate(r.bounds);
        }
        b.Expand(0.5f); return b;
    }

    // Reflection probe: phòng khách 1 probe; cả nhà chia lưới ô ~7 m × 3,6 m, mỗi probe chụp đúng 1 lần (mỗi khung 1 probe → không giật)
    IEnumerator TaoProbes()
    {
        foreach (var g in probes) if (g) Destroy(g);
        probes.Clear();
        var ds = new List<Bounds>();
        if (Muc == 1) ds.Add(PK);
        else
        {
            const float O = 7f, T = 3.6f;
            int nx = Mathf.Max(1, Mathf.CeilToInt(VUNG.size.x / O)), nz = Mathf.Max(1, Mathf.CeilToInt(VUNG.size.z / O)), ny = Mathf.Max(1, Mathf.CeilToInt(VUNG.size.y / T));
            if (nx * nz * ny > 40) { nx = Mathf.Min(nx, 6); nz = Mathf.Min(nz, 6); ny = Mathf.Min(ny, 3); }
            var sz = new Vector3(VUNG.size.x / nx, VUNG.size.y / ny, VUNG.size.z / nz);
            for (int x = 0; x < nx; x++) for (int y = 0; y < ny; y++) for (int z = 0; z < nz; z++)
                ds.Add(new Bounds(VUNG.min + new Vector3((x + 0.5f) * sz.x, (y + 0.5f) * sz.y, (z + 0.5f) * sz.z), sz));
        }
        foreach (var d in ds)
        {
            if (!dangBat) yield break;
            var go = new GameObject("LocThucTeProbe"); go.transform.SetParent(transform, false); go.transform.position = d.center;
            var p = go.AddComponent<ReflectionProbe>();
            p.mode = ReflectionProbeMode.Realtime; p.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
            p.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
            p.resolution = Muc == 1 ? 128 : 64; p.hdr = true; p.boxProjection = true;
            p.size = d.size; p.center = Vector3.zero; p.intensity = 1f; p.importance = 10;
            p.nearClipPlane = 0.1f; p.farClipPlane = 20f;
            probes.Add(go);
            p.RenderProbe();
            yield return null;
        }
    }
}
