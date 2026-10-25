using System.IO;
using UnityEditor;
using UnityEngine;

// [23/10] Ma nữ (MaNu v2) thay model đứng Ma_VanNguoi.fbx (cảnh gặp ma mắt đỏ ở phòng Nhím + các lần hiện khi mất điện).
//  · Dựng vật liệu riêng (shader LOC/MaNuHon: trong suốt, viền sáng lạnh, chân tan thành khói) rồi gán vào FBX.
//  · LocMa giữ nguyên vật liệu có tên chứa "MaNu" (không phủ khói đen); mắt Mat_L/Mat_R vẫn trắng lúc sáng, đỏ lúc mất điện.
//  · Chạy: menu LOC/MaNu: dựng vật liệu ma nữ  (hoặc ghi dòng đó vào LOC_Lenh.txt). Kết quả ghi LOC_MaNu_KetQua.txt.
[InitializeOnLoad]
public static class LocMaNu
{
    // [23/10 c] tự dựng lại vật liệu MỘT lần sau khi Unity biên dịch bản script mới (khỏi phải chạy menu tay)
    const string VER = "MaNu-v15-mat-noi";
    static LocMaNu()
    {
        if (EditorPrefs.GetString("LOC_MaNu_Ver") == VER) return;
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            EditorPrefs.SetString("LOC_MaNu_Ver", VER);
            DungVatLieu();
            ChupXemTruoc();
        };
    }

    const string FBX = "Assets/LOC_House/Models/Ma/Ma_VanNguoi.fbx";
    static readonly string[] TU_THE = { "LoDau", "Tay", "LanCan", "VanNguoi", "NgoiXom", "BoTran" };
    const string TEX = "Assets/LOC_House/Models/Ma/MaNu/";
    const string MAT = "Assets/LOC_House/Materials/MaNu/";
    const string LOG = "LOC_MaNu_KetQua.txt";

    [MenuItem("LOC/MaNu: dựng vật liệu ma nữ")]
    public static void DungVatLieu()
    {
        var log = new System.Text.StringBuilder($"MA NỮ — {System.DateTime.Now:dd/MM HH:mm:ss}\n");
        try
        {
            AssetDatabase.Refresh();
            // ── texture: mặt/thân/váy kiểu PSX (Point), tóc lọc mịn + alpha, normal map đúng loại
            foreach (var f in Directory.GetFiles(TEX, "*.png"))
            {
                var p = f.Replace('\\', '/');
                var ti = AssetImporter.GetAtPath(p) as TextureImporter;
                if (ti == null) { log.AppendLine("KHÔNG ĐỌC ĐƯỢC " + p); continue; }
                bool normal = p.Contains("Normal"), toc = p.Contains("Toc");
                ti.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                ti.sRGBTexture = !normal;
                ti.alphaSource = toc ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None;
                ti.alphaIsTransparency = toc;
                ti.filterMode = toc ? FilterMode.Bilinear : FilterMode.Point;
                ti.mipmapEnabled = true;
                ti.SaveAndReimport();
                log.AppendLine("texture " + Path.GetFileName(p));
            }

            if (!AssetDatabase.IsValidFolder("Assets/LOC_House/Materials/MaNu")) AssetDatabase.CreateFolder("Assets/LOC_House/Materials", "MaNu");
            var hon = Shader.Find("LOC/MaNuHon");
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (!hon) { log.AppendLine("LỖI: không thấy shader LOC/MaNuHon (xem LOC_BienDich.txt / Console)"); File.WriteAllText(LOG, log.ToString()); return; }

            // [23/10 d] ma nữ thay cả 6 tư thế (pose_manu.py): cùng bộ vật liệu cho mọi Ma_*.fbx
            var gan = new System.Collections.Generic.List<(string, Material)>();
            void Gan(string tenFbx, Material m)
            {
                gan.Add((tenFbx, m));
                log.AppendLine($"gán {tenFbx} → {m.name} ({m.shader.name}, hàng {m.renderQueue})");
            }

            // ── thân hồn ma (trong suốt, tan khói ở chân) — số liệu khớp bản xem trước MaNu_Hon_v2
            // [23/10 b] váy, tay áo trắng đặc che kín thân (gấu váy vẫn tan khói); đầu chỉ vẽ mặt hướng camera (không nhìn xuyên mặt từ sau gáy);
            //           da đầu tối vẽ sau mặt để phủ kín sau gáy; tóc đen, không ánh xanh
            var mThan = Hon("M_MaNu_Than", "MaNu_Than_Mau.png", 0.7f, 3000, cull: 2);
            mThan.SetFloat("_HideLegs", 0); EditorUtility.SetDirty(mThan);   // thân trong váy đã xoá hẳn khỏi lưới (pose_manu.py) — không cần cắt bằng shader
            Gan("MaNu_Than", mThan);
            // váy, tay áo: trắng đặc từ gấu lên — chỉ mép gấu dưới cùng (~12 cm) rách tan nhẹ
            foreach (var (tenFbx, tenMat, tex) in new[] { ("MaNu_Vay", "M_MaNu_Vay", "MaNu_Vay_Mau.png"), ("MaNu_TayAo_L", "M_MaNu_TayAo_L", "MaNu_TayAo_L_Mau.png"), ("MaNu_TayAo_R", "M_MaNu_TayAo_R", "MaNu_TayAo_R_Mau.png") })
            {
                var mv = Hon(tenMat, tex, 1f, 3001);
                mv.SetFloat("_FadeStart", -0.04f); mv.SetFloat("_FadeEnd", 0.12f); mv.SetFloat("_NoiseAmp", 0.05f); mv.SetFloat("_FlickerAmount", 0f);
                EditorUtility.SetDirty(mv); Gan(tenFbx, mv);
            }
            Gan("MaNu_Mong", Hon("M_MaNu_Mong", null, 0.85f, 3001, mauNen: new Color(0.015f, 0.016f, 0.014f)));
            var mMat = Hon("M_MaNu_Mat", "MaNu_Dau_Mau_1024.png", 1.0f, 3002, rim: 0.10f, mau: true, cull: 2);
            mMat.SetColor("_BaseColor", new Color(1.62f, 1.55f, 1.55f, 1f));
            mMat.SetFloat("_Contrast", 1.45f); mMat.SetFloat("_RedBoost", 1f);   // [23/10] mặt nổi bật: tương phản + máu đỏ đậm   // [23/10] mặt đặc + tự sáng hơn: thấy rõ lúc mất điện
            mMat.SetFloat("_BackDark", 1); mMat.SetFloat("_BackZ0", 0.06f); mMat.SetFloat("_BackZ1", 0.03f); EditorUtility.SetDirty(mMat);
            Gan("MaNu_Dau", mMat);
            Gan("MaNu_DaDau", Hon("M_MaNu_DaDau", null, 0.92f, 3003, rim: 0f, mauNen: new Color(0.004f, 0.004f, 0.005f), zwrite: false, cull: 2));
            Gan("MaNu_Toc", Hon("M_MaNu_Toc", "MaNu_Toc_Soi.png", 1f, 3004, rim: 0f, toc: true, zwrite: false));

            // ── răng ngà sáng, con ngươi đen, nhãn cầu (LocMa sẽ thay bằng vật liệu mắt trắng/đỏ lúc chạy)
            var rang = Lay("M_MaNu_Rang", lit);
            rang.SetColor("_BaseColor", new Color(0.95f, 0.9f, 0.74f)); rang.SetFloat("_Smoothness", 0.65f);
            rang.EnableKeyword("_EMISSION"); rang.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.6f) * 0.8f);
            rang.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(rang); Gan("MaNu_Rang", rang);
            var den = Lay("M_MaNu_ConNguoi", unlit); den.SetColor("_BaseColor", new Color(0.01f, 0f, 0.002f)); EditorUtility.SetDirty(den); Gan("MaNu_Den", den);
            var mat = Lay("M_MaNu_NhanCau", unlit); mat.SetColor("_BaseColor", new Color(1f, 0.06f, 0.05f)); EditorUtility.SetDirty(mat); Gan("MaNu_Mat_DoMau", mat);

            foreach (var t in TU_THE)
            {
                var duong = $"Assets/LOC_House/Models/Ma/Ma_{t}.fbx";
                var mi = AssetImporter.GetAtPath(duong) as ModelImporter;
                if (mi == null) { log.AppendLine("LỖI: không thấy " + duong); continue; }
                foreach (var (ten, m) in gan) mi.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), ten), m);
                mi.isReadable = true; mi.addCollider = false;   // như LocMaImport: LocMa đọc đỉnh lưới để đo lấn tường
                mi.SaveAndReimport();
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(duong);
                if (!go) continue;
                var rs = go.GetComponentsInChildren<Renderer>(true); var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
                int thieu = 0; foreach (var r in rs) foreach (var m in r.sharedMaterials) if (!m || !m.name.Contains("MaNu")) thieu++;
                log.AppendLine($"Ma_{t}: {rs.Length} lưới, {b.size.x:0.00} × {b.size.y:0.00} × {b.size.z:0.00} m · Dau={(go.transform.Find("Dau") ? "có" : "không")} · vật liệu chưa gán: {thieu}");
            }
            AssetDatabase.SaveAssets();
            log.AppendLine("XONG");
        }
        catch (System.Exception e) { log.AppendLine("EXC " + e); }
        File.WriteAllText(LOG, log.ToString());
        Debug.Log(log.ToString());
    }

    // ───── chụp riêng con ma trong cảnh trống (không hậu kỳ) + liệt kê vật liệu từng lưới → LOC_KiemTra/MaNu_XemTruoc_*.png, LOC_MaNu_KetQua.txt
    [MenuItem("LOC/MaNu: chụp xem trước")]
    public static void ChupXemTruoc()
    {
        var log = new System.Text.StringBuilder($"MA NỮ — chụp xem trước {System.DateTime.Now:HH:mm:ss}\n");
        try
        {
            Directory.CreateDirectory("LOC_KiemTra");
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(FBX);
            var pr = new PreviewRenderUtility();
            var inst = pr.InstantiatePrefabInScene(go);
            pr.AddSingleGO(inst);
            foreach (var r in inst.GetComponentsInChildren<Renderer>(true))
            {
                var m = r.sharedMaterial; if (m && m.HasProperty("_Cull")) log.Append($"[Cull {m.GetFloat("_Cull")}] ");
                log.AppendLine($"{r.name}: {(m ? m.name : "-")} | {(m ? m.shader.name : "-")} | hàng {(m ? m.renderQueue : 0)} | ZWrite {(m && m.HasProperty("_ZWrite") ? m.GetFloat("_ZWrite") : -1)} | Alpha {(m && m.HasProperty("_Alpha") ? m.GetFloat("_Alpha") : -1)} | tex {(m && m.HasProperty("_BaseMap") && m.GetTexture("_BaseMap") ? m.GetTexture("_BaseMap").name : "-")} | tâm {r.bounds.center:F2} cỡ {r.bounds.size:F2}");
            }
            var sh = Shader.Find("LOC/MaNuHon");
            log.AppendLine($"shader LOC/MaNuHon: lỗi={ShaderUtil.ShaderHasError(sh)} · số thuộc tính={sh.GetPropertyCount()} · có _ZWrite={sh.FindPropertyIndex("_ZWrite") >= 0} · đường dẫn={AssetDatabase.GetAssetPath(sh)}");
            foreach (var msg in ShaderUtil.GetShaderMessages(sh)) log.AppendLine($"  {msg.severity}: {msg.message} (dòng {msg.line})");
            foreach (var r in inst.GetComponentsInChildren<Renderer>(true)) log.AppendLine($"  gốc {r.name}: {r.transform.position:F3} xoay {r.transform.rotation.eulerAngles:F0} cỡ {r.transform.lossyScale:F3}");
            var cam = pr.camera;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.13f); cam.clearFlags = CameraClearFlags.SolidColor;
            cam.fieldOfView = 30; cam.nearClipPlane = 0.05f; cam.farClipPlane = 50;
            pr.lights[0].intensity = 1f; pr.lights[0].transform.rotation = Quaternion.Euler(30, 20, 0);
            pr.lights[1].intensity = 0.5f;
            var dau = inst.transform.Find("MatMa"); var tam = dau ? dau.GetComponent<Renderer>().bounds.center : new Vector3(0, 1.68f, 0);
            foreach (var (ten, pos, nhin, w, h) in new[] {
                ("ToanThan", new Vector3(0, 1.0f, 4.2f), new Vector3(0, 0.95f, 0), 480, 720),
                ("Mat", tam + new Vector3(0, 0, 0.9f), tam, 480, 480),
                ("Mat_Sau", tam + new Vector3(0, 0, -0.9f), tam, 480, 480) })
            {
                cam.transform.position = pos; cam.transform.LookAt(nhin);
                pr.BeginStaticPreview(new Rect(0, 0, w, h));
                pr.Render(true);
                var tex = pr.EndStaticPreview();
                File.WriteAllBytes($"LOC_KiemTra/MaNu_XemTruoc_{ten}.png", tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                log.AppendLine("chụp " + ten);
            }
            pr.Cleanup();
            log.AppendLine("XONG");
        }
        catch (System.Exception e) { log.AppendLine("EXC " + e); }
        File.WriteAllText(LOG, log.ToString());
    }

    static Material Lay(string ten, Shader sh)
    {
        string p = MAT + ten + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(p);
        if (!m) { m = new Material(sh) { name = ten }; AssetDatabase.CreateAsset(m, p); }
        if (m.shader != sh) m.shader = sh;
        return m;
    }

    static Material Hon(string ten, string tex, float alpha, int hang, float rim = 0.75f, bool mau = false, bool toc = false, Color? mauNen = null, bool zwrite = true, int cull = 0)
    {
        var m = Lay(ten, Shader.Find("LOC/MaNuHon"));
        var t = tex != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(TEX + tex) : null;
        m.SetTexture("_BaseMap", t);
        m.SetColor("_BaseColor", mauNen ?? Color.white);
        m.SetFloat("_Alpha", alpha);
        m.SetFloat("_UseAlphaTex", toc ? 1 : 0);
        m.SetFloat("_Cutoff", 0.05f);
        m.SetFloat("_BloodOpaque", mau ? 1 : 0);
        m.SetColor("_RimColor", new Color(0.45f, 0.75f, 0.95f));
        m.SetFloat("_RimStrength", rim);
        m.SetFloat("_RimPower", 2.2f);
        m.SetFloat("_FadeStart", 0.15f); m.SetFloat("_FadeEnd", 0.95f);
        m.SetFloat("_NoiseScale", 7f); m.SetFloat("_NoiseAmp", 0.22f);
        m.SetFloat("_ZWrite", zwrite ? 1 : 0);   // thân/mặt/váy ghi chiều sâu: tóc sau gáy không đè lên mặt
        m.SetFloat("_Cull", cull);               // 0 Off · 2 Back
        m.renderQueue = hang;
        EditorUtility.SetDirty(m);
        return m;
    }
}
