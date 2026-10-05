using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// [3/10] Con ma lấp ló + chế độ tức giận (kịch bản v3.1, mục 6.6 và 7).
//  · Đêm 2–3: bóng đen ~1,9 m ló nửa đầu sau mép cửa / bàn tay bám mép / đầu thõng qua lan can thang. Đèn quanh nó nháy nhẹ khi hiện.
//    Người chơi lại gần (< lui m) thì rụt vào, biến mất. Không đụng tới người chơi. Không bao giờ ở hầm.
//  · Đêm 3, sau khi hé hũ (LocHuCot): tiếng thét → cắt cảnh lên phòng Nhím → gặp ma mắt đỏ lần đầu → chế độ tức giận:
//    cứ 60–120 s đèn cả nhà (trừ hầm) nháy rồi tắt hẳn ~6 s; ma hiện ở chỗ ngẫu nhiên cách 3–6 m, luôn nằm trong tầm nhìn nếu quay đầu;
//    bước vào trong 2 m lúc tối = Death Ending. Đứng chờ đèn sáng lại là an toàn.
//  · Phím thử: F1/F2/F3 đổi đêm · F4 bật/tắt tức giận · F5 ép lấp ló · F6 ép tắt đèn.
public class LocMa : MonoBehaviour
{
    public static LocMa I;
    public static int dem;
    public static bool huDaHe, maTucGian, tamDung;   // tamDung: cảnh khoá / đang mở Tab — script khác đặt true để ma không làm gì
    static bool khoiTao;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset0() { khoiTao = false; huDaHe = maTucGian = tamDung = false; dem = 0; }

    [Header("Đêm khi bấm Play (đổi trong game bằng F1/F2/F3)")]
    public int demBatDau = 2;
    [Header("Model: 0 LoDau · 1 Tay · 2 LanCan · 3 VanNguoi · 4 NgoiXom · 5 BoTran")]
    public GameObject[] mau = new GameObject[6];
    public Material matDen, matMat;
    public AudioClip tiengThet, chenVo;
    [Header("Điểm lấp ló (LocHouseBuilder_Ma dựng sẵn)")]
    public Vector3[] cuaP, cuaD, cuaN; public LocDoor[] cuaRef;
    public Vector3[] lanCanA, lanCanB;
    public Bounds[] vungNha;   // trong nhà (ngoài hầm) — chỉ tắt đèn khi người chơi ở trong này
    [Header("Cảnh chạy lên phòng Nhím")]
    public Bounds nhimPhong = new(new Vector3(5.4f, 5.2f, 10.45f), new Vector3(4.6f, 3.4f, 4.1f));
    public Vector3 nhimDung, nhimNhin; public Vector3[] nhimMa; public LocDoor nhimCua; public Light nhimDen;
    [Header("Thông số — chỉnh khi chơi thử")]
    public Vector2 khoangLo = new(18, 40);
    public float lui = 3.5f, loToiDa = 14f, banKinhNhay = 7f;
    public Vector2 khoangTatDen = new(60, 120);
    public float nhayTruoc = 1f, thoiGianToi = 6f, banKinhChet = 2f;
    public Vector2 cachMaKhiToi = new(3, 6);
    public float alphaSang = 0.6f, alphaToi = 0.9f;
    [Range(0.05f, 1f)] public float vienTan = 0.35f;   // viền tan rộng hay hẹp (shader LOC/MaKhoi)
    [Range(0f, 1f)] public float nhieu = 0.35f;   // độ đặc thân ma lúc đèn sáng / lúc mất điện (thấp = mờ ảo hơn)
    public float nhinLau = 1.4f;                         // bị nhìn chằm chằm quá lâu thì rụt vào
    public float ambientKhiToi = 0.5f;   // nhân với ambient gốc lúc mất điện (nhỏ hơn = tối hơn)
    public UnityEngine.Rendering.Volume volToi;   // lớp hậu kỳ "mất điện" (sập sáng, rút màu) — trọng số 0→1 khi tắt đèn
    public bool hienDebug = true;

    const int LO_DAU = 0, TAY = 1, LAN_CAN = 2, VAN = 3, XOM = 4, TRAN = 5;
    static readonly float[] ROLL = { -20, 0, 15, 70, -10, 0 };
    static readonly float[] XOAY_MAX = { 30, 0, 45, 120, 110, 150 };   // độ xoay đầu tối đa theo tư thế — lấp ló sau mép thì gần như không xoay để sọ không lấn ra mặt tường
    public float sauMat = 0.26f;   // lấp ló nửa đầu: mặt tường phía người chơi nằm trước gốc bóng bao nhiêu mét (lớn = lùi sâu, không lấn tường)

    class Bong { public int kieu; public Transform w, dau; public Vector3 face, upL, matLocal; public Quaternion nghi, dauGoc; public Material than; public Bounds bLocal; public Material mat, hao; public Transform[] quad; }
    Bong[] bong;
    Transform cam, player; LocFirstPerson fp; CharacterController cc; AudioSource am;
    readonly List<Light> dens = new();
    readonly List<(Renderer r, Material[] goc, Material[] tat)> phat = new();
    readonly List<GameObject> vatSang = new();
    readonly Dictionary<Material, Material> banTat = new();
    static bool PhatSang(Material m) =>
        (m.HasProperty("_EmissionColor") && m.GetColor("_EmissionColor").maxColorComponent > 0.02f) ||
        (m.HasProperty("emissiveFactor") && m.GetColor("emissiveFactor").maxColorComponent > 0.02f);
    Material BanTat(Material m)
    {
        if (banTat.TryGetValue(m, out var t)) return t;
        t = new Material(m) { name = m.name + "_Tat" };
        if (t.HasProperty("_EmissionColor")) t.SetColor("_EmissionColor", Color.black);
        if (t.HasProperty("emissiveFactor")) t.SetColor("emissiveFactor", Color.black);
        t.DisableKeyword("_EMISSION");
        return banTat[m] = t;
    }
    // bật/tắt mọi thứ "ăn điện" trong nhà (trừ hầm): đèn, vật liệu phát sáng, bóng đèn của công tắc
    void Dien(Dictionary<Light, bool> bat, Dictionary<GameObject, bool> vat, bool on, Bounds? vung = null)
    {
        foreach (var kv in bat) if (kv.Key) kv.Key.enabled = kv.Value && on;
        foreach (var kv in vat) if (kv.Key) kv.Key.SetActive(kv.Value && on);
        foreach (var p in phat) if (p.r && (vung == null || vung.Value.Contains(p.r.bounds.center))) p.r.sharedMaterials = on ? p.goc : p.tat;
    } readonly Dictionary<Light, float> goc = new();
    Color ambient0;
    float hLo, hToi; bool dangLo, dangToi, dangCanh, dangChet, choR;
    Coroutine loCo;
    Bong dangHien; Vector3 chanMa;   // điểm sàn dưới con ma đang hiện lúc tối (đo khoảng cách chết)
    string dbg; float dbgHet;

    void Awake()
    {
        I = this;
        if (!khoiTao) { dem = demBatDau; khoiTao = true; }
        fp = FindAnyObjectByType<LocFirstPerson>();
        player = fp ? fp.transform : null; cc = fp ? fp.GetComponent<CharacterController>() : null;
        cam = fp && fp.cam ? fp.cam : Camera.main ? Camera.main.transform : null;
        am = gameObject.AddComponent<AudioSource>(); am.playOnAwake = false; am.spatialBlend = 0;
        foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (l.type != LightType.Directional && !l.name.Contains("Ham") && !l.transform.IsChildOf(transform)) { dens.Add(l); goc[l] = l.intensity; }
        ambient0 = RenderSettings.ambientLight;
        foreach (var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (r.transform.IsChildOf(transform) || r.bounds.center.y < -0.4f) continue;
            var ms = r.sharedMaterials; bool co = false; var tat = new Material[ms.Length];
            for (int i = 0; i < ms.Length; i++) { tat[i] = ms[i]; if (ms[i] && PhatSang(ms[i])) { tat[i] = BanTat(ms[i]); co = true; } }
            if (co) phat.Add((r, ms, tat));
        }
        foreach (var ct in FindObjectsByType<LocCongTac>(FindObjectsSortMode.None))
            if (ct.transform.position.y > -0.4f && ct.hienKhiBat != null) foreach (var g in ct.hienKhiBat) if (g) vatSang.Add(g);
        TaoBong();
        hLo = Random.Range(6f, 12f); hToi = Random.Range(khoangTatDen.x, khoangTatDen.y);
#if UNITY_EDITOR
        if (UnityEditor.EditorPrefs.GetBool("LOC_TuKiemMa")) { UnityEditor.EditorPrefs.SetBool("LOC_TuKiemMa", false); StartCoroutine(TuKiem()); }
#endif
    }

    // ───── dựng 6 bóng từ FBX; hiệu chỉnh trục theo LoDau: mặt trước = +Z, phía trái của ma (phía nó vươn/ló ra) = −X
    void TaoBong()
    {
        Quaternion rq = Quaternion.identity; Vector3 sc = Vector3.one;
        if (mau[LO_DAU])
        {
            var p = Instantiate(mau[LO_DAU]); p.transform.position = Vector3.zero;   // giữ nguyên xoay gốc của FBX
            var d = Tim(p.transform, "Dau"); var mL = Tim(p.transform, "Mat_L"); var mR = Tim(p.transform, "Mat_R");
            if (d && mL && mR)
            {
                Vector3 mat = (mL.position + mR.position) / 2, dc = d.position;
                if (mat.z - dc.z < 0) rq = Quaternion.Euler(0, 180, 0);
                if ((rq * dc).x > 0) sc.x = -1;
            }
            DestroyImmediate(p);
        }
        bong = new Bong[6];
        var tex = HaoTex();
        for (int i = 0; i < 6; i++)
        {
            if (!mau[i]) continue;
            var b = new Bong { kieu = i, w = new GameObject("Ma_" + i).transform };
            b.w.SetParent(transform, false);
            var m = Instantiate(mau[i], b.w).transform; m.localPosition = Vector3.zero;
            m.localRotation = rq * mau[i].transform.localRotation; m.localScale = Vector3.Scale(sc, mau[i].transform.localScale);
            b.mat = new Material(matMat);
            b.than = new Material(matDen);
            b.than.SetFloat("_Vien", vienTan); b.than.SetFloat("_Nhieu", nhieu);
            b.hao = new Material(Shader.Find("Sprites/Default")) { mainTexture = tex };
            foreach (var r in m.GetComponentsInChildren<Renderer>(true))
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
                r.sharedMaterial = r.name.StartsWith("Mat_") ? b.mat : b.than;
            }
            bool coB = false;   // hộp bao thân trong hệ toạ độ của bóng (để chặn xuyên tường)
            foreach (var r in m.GetComponentsInChildren<Renderer>(true))
            {
                if (r.name.StartsWith("Mat_")) continue;
                var bb = r.bounds; var c = b.w.InverseTransformPoint(bb.center);
                if (!coB) { b.bLocal = new Bounds(c, bb.size); coB = true; } else b.bLocal.Encapsulate(new Bounds(c, bb.size));
            }
            b.dau = Tim(m, "Dau"); if (b.dau) b.dauGoc = b.dau.localRotation;
            var eyes = new[] { Tim(m, "Mat_L"), Tim(m, "Mat_R") };
            b.quad = new Transform[2];
            for (int k = 0; k < 2; k++)
            {
                if (!eyes[k]) continue;
                var q = GameObject.CreatePrimitive(PrimitiveType.Quad); Destroy(q.GetComponent<Collider>());
                q.name = "Hao"; q.transform.SetParent(eyes[k], false); q.transform.localPosition = Vector3.zero;
                q.transform.localScale = Vector3.one * 0.075f / Mathf.Max(1e-4f, eyes[k].lossyScale.x);
                var qr = q.GetComponent<Renderer>(); qr.sharedMaterial = b.hao; qr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                b.quad[k] = q.transform;
            }
            if (b.dau && eyes[0] && eyes[1])
            {
                var mid = (eyes[0].position + eyes[1].position) / 2;
                b.face = Quaternion.Inverse(b.dau.rotation) * (mid - b.dau.position);
                b.upL = Quaternion.Inverse(b.dau.rotation) * b.w.up;
                b.matLocal = b.w.InverseTransformPoint(mid);
                b.nghi = Quaternion.Inverse(b.w.rotation) * b.dau.rotation;
            }
            b.w.gameObject.SetActive(false);
            bong[i] = b;
        }
    }

    static Texture2D HaoTex()
    {
        var t = new Texture2D(32, 32, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
        {
            float r = new Vector2(x - 15.5f, y - 15.5f).magnitude / 15.5f;
            t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - r), 2.2f)));
        }
        t.Apply(); return t;
    }

    static Transform Tim(Transform t, string ten)
    {
        foreach (var c in t.GetComponentsInChildren<Transform>(true)) if (c.name == ten) return c;
        return null;
    }

    void MauMat(Bong b)
    {
        Color c = maTucGian ? new Color(1f, 0.1f, 0.06f) : new Color(0.82f, 0.84f, 0.8f);
        b.mat.SetColor("_BaseColor", c); b.mat.color = c;
        b.hao.color = new Color(c.r, c.g, c.b, maTucGian ? 0.75f : 0.4f);
    }

    Bong Hien(int kieu, Vector3 pos, Quaternion rot, bool latX = false, bool chiDau = false)
    {
        var b = bong[kieu]; if (b == null) return null;
        foreach (var r in b.w.GetComponentsInChildren<Renderer>(true)) r.enabled = !chiDau || (b.dau && r.transform.IsChildOf(b.dau));
        b.w.SetPositionAndRotation(pos, rot);
        b.w.localScale = new Vector3(latX ? -1 : 1, 1, 1);
        MauMat(b);
        b.than.SetFloat("_Alpha", dangToi ? alphaToi : alphaSang);
        b.w.gameObject.SetActive(true);
        // [4/10] bóng lật gương (scale X −1): quay đầu theo hướng nhìn bị lật ngược → đầu ngoảnh vào tường, không ló. Giữ tư thế gốc của model.
        if (b.dau) { if (latX) b.dau.localRotation = b.dauGoc; else b.dau.rotation = HuongDau(b); }
        dangHien = b;
        return b;
    }
    void An() { if (dangHien != null) dangHien.w.gameObject.SetActive(false); dangHien = null; }

    Quaternion HuongDau(Bong b)
    {
        var dir = cam.position - b.dau.position;
        var q = Quaternion.LookRotation(dir, b.w.up) * Quaternion.Inverse(Quaternion.LookRotation(b.face, b.upL));
        q = Quaternion.AngleAxis(ROLL[b.kieu], dir) * q;
        var nghi = b.w.rotation * b.nghi;
        return Quaternion.RotateTowards(nghi, q, XOAY_MAX[b.kieu]);
    }

    void LateUpdate()
    {
        if (dangHien == null || !cam) return;
        var b = dangHien;
        if (b.dau && b.w.localScale.x > 0) b.dau.rotation = Quaternion.Slerp(b.dau.rotation, HuongDau(b), 1 - Mathf.Exp(-6f * Time.deltaTime));
        foreach (var q in b.quad) if (q) q.rotation = Quaternion.LookRotation(q.position - cam.position);
    }

    // ───── vòng chính
    void Update()
    {
        if (!cam || !player) return;
        PhimThu();
        if (dangChet) { if (choR && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) ChoiLai(); return; }
        if (dem < 2 || tamDung || dangCanh || dangToi || player.position.y < -0.6f) return;
        if (!maTucGian && !dangLo && (hLo -= Time.deltaTime) <= 0) hLo = ThuLo() ? Random.Range(khoangLo.x, khoangLo.y) : 3f;
        if (maTucGian && !dangLo && TrongNha() && (hToi -= Time.deltaTime) <= 0) { hToi = Random.Range(khoangTatDen.x, khoangTatDen.y); StartCoroutine(TatDen()); }
    }

    bool TrongNha()
    {
        if (vungNha == null || vungNha.Length == 0) return player.position.y > -0.6f;
        foreach (var v in vungNha) if (v.Contains(player.position + Vector3.up * 0.5f)) return true;
        return false;
    }

    string lyDoVuong = "";
    // hộp bao của bóng đặt tại pos/rot có đâm vào tường, đồ đạc không (bỏ qua người chơi và collider có cha bắt đầu bằng boQuaCha)
    bool Vuong(Bong b, Vector3 pos, Quaternion rot, bool lat = false, string boQuaCha = null)
    {
        var c = b.bLocal.center; if (lat) c.x = -c.x;
        var e = Vector3.Max(b.bLocal.extents - Vector3.one * 0.025f, Vector3.one * 0.02f);
        foreach (var o in Physics.OverlapBox(pos + rot * c, e, rot, ~0, QueryTriggerInteraction.Ignore))
        {
            if (o is CharacterController) continue;
            if (boQuaCha != null && o.transform.parent && o.transform.parent.name.StartsWith(boQuaCha)) continue;
            lyDoVuong = o.name; return true;
        }
        return false;
    }

    bool ThayDuoc(Vector3 p, float bo = 0.2f)
    {
        var d = p - cam.position;
        return !Physics.Raycast(cam.position, d.normalized, d.magnitude - bo, ~0, QueryTriggerInteraction.Ignore);
    }

    // ───── lấp ló
    public string lyDoLo = "";
    struct ChoLo { public int kieu; public Vector3 an, hien; public Quaternion rot; public bool lat, chiDau; }   // chiDau: chỉ hiện cái đầu (WC bố mẹ — thân sẽ đè vách)

    bool ThuLo(int ep = -1)
    {
        var ds = new List<ChoLo>();
        Vector3 pp = player.position;
        if (cuaP != null)
            for (int i = 0; i < cuaP.Length; i++)
            {
                if (cuaRef != null && i < cuaRef.Length && cuaRef[i] && !cuaRef[i].DangMo) continue;
                Vector3 P = cuaP[i], d = cuaD[i], n = cuaN[i];
                var ph = pp - P; ph.y = 0; float kc = ph.magnitude;
                if (kc < 4f || kc > 14f || Mathf.Abs(pp.y - P.y) > 1.2f || Vector3.Dot(ph, n) < 0.3f * kc) { lyDoLo = $"kc {kc:0.0} dy {pp.y - P.y:0.0} dot {Vector3.Dot(ph, n) / Mathf.Max(kc, 0.01f):0.00}"; continue; }
                foreach (var k in new[] { LO_DAU })   // [4/10] bỏ kiểu chỉ ló bàn tay
                {
                    var b = bong[k]; if (b == null || (ep >= 0 && ep != k)) continue;
                    TinhCua(i, k, out var c);
                    var mat = P + d * 0.03f + n * (b.matLocal.z - sauMat) + Vector3.up * b.matLocal.y;   // mắt ló qua mép
                    if (Vector3.Angle(cam.forward, mat - cam.position) > 70f) { lyDoLo = $"góc {Vector3.Angle(cam.forward, mat - cam.position):0}°"; continue; }
                    if (!ThayDuoc(mat, 0.08f)) { Physics.Raycast(cam.position, (mat - cam.position).normalized, out var hc, 99f); lyDoLo = $"khuất bởi {hc.collider?.name} cách mắt {Vector3.Distance(hc.point, mat):0.00} m; mắt={mat:F2} matLocal={b.matLocal:F2}"; continue; }
                    if (!HopLeCua(i, ref c, out _, out _)) { lyDoLo = $"cửa {i}: thân đè {vatBiLan}"; continue; }
                    ds.Add(c);
                }
            }
        if (lanCanA != null && bong[LAN_CAN] != null && (ep < 0 || ep == LAN_CAN))
            for (int i = 0; i < lanCanA.Length; i++)
            {
                var T = Vector3.Lerp(lanCanA[i], lanCanB[i], Random.Range(0.25f, 0.75f));
                if (cam.position.y > T.y - 0.4f) continue;   // người chơi phải ở dưới
                var seg = lanCanB[i] - lanCanA[i]; seg.y = 0;
                var n = Vector3.Cross(Vector3.up, seg).normalized; var ph = pp - T; ph.y = 0;
                if (ph.magnitude < 1.2f || ph.magnitude > 9f) continue;
                if (Vector3.Dot(ph, n) < 0) n = -n;
                var than = T - n * 0.35f;
                if (!Physics.Raycast(than + Vector3.up * 0.2f, Vector3.down, out var san, 1.6f, ~0, QueryTriggerInteraction.Ignore)) continue;
                var c = new ChoLo { kieu = LAN_CAN, rot = Quaternion.LookRotation(n, Vector3.up) };
                c.hien = new Vector3(T.x, san.point.y, T.z) - n * 0.18f;
                if (Vuong(bong[LAN_CAN], c.hien, c.rot, false, "LanCan")) { lyDoLo = "lan can vướng " + lyDoVuong; continue; }
                c.hien += Vector3.up * 0.03f; c.an += Vector3.up * 0.03f;
                if (!DatThu(LAN_CAN, c.hien, c.rot, false, "LanCan")) { lyDoLo = "lan can: đỉnh lấn tường"; continue; }
                An();
                c.an = c.hien - n * 0.4f;   // lùi khỏi lan can (không chìm xuống sàn — chân sẽ lòi ra trần tầng dưới)
                var mat = c.hien + n * 0.45f + Vector3.up * 0.9f;
                if (Vector3.Angle(cam.forward, mat - cam.position) > 75f || !ThayDuoc(mat, 0.1f)) continue;
                ds.Add(c);
            }
        if (ds.Count == 0) return false;
        loCo = StartCoroutine(Lo(ds[Random.Range(0, ds.Count)]));
        return true;
    }

    // vị trí hiện/ẩn của kiểu lấp ló k ở điểm cửa i: trái của ma chỉ ra phía ô cửa, mặt hướng về phía người chơi
    void TinhCua(int i, int k, out ChoLo c)
    {
        Vector3 P = cuaP[i], d = cuaD[i], n = cuaN[i]; var b = bong[k];
        var rot = Quaternion.LookRotation(n, Vector3.up);
        c = new ChoLo { kieu = k, rot = rot, lat = Vector3.Dot(rot * Vector3.left, d) < 0, chiDau = cuaRef != null && i < cuaRef.Length && LaWCBoMe(cuaRef[i]) };
        if (k == LO_DAU)
        {
            c.hien = P + d * (0.015f + b.matLocal.x) - n * sauMat;   // một mắt vừa ló qua mép, cả khối lùi sau mặt tường
            c.an = c.chiDau ? c.hien : c.hien - d * 0.30f;   // chỉ ló đầu: hiện mờ dần tại chỗ, không trượt (trượt sẽ cạ vào tường bên)
        }
        else
        {
            c.hien = P - d * 0.33f - n * 0.175f;                      // đốt ngón vòng qua mép, áp sát mặt tường phía người chơi
            c.an = c.hien - n * 0.25f;
        }
    }

    static bool LaWCBoMe(LocDoor d)
    {
        if (!d) return false;
        foreach (var t in d.GetComponentsInChildren<Transform>(true)) if (t.name.Contains("WC_BoMe")) return true;
        return d.transform.parent && d.transform.parent.name.Contains("WC_BoMe");
    }
    // [4/10] tường che của điểm cửa i: collider đầu tiên phía sau mặt tường, ngay cạnh mép, ngang đầu
    Collider TuongChe(int i) =>
        Physics.Raycast(cuaP[i] - cuaD[i] * 0.06f + cuaN[i] * 0.3f + Vector3.up * 1.5f, -cuaN[i], out var h, 0.6f, ~0, QueryTriggerInteraction.Ignore) ? h.collider : null;
    static string GocTen(string t) { int k = t.LastIndexOf('_'); return k > 0 && int.TryParse(t.Substring(k + 1), out _) ? t.Substring(0, k) : t; }
    // đỉnh lưới nằm trong vật KHÁC bức tường che (tường vuông góc, vách WC, cánh cửa mở, đồ đạc) — thấy rõ là ma đè tường
    int DemNgoaiTuong(Bong b, Collider tuong, int buoc = 4, bool chiDau = false)
    {
        int c = 0; vatBiLan = ""; string goc = tuong ? GocTen(tuong.name) : null;
        foreach (var (v, dau) in Dinh(b, buoc))
            if (!chiDau || dau) foreach (var o in Physics.OverlapSphere(v, 0.006f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (o is CharacterController || o == tuong || (goc != null && GocTen(o.name) == goc)) continue;
                c++; if (vatBiLan.Length < 120 && !vatBiLan.Contains(o.name)) vatBiLan += o.name + " "; break;
            }
        return c;
    }
    // đặt thử ở chỗ hiện và chỗ ẩn (lúc trượt ra ma đã hiện mờ): không đè vật nào ngoài tường che, đầu ló ra ≥ 10 đỉnh
    bool HopLeCua(int i, ref ChoLo c, out int ngoai, out int dauRa)
    {
        if (HopLeCua0(i, c, out ngoai, out dauRa) || !c.chiDau) return ngoai == 0 && dauRa >= 10;
        // chỉ ló đầu (góc WC bố mẹ): đầu chạm tường bên cạnh thì nhích 1–4 cm cho tới khi sạch
        foreach (var k in new[] { 0.01f, 0.02f, 0.03f, 0.04f })
            foreach (var v in new[] { -cuaN[i], cuaN[i], -cuaD[i], cuaD[i], Vector3.down })
            {
                var c2 = c; c2.hien += v * k; c2.an += v * k;
                if (HopLeCua0(i, c2, out ngoai, out dauRa)) { c = c2; return true; }
            }
        return false;
    }
    bool HopLeCua0(int i, ChoLo c, out int ngoai, out int dauRa)
    {
        ngoai = 0; dauRa = 0; var tuong = TuongChe(i);
        foreach (var pos in new[] { c.hien, Vector3.Lerp(c.an, c.hien, 0.5f) })
        {
            var b = Hien(c.kieu, pos, c.rot, c.lat, c.chiDau); if (b == null) return false;
            Physics.SyncTransforms();
            ngoai += DemNgoaiTuong(b, tuong, 4, c.chiDau);
            if (pos == c.hien) DoCua(b, i, c.kieu, out _, out _, out dauRa);
        }
        An();
        return ngoai == 0 && dauRa >= 10;
    }

    IEnumerator Lo(ChoLo c)
    {
        dangLo = true;
        var b = Hien(c.kieu, c.an, c.rot, c.lat, c.chiDau);
        StartCoroutine(NhayGan(c.hien, banKinhNhay));
        for (float t = 0; t < 1; t += Time.deltaTime / 0.7f)
        {
            float e = Mathf.SmoothStep(0, 1, t);
            b.w.position = Vector3.Lerp(c.an, c.hien, e); b.than.SetFloat("_Alpha", alphaSang * e); yield return null;
        }
        b.w.position = c.hien; b.than.SetFloat("_Alpha", alphaSang);
        float song = 0, nhin = 0;
        while (song < loToiDa && !dangToi && !dangCanh && !dangChet)
        {
            var ph = player.position - c.hien; float dy = Mathf.Abs(ph.y); ph.y = 0;
            if (ph.magnitude < lui && dy < 2.5f) break;
            var mat = b.quad[0] ? b.quad[0].position : c.hien + Vector3.up * 1.7f;
            nhin = Vector3.Angle(cam.forward, mat - cam.position) < 7f ? nhin + Time.deltaTime : 0;
            if (nhin > nhinLau) break;   // nhìn chằm chằm thì nó rụt
            song += Time.deltaTime; yield return null;
        }
        for (float t = 0; t < 1; t += Time.deltaTime / 0.25f)
        {
            b.w.position = Vector3.Lerp(c.hien, c.an, t * t); b.than.SetFloat("_Alpha", alphaSang * (1 - t)); yield return null;
        }
        if (dangHien == b) An();
        dangLo = false;
    }

    IEnumerator NhayGan(Vector3 tam, float r)
    {
        var gan = dens.FindAll(l => l && l.enabled && !l.GetComponent<LocLuaNhapNhay>() && (l.transform.position - tam).sqrMagnitude < r * r);
        float[] buoc = { 0.25f, 1f, 0.4f, 1f, 0.15f, 0.7f, 1f };
        foreach (var k in buoc) { foreach (var l in gan) if (l) l.intensity = goc[l] * k; yield return new WaitForSeconds(0.07f); }
        foreach (var l in gan) if (l) l.intensity = goc[l];
    }

    // ───── tắt đèn (chế độ tức giận)
    IEnumerator TatDen(int ep = -1)
    {
        dangToi = true;
        var bat = new Dictionary<Light, bool>(); foreach (var l in dens) if (l) bat[l] = l.enabled;
        var vat = new Dictionary<GameObject, bool>(); foreach (var g in vatSang) if (g) vat[g] = g.activeSelf;
        for (float t = 0; t < nhayTruoc;)
        {
            Dien(bat, vat, Random.value < 0.45f);
            float w = Random.Range(0.05f, 0.12f); t += w;
            yield return new WaitForSeconds(w);
        }
        Dien(bat, vat, false);
        RenderSettings.ambientLight = ambient0 * ambientKhiToi;
        if (volToi) volToi.weight = 1;
        DatMaTrongToi(ep);
        for (float t = 0; t < thoiGianToi && !dangChet; t += Time.deltaTime)
        {
            if (dangHien != null)
            {
                var ph = player.position - chanMa; float dy = Mathf.Abs(ph.y); ph.y = 0;
                if (ph.magnitude < banKinhChet && dy < 2.6f) { Chet(); yield break; }
            }
            yield return null;
        }
        if (dangChet) yield break;
        An();   // [4/10] cất ma trước, đèn mới sáng — không bao giờ thấy mắt đỏ lúc có đèn
        Dien(bat, vat, true);
        yield return new WaitForSeconds(0.08f);
        Dien(bat, vat, false);
        yield return new WaitForSeconds(0.12f);
        Dien(bat, vat, true);
        RenderSettings.ambientLight = ambient0;
        if (volToi) volToi.weight = 0;
        dangToi = false;
    }

    // chọn chỗ ngẫu nhiên cách 3–6 m, đứng/ngồi trên sàn cùng tầng hoặc bò trên trần, mắt nằm trong tầm nhìn (quay đầu là thấy)
    public string lyDoToi = "";
    // đặt thử rồi đếm đỉnh lấn vào tường/đồ đạc; lấn thì cất đi
    bool DatThu(int k, Vector3 pos, Quaternion rot, bool lat = false, string boQuaCha = null)
    {
        var b = Hien(k, pos, rot, lat); if (b == null) return false;
        Physics.SyncTransforms();
        if (DemTrongVat(b, boQuaCha, 6) == 0) return true;
        An(); return false;
    }
    bool DatMaTrongToi(int ep = -1)
    {
        var mask = ~0; var qi = QueryTriggerInteraction.Ignore;
        int kSan = 0, kTang = 0, kCho = 0, kNhin = 0; string vuong = "";
        for (int thu = 0; thu < 60; thu++)
        {
            float a = Random.Range(0f, 360f), r = Random.Range(cachMaKhiToi.x, cachMaKhiToi.y);
            var dir = Quaternion.Euler(0, a, 0) * Vector3.forward;
            var Q = player.position + dir * r;
            if (!Physics.Raycast(Q + Vector3.up * 1.2f, Vector3.down, out var san, 1.8f, mask, qi)) { kSan++; continue; }
            if (Mathf.Abs(san.point.y - player.position.y) > 0.35f) { kTang++; if (vuong.Length < 200) vuong += $" san:{san.collider.name}@{san.point.y:0.00}"; continue; }
            var G = san.point;
            var huong = player.position - G; huong.y = 0; var rot = Quaternion.LookRotation(huong.normalized, Vector3.up);
            var kieuS = ep >= 0 ? new[] { ep } : new[] { VAN, XOM, TRAN };
            if (ep < 0) for (int i = 0; i < 3; i++) { int j = Random.Range(i, 3); (kieuS[i], kieuS[j]) = (kieuS[j], kieuS[i]); }
            foreach (var k in kieuS)
            {
                if (bong[k] == null) continue;
                if (k == VAN)
                {
                    if (Vuong(bong[k], G, rot)) { kCho++; if (vuong.Length < 400) vuong += " van:" + lyDoVuong; continue; }
                    if (!ThayDuoc(G + Vector3.up * 1.78f)) { kNhin++; continue; }
                    if (DatThu(k, G + Vector3.up * 0.03f, rot)) { chanMa = G; return true; }   // nhấc 3 cm: chân/tay không cắm xuống sàn
                    kCho++; continue;
                }
                if (k == XOM)
                {
                    if (Vuong(bong[k], G, rot)) { kCho++; if (vuong.Length < 400) vuong += " xom:" + lyDoVuong; continue; }
                    if (!ThayDuoc(G + Vector3.up * 0.95f)) { kNhin++; continue; }
                    if (DatThu(k, G + Vector3.up * 0.03f, rot)) { chanMa = G; return true; }   // nhấc 3 cm: chân/tay không cắm xuống sàn
                    kCho++; continue;
                }
                if (k == TRAN && Physics.Raycast(G + Vector3.up * 1.0f, Vector3.up, out var tran, 3.2f, mask, qi))
                {
                    float h = tran.point.y - G.y; var C = tran.point;
                    if (h < 2.3f || h > 3.9f) { kTang++; continue; }
                    if (Vuong(bong[k], C, rot * Quaternion.Euler(0, 0, 180))) { kCho++; if (vuong.Length < 400) vuong += " tran:" + lyDoVuong; continue; }
                    if (!ThayDuoc(C - Vector3.up * 0.45f)) { kNhin++; continue; }
                    if (DatThu(k, C - Vector3.up * 0.03f, rot * Quaternion.Euler(0, 0, 180))) { chanMa = G; return true; }   // lật ngược, lưng áp trần
                    kCho++; continue;
                }
            }
        }
        lyDoToi = $"không sàn {kSan} · khác tầng {kTang} · vướng {kCho} · khuất {kNhin} ·{vuong}";
        return false;
    }

    // ───── đo lấn tường bằng đỉnh lưới (model Ma_*.fbx bật Read/Write — LocMaImport)
    System.Collections.Generic.IEnumerable<(Vector3 v, bool dau)> Dinh(Bong b, int buoc = 3)
    {
        foreach (var mf in b.w.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.name.StartsWith("Mat_") || mf.name == "Hao" || !mf.sharedMesh) continue;
            bool dau = b.dau && mf.transform.IsChildOf(b.dau);
            var vs = mf.sharedMesh.vertices; var tr = mf.transform;
            for (int i = 0; i < vs.Length; i += buoc) yield return (tr.TransformPoint(vs[i]), dau);
        }
    }

    // số đỉnh nằm trong tường/đồ đạc (bỏ qua người chơi và collider có cha bắt đầu bằng boQuaCha)
    public string vatBiLan = "";
    int DemTrongVat(Bong b, string boQuaCha = null, int buoc = 4)
    {
        int c = 0; vatBiLan = "";
        foreach (var (v, _) in Dinh(b, buoc))
            foreach (var o in Physics.OverlapSphere(v, 0.006f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (o is CharacterController || (boQuaCha != null && o.transform.parent && o.transform.parent.name.StartsWith(boQuaCha))) continue;
                c++; if (vatBiLan.Length < 120 && !vatBiLan.Contains(o.name)) vatBiLan += o.name + " "; break;
            }
        return c;
    }

    // ───── hé hũ (Đêm 3) → tiếng thét → phòng Nhím → ma mắt đỏ lần đầu
    public void HeHu(LocHuCot h) { if (!dangCanh && !huDaHe) StartCoroutine(CanhHeHu(h)); }

    IEnumerator CanhHeHu(LocHuCot h)
    {
        dangCanh = true; LocFirstPerson.khoa = true;
        if (dangLo) { if (loCo != null) StopCoroutine(loCo); An(); dangLo = false; }
        if (h) yield return h.CanhHe(cam);   // [4/10] cảnh hé hũ chi tiết nằm trong LocHuCot
        huDaHe = true;
        if (tiengThet) am.PlayOneShot(tiengThet, 1f);
        StartCoroutine(NhayCaNha(1.6f));
        yield return new WaitForSeconds(0.9f);
        LocFirstPerson.PhuDe("Nhím.", 1.4f); yield return new WaitForSeconds(1.1f);
        for (float t = 0; t < 1; t += Time.deltaTime / 0.3f) { LocFirstPerson.den = t; yield return null; }
        LocFirstPerson.den = 1;
        yield return new WaitForSeconds(1.4f);
        if (fp) fp.DatTuThe(nhimDung, nhimNhin);
        if (nhimCua) nhimCua.Mo();
        maTucGian = true;
        // [4/10] lúc màn còn đen: cả nhà mất điện HẲN (đèn, vật phát sáng, ambient, lớp tối) rồi mới đặt ma — mắt đỏ chỉ hiện trong tối
        var bat = new Dictionary<Light, bool>(); foreach (var l in dens) if (l) bat[l] = l.enabled;
        if (nhimDen && !bat.ContainsKey(nhimDen)) bat[nhimDen] = nhimDen.enabled;
        var vat = new Dictionary<GameObject, bool>(); foreach (var g in vatSang) if (g) vat[g] = g.activeSelf;
        Dien(bat, vat, false);
        RenderSettings.ambientLight = ambient0 * ambientKhiToi; if (volToi) volToi.weight = 1;
        dangToi = true;
        Bong b = null;
        if (nhimMa != null)
            foreach (var p in nhimMa)
            {
                var huong = nhimDung - p; huong.y = 0;
                if (bong[VAN] == null || Vuong(bong[VAN], p, Quaternion.LookRotation(huong.normalized, Vector3.up))) continue;
                if (!DatThu(VAN, p + Vector3.up * 0.03f, Quaternion.LookRotation(huong.normalized, Vector3.up))) continue;
                b = dangHien;
                if (b != null && !ThayDuocSauKhiDat(p + Vector3.up * 1.78f)) { An(); b = null; continue; }
                break;
            }
        yield return new WaitForSeconds(0.9f);
        for (float t = 1; t > 0; t -= Time.deltaTime / 0.4f) { LocFirstPerson.den = t; yield return null; }
        LocFirstPerson.den = 0;
        yield return new WaitForSeconds(3.2f);
        // ma biến trong tối, rồi điện chập chờn sáng lại
        An();
        yield return new WaitForSeconds(0.5f);
        foreach (var on in new[] { true, false, true, false })
        { Dien(bat, vat, on); yield return new WaitForSeconds(on ? 0.07f : 0.12f); }
        Dien(bat, vat, true);
        RenderSettings.ambientLight = ambient0; if (volToi) volToi.weight = 0;
        dangToi = false;
        yield return new WaitForSeconds(0.6f);
        LocFirstPerson.khoa = false; dangCanh = false;
        hToi = Random.Range(40f, 60f); hLo = Random.Range(12f, 20f);
    }

    bool ThayDuocSauKhiDat(Vector3 p) { Physics.SyncTransforms(); return ThayDuoc(p, 0.25f); }

    IEnumerator NhayCaNha(float giay)
    {
        var bat = new Dictionary<Light, bool>(); foreach (var l in dens) if (l) bat[l] = l.enabled;
        for (float t = 0; t < giay; t += 0.07f)
        {
            bool on = Random.value < 0.5f;
            foreach (var kv in bat) if (kv.Key) kv.Key.enabled = kv.Value && on;
            yield return new WaitForSeconds(0.07f);
        }
        foreach (var kv in bat) if (kv.Key) kv.Key.enabled = kv.Value;
    }

    // ───── Death Ending: cắt đen ngay → chén nứt, ba giây, vỡ → NHÀ NÀY CÒN MỘT CÁI CHÉN.
    // ponytail: chưa dựng khung hình tĩnh bàn thờ hầm với chén số 4 vỡ — thêm khi có biến thể chén vỡ trong scene
    void Chet() { if (!dangChet) StartCoroutine(CanhChet()); }
    IEnumerator CanhChet()
    {
        dangChet = true; LocFirstPerson.khoa = true; LocFirstPerson.den = 1; An();
        if (chenVo) am.PlayOneShot(chenVo, 1f);
        yield return new WaitForSeconds(5.2f);
        LocFirstPerson.chuGiua = "NHÀ NÀY CÒN MỘT CÁI CHÉN.";
        yield return new WaitForSeconds(2.5f);
        LocFirstPerson.chuGiua += "\n\n\n\n[R]  Chơi lại từ đầu đêm";
        choR = true;
    }

    void ChoiLai()
    {
        huDaHe = false; maTucGian = false; tamDung = false;
        LocFirstPerson.khoa = false; LocFirstPerson.den = 0; LocFirstPerson.chuGiua = null;
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(SceneManager.GetActiveScene().path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#endif
    }

    // ───── phím thử
    void PhimThu()
    {
        var k = Keyboard.current; if (k == null) return;
        if (k.f1Key.wasPressedThisFrame) DatDem(1);
        if (k.f2Key.wasPressedThisFrame) DatDem(2);
        if (k.f3Key.wasPressedThisFrame) DatDem(3);
        if (k.f4Key.wasPressedThisFrame) { maTucGian = !maTucGian; huDaHe |= maTucGian; Bao(maTucGian ? "ma TỨC GIẬN (mắt đỏ, tắt đèn)" : "ma bình thường"); hToi = 8f; }
        if (k.f5Key.wasPressedThisFrame && !dangLo && !dangToi) Bao(ThuLo() ? "ép lấp ló" : "không có chỗ lấp ló hợp lệ quanh đây");
        if (k.f6Key.wasPressedThisFrame && !dangToi && !dangCanh) { StartCoroutine(TatDen()); Bao("ép tắt đèn"); }
    }
    void DatDem(int d) { dem = d; if (d < 3) { maTucGian = false; huDaHe = false; } Bao("Đêm " + d); }
    void Bao(string s) { dbg = s; dbgHet = Time.time + 3f; }

    void OnGUI()
    {
        if (!hienDebug) return;
        var st = new GUIStyle(GUI.skin.label) { fontSize = 13 }; st.normal.textColor = new Color(1, 1, 1, 0.55f);
        string s = $"[thử] Đêm {dem} · {(maTucGian ? "tức giận" : "bình thường")} · F1-F3 đêm · F4 tức giận · F5 ló · F6 tắt đèn";
        if (Time.time < dbgHet) s += "\n→ " + dbg;
        GUI.Label(new Rect(10, 8, 900, 40), s, st);
    }

#if UNITY_EDITOR
    // ───── tự kiểm (menu LOC → Tự kiểm con ma): đo đè tường bằng hình học ở MỌI chỗ ma có thể hiện + chụp từng khung lúc ma hiện → LOC_KiemTra/Ma/
    // lấp ló cửa: truoc = đỉnh nằm TRƯỚC mặt tường phía người chơi trong phần tường (vẽ đè lên tường) · lo = đỉnh thân (không tính đầu) lộ vào ô cửa · dauRa = đỉnh đầu ló qua mép
    void DoCua(Bong b, int i, int k, out int truoc, out int lo, out int dauRa)
    {
        truoc = lo = dauRa = 0;
        Vector3 P = cuaP[i], d = cuaD[i], n = cuaN[i];
        foreach (var (v, dau) in Dinh(b))
        {
            float fz = Vector3.Dot(v - P, n), fx = Vector3.Dot(v - P, d);
            if (fx < -0.004f && fz > (k == TAY ? 0.035f : 0.004f)) truoc++;
            if (k == LO_DAU && !dau && fx > 0.01f && fz > -0.6f) lo++;
            if (dau && fx > 0) dauRa++;
        }
    }

    Vector3 ChoDung(int i, float xa)
    {
        for (float x = xa + 1.5f; x >= xa - 0.3f; x -= 0.3f)
        {
            var q = cuaP[i] + cuaN[i] * x + cuaD[i] * 0.5f;
            if (Physics.Raycast(q + Vector3.up * 1.0f, Vector3.down, out var s0, 1.3f) && !Physics.CheckCapsule(q + Vector3.up * 0.4f, q + Vector3.up * 1.5f, 0.25f)
                && !Physics.Linecast(q + Vector3.up * 1.5f, cuaP[i] + cuaD[i] * 0.05f + cuaN[i] * 0.05f + Vector3.up * 1.5f)) return s0.point;
        }
        return cuaP[i] + cuaN[i] * xa;
    }

    IEnumerator TuKiem()
    {
        var log = new System.Text.StringBuilder($"TỰ KIỂM CON MA {System.DateTime.Now:HH:mm:ss}\n");
        System.IO.Directory.CreateDirectory("LOC_KiemTra/Ma");
        foreach (var f in System.IO.Directory.GetFiles("LOC_KiemTra/Ma", "*.png")) try { System.IO.File.Delete(f); } catch { }
        yield return new WaitForSeconds(1f);
        hLo = hToi = 9999; dem = 2;
        int loi = 0, tong = 0;
        // [4/10] ghi lỗi Console vào báo cáo + ghi tạm sau mỗi phần (kẹt giữa chừng vẫn còn kết quả)
        Application.LogCallback batLoi = (msg, st, ty) => { if (ty == LogType.Error || ty == LogType.Exception) log.AppendLine($"  [CONSOLE {ty}] {msg} {st.Split('\n')[0]}"); };
        Application.logMessageReceived += batLoi;
        void GhiTam() => System.IO.File.WriteAllText("LOC_KiemTra/Ma/BaoCao_TuKiem.txt", $"(ĐANG CHẠY — tạm {tong - loi}/{tong})\n" + log);

        // 1. mọi điểm cửa × 2 kiểu, đặt tĩnh ở tư thế hiện, đầu đã quay về người chơi đứng cách 4 m
        log.AppendLine("— 1. LẤP LÓ SAU MÉP CỬA: đo tĩnh mọi điểm —");
        for (int i = 0; i < (cuaP?.Length ?? 0); i++)
            {
                int k = LO_DAU;
                TinhCua(i, k, out var c);
                fp.DatTuThe(ChoDung(i, 4f), cuaP[i] + Vector3.up * 1.6f);
                for (int f = 0; f < 5; f++) yield return null;
                bool hopLe = HopLeCua(i, ref c, out int ngoai, out _); string tenLan = vatBiLan;
                var b = Hien(k, c.hien, c.rot, c.lat, c.chiDau);
                for (int f = 0; f < 20; f++) yield return null;
                DoCua(b, i, k, out int truoc, out int lo, out int dauRa);
                // điểm không hợp lệ thì trò chơi tự bỏ qua (ThuLo) — chỉ tính LỖI khi hợp lệ mà vẫn lấn mặt tường / lộ thân
                bool ok = !hopLe || (truoc == 0 && lo == 0);
                if (hopLe) { tong++; if (!ok) loi++; }
                log.AppendLine($"cửa {i,2} {(cuaRef[i] ? cuaRef[i].ten : ""),-26}: {(hopLe ? "DÙNG" : "bỏ  ")} · lấn mặt tường {truoc} · thân lộ {lo} · đầu ló {dauRa} · đè vật khác {ngoai} {tenLan}→ {(ok ? "OK" : "LỖI")}");
                An();
            }

        // 2. chụp từng khung: 6 lần lấp ló thật (trượt ra · đứng · rụt vào)
        GhiTam();
        log.AppendLine("\n— 2. KHUNG HÌNH LẤP LÓ (trượt ra 0,15/0,35/0,55 s · đứng 1,6 s · rụt 0,1 s) —");
        int chup = 0;
        int nCua = cuaP?.Length ?? 0;
        for (int j = 0; j < nCua * 2 && chup < 6; j++)   // lượt đầu: điểm WC bố mẹ (chỉ ló đầu) — lượt sau: các cửa khác
        {
            int i = j % nCua; if ((j < nCua) != LaWCBoMe(cuaRef[i])) continue;
            if (cuaRef[i] && !cuaRef[i].DangMo) continue;
            int k = LO_DAU;
            TinhCua(i, k, out var c);
            if (!HopLeCua(i, ref c, out _, out _)) continue;
            var dung = ChoDung(i, 4.3f);
            var ngam = cuaP[i] + cuaD[i] * 0.05f + Vector3.up * 1.72f;
            fp.DatTuThe(dung, ngam); yield return null;
            if (!ThayDuoc(ngam, 0.15f)) continue;
            loCo = StartCoroutine(Lo(c));
            float t0 = Time.time; int fi = 0; int maxTruoc = 0;
            foreach (var moc in new[] { 0.15f, 0.35f, 0.55f, 1.6f })
            {
                while (Time.time - t0 < moc) yield return null;
                if (dangHien != null) { DoCua(dangHien, i, k, out int tr, out _, out _); maxTruoc = Mathf.Max(maxTruoc, tr); }
                Chup($"lo{i:00}_f{fi++}");
            }
            var gan = cuaP[i] + cuaN[i] * 2.4f; gan.y = player.position.y; fp.DatTuThe(gan, ngam);   // bước lại gần → rụt
            yield return new WaitForSeconds(0.1f); Chup($"lo{i:00}_f{fi++}");
            while (dangLo) yield return null;
            log.AppendLine($"cửa {i} {cuaRef[i]?.ten}: lấn mặt tường lớn nhất trong các khung {maxTruoc}");
            chup++;
            yield return new WaitForSeconds(0.6f);
        }

        // 3. lan can
        GhiTam();
        log.AppendLine("\n— 3. LAN CAN —");
        for (int i = 0; lanCanA != null && i < lanCanA.Length; i++)
        {
            var T = (lanCanA[i] + lanCanB[i]) / 2; var seg = lanCanB[i] - lanCanA[i]; seg.y = 0; var n = Vector3.Cross(Vector3.up, seg).normalized;
            bool ok = false;
            foreach (var sgn in new[] { 1f, -1f })
                for (float xa = 2.5f; xa <= 5.5f && !ok; xa += 1f)
                {
                    var q = T + n * sgn * xa;
                    if (!Physics.Raycast(q + Vector3.up * 0.2f, Vector3.down, out var s1, 6f) || s1.point.y > T.y - 1.4f) continue;
                    fp.DatTuThe(s1.point, T + Vector3.up * 0.1f); yield return null;
                    ok = ThuLo(LAN_CAN);
                }
            if (!ok) { log.AppendLine($"lan can {i}: không tìm được chỗ đứng phía dưới / {lyDoLo}"); continue; }
            float t0 = Time.time; int fi = 0;
            int vv = -1;
            foreach (var moc in new[] { 0.2f, 0.5f, 0.9f }) { while (Time.time - t0 < moc) yield return null; if (dangHien != null) vv = Mathf.Max(vv, DemTrongVat(dangHien, "LanCan")); Chup($"lancan{i}_f{fi++}"); }
            tong++; if (vv != 0) loi++;
            log.AppendLine($"lan can {i}: đỉnh lấn vào tường/đồ đạc {vv} → {(vv == 0 ? "OK" : "LỖI")}");
            if (loCo != null) StopCoroutine(loCo); An(); dangLo = false;
        }

        // 4. tắt đèn: đặt ma ngẫu nhiên ở 8 chỗ × 4 lần, đo lấn tường; chụp 1 khung mỗi tư thế lúc tối thật
        GhiTam();
        log.AppendLine("\n— 4. MẤT ĐIỆN: đặt ngẫu nhiên, đo lấn tường —");
        maTucGian = true; huDaHe = true;
        var cho = new[] { new Vector3(3.8f, 0f, 2.6f), new Vector3(3.8f, 0f, 12.5f), new Vector3(2.4f, 3.6f, 7.5f), new Vector3(5.4f, 3.6f, 15.0f),
                          new Vector3(3.4f, 3.6f, 2.5f), new Vector3(4.0f, 7.0f, 8.6f), new Vector3(3.8f, 7.0f, 4.0f), new Vector3(5.0f, 3.6f, 10.5f) };
        int datDuoc = 0, khongDat = 0;
        foreach (var p in cho)
            for (int l = 0; l < 4; l++)
            {
                fp.DatTuThe(p, p + Vector3.forward * 3 + Vector3.up * 1.4f); yield return null;
                if (!DatMaTrongToi()) { khongDat++; continue; }
                datDuoc++;
                for (int f = 0; f < 20; f++) yield return null;
                int vv = DemTrongVat(dangHien);
                tong++; if (vv != 0) loi++;
                if (vv != 0) log.AppendLine($"  LỖI kiểu {dangHien.kieu} ở {dangHien.w.position:F2} (người chơi {p:F2}): {vv} đỉnh lấn vào {vatBiLan}");
                An();
            }
        log.AppendLine($"đặt được {datDuoc} lần, không tìm được chỗ {khongDat} lần");
        foreach (var (kieu, p) in new[] { (VAN, cho[1]), (XOM, cho[2]), (TRAN, cho[0]) })
        {
            fp.DatTuThe(p, p + Vector3.forward * 3 + Vector3.up * 1.4f); yield return null;
            lyDoToi = "";
            StartCoroutine(TatDen(kieu));
            for (float w = 0; w < nhayTruoc + 2f && dangHien == null && lyDoToi == ""; w += Time.deltaTime) yield return null;
            if (dangHien != null)
            {
                fp.DatTuThe(player.position, dangHien.w.position + Vector3.up * (kieu == TRAN ? -0.4f : kieu == XOM ? 0.8f : 1.6f));
                yield return null; yield return null; Chup($"toi_{kieu}");
            }
            while (dangToi) yield return null;
        }

        // 5. hé hũ → phòng Nhím: chụp mỗi 0,3 s từ lúc sáng lại tới lúc ma biến
        GhiTam();
        log.AppendLine("\n— 5. HÉ HŨ → PHÒNG NHÍM —");
        var hu = FindAnyObjectByType<LocHuCot>();
        if (hu)
        {
            dem = 3; maTucGian = false; huDaHe = false;
            var hp = hu.GetComponentInChildren<Collider>().bounds.center;
            var ben = hp + Vector3.forward * 1.1f; ben.y = -2.3f; fp.DatTuThe(ben, hp);
            yield return null; Chup("hu_0_truoc");
            HeHu(hu);
            for (int k = 0; !huDaHe && k < 40; k++) { yield return new WaitForSeconds(0.5f); Chup($"hu_f{k:00}"); }
            while (LocFirstPerson.den < 0.99f) yield return null;
            while (dangHien == null && dangCanh) yield return null;
            int vv = dangHien != null ? DemTrongVat(dangHien) : -1;
            tong++; if (vv != 0) loi++;
            log.AppendLine($"ma trong phòng Nhím ở {(dangHien != null ? dangHien.w.position.ToString("F2") : "—")}: đỉnh lấn vào tường/đồ đạc {vv} → {(vv == 0 ? "OK" : "LỖI")}");
            int fi = 0;
            while (dangCanh && fi < 20) { Chup($"nhim_f{fi++:00}"); yield return new WaitForSeconds(0.3f); }
            while (dangCanh) yield return null;
        }

        // 6. chết
        fp.DatTuThe(cho[0], cho[0] + Vector3.forward * 3 + Vector3.up * 1.4f); yield return null;
        StartCoroutine(TatDen(VAN));
        for (float w = 0; w < nhayTruoc + 2f && dangHien == null; w += Time.deltaTime) yield return null;
        if (dangHien != null) { var to = chanMa - player.position; to.y = 0; fp.DatTuThe(chanMa - to.normalized * 1.5f, chanMa + Vector3.up * 1.5f); }
        yield return new WaitForSeconds(0.5f);
        log.AppendLine($"\nthử chết (đứng cách 1,5 m lúc tối): {(dangChet ? "ĐÃ CHẾT — đúng" : "chưa chết — SAI")}");
        log.Insert(0, $"TỔNG: {tong - loi}/{tong} chỗ không lấn tường\n");
        yield return new WaitForSeconds(8f); Chup("chet");
        Application.logMessageReceived -= batLoi;
        System.IO.File.WriteAllText("LOC_KiemTra/Ma/BaoCao_TuKiem.txt", log.ToString());
        UnityEditor.EditorApplication.isPlaying = false;
    }

    void Chup(string ten)
    {
        var c = cam.GetComponent<Camera>(); const int W = 960, H = 540;
        var rt = new RenderTexture(W, H, 24); var old = c.targetTexture; c.targetTexture = rt; c.Render();
        RenderTexture.active = rt; var t = new Texture2D(W, H, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, W, H), 0, 0); t.Apply();
        System.IO.File.WriteAllBytes($"LOC_KiemTra/Ma/{ten}.png", t.EncodeToPNG());
        RenderTexture.active = null; c.targetTexture = old; Destroy(t); rt.Release();
    }
#endif
}
