using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// [3/10] Con ma lấp ló (kịch bản v3.1): đặt LocMa vào scene + tính sẵn điểm lấp ló từ cửa và lan can thang.
//  · Điểm cửa: mép khung phía chốt của mỗi cửa phòng (tư thế ĐÓNG), mỗi mặt tường có tường thật thì thành một điểm.
//    P = mép khung ở mặt tường phía người chơi (cốt sàn) · D = hướng từ mép vào lòng ô cửa · N = pháp tuyến mặt tường, chỉ ra phía người chơi.
//  · Lan can: các đoạn LanCan* trong nhà (bỏ ban công, sân phơi, rào) — A/B = hai đầu mép trên.
public static partial class LocHouseBuilder
{
    static readonly string[] MaTen = { "LoDau", "Tay", "LanCan", "VanNguoi", "NgoiXom", "BoTran" };

    static void Ma()
    {
        Physics.SyncTransforms();
        var go = new GameObject("LocMa"); go.transform.SetParent(root, false);
        var m = go.AddComponent<LocMa>();
        var log = new System.Text.StringBuilder("CON MA LẤP LÓ — LocHouseBuilder_Ma\n");
        m.mau = MaTen.Select(t => AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/LOC_House/Models/Ma/Ma_{t}.fbx")).ToArray();
        for (int i = 0; i < MaTen.Length; i++)
        {
            if (!m.mau[i]) { log.AppendLine($"THIẾU model Ma_{MaTen[i]}.fbx"); continue; }
            var rs = m.mau[i].GetComponentsInChildren<Renderer>(); var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
            log.AppendLine($"model Ma_{MaTen[i]}: {b.size.x:0.00} × {b.size.y:0.00} × {b.size.z:0.00} m");
        }
        m.matDen = MatMa("M_Ma_Den", Color.black, "LOC/MaKhoi");          // thân: khói đen, viền tan (Shaders/LOC_MaKhoi.shader)
        m.matMat = MatMa("M_Ma_Mat", Color.white, "Universal Render Pipeline/Unlit");
        m.tiengThet = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LOC_House/Audio/Ma_TiengThet.wav");
        m.chenVo = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/LOC_House/Audio/Ma_ChenVo.wav");

        // ── điểm lấp ló ở mép cửa
        string[] boQua = { "tủ", "két", "sổ", "cổng", "chính", "ban công", "sân phơi", "ngách", "hầm", "ngăn" };
        var P = new System.Collections.Generic.List<Vector3>(); var D = new System.Collections.Generic.List<Vector3>();
        var N = new System.Collections.Generic.List<Vector3>(); var R = new System.Collections.Generic.List<LocDoor>();
        foreach (var dr in root.GetComponentsInChildren<LocDoor>(true))
        {
            if (!dr.ten.StartsWith("cửa") || boQua.Any(x => dr.ten.Contains(x)) || dr.truot != Vector3.zero || dr.gapX > 0) continue;
            var tr = dr.transform; var H = tr.TransformPoint(dr.banLe);
            if (H.y < -0.5f) continue;
            var lb = LocSceneAudit.LocalBounds(tr, tr); if (lb.size.x < 0.4f) continue;
            float xa = Mathf.Abs(lb.max.x - dr.banLe.x) > Mathf.Abs(lb.min.x - dr.banLe.x) ? lb.max.x : lb.min.x;
            var latchBuilt = tr.TransformPoint(new Vector3(xa, dr.banLe.y, lb.center.z));
            var q = dr.dungSanLaMo ? Quaternion.AngleAxis(dr.goc, Vector3.up) : Quaternion.identity;   // tư thế đóng
            var L = H + q * (latchBuilt - H);
            var u = L - H; u.y = 0; float w = u.magnitude; if (w < 0.4f) continue;
            u = Mathf.Abs(u.x) > Mathf.Abs(u.z) ? new Vector3(Mathf.Sign(u.x), 0, 0) : new Vector3(0, 0, Mathf.Sign(u.z));   // nhà vuông góc trục → nắn về trục
            L = H + u * w;
            var n = Vector3.Cross(Vector3.up, u);
            float y0 = H.y;
            // hai mép khung: phía chốt (tường tiếp tục theo +u) và phía bản lề (tường tiếp tục theo −u) — cửa đôi chỉ còn phía bản lề
            foreach (var (J, ra) in new[] { (L, u) })   // [4/10] chỉ lấp ló phía KHÔNG có cánh cửa (mép chốt) — bỏ mép bản lề
            foreach (var s in new[] { n, -n })
            {
                var o = new Vector3(J.x, y0 + 1.2f, J.z) + ra * 0.12f + s * 0.9f;   // trước mặt tường, ngay ngoài mép
                if (!Physics.Raycast(o, -s, out var hit, 1.2f, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (Mathf.Abs(0.9f - hit.distance) > 0.25f || hit.collider.GetComponentInParent<LocDoor>()) continue;   // mặt tường phải sát mép khung
                if (Physics.CheckSphere(o + s * 0.4f, 0.25f, ~0, QueryTriggerInteraction.Ignore)) continue;   // phía người chơi phải trống
                var p = new Vector3(J.x, y0, J.z) + s * (0.9f - hit.distance);
                P.Add(p); D.Add(-ra); N.Add(s); R.Add(dr);
                log.AppendLine($"cửa  {dr.ten,-28} {(ra == u ? "chốt  " : "bản lề")} P=({p.x:0.00},{p.y:0.00},{p.z:0.00})  D=({-ra.x:0.0},{-ra.z:0.0})  N=({s.x:0.0},{s.z:0.0})");
            }
        }
        m.cuaP = P.ToArray(); m.cuaD = D.ToArray(); m.cuaN = N.ToArray(); m.cuaRef = R.ToArray();

        // ── lan can trong nhà (đầu thang T3 + tay vịn xiên các vế thang)
        var A = new System.Collections.Generic.List<Vector3>(); var B = new System.Collections.Generic.List<Vector3>();
        foreach (var bc in root.GetComponentsInChildren<BoxCollider>(true))
        {
            var par = bc.transform.parent; if (bc.name != "Collider" || !par || !par.name.StartsWith("LanCan")) continue;
            if (new[] { "BanCong", "SanPhoi", "RaoSong" }.Any(x => par.name.Contains(x))) continue;
            var bb = bc.bounds; if (bb.center.y < 0.5f) continue;
            bool theoX = bb.size.x > bb.size.z;
            Vector3 a = theoX ? new Vector3(bb.min.x + 0.1f, 0, bb.center.z) : new Vector3(bb.center.x, 0, bb.min.z + 0.1f);
            Vector3 e = theoX ? new Vector3(bb.max.x - 0.1f, 0, bb.center.z) : new Vector3(bb.center.x, 0, bb.max.z - 0.1f);
            float Top(Vector3 v) => bc.Raycast(new Ray(new Vector3(v.x, bb.max.y + 0.2f, v.z), Vector3.down), out var h, bb.size.y + 0.5f) ? h.point.y : bb.max.y;
            a.y = Top(a); e.y = Top(e);
            A.Add(a); B.Add(e);
            log.AppendLine($"lan can {par.name,-24} A=({a.x:0.00},{a.y:0.00},{a.z:0.00}) B=({e.x:0.00},{e.y:0.00},{e.z:0.00})");
        }
        m.lanCanA = A.ToArray(); m.lanCanB = B.ToArray();

        // ── vùng trong nhà (tắt đèn chỉ khi người chơi ở trong này; hầm < −0,6 tự loại)
        m.vungNha = new[]
        {
            new Bounds(new Vector3(3.8f, 5.0f, 12.3f), new Vector3(8.0f, 11.0f, 25.0f)),    // nhà X −0,2…7,8 · Z −0,2…24,8
            new Bounds(new Vector3(-2.3f, 2.0f, 0.8f), new Vector3(4.2f, 5.0f, 12.4f)),     // tiệm X −4,4…−0,2 · Z −5,4…7,0
        };

        // ── cảnh chạy lên phòng Nhím: đứng ngoài cửa (hành lang T2), ma đứng góc trong phòng
        m.nhimDung = new Vector3(2.45f, Y2, 9.4f);
        m.nhimNhin = new Vector3(6.5f, Y2 + 1.2f, 10.0f);
        m.nhimMa = new[] { new Vector3(7.05f, Y2, 10.4f), new Vector3(6.6f, Y2, 10.95f), new Vector3(5.95f, Y2, 10.25f), new Vector3(6.35f, Y2, 10.6f) };   // [4/10] giường dời vào góc trước–phải
        m.nhimCua = root.GetComponentsInChildren<LocDoor>(true).FirstOrDefault(x => x.ten == "cửa phòng Nhím");
        m.nhimDen = root.GetComponentsInChildren<Light>(true).FirstOrDefault(x => x.name == "Den_DenNgu_Nhim");
        log.AppendLine($"\n{P.Count} điểm cửa · {A.Count} đoạn lan can · cửa Nhím: {(m.nhimCua ? "có" : "KHÔNG")} · đèn ngủ Nhím: {(m.nhimDen ? "có" : "KHÔNG")}");
        m.volToi = HinhCu();
        System.IO.File.WriteAllText("Assets/LOC_House/BaoCao_Ma.txt", log.ToString());
    }

    // ───── lớp hình "cũ kỹ, mờ ảo" cho tầm nhìn người chơi (URP Volume) + lớp "mất điện" do LocMa bật
    static Volume HinhCu()
    {
        var cam = root.GetComponentInChildren<Camera>(true);
        if (cam) { var cd = cam.GetUniversalAdditionalCameraData(); cd.renderPostProcessing = true; cd.antialiasing = AntialiasingMode.None; }

        var hc = HoSo("LOC_HinhCu");
        var ca = Them<ColorAdjustments>(hc);
        ca.saturation.Override(-32f); ca.contrast.Override(10f); ca.postExposure.Override(-0.15f);
        ca.colorFilter.Override(new Color(1f, 0.93f, 0.82f));                 // ngả vàng phim cũ
        var fg = Them<FilmGrain>(hc); fg.type.Override(FilmGrainLookup.Medium4); fg.intensity.Override(0.6f); fg.response.Override(0.75f);
        var vg = Them<Vignette>(hc); vg.intensity.Override(0.38f); vg.smoothness.Override(0.5f);
        var cab = Them<ChromaticAberration>(hc); cab.intensity.Override(0.28f);
        var bl = Them<Bloom>(hc); bl.threshold.Override(0.8f); bl.intensity.Override(0.55f); bl.scatter.Override(0.75f);
        var ld = Them<LensDistortion>(hc); ld.intensity.Override(-0.12f);    // rìa hơi méo (kịch bản mục 11)
        var dof = Them<DepthOfField>(hc); dof.mode.Override(DepthOfFieldMode.Gaussian);
        dof.gaussianStart.Override(3.5f); dof.gaussianEnd.Override(16f); dof.gaussianMaxRadius.Override(0.9f);   // xa thì nhoè
        var g1 = new GameObject("LocHinhCu"); g1.transform.SetParent(root, false);
        var v1 = g1.AddComponent<Volume>(); v1.isGlobal = true; v1.priority = 10; v1.sharedProfile = hc;

        var toi = HoSo("LOC_MatDien");
        var ct = Them<ColorAdjustments>(toi); ct.postExposure.Override(-1.6f); ct.saturation.Override(-60f);
        var vt = Them<Vignette>(toi); vt.intensity.Override(0.6f); vt.smoothness.Override(0.6f);
        var g2 = new GameObject("LocMatDien"); g2.transform.SetParent(root, false);
        var v2 = g2.AddComponent<Volume>(); v2.isGlobal = true; v2.priority = 11; v2.weight = 0; v2.sharedProfile = toi;
        AssetDatabase.SaveAssets();
        return v2;
    }

    static VolumeProfile HoSo(string ten)
    {
        string path = $"Assets/LOC_House/Materials/{ten}.asset";
        AssetDatabase.DeleteAsset(path);
        var p = ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(p, path); return p;
    }
    static T Them<T>(VolumeProfile p) where T : VolumeComponent
    {
        var c = p.Add<T>(true); c.name = typeof(T).Name; AssetDatabase.AddObjectToAsset(c, p); return c;
    }

    static Material MatMa(string ten, Color c, string shader)
    {
        string path = $"Assets/LOC_House/Materials/{ten}.mat";
        var sh = Shader.Find(shader) ?? Shader.Find("Universal Render Pipeline/Unlit");
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!mat) { mat = new Material(sh) { name = ten }; AssetDatabase.CreateAsset(mat, path); }
        if (mat.shader != sh) mat.shader = sh;
        mat.SetColor("_BaseColor", c); mat.color = c;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    [MenuItem("LOC/Tự kiểm con ma (Play + chụp)")]
    static void TuKiemMa()
    {
        // [4/10] tắt Error Pause của Console để lỗi lặt vặt không dừng Play giữa bài kiểm (ErrorPause = bit 4)
        try { System.Type.GetType("UnityEditor.LogEntries,UnityEditor").GetMethod("SetConsoleFlag").Invoke(null, new object[] { 4, false }); } catch { }
        EditorPrefs.SetBool("LOC_TuKiemMa", true);
        EditorApplication.isPlaying = true;
    }
}
