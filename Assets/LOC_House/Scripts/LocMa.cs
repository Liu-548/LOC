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
    public static void Reset0() { khoiTao = false; huDaHe = maTucGian = tamDung = false; dem = 0; }
    public static void VaoDem(int d) { Reset0(); dem = d; khoiTao = true; }   // [22/10] LocMenu: vào game ở đêm d

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
    public Vector2 khoangLo = new(90, 120);   // [v3.3] 90–120 s/lần, không giới hạn số lần (chốt 25/10)
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
    public bool hienDebug = false;   // chữ thử góc trên màn hình — bật trong Inspector khi cần

    const int LO_DAU = 0, TAY = 1, LAN_CAN = 2, VAN = 3, XOM = 4, TRAN = 5;
    static readonly float[] ROLL = { -20, 0, 15, 70, -10, 0 };
    static readonly float[] XOAY_MAX = { 30, 0, 45, 120, 110, 150 };   // độ xoay đầu tối đa theo tư thế — lấp ló sau mép thì gần như không xoay để sọ không lấn ra mặt tường
    public float sauMat = 0.26f;   // lấp ló nửa đầu: mặt tường phía người chơi nằm trước gốc bóng bao nhiêu mét (lớn = lùi sâu, không lấn tường)

    class Bong { public bool coDinh; public Transform mo; public Quaternion moGoc; public Vector3 moPos; public int kieu; public Transform w, dau; public Vector3 face, upL, matLocal, huong; public Quaternion nghi, dauGoc; public Vector3 dauPos; public Material than; public Bounds bLocal; public Material mat, hao; public Transform[] quad; }
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
        TaoBong(); TaoAmThanh(); hDoa = Random.Range(35f, 70f);
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
            bool giu = false;   // [23/10] model ma nữ (vật liệu tên chứa "MaNu", Editor/LocMaNu.cs) giữ vật liệu riêng — không phủ khói đen
            foreach (var r in m.GetComponentsInChildren<Renderer>(true)) foreach (var sm in r.sharedMaterials) if (sm && sm.name.Contains("MaNu")) giu = true;
            foreach (var r in m.GetComponentsInChildren<Renderer>(true))
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
                if (r.name.StartsWith("Mat_")) r.sharedMaterial = b.mat; else if (!giu) r.sharedMaterial = b.than;
            }
            bool coB = false;   // hộp bao thân trong hệ toạ độ của bóng (để chặn xuyên tường)
            foreach (var r in m.GetComponentsInChildren<Renderer>(true))
            {
                if (r.name.StartsWith("Mat_")) continue;
                var bb = r.bounds; var c = b.w.InverseTransformPoint(bb.center);
                if (!coB) { b.bLocal = new Bounds(c, bb.size); coB = true; } else b.bLocal.Encapsulate(new Bounds(c, bb.size));
            }
            b.dau = Tim(m, "Dau"); if (b.dau) { b.dauGoc = b.dau.localRotation; b.dauPos = b.dau.localPosition; }
            b.coDinh = giu;   // [23/10] ma nữ: đầu và tóc là một khối — không dùng HuongDau (xoay lớn làm tóc văng), có chuyển động riêng: DongMaNu
            b.mo = m; b.moGoc = m.localRotation; b.moPos = m.localPosition;
            var eyes = new[] { Tim(m, "Mat_L"), Tim(m, "Mat_R") };
            b.quad = new Transform[2];
            for (int k = 0; k < 2; k++)
            {
                if (!eyes[k]) continue;
                var q = GameObject.CreatePrimitive(PrimitiveType.Quad); Destroy(q.GetComponent<Collider>());
                q.name = "Hao"; q.transform.SetParent(eyes[k], false); q.transform.localPosition = Vector3.zero;
                q.transform.localScale = Vector3.one * 0.10f / Mathf.Max(1e-4f, eyes[k].lossyScale.x);
                var qr = q.GetComponent<Renderer>(); qr.sharedMaterial = b.hao; qr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                b.quad[k] = q.transform;
            }
            if (b.dau && eyes[0] && eyes[1])
            {
                var mid = (eyes[0].position + eyes[1].position) / 2;
                b.face = Quaternion.Inverse(b.dau.rotation) * (mid - b.dau.position);
                var hu = Tim(m, "MatHuong");   // [23/10] ma nữ: điểm ngay trước mặt → hướng mặt thật (không phải hướng cổ→mắt)
                //  tính trong không gian của cha khối Dau (model có thể bị lật gương sc.x = −1 → không dùng rotation thế giới)
                var cha = b.dau.parent;
                var fP = cha.InverseTransformVector(hu ? hu.position - mid : mid - b.dau.position).normalized;
                b.huong = Quaternion.Inverse(b.dau.localRotation) * fP;
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
        b.hao.color = new Color(c.r, c.g, c.b, maTucGian ? 0.9f : 0.55f);
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
        if (b.dau && !b.coDinh) { if (latX) b.dau.localRotation = b.dauGoc; else b.dau.rotation = HuongDau(b); }
        if (b.coDinh) { if (b.dau) { b.dau.localRotation = b.dauGoc; b.dau.localPosition = b.dauPos; } mnYaw = 0; mnPitch = 0; mnMoi = true; mnGiatSau = Time.time + Random.Range(0.6f, 2f); }
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

    // [23/10] ma nữ: chuyển động (đầu + tóc là một khối "Dau")
    //  · mặt NHÌN THẲNG vào người chơi: quay ngang (yaw) + ngẩng/cúi (pitch) theo mắt camera, trong giới hạn từng tư thế
    //    vừa hiện là đã nhìn mình · đang bị nhìn thì vẫn bám theo (nhanh, có trễ nhẹ) · ngoảnh đi thì khoá ngay vào người chơi
    //  · thỉnh thoảng giật cục (nghiêng 3–7° rồi về) · thân nhún/lắc rất nhẹ như đang thở
    static readonly float[] MANU_YAW = { 8, 0, 40, 75, 60, 55 };    // LO_DAU nhỏ: đầu đang ló sát mép tường
    static readonly float[] MANU_PITCH = { 6, 0, 25, 30, 35, 30 };   // LAN_CAN: đứng thẳng sau lan can, đầu cúi nhìn xuống
    float mnYaw, mnPitch, mnGiat, mnGiatHet, mnGiatSau; bool mnMoi;
    void DongMaNu(Bong b)
    {
        float t = Time.time, dt = Time.deltaTime;
        if (b.mo) { b.mo.localRotation = b.moGoc * Quaternion.Euler(Mathf.Sin(t * 0.7f) * 0.8f, 0, Mathf.Sin(t * 0.53f) * 1.2f); b.mo.localPosition = b.moPos + Vector3.up * (Mathf.Sin(t * 1.1f) * 0.006f); }
        if (!b.dau || b.huong.sqrMagnitude < 1e-8f) return;
        // mọi phép tính trong không gian của cha khối Dau (đúng cả khi model lật gương) — xoay xong gán localRotation
        var cha = b.dau.parent;
        var mid = (b.quad != null && b.quad[0] && b.quad[1]) ? (b.quad[0].position + b.quad[1].position) / 2 : b.dau.position;
        var tP = cha.InverseTransformVector(cam.position - mid).normalized;   // hướng tới camera
        var upP = cha.InverseTransformVector(Vector3.up).normalized;
        var f0 = b.dauGoc * b.huong;                                             // hướng mặt ở tư thế gốc
        // yaw: quay quanh trục đứng cho hình chiếu ngang của mặt trùng hướng camera
        var f0h = Vector3.ProjectOnPlane(f0, upP); var th = Vector3.ProjectOnPlane(tP, upP);
        float yaw = 0, pitch = 0;
        if (f0h.sqrMagnitude > 1e-6f && th.sqrMagnitude > 1e-6f) yaw = Mathf.Clamp(Vector3.SignedAngle(f0h, th, upP), -MANU_YAW[b.kieu], MANU_YAW[b.kieu]);
        var f1 = Quaternion.AngleAxis(yaw, upP) * f0;
        // pitch: ngẩng/cúi cho độ cao của hướng mặt bằng độ cao hướng camera
        float e1 = Mathf.Asin(Mathf.Clamp(Vector3.Dot(f1.normalized, upP), -1, 1)) * Mathf.Rad2Deg;
        float e2 = Mathf.Asin(Mathf.Clamp(Vector3.Dot(tP, upP), -1, 1)) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(e2 - e1, -MANU_PITCH[b.kieu], MANU_PITCH[b.kieu]);
        bool biNhin = Vector3.Angle(cam.forward, b.dau.position - cam.position) < 40f;
        if (mnMoi || !biNhin) { mnYaw = yaw; mnPitch = pitch; mnMoi = false; }
        else { float k = 1 - Mathf.Exp(-5f * dt); mnYaw = Mathf.LerpAngle(mnYaw, yaw, k); mnPitch = Mathf.LerpAngle(mnPitch, pitch, k); }
        if (t > mnGiatSau) { mnGiat = Random.Range(3f, 7f) * (Random.value < 0.5f ? -1 : 1); mnGiatHet = t + Random.Range(0.06f, 0.12f); mnGiatSau = t + Random.Range(1.5f, 4.5f); }
        float giat = t < mnGiatHet ? mnGiat : mnGiat * Mathf.Exp(-(t - mnGiatHet) * 14f);
        if (b.kieu == LO_DAU) giat *= 0.4f;   // đầu đang sát mép tường: giật nhẹ thôi
        var qy = Quaternion.AngleAxis(mnYaw, upP);
        var g1 = (qy * f0).normalized;
        float eg = Mathf.Asin(Mathf.Clamp(Vector3.Dot(g1, upP), -1, 1)) * Mathf.Rad2Deg + mnPitch;
        var gh = Vector3.ProjectOnPlane(g1, upP).normalized;
        var g2 = gh * Mathf.Cos(eg * Mathf.Deg2Rad) + upP * Mathf.Sin(eg * Mathf.Deg2Rad);
        var qp = gh.sqrMagnitude > 1e-6f ? Quaternion.FromToRotation(g1, g2) : Quaternion.identity;
        b.dau.localRotation = qp * qy * b.dauGoc * Quaternion.AngleAxis(giat, b.huong);
    }

    // [25/10] NHẠC NỀN LÀM LỄ DƯỚI HẦM (file người dùng đưa, đã nối thành vòng lặp 42 s):
    //  bật khi đang mở mini game lễ ở bàn thờ hầm — Dán chén (Đêm 2), Bày mâm, Giữ ngọn nhang (Đêm 3); vào/ra mờ dần.
    public float amLeHam = 0.8f;
    AudioSource amLe; AudioClip clipLe; bool daTimLe;
    void NhacLeHam()
    {
        if (!daTimLe) { daTimLe = true; clipLe = Resources.Load<AudioClip>("LOC_MaNu/Nen_LeHam"); }
        if (!clipLe) return;
        var mg = LocMiniGame.hienTai;
        bool le = mg is LocMG_GhepChen || mg is LocMG_MamCung || mg is LocMG_GiuLua;
        if (!amLe)
        {
            if (!le) return;
            amLe = gameObject.AddComponent<AudioSource>(); amLe.clip = clipLe; amLe.loop = true; amLe.spatialBlend = 0; amLe.playOnAwake = false; amLe.volume = 0;
        }
        float dich = le && !dangChet ? amLeHam : 0;
        amLe.volume = Mathf.MoveTowards(amLe.volume, dich, Time.unscaledDeltaTime * amLeHam / (le ? 2f : 2.5f));
        if (amLe.volume > 0 && !amLe.isPlaying) { amLe.time = Random.Range(0f, clipLe.length * 0.8f); amLe.Play(); }
        if (amLe.volume <= 0 && amLe.isPlaying) amLe.Stop();
    }
    void LateUpdate()
    {
        NhacLeHam();
        if (dangHien == null || !cam) return;
        var b = dangHien;
        if (b.dau && !b.coDinh && b.w.localScale.x > 0) b.dau.rotation = Quaternion.Slerp(b.dau.rotation, HuongDau(b), 1 - Mathf.Exp(-6f * Time.deltaTime));
        if (b.coDinh) DongMaNu(b);
        foreach (var q in b.quad) if (q) q.rotation = Quaternion.LookRotation(q.position - cam.position);
    }

    // ───── vòng chính
    void Update()
    {
        if (!cam || !player) return;
        PhimThu();
        if (dangChet) { if (choR && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) ChoiLai(); return; }
        if (dem < 2 || tamDung || dangCanh || dangToi || dangDoa || player.position.y < -0.6f) return;
        if (MaBatRadioUpdate()) return;
        if (maTatRadio && !choNguoiTat && !dangLo && demTatRadio != dem && LocRadio.I && LocRadio.I.DangBat && LocRadio.I.BatDuoc > nguongTatRadio && CoTheTatRadio()) { demTatRadio = dem; StartCoroutine(CanhTatRadio()); return; }
        if (doaBatNgo && !dangLo && TrongNha() && (hDoa -= Time.deltaTime) <= 0) { hDoa = ThuDoa() ? Random.Range(khoangDoa.x, khoangDoa.y) : 4f; return; }
        if (dem == 2 && !LocNhim.daHoangLoan) return;   // [v3.3] lấp ló chỉ bật sau đoạn hoảng loạn Đêm 2
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
                var T = Vector3.Lerp(lanCanA[i], lanCanB[i], Random.Range(0.45f, 0.55f));   // [23/10] gần giữa: lan can hẹp (0,88 m), sát tường bên phải
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
                var mat = c.hien + c.rot * bong[LAN_CAN].matLocal;   // [23/10] mắt thật của tư thế (đứng thẳng nhìn xuống)
                if (Vector3.Angle(cam.forward, mat - cam.position) > 75f || !ThayDuoc(mat, 0.1f)) continue;
                ds.Add(c);
            }
        if (ds.Count > 1) ds.RemoveAll(c => (c.hien - loCu).sqrMagnitude < 0.5f);   // [v3.3] không lặp lại điểm vừa dùng
        if (ds.Count == 0) return false;
        var chon = ds[Random.Range(0, ds.Count)]; loCu = chon.hien;
        loCo = StartCoroutine(Lo(chon));
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

    // [v3.3 C1.2b] phản ứng thân thể khi thấy lấp ló, nhạt dần: lần 1 camera kéo về phía nó + thở nhanh 2 s + "...Gì đấy." · lần 2 khựng + thở 1 s · lần 3 thở hắt · từ lần 4 thôi.
    //  Đếm chung cả game, chết rồi chơi lại không đếm lại (không reset ở Reset0).
    public static int soLanLapLo;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset1() => soLanLapLo = 0;
    Vector3 loCu = Vector3.one * 9999;
    public void LoSom() { hLo = Random.Range(15f, 40f); }   // lần lấp ló đầu tiên: trong vòng một phút sau đoạn hoảng loạn
    IEnumerator PhanUngLo(Vector3 mat)
    {
        int n = ++soLanLapLo; if (n > 3) yield break;
        var tho = LocNhim.I ? LocNhim.I.khoiTho : null;
        if (tho) am.PlayOneShot(tho, n == 3 ? 0.45f : 0.8f);
        if (n == 3) yield break;
        var fp = player.GetComponent<LocFirstPerson>(); if (!fp) yield break;
        LocFirstPerson.khoa = true;
        var tu = cam.position + cam.forward * 3f; var toi = n == 1 ? mat : Vector3.Lerp(tu, mat, 0.25f);
        for (float t = 0; t < 1; t += Time.deltaTime / (n == 1 ? 0.5f : 0.25f)) { fp.DatTuThe(player.position, Vector3.Lerp(tu, toi, t * (2 - t))); yield return null; }
        LocFirstPerson.khoa = false;
        if (n == 1) LocFirstPerson.PhuDe("...Gì đấy.", 2f);
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
        StartCoroutine(PhanUngLo(b.quad[0] ? b.quad[0].position : c.hien + Vector3.up * 1.7f));
        if (Random.value < 0.6f) PhatTai(khucKhich, MatMa(b), 0.45f, 14f);   // [25/10] khúc khích nhỏ từ mép cửa
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
        bool lanDau = !LocTui.Co("mat_dien_dau") && ep < 0;
        if (lanDau) { LocTui.Dat("mat_dien_dau"); if (!DatMaTruocMat()) DatMaTrongToi(ep); LocFirstPerson.PhuDe("Đứng yên. Bố viết thế.", 3f); }
        else DatMaTrongToi(ep);
        if (dangHien != null) PhatTai(cuoiXa, MatMa(dangHien), 0.7f, 18f);   // [25/10] cười vọng từ chỗ con ma trong tối
        for (float t = 0; t < thoiGianToi && !dangChet; t += Time.deltaTime)
        {
            if (dangHien != null)
            {
                var ph = player.position - chanMa; float dy = Mathf.Abs(ph.y); ph.y = 0;
                if (lanDau) { if (ph.magnitude < banKinhChet + 1.2f) break; }   // lần đầu: biến trước khi người chơi vào tới 2 m
                else if (ph.magnitude < banKinhChet && dy < 2.6f) { Chet(); yield break; }
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
        if (demDoa != dem) { demDoa = dem; daDoa.Clear(); }
        if (doaBatNgo && !dangDoa && !(moiKieuMotLanMoiDem && daDoa.Contains(KieuDoa.TuongAnToan)) && Random.value < tiLeTuongAnToan && ep < 0)
            StartCoroutine(CanhTuongAnToan(Random.Range(2.6f, 3.6f)));
    }

    // chọn chỗ ngẫu nhiên cách 2,5–4,5 m, đứng/ngồi trên sàn cùng tầng hoặc bò trên trần, mắt nằm trong tầm nhìn (quay đầu là thấy)
    public string lyDoToi = "";
    // đặt thử rồi đếm đỉnh lấn vào tường/đồ đạc; lấn thì cất đi
    bool DatThu(int k, Vector3 pos, Quaternion rot, bool lat = false, string boQuaCha = null)
    {
        var b = Hien(k, pos, rot, lat); if (b == null) return false;
        Physics.SyncTransforms();
        if (DemTrongVat(b, boQuaCha, 6) == 0) return true;
        An(); return false;
    }
    bool DatMaTruocMat()
    {
        var f = cam.forward; f.y = 0; f.Normalize();
        for (int thu = 0; thu < 60; thu++)
        {
            var dir = Quaternion.Euler(0, Random.Range(-30f, 30f), 0) * f;
            var Q = player.position + dir * (6f - thu / 10 * 0.5f);   // 6 m trước, không có chỗ thì lùi dần tới 3,5 m
            if (!Physics.Raycast(Q + Vector3.up * 1.2f, Vector3.down, out var san, 1.8f, ~0, QueryTriggerInteraction.Ignore) || Mathf.Abs(san.point.y - player.position.y) > 0.35f) continue;
            var G = san.point; var rot = Quaternion.LookRotation(-dir, Vector3.up);
            if (bong[VAN] == null || Vuong(bong[VAN], G, rot) || !ThayDuoc(G + Vector3.up * 1.78f)) continue;
            if (DatThu(VAN, G + Vector3.up * 0.03f, rot)) { chanMa = G; return true; }
        }
        return false;
    }
    bool DatMaTrongToi(int ep = -1)
    {
        var mask = ~0; var qi = QueryTriggerInteraction.Ignore;
        int kSan = 0, kTang = 0, kCho = 0, kNhin = 0; string vuong = "";
        for (int thu = 0; thu < 60; thu++)
        {
            float a = Random.Range(0f, 360f), r = Random.Range(Mathf.Min(cachMaKhiToi.x, 2.5f), Mathf.Min(cachMaKhiToi.y, 4.5f));   // [23/10] gần hơn để thấy rõ mặt
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

    // ═════ [23/10] DỌA BẤT NGỜ ═════
    //  1 SauLung    — đèn chớp một cái, ma đứng sát sau lưng (bám theo khi người chơi đi); quay lại → đứng yên nửa giây → tối phụt + thét + biến
    //  3 BamTran    — tiếng gõ cộc cộc + sột soạt trên trần sau lưng; nó bám trên trần (hiện lúc ngoài khung hình), quay lại ngẩng lên thấy nó nhìn chằm chằm xuống → tắt phụt, biến
    //  4 TuongAnToan — sau một lần mất điện, đèn sáng lại ~3 giây rồi tắt phụt, ma đứng ngay trước mặt
    //  + báo động giả: đèn chớp mà không có gì · mỗi kiểu một lần mỗi đêm · không dọa khi đang ló/mất điện/cảnh khoá
    public enum KieuDoa { SauLung, BamTran, TuongAnToan }   // [23/10] bỏ DenChop (đèn chớp tiến gần) theo yêu cầu
    [Header("Dọa bất ngờ")]
    public bool doaBatNgo = true;
    public Vector2 khoangDoa = new(45, 100);          // giây giữa hai lần dọa
    [Range(0f, 1f)] public float tiLeBaoGia = 0.3f;    // tỉ lệ chỉ chớp đèn mà không có gì
    public bool moiKieuMotLanMoiDem = true;
    [Range(0f, 1f)] public float tiLeTuongAnToan = 0.7f;   // sau mỗi lần mất điện
    float hDoa = 40; bool dangDoa; int demDoa = -1; readonly HashSet<KieuDoa> daDoa = new();
    [System.NonSerialized] public string doaPha = "";   // pha hiện tại (tự kiểm chụp đúng lúc)
    AudioClip amTach, amSoat, amDap, amThetTong, amGo; AudioSource am3D;
    // [25/10] giọng ma nữ (từ bản ghi người dùng đưa, cắt thẳng, không xử lý giọng) — Resources/LOC_MaNu: cười gần · khúc khích · cười vọng phòng bên · cười dài; tiếng thét giữ bản gốc của game. méo
    AudioClip cuoiGan, khucKhich, cuoiXa, cuoiCham, xuatHien; AudioSource cuoiDang;
    // [25/10] tiếng 'ma xuất hiện bất ngờ' (file người dùng đưa, đã làm to) — phát thẳng vào tai đúng lúc người chơi thấy nó
    void XuatHien() { Phat(xuatHien, 1f); HoangHot(); }
    // [25/10] nhân vật chính hoảng hốt thở dốc (file người dùng đưa) — mỗi lần gặp ma xuất hiện; đang thở rồi thì không chồng thêm
    AudioClip hoangHot; AudioSource amHH;
    void HoangHot()
    {
        if (!hoangHot) return;
        if (!amHH) { amHH = gameObject.AddComponent<AudioSource>(); amHH.playOnAwake = false; amHH.spatialBlend = 0; amHH.volume = 0.85f; }
        if (amHH.isPlaying) return;
        amHH.clip = hoangHot; amHH.Play();
    }
    // phát tiếng ma ĐÚNG CHỖ con ma (âm 3D: gần to, xa nhỏ, trái/phải rõ); tiếng thét sẽ cắt tiếng cười đang dở
    AudioSource PhatTai(AudioClip c, Vector3 p, float v = 1f, float xa = 22f)
    {
        if (!c) return null;
        var g = new GameObject("LOC_TiengMa"); g.transform.position = p;
        var a = g.AddComponent<AudioSource>(); a.clip = c; a.volume = v; a.spatialBlend = 1f; a.dopplerLevel = 0; a.spread = 40;
        a.rolloffMode = AudioRolloffMode.Linear; a.minDistance = 1.2f; a.maxDistance = xa; a.Play();
        Destroy(g, c.length + 0.3f); cuoiDang = a; return a;
    }
    void ImCuoi() { if (cuoiDang) Destroy(cuoiDang.gameObject); cuoiDang = null; }
    Dictionary<Light, bool> batD; Dictionary<GameObject, bool> vatD;

    void ChupDien()
    {
        batD = new Dictionary<Light, bool>(); foreach (var l in dens) if (l) batD[l] = l.enabled;
        vatD = new Dictionary<GameObject, bool>(); foreach (var g in vatSang) if (g) vatD[g] = g.activeSelf;
    }
    void DenDoa(bool on)
    {
        Dien(batD, vatD, on);
        RenderSettings.ambientLight = on ? ambient0 : ambient0 * ambientKhiToi;
        if (volToi) volToi.weight = on ? 0 : 1;
    }
    void Phat(AudioClip c, float v = 1f) { if (c) am.PlayOneShot(c, v); }
    void Thet() { ImCuoi(); Phat(tiengThet ? tiengThet : amThetTong, 1f); Phat(amDap, 0.8f); }

    // ── âm thanh tổng hợp (không cần file): tách điện · sột soạt · đập trầm · tiếng rít (khi chưa có tiengThet)
    static AudioClip TaoClip(string ten, float giay, System.Func<float, System.Random, float> f)
    {
        int sr = 44100, n = (int)(giay * sr); var d = new float[n]; var r = new System.Random(ten.GetHashCode());
        for (int i = 0; i < n; i++) d[i] = Mathf.Clamp(f(i / (float)sr, r), -1f, 1f);
        var c = AudioClip.Create(ten, n, 1, sr, false); c.SetData(d, 0); return c;
    }
    void TaoAmThanh()
    {
        cuoiGan = Resources.Load<AudioClip>("LOC_MaNu/Ma_Cuoi_Gan"); khucKhich = Resources.Load<AudioClip>("LOC_MaNu/Ma_KhucKhich");
        cuoiXa = Resources.Load<AudioClip>("LOC_MaNu/Ma_Cuoi_Xa"); cuoiCham = Resources.Load<AudioClip>("LOC_MaNu/Ma_Cuoi_Dai");
        xuatHien = Resources.Load<AudioClip>("LOC_MaNu/Ma_XuatHien"); hoangHot = Resources.Load<AudioClip>("LOC_MaNu/Nv_HoangHot");
        float N(System.Random r) => (float)(r.NextDouble() * 2 - 1);
        amTach = TaoClip("LOC_Tach", 0.12f, (t, r) => N(r) * Mathf.Exp(-t / 0.012f) * 0.7f + Mathf.Sin(2 * Mathf.PI * 120 * t) * Mathf.Exp(-t / 0.05f) * 0.35f);
        float truoc = 0, cong = 0, lp = 0;
        amSoat = TaoClip("LOC_SotSoat", 2.6f, (t, r) =>
        {
            float x = N(r); float hp = x - truoc; truoc = x;                      // lọc thông cao: tiếng cào khô
            if (r.NextDouble() < 1.0 / 2600) cong = r.NextDouble() < 0.6 ? 1 : 0;  // từng nhịp cào
            lp += (cong - lp) * 0.004f;
            return hp * lp * 0.55f * Mathf.Clamp01(t * 3) * Mathf.Clamp01((2.6f - t) * 3);
        });
        float l2 = 0;
        amDap = TaoClip("LOC_Dap", 1.2f, (t, r) => { l2 += (N(r) - l2) * 0.02f; return (Mathf.Sin(2 * Mathf.PI * (55 - 15 * t) * t) * 0.9f + l2 * 2.5f) * Mathf.Exp(-t / 0.45f) * Mathf.Clamp01(t * 400); });
        float ph1 = 0, ph2 = 0, l3 = 0;
        amThetTong = TaoClip("LOC_Thet", 1.5f, (t, r) =>
        {
            float fq = 900 + 500 * t + 40 * Mathf.Sin(2 * Mathf.PI * 7 * t);
            ph1 = (ph1 + fq / 44100f) % 1; ph2 = (ph2 + fq * 1.06f / 44100f) % 1;
            float x = N(r); l3 += (x - l3) * 0.3f;
            float s = (ph1 * 2 - 1) * 0.35f + (ph2 * 2 - 1) * 0.3f + (x - l3) * 0.35f;
            return (float)System.Math.Tanh(s * 1.8f * Mathf.Exp(-t / 0.5f) * Mathf.Clamp01(t * 250)) * 0.8f;
        });
        // gõ cộc… cộc… cộc trên trần (gỗ, trầm)
        amGo = TaoClip("LOC_Go", 1.1f, (t, r) =>
        {
            float s = 0;
            foreach (var t0 in new[] { 0f, 0.42f, 0.78f }) { float u = t - t0; if (u >= 0) s += (Mathf.Sin(2 * Mathf.PI * 95 * u) * 0.8f + N(r) * 0.25f) * Mathf.Exp(-u / 0.06f); }
            return s * 0.9f;
        });
        var g = new GameObject("LocMa_Am3D"); g.transform.SetParent(transform, false);
        am3D = g.AddComponent<AudioSource>(); am3D.playOnAwake = false; am3D.spatialBlend = 1; am3D.minDistance = 1.2f; am3D.maxDistance = 14f; am3D.rolloffMode = AudioRolloffMode.Linear;
    }

    Vector3 TruocMat() { var f = cam.forward; f.y = 0; return f.sqrMagnitude < 1e-4f ? player.forward : f.normalized; }
    bool SanCungTang(Vector3 q, out Vector3 san)
    {
        san = q;
        if (!Physics.Raycast(q + Vector3.up * 1.2f, Vector3.down, out var h, 1.8f, ~0, QueryTriggerInteraction.Ignore)) return false;
        san = h.point; return Mathf.Abs(h.point.y - player.position.y) < 0.3f;
    }
    Vector3 MatMa(Bong b) => b.quad != null && b.quad[0] && b.quad[1] ? (b.quad[0].position + b.quad[1].position) / 2 : b.w.position + Vector3.up * 1.7f;
    bool DuocNhin(Bong b, float goc) { var m = MatMa(b); return Vector3.Angle(cam.forward, m - cam.position) < goc && ThayDuoc(m, 0.25f); }
    // đứng ở san, quay mặt về phía người chơi — hợp lệ nếu không vướng, không lấn, không có tường chắn giữa
    bool TrongNhaDiem(Vector3 q)   // chỗ đặt ma phải ở trong nhà (không ra sân, ra đường)
    {
        if (vungNha == null || vungNha.Length == 0) return true;
        foreach (var v in vungNha) if (v.Contains(q + Vector3.up * 0.5f)) return true;
        return false;
    }
    bool DungDuoc(int k, Vector3 san, Vector3 nhinVe, out Quaternion rot)
    {
        var h = nhinVe - san; h.y = 0; rot = Quaternion.LookRotation(h.sqrMagnitude > 1e-4f ? h.normalized : Vector3.forward, Vector3.up);
        if (!TrongNhaDiem(san)) return false;
        if (Physics.Linecast(cam.position, san + Vector3.up * 1.5f, ~0, QueryTriggerInteraction.Ignore)) return false;
        if (Vuong(bong[k], san, rot)) return false;
        if (!DatThu(k, san + Vector3.up * 0.03f, rot)) return false;
        An(); return true;
    }

    bool ChoSauLung(out Vector3 pos, out Quaternion rot, bool chiThu = false)
    {
        pos = default; rot = default; var f = TruocMat();
        foreach (var kc in new[] { 1.05f, 1.25f, 0.95f })
            foreach (var lech in new[] { 0f, -25f, 25f })
            {
                var q = player.position - Quaternion.Euler(0, lech, 0) * f * kc;
                if (!SanCungTang(q, out var san)) continue;
                if (chiThu) { var hh = player.position - san; hh.y = 0; rot = Quaternion.LookRotation(hh.normalized); if (!Vuong(bong[VAN], san, rot) && !Physics.Linecast(cam.position, san + Vector3.up * 1.5f, ~0, QueryTriggerInteraction.Ignore)) { pos = san + Vector3.up * 0.03f; return true; } continue; }
                if (DungDuoc(VAN, san, player.position, out rot)) { pos = san + Vector3.up * 0.03f; return true; }
            }
        return false;
    }
    bool ChoBamTran(out Vector3 tren, out Quaternion rot)
    {
        // trên trần, ưu tiên phía sau / hai bên người chơi (ngoài khung hình) — nghe tiếng mới quay lại, ngẩng lên thấy
        tren = default; rot = default; var f0 = TruocMat(); var b = bong[TRAN];
        foreach (var huong in new[] { 180f, 150f, -150f, 110f, -110f })
            foreach (var kc in new[] { 1.7f, 2.2f, 1.3f, 2.8f })
            {
                var f = Quaternion.Euler(0, huong, 0) * f0;
                if (!SanCungTang(player.position + f * kc, out var san) || !TrongNhaDiem(san)) continue;
                if (!Physics.Raycast(san + Vector3.up * 1.0f, Vector3.up, out var tr, 3.2f, ~0, QueryTriggerInteraction.Ignore)) continue;
                float h = tr.point.y - san.y; if (h < 2.3f || h > 3.9f) continue;
                var hh = player.position - san; hh.y = 0; rot = Quaternion.LookRotation(hh.normalized, Vector3.up) * Quaternion.Euler(0, 0, 180);   // đầu hướng về người chơi
                tren = tr.point - Vector3.up * 0.03f;
                if (Vuong(b, tren, rot)) continue;
                if (Physics.Linecast(cam.position, tren + rot * b.matLocal, ~0, QueryTriggerInteraction.Ignore)) continue;   // ngẩng lên phải thấy mặt
                if (!DatThu(TRAN, tren, rot)) continue;
                bool trongKhung = TrongKhung(dangHien); An();
                if (trongKhung) continue;
                return true;
            }
        return false;
    }
    bool ChoTruocMat(out Vector3 pos, out Quaternion rot)
    {
        pos = default; rot = default; var f = TruocMat();
        foreach (var kc in new[] { 1.35f, 1.6f, 1.15f })
            foreach (var lech in new[] { 0f, -15f, 15f })
            {
                if (!SanCungTang(player.position + Quaternion.Euler(0, lech, 0) * f * kc, out var san)) continue;
                if (DungDuoc(VAN, san, player.position, out rot)) { pos = san + Vector3.up * 0.03f; return true; }
            }
        return false;
    }

    bool ThuDoa(int ep = -1)
    {
        if (demDoa != dem) { demDoa = dem; daDoa.Clear(); }
        if (ep < 0 && Random.value < tiLeBaoGia) { StartCoroutine(BaoGia()); return true; }
        var ds = new List<KieuDoa> { KieuDoa.SauLung, KieuDoa.BamTran };
        for (int i = 0; i < ds.Count; i++) { int j = Random.Range(i, ds.Count); (ds[i], ds[j]) = (ds[j], ds[i]); }
        foreach (var k in ds)
        {
            if (ep >= 0 && (int)k != ep) continue;
            if (ep < 0 && moiKieuMotLanMoiDem && daDoa.Contains(k)) continue;
            if (k == KieuDoa.SauLung && ChoSauLung(out var p1, out var r1, true)) { StartCoroutine(CanhSauLung()); return true; }
            if (k == KieuDoa.BamTran && ChoBamTran(out var t3, out var r3)) { StartCoroutine(CanhBamTran(t3, r3)); return true; }
        }
        if (ep == (int)KieuDoa.TuongAnToan) { StartCoroutine(CanhTuongAnToan(0.5f)); return true; }
        return false;
    }

    IEnumerator BaoGia()
    {
        dangDoa = true; doaPha = "baogia"; ChupDien();
        int lan = Random.Range(1, 3);
        for (int i = 0; i < lan; i++) { DenDoa(false); Phat(amTach, 0.7f); yield return new WaitForSeconds(Random.Range(0.06f, 0.14f)); DenDoa(true); yield return new WaitForSeconds(Random.Range(0.08f, 0.3f)); }
        doaPha = ""; dangDoa = false;
    }

    IEnumerator CanhSauLung(bool chopDen = true)
    {
        dangDoa = true; doaPha = "saulung"; ChupDien();
        if (chopDen) { DenDoa(false); Phat(amTach, 0.8f); }
        Bong b = null;
        if (ChoSauLung(out var pos, out var rot)) b = Hien(VAN, pos, rot);   // (ChoSauLung thử đặt rồi cất — Hien lại ở chỗ đã chọn)
        if (chopDen) { yield return new WaitForSeconds(0.12f); DenDoa(true); }
        if (b == null) { doaPha = ""; dangDoa = false; yield break; }
        PhatTai(khucKhich, MatMa(b), 0.55f, 10f);   // [25/10] khúc khích nhỏ sau gáy
        doaPha = "saulung_cho";
        bool thay = false; float t0 = Time.time, kiemLai = 0;
        while (Time.time - t0 < 12f && !dangCanh && !dangChet)
        {
            if (DuocNhin(b, 32f)) { thay = true; break; }
            // người chơi đi tiếp → nó lặng lẽ bám theo sau lưng (chỉ dời khi chỗ mới hợp lệ và người chơi không nhìn)
            var xa = player.position - b.w.position; xa.y = 0;
            if (xa.magnitude > 1.6f && (kiemLai -= Time.deltaTime) <= 0)
            {
                kiemLai = 0.25f;
                bool moi = ChoSauLung(out var p2, out var r2);      // thử đặt có Hien/An bóng → luôn hiện lại sau đó
                if (moi) { pos = p2; rot = r2; }
                b = Hien(VAN, pos, rot);
                if (!moi && xa.magnitude > 3.2f) break;
            }
            yield return null;
        }
        if (thay)
        {
            doaPha = "saulung_thay"; XuatHien();
            PhatTai(cuoiGan, MatMa(b), 1f);            // quay lại → nó cười sát mặt
            yield return new WaitForSeconds(0.9f);
            DenDoa(false); Thet(); An();
            yield return new WaitForSeconds(0.4f);
            DenDoa(true);
        }
        else An();
        daDoa.Add(KieuDoa.SauLung); doaPha = ""; dangDoa = false;
    }

    bool TrongKhung(Bong b)
    {
        var c = cam.GetComponent<Camera>(); if (!c || b == null) return false;
        var pl = GeometryUtility.CalculateFrustumPlanes(c);
        foreach (var r in b.w.GetComponentsInChildren<Renderer>()) if (r.enabled && GeometryUtility.TestPlanesAABB(pl, r.bounds)) return true;
        return false;
    }
    IEnumerator CanhBamTran(Vector3 tren, Quaternion rot)
    {
        dangDoa = true; doaPha = "bamtran"; ChupDien();
        // 1. tiếng động trên trần: gõ cộc… cộc… rồi sột soạt
        am3D.transform.position = tren; am3D.PlayOneShot(amGo, 1f);
        yield return new WaitForSeconds(1.1f);
        am3D.PlayOneShot(amSoat, 0.9f);
        // 2. hiện ra bám trên trần — chỉ lúc người chơi chưa nhìn tới chỗ đó (không hiện bụp trước mắt)
        Bong bb = null;
        for (float t = 0; t < 6f && !dangCanh && !dangChet; t += Time.deltaTime)
        {
            bb = Hien(TRAN, tren, rot);
            if (!TrongKhung(bb)) break;   // cả thân lẫn tóc đều ngoài khung hình mới cho hiện
            An(); bb = null; yield return null;
        }
        if (bb == null || dangCanh || dangChet) { An(); daDoa.Add(KieuDoa.BamTran); doaPha = ""; dangDoa = false; yield break; }
        doaPha = "bamtran_hien";
        // 3. chờ người chơi ngẩng lên nhìn — nó nhìn chằm chằm xuống (đầu quay theo người chơi), bất động
        bool thay = false;
        for (float t = 0; t < 9f && !dangCanh && !dangChet; t += Time.deltaTime)
        {
            if (DuocNhin(bb, 30f)) { thay = true; break; }
            yield return null;
        }
        if (thay)
        {
            doaPha = "bamtran_nhin"; XuatHien();
            Phat(amDap, 0.5f); PhatTai(cuoiGan, MatMa(bb), 1f);   // [25/10] cười từ trên trần
            yield return new WaitForSeconds(2.0f);                      // nhìn nhau
            DenDoa(false); Phat(amTach); Thet(); An();                  // tắt phụt — biến
            yield return new WaitForSeconds(0.35f); DenDoa(true);
        }
        else An();
        daDoa.Add(KieuDoa.BamTran); doaPha = ""; dangDoa = false;
    }

    IEnumerator CanhTuongAnToan(float cho = 3f)
    {
        dangDoa = true; doaPha = "antoan";
        yield return new WaitForSeconds(cho);   // đèn đã sáng lại, người chơi thở phào…
        if (dangCanh || dangChet || dangToi || !ChoTruocMat(out var pos, out var rot)) { doaPha = ""; dangDoa = false; yield break; }
        ChupDien();
        DenDoa(false); Phat(amTach, 1f);
        Hien(VAN, pos, rot); doaPha = "antoan_hien"; XuatHien();
        yield return new WaitForSeconds(1.0f);
        Thet(); yield return new WaitForSeconds(0.25f);
        An(); yield return new WaitForSeconds(0.5f);
        DenDoa(true);
        daDoa.Add(KieuDoa.TuongAnToan); doaPha = ""; dangDoa = false;
    }

    // ═════ [23/10] MA TẮT RADIO ═════
    //  radio đang phát (≥ 25–60 s), người chơi cùng tầng, quay lưng lại radio → băng kéo chậm, méo dần rồi tắt phụt → im lặng…
    //  ma đứng cạnh radio nhìn về phía người chơi (chỉ hiện lúc ngoài khung hình) → quay lại thấy nó → radio xẹt chói, tắt đèn, thét, biến
    //  mỗi đêm một lần · trong lúc đó không bật lại được radio
    [Header("Ma tắt radio")]
    public bool maTatRadio = true;
    public Vector2 choTatRadio = new(25, 60);   // radio phát bao lâu thì ma mới tắt
    float nguongTatRadio = 40; int demTatRadio = -1;
    bool CoTheTatRadio()
    {
        var R = LocRadio.I; var d = R.ViTri - cam.position; float dy = Mathf.Abs(R.ViTri.y - 1.9f - player.position.y);
        if (dy > 1.5f) return false;                                   // khác tầng
        var dh = d; dh.y = 0; if (dh.magnitude < 2.5f || dh.magnitude > 14f) return false;
        return Vector3.Angle(cam.forward, d) > 65f;                     // không nhìn về phía radio
    }
    bool ChoCanhRadio(out Vector3 pos, out Quaternion rot)
    {
        pos = default; rot = default; var R = LocRadio.I;
        var g = R.ViTri; g.y = player.position.y;
        var huong = player.position - g; huong.y = 0; huong = huong.sqrMagnitude > 1e-4f ? huong.normalized : Vector3.forward;
        var ngang = Vector3.Cross(Vector3.up, huong);
        foreach (var xa in new[] { 0.75f, 1.0f, 1.3f })
            foreach (var lech in new[] { 0f, 0.5f, -0.5f, 0.85f, -0.85f })
            {
                if (!SanCungTang(g + huong * xa + ngang * lech, out var san) || !TrongNhaDiem(san)) continue;
                if (DungDuoc(VAN, san, player.position, out rot)) { pos = san + Vector3.up * 0.03f; return true; }
            }
        return false;
    }
    IEnumerator CanhTatRadio()
    {
        var R = LocRadio.I; if (!R) yield break;
        dangDoa = true; doaPha = "tatradio"; ChupDien(); R.khoa = true;
        nguongTatRadio = Random.Range(choTatRadio.x, choTatRadio.y);
        yield return R.KeoBang(1.4f);                 // băng kéo chậm → tắt phụt
        doaPha = "tatradio_im";
        PhatTai(cuoiXa, R.ViTri, 0.55f, 18f);         // [25/10] im lặng… rồi tiếng cười vọng nhỏ từ phía radio
        yield return new WaitForSeconds(1.6f);
        Bong b = null;
        if (!dangCanh && !dangChet && ChoCanhRadio(out var pos, out var rot))
            for (float t = 0; t < 8f && !dangCanh && !dangChet; t += Time.deltaTime)
            {
                b = Hien(VAN, pos, rot);
                if (!TrongKhung(b)) break;           // chỉ hiện lúc người chơi chưa nhìn tới
                An(); b = null; yield return null;
            }
        if (b != null)
        {
            doaPha = "tatradio_dung";
            bool thay = false;
            for (float t = 0; t < 12f && !dangCanh && !dangChet; t += Time.deltaTime)
            {
                if (DuocNhin(b, 30f)) { thay = true; break; }
                yield return null;
            }
            if (thay)
            {
                doaPha = "tatradio_thay"; XuatHien();
                yield return new WaitForSeconds(1.1f);    // nó đứng cạnh radio nhìn mình
                R.Re(1f); DenDoa(false); Thet(); An();     // radio xẹt chói, tắt đèn, thét — biến
                yield return new WaitForSeconds(0.45f); DenDoa(true);
            }
            else An();
        }
        R.khoa = false; doaPha = ""; dangDoa = false;
    }

    // ═════ [25/10] MA TỰ BẬT RADIO ═════
    //  người chơi ở xa radio (khác tầng hoặc > 7 m) → radio rè xẹt rồi tự phát nhạc (mỗi đêm một lần, sau 50–120 s yên).
    //  Người chơi xuống bấm E tắt → có tiLeSauLungKhiTat khả năng: im lặng 1,2–2 s → ma đứng ngay sau lưng
    //  (khúc khích sau gáy) → quay lại thấy → tiếng xuất hiện + thở dốc → tắt phụt, thét, biến. Không quay lại thì nó bám theo ~12 s rồi biến.
    [Header("Ma tự bật radio")]
    public bool maBatRadio = true;
    public Vector2 choBatRadio = new(50, 120);
    [Range(0f, 1f)] public float tiLeSauLungKhiTat = 0.6f;
    float hBatRadio = -1; int demBatRadio = -1; bool choNguoiTat;
    bool MaBatRadioUpdate()
    {
        var R = LocRadio.I;
        if (!maBatRadio || !R || R.DangBat || R.khoa || dangLo || demBatRadio == dem) return false;
        if (hBatRadio < 0) hBatRadio = Random.Range(choBatRadio.x, choBatRadio.y);
        var d = R.ViTri - player.position; float dy = Mathf.Abs(R.ViTri.y - 1.9f - player.position.y); d.y = 0;
        if (dy < 1.5f && d.magnitude < 7f) return false;      // người chơi đang ở gần radio → chờ họ đi xa
        if ((hBatRadio -= Time.deltaTime) > 0) return false;
        demBatRadio = dem; hBatRadio = -1; MaBatRadio(); return true;
    }
    void MaBatRadio()
    {
        var R = LocRadio.I; if (!R || R.DangBat) return;
        R.MaBat(); choNguoiTat = true;
        PhatTai(khucKhich, R.ViTri, 0.35f, 12f);           // một tiếng khúc khích khẽ cạnh radio
        Bao("ma tự bật radio");
    }
    void KhiNguoiChoiBamRadio(bool bat)
    {
        if (bat) { choNguoiTat = false; return; }           // người chơi tự bật lại → thôi
        if (!choNguoiTat) return;
        choNguoiTat = false;
        if (Random.value < tiLeSauLungKhiTat && !dangDoa && !dangCanh && !dangToi && !dangChet) StartCoroutine(SauLungKhiTatRadio());
    }
    IEnumerator SauLungKhiTatRadio()
    {
        dangDoa = true;                                     // giữ chỗ, không cho cảnh khác chen
        yield return new WaitForSeconds(Random.Range(1.2f, 2.0f));   // tắt xong… im lặng
        dangDoa = false;
        if (dangCanh || dangToi || dangChet || !ChoSauLung(out _, out _, true)) yield break;
        yield return CanhSauLung(false);                    // không chớp đèn: nó lặng lẽ đứng sau lưng
    }
    void OnEnable() { LocRadio.NguoiChoiBam += KhiNguoiChoiBamRadio; }
    void OnDisable() { LocRadio.NguoiChoiBam -= KhiNguoiChoiBamRadio; }

    // ───── hé hũ (Đêm 3) → tiếng thét → phòng Nhím → ma mắt đỏ lần đầu
    public void HeHu(LocHuCot h) { if (!dangCanh && !huDaHe) StartCoroutine(CanhHeHu(h)); }

    IEnumerator CanhHeHu(LocHuCot h)
    {
        dangCanh = true; LocFirstPerson.khoa = true;
        if (dangLo) { if (loCo != null) StopCoroutine(loCo); An(); dangLo = false; }
        StartCoroutine(NhacLayHu());          // [25/10] nhạc dồn lên suốt cảnh kéo hũ, đỉnh đúng lúc "Lấy ít tóc"
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
        // [25/10] cửa phòng Nhím (bản mới của nhóm có thể đang khoá): mở NGAY để thấy ma, xong cảnh trả lại như cũ
        bool cuaNhimMo0 = nhimCua && nhimCua.DangMo;
        if (nhimCua) { nhimCua.DatNgay(true); Physics.SyncTransforms(); }
        maTucGian = true;
        // [4/10] lúc màn còn đen: cả nhà mất điện HẲN (đèn, vật phát sáng, ambient, lớp tối) rồi mới đặt ma — mắt đỏ chỉ hiện trong tối
        var bat = new Dictionary<Light, bool>(); foreach (var l in dens) if (l) bat[l] = l.enabled;
        if (nhimDen && !bat.ContainsKey(nhimDen)) bat[nhimDen] = nhimDen.enabled;
        var vat = new Dictionary<GameObject, bool>(); foreach (var g in vatSang) if (g) vat[g] = g.activeSelf;
        Dien(bat, vat, false);
        RenderSettings.ambientLight = ambient0 * ambientKhiToi; if (volToi) volToi.weight = 1;
        dangToi = true;
        Bong b = null; lyDoNhim = "";
        // [25/10] điểm khai báo (nhimMa) trước; nếu cánh cửa (bản lề mới của nhóm) che hết thì dò thêm lưới trong phòng, ưu tiên xa và thẳng ô cửa
        var ds = new List<Vector3>(); if (nhimMa != null) ds.AddRange(nhimMa);
        var luoi = new List<Vector3>();
        for (float x = 7.0f; x >= 4.3f; x -= 0.3f) for (float z = 8.5f; z <= 11.3f; z += 0.3f) luoi.Add(new Vector3(x, nhimDung.y, z));
        luoi.Sort((u, v) => (Mathf.Abs(u.z - nhimDung.z) - u.x * 0.5f).CompareTo(Mathf.Abs(v.z - nhimDung.z) - v.x * 0.5f));
        int soKhaiBao = ds.Count; ds.AddRange(luoi);
        for (int i = 0; i < ds.Count && bong[VAN] != null; i++)
        {
            var p = ds[i];
            if (i >= soKhaiBao) { if (!SanCungTang(p, out p) || Mathf.Abs(p.y - nhimDung.y) > 0.08f || !TrongNhaDiem(p)) continue; }   // đứng trên sàn, không trèo lên giường/đồ
            var huong = nhimDung - p; huong.y = 0;
            var rot = Quaternion.LookRotation(huong.normalized, Vector3.up);
            if (i >= soKhaiBao && !ThayDuoc(p + Vector3.up * 1.78f, 0.25f)) continue;   // lọc nhanh trước khi dựng
            if (Vuong(bong[VAN], p, rot)) { if (i < soKhaiBao) lyDoNhim += $" {p}: vướng {lyDoVuong};"; continue; }
            if (!DatThu(VAN, p + Vector3.up * 0.03f, rot)) { if (i < soKhaiBao) lyDoNhim += $" {p}: DatThu hỏng;"; continue; }
            b = dangHien;
            if (b != null && !ThayDuocSauKhiDat(p + Vector3.up * 1.78f)) { if (i < soKhaiBao) lyDoNhim += $" {p}: bị che;"; An(); b = null; continue; }
            if (i >= soKhaiBao) lyDoNhim += $" → dùng điểm dò thêm {p:F2}";
            break;
        }
        if (b != null) PhatTai(cuoiCham, MatMa(b), 0.95f, 16f);   // [25/10] màn còn đen: tràng cười dài trong phòng Nhím
        yield return new WaitForSeconds(1.6f);
        for (float t = 1; t > 0; t -= Time.deltaTime / 0.4f) { LocFirstPerson.den = t; yield return null; }
        LocFirstPerson.den = 0;
        if (b != null) XuatHien();
        yield return new WaitForSeconds(3.2f);
        // ma biến trong tối, rồi điện chập chờn sáng lại
        An(); ImCuoi();
        if (nhimCua && !cuaNhimMo0) nhimCua.DatNgay(false);
        yield return new WaitForSeconds(0.5f);
        foreach (var on in new[] { true, false, true, false })
        { Dien(bat, vat, on); yield return new WaitForSeconds(on ? 0.07f : 0.12f); }
        Dien(bat, vat, true);
        RenderSettings.ambientLight = ambient0; if (volToi) volToi.weight = 0;
        dangToi = false;
        yield return new WaitForSeconds(0.6f);
        LocFirstPerson.khoa = false; dangCanh = false;
        hToi = Random.Range(40f, 60f); hLo = Random.Range(12f, 20f);
        LocFirstPerson.PhuDe("...Nó vào phòng rồi.", 2.4f);
        LocTui.Dat("he_hu"); LocCotTruyen.DiemLuu("diem_luu_1");   // [v3.3] điểm lưu 1
    }

    public string lyDoNhim = "";
    // [25/10] nhạc cảnh lấy hũ (file người dùng đưa, 30 s, dồn dần, đỉnh ở giây 25).
    //  Cảnh LocHuCot.CanhHe: kéo hũ ~1,2–2,4 s · lật giấy thấy tóc ~5,9 s · "Lấy ít tóc." ~10,5 s · đứng dậy ~14,8 s.
    //  Phát từ giây nhacLayHuTu của bản nhạc → đỉnh 25 s rơi đúng lúc lấy tóc; nhạc chạy hết đuôi rồi tiếng thét nối vào.
    public float nhacLayHuTu = 14.5f, nhacLayHuAm = 1f;
    IEnumerator NhacLayHu()
    {
        var c = Resources.Load<AudioClip>("LOC_MaNu/Nhac_LayHu"); if (!c) yield break;
        var a = gameObject.AddComponent<AudioSource>(); a.playOnAwake = false; a.spatialBlend = 0; a.clip = c;
        a.time = Mathf.Clamp(nhacLayHuTu, 0, c.length - 1); a.volume = 0; a.Play();
        for (float t = 0; t < 1; t += Time.deltaTime / 1.5f) { a.volume = nhacLayHuAm * t; yield return null; }
        a.volume = nhacLayHuAm;
        while (a && a.isPlaying) { if (dangChet) a.Stop(); yield return null; }
        if (a) Destroy(a);
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
    public static void BatChet() { if (I) I.Chet(); }
    void Chet() { if (amHH) amHH.Stop(); if (!dangChet) StartCoroutine(CanhChet()); }
    IEnumerator CanhChet()
    {
        dangChet = true; LocFirstPerson.khoa = true;
        LocMenu.DatKet("death"); yield return null;   // [22/10] chụp khung cuối (đôi mắt) cho màn Ending trước khi cắt đen
        yield return new WaitForSeconds(0.4f);        // "Đôi mắt đỏ ngay trước mặt. Cắt đen ngay." (C3)
        LocFirstPerson.den = 1; An();
        if (chenVo) am.PlayOneShot(chenVo, 1f);
        yield return new WaitForSeconds(5.2f);
        LocFirstPerson.chuGiua = "NHÀ NÀY CÒN MỘT CÁI CHÉN.";
        yield return new WaitForSeconds(2.5f);
        LocMenu.ManChet();   // [22/10] hai lựa chọn: chơi lại từ đầu đêm / về menu chính (R vẫn chơi lại)
        choR = true;
    }

    public static void ChoiLai()
    {
        huDaHe = false; maTucGian = false; tamDung = false;
        LocFirstPerson.khoa = false; LocFirstPerson.den = 0; LocFirstPerson.chuGiua = null;
        LocMenu.NapLai();
    }

    // ───── phím thử
    void PhimThu()
    {
        var k = Keyboard.current; if (k == null) return;
        bool sh = k.shiftKey.isPressed;   // [25/10] dọa thử dời sang Shift+1..4 — F7–F12 nay là phím thử của Nhím (bản nhóm)
        if (k.f1Key.wasPressedThisFrame) DatDem(1);
        if (k.f2Key.wasPressedThisFrame) DatDem(2);
        if (k.f3Key.wasPressedThisFrame) DatDem(3);
        if (k.f4Key.wasPressedThisFrame) { maTucGian = !maTucGian; huDaHe |= maTucGian; Bao(maTucGian ? "ma TỨC GIẬN (mắt đỏ, tắt đèn)" : "ma bình thường"); hToi = 8f; }
        if (k.f5Key.wasPressedThisFrame && !dangLo && !dangToi) Bao(ThuLo() ? "ép lấp ló" : "không có chỗ lấp ló hợp lệ quanh đây");
        if (k.f6Key.wasPressedThisFrame && !dangToi && !dangCanh) { StartCoroutine(TatDen()); Bao("ép tắt đèn"); }
        if (!dangDoa && !dangToi && !dangCanh)
        {
            if (sh && k.digit1Key.wasPressedThisFrame) Bao(ThuDoa((int)KieuDoa.SauLung) ? "dọa: sau lưng" : "không có chỗ sau lưng");
            if (sh && k.digit2Key.wasPressedThisFrame) Bao(ThuDoa((int)KieuDoa.BamTran) ? "dọa: bám trần nhìn chằm chằm" : "không có trần hợp lệ phía trước");
            if (sh && k.digit3Key.wasPressedThisFrame) { ThuDoa((int)KieuDoa.TuongAnToan); Bao("dọa: tưởng an toàn (0,5 s)"); }
            if (sh && k.digit4Key.wasPressedThisFrame && LocRadio.I) { if (!LocRadio.I.DangBat) LocRadio.I.Bat(); StartCoroutine(CanhTatRadio()); Bao("dọa: ma tắt radio"); }
            if (sh && k.digit5Key.wasPressedThisFrame && LocRadio.I) { if (LocRadio.I.DangBat) LocRadio.I.Tat(false); demBatRadio = dem; MaBatRadio(); }
        }
    }
    void DatDem(int d) { dem = d; if (d < 3) { maTucGian = false; huDaHe = false; } Bao("Đêm " + d); }
    void Bao(string s) { dbg = s; dbgHet = Time.time + 3f; }

    void OnGUI()
    {
        if (!hienDebug || LocMenu.DangChe) return;
        var st = new GUIStyle(GUI.skin.label) { fontSize = 13 }; st.normal.textColor = new Color(1, 1, 1, 0.55f);
        string s = $"[thử] Đêm {dem} · {(maTucGian ? "tức giận" : "bình thường")} · F1-F3 đêm · F4 tức giận · F5 ló · F6 tắt đèn · Shift+1 sau lưng · Shift+2 bám trần · Shift+3 tưởng an toàn · Shift+4 ma tắt radio · Shift+5 ma bật radio";
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
        hLo = hToi = hDoa = 9999; dem = 2;
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
            if (lyDoNhim.Length > 0) log.AppendLine("  chỗ bị loại:" + lyDoNhim);
            int fi = 0;
            while (dangCanh && fi < 20) { Chup($"nhim_f{fi++:00}"); yield return new WaitForSeconds(0.3f); }
            while (dangCanh) yield return null;
        }

        // 6. dọa bất ngờ: mỗi kiểu thử ở các chỗ của mục 4 tới khi được một lần — đo lấn tường + chụp đúng lúc
        GhiTam();
        log.AppendLine("\n— 6. DỌA BẤT NGỜ —");
        hDoa = 9999;
        foreach (KieuDoa kd in System.Enum.GetValues(typeof(KieuDoa)))
        {
            bool xong = false; string ketQua = "không tìm được chỗ ở 8 vị trí × 4 hướng";
            foreach (var p in cho)
            for (int hg = 0; hg < 4; hg++)
            {
                if (xong) break;
                fp.DatTuThe(p, p + Quaternion.Euler(0, 90 * hg, 0) * Vector3.forward * 3 + Vector3.up * 1.4f); yield return null; yield return null;
                if (!ThuDoa((int)kd)) continue;
                xong = true; int vv = -1; int hinh = 0;
                for (float w = 0; w < 14f && dangDoa; w += Time.deltaTime)
                {
                    if (doaPha == "saulung_cho" && dangHien != null)
                    {
                        vv = Mathf.Max(vv, DemTrongVat(dangHien)); Chup("doa_saulung_0"); hinh++;
                        yield return new WaitForSeconds(0.4f);
                        if (dangHien != null) fp.DatTuThe(player.position, MatMa(dangHien));   // quay lại nhìn
                        yield return null; yield return null; Chup("doa_saulung_1"); hinh++;
                        while (dangDoa && doaPha != "saulung_thay") yield return null;
                        yield return null; Chup("doa_saulung_2"); hinh++;
                        while (dangDoa) yield return null; break;
                    }
                    if (doaPha == "bamtran_hien" && dangHien != null)
                    {
                        vv = Mathf.Max(vv, DemTrongVat(dangHien)); yield return null; Chup("doa_bamtran_0"); hinh++;
                        fp.DatTuThe(player.position, MatMa(dangHien));            // ngẩng lên nhìn
                        while (dangDoa && doaPha != "bamtran_nhin") yield return null;
                        yield return new WaitForSeconds(0.6f); Chup("doa_bamtran_1"); hinh++;
                        while (dangDoa) yield return null; break;
                    }
                    if (doaPha == "antoan_hien" && dangHien != null)
                    {
                        vv = Mathf.Max(vv, DemTrongVat(dangHien)); yield return null; yield return null; Chup("doa_antoan"); hinh++;
                        while (dangDoa) yield return null; break;
                    }
                    yield return null;
                }
                while (dangDoa) yield return null;
                if (vv < 0) { xong = false; ketQua = $"ở {p:F1}: bắt đầu nhưng ma không hiện — thử chỗ khác"; yield return new WaitForSeconds(0.3f); continue; }
                tong++; if (vv != 0) loi++;
                ketQua = vv < 0 ? $"ở {p:F1}: bắt đầu nhưng ma không hiện (người chơi không nhìn/không có chỗ bám theo)" : $"ở {p:F1}: đỉnh lấn vào tường/đồ đạc {vv} · {hinh} ảnh → {(vv == 0 ? "OK" : "LỖI " + vatBiLan)}";
            }
            log.AppendLine($"{kd}: {ketQua}");
            yield return new WaitForSeconds(0.5f);
        }

        // 7. radio + ma tắt radio
        GhiTam();
        log.AppendLine($"\n— TIẾNG MA NỮ — thét: {(tiengThet ? tiengThet.name + $" {tiengThet.length:0.0}s" : "THIẾU")} · cười gần: {(cuoiGan ? $"{cuoiGan.length:0.0}s" : "THIẾU")} · khúc khích: {(khucKhich ? $"{khucKhich.length:0.0}s" : "THIẾU")} · cười xa: {(cuoiXa ? $"{cuoiXa.length:0.0}s" : "THIẾU")} · cười dài: {(cuoiCham ? $"{cuoiCham.length:0.0}s" : "THIẾU")} · xuất hiện: {(xuatHien ? $"{xuatHien.length:0.0}s" : "THIẾU")} · nhân vật hoảng hốt: {(hoangHot ? $"{hoangHot.length:0.0}s" : "THIẾU")}");
        log.AppendLine("\n— 7. RADIO —");
        var R = LocRadio.I;
        if (!R) log.AppendLine("KHÔNG thấy radio (RadioCassette_2Loa) — LocRadio chưa gắn");
        else
        {
            var clip = Resources.Load<AudioClip>("LOC_Radio_Nhac");
            log.AppendLine($"radio ở {R.ViTri:F2} · nhạc {(clip ? clip.name + " " + clip.length.ToString("0") + " s" : "THIẾU")}");
            var nguon = R.GetComponentInChildren<AudioSource>();
            if (nguon) { var cv = nguon.GetCustomCurve(AudioSourceCurveType.CustomRolloff); string amS = ""; foreach (var kcR in new[] { 1f, 3f, 6f, 10f, 15f, 20f }) amS += $" {kcR:0}m={cv.Evaluate(kcR / nguon.maxDistance):0.00}"; log.AppendLine("độ to theo khoảng cách:" + amS); }
            var g0 = R.ViTri; g0.y = 0.05f; bool datTest = false;
            foreach (var o in new[] { new Vector3(-4f, 0, 0), new Vector3(-3.5f, 0, -1.2f), new Vector3(-3.5f, 0, 1.2f), new Vector3(-3f, 0, 0), new Vector3(0, 0, -3.5f), new Vector3(0, 0, 3.5f) })
            {
                var q = g0 + o;
                if (!Physics.Raycast(q + Vector3.up * 1.2f, Vector3.down, out var sh, 2f, ~0, QueryTriggerInteraction.Ignore) || Mathf.Abs(sh.point.y) > 0.3f) continue;
                var p = sh.point; fp.DatTuThe(p, p + o.normalized * 3 + Vector3.up * 1.4f); yield return null; yield return null;   // quay lưng lại radio
                R.Tat(false); R.Bat(); yield return new WaitForSeconds(0.5f);
                log.AppendLine($"người chơi ở {p:F1}, cách radio {Vector3.Distance(p, g0):0.0} m, quay lưng · radio {(R.DangBat ? "đang phát" : "KHÔNG phát")} · đủ điều kiện ma tắt: {CoTheTatRadio()}");
                StartCoroutine(CanhTatRadio()); yield return null;
                int vv = -1;
                for (float w = 0; w < 14f && dangDoa; w += Time.deltaTime)
                {
                    if (doaPha == "tatradio_dung" && dangHien != null)
                    {
                        vv = DemTrongVat(dangHien); log.AppendLine($"  radio tắt: {(!R.DangBat ? "đúng" : "SAI")} · ma đứng cạnh radio ở {dangHien.w.position:F2}");
                        fp.DatTuThe(player.position, MatMa(dangHien)); yield return null; yield return null; Chup("radio_ma");
                        while (dangDoa && doaPha != "tatradio_thay") yield return null;
                        yield return new WaitForSeconds(0.5f); Chup("radio_ma_nhin");
                        while (dangDoa) yield return null; break;
                    }
                    yield return null;
                }
                while (dangDoa) yield return null;
                if (vv < 0) { log.AppendLine("  ma không tìm được chỗ đứng cạnh radio từ chỗ này — thử chỗ khác"); continue; }
                tong++; if (vv != 0) loi++;
                log.AppendLine($"  ma cạnh radio: đỉnh lấn vào tường/đồ đạc {vv} → {(vv == 0 ? "OK" : "LỖI " + vatBiLan)}");
                datTest = true; break;
            }
            if (!datTest) log.AppendLine("không thử được cảnh ma tắt radio ở các chỗ quanh phòng khách");
            R.Tat(false);
            // đứng trước radio, nhìn vào nó: tia [E] (như LocFirstPerson) có trúng radio không
            bool trungE = false; string gan = "";
            foreach (var o in new[] { new Vector3(-1.1f, 0, 0), new Vector3(-1.0f, 0, -0.6f), new Vector3(-1.0f, 0, 0.6f), new Vector3(0, 0, -1.1f), new Vector3(0, 0, 1.1f) })
            {
                var q = g0 + o;
                if (!Physics.Raycast(q + Vector3.up * 1.2f, Vector3.down, out var sh, 2f, ~0, QueryTriggerInteraction.Ignore) || Mathf.Abs(sh.point.y) > 0.3f) continue;
                fp.DatTuThe(sh.point, R.ViTri); yield return null; yield return null;
                trungE = R.NhinTrung(cam, LocFirstPerson.TamVoiRadio); gan = $"cách {Vector3.Distance(cam.position, R.ViTri):0.0} m";
                if (trungE) { R.Toggle(); yield return new WaitForSeconds(0.3f); Chup("radio_gan"); log.AppendLine($"đứng ở {sh.point:F1} nhìn radio ({gan}): [E] trúng radio · bấm E → {(R.DangBat ? "đang phát" : "KHÔNG phát")}"); R.Toggle(); log.AppendLine($"  bấm E lần nữa → {(R.DangBat ? "VẪN phát" : "đã tắt")}"); break; }
            }
            if (!trungE) log.AppendLine($"KHÔNG bấm E trúng radio từ phía trước ({gan})");
            // [25/10] quét nhiều chỗ đứng trước tủ (gần sát → xa), đọc đúng dòng gợi ý người chơi thấy trên màn hình
            int duoc = 0, thu = 0; var sbE = new System.Text.StringBuilder();
            foreach (var hg in new[] { new Vector3(-1, 0, 0), new Vector3(-1, 0, -0.5f).normalized, new Vector3(-1, 0, 0.5f).normalized })
                foreach (var kc in new[] { 0.45f, 0.7f, 1.0f, 1.4f, 1.8f })
                {
                    var q = g0 + hg * kc;
                    if (!Physics.Raycast(q + Vector3.up * 1.2f, Vector3.down, out var sh, 2f, ~0, QueryTriggerInteraction.Ignore) || Mathf.Abs(sh.point.y) > 0.3f) continue;
                    if (Physics.CheckCapsule(sh.point + Vector3.up * 0.4f, sh.point + Vector3.up * 1.5f, 0.25f, ~0, QueryTriggerInteraction.Ignore)) continue;   // chỗ có đồ, người chơi không đứng được
                    fp.DatTuThe(sh.point, R.ViTri); yield return null; yield return null; yield return null;
                    thu++; string gy = fp.GoiYDangHien; bool ok = gy.Contains("radio");
                    if (ok) duoc++; else sbE.Append($" {sh.point:F1}→\"{gy}\"");
                }
            { R.Bat(); float t0 = R.Nguon.time; yield return new WaitForSeconds(1f); float t1 = R.Nguon.time;
              var ls = FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
              log.AppendLine($"âm thanh: nguồn đang chạy {R.Nguon.isPlaying} · nhạc trôi {t1 - t0:0.00} s/1 s · âm lượng nguồn {R.Nguon.volume:0.00} · AudioListener: {ls.Length} cái ({string.Join(", ", System.Array.ConvertAll(ls, x => x.name + (x.enabled ? "" : "[tắt]")))}) · tổng âm lượng {AudioListener.volume:0.00} · tạm dừng {AudioListener.pause}");
              R.Tat(false); }
            log.AppendLine($"quét {thu} chỗ đứng trước tủ: hiện \"[E] Bật radio\" ở {duoc}/{thu}" + (sbE.Length > 0 ? " · không hiện:" + sbE : ""));
            // [25/10] 7b. ma tự bật radio → người chơi tắt → ma sau lưng
            // [25/10] 7c. nhạc làm lễ dưới hầm
            {
                LocMiniGame mgL = FindAnyObjectByType<LocMG_GhepChen>(FindObjectsInactive.Include);
                if (!mgL) mgL = FindAnyObjectByType<LocMG_MamCung>(FindObjectsInactive.Include);
                if (!mgL) log.AppendLine("\n— 7c. LỄ HẦM — không thấy mini game lễ trong cảnh");
                else
                {
                    bool mo = false; try { mgL.Mo(null); mo = LocMiniGame.hienTai == mgL; } catch (System.Exception ex) { log.AppendLine("  mở mini game lỗi: " + ex.Message); }
                    yield return new WaitForSeconds(2.6f);
                    log.AppendLine($"\n— 7c. LỄ HẦM — mở \"{mgL.GetType().Name}\": {(mo ? "có" : "KHÔNG")} · nhạc lễ {(amLe && amLe.isPlaying ? $"đang phát, âm lượng {amLe.volume:0.00}" : "KHÔNG phát")}");
                    if (mo) mgL.Dong();
                    yield return new WaitForSeconds(3f);
                    log.AppendLine($"  đóng mini game → nhạc lễ {(amLe && amLe.isPlaying ? $"VẪN phát ({amLe.volume:0.00})" : "đã tắt dần")}");
                    LocFirstPerson.khoa = false; tamDung = false;
                }
            }
            log.AppendLine("\n— 7b. MA TỰ BẬT RADIO —");
            if (R.DangBat) R.Tat(false);
            var gOld = tiLeSauLungKhiTat; tiLeSauLungKhiTat = 1f; demBatRadio = -1;
            MaBatRadio(); yield return new WaitForSeconds(0.4f);
            log.AppendLine($"ma bật: radio {(R.DangBat ? "đang phát" : "KHÔNG phát")} · chờ người chơi tắt: {choNguoiTat}");
            fp.DatTuThe(new Vector3(6.2f, 0f, 4.2f), R.ViTri); yield return null; yield return null;
            R.Toggle();                                    // người chơi bấm E tắt
            log.AppendLine($"người chơi tắt: radio {(R.DangBat ? "VẪN phát" : "đã tắt")}");
            Bong bS = null;
            for (float w = 0; w < 3f && bS == null; w += Time.deltaTime) { bS = dangHien; yield return null; }
            if (bS == null) log.AppendLine("  ma KHÔNG xuất hiện sau lưng (có thể không có chỗ trống phía sau ở góc này)");
            else
            {
                var sau = bS.w.position - player.position; sau.y = 0;
                float gocSau = Vector3.Angle(new Vector3(cam.forward.x, 0, cam.forward.z), sau);
                int vS = DemTrongVat(bS); tong++; if (vS != 0) loi++;
                log.AppendLine($"  ma đứng ở {bS.w.position:F2}, cách {sau.magnitude:0.0} m, lệch {gocSau:0}° so với hướng nhìn (≥ 120° là sau lưng) · lấn tường/đồ đạc {vS} → {(vS == 0 ? "OK" : "LỖI")}");
                Chup("radio_saulung_0");
                fp.DatTuThe(player.position, MatMa(bS)); yield return new WaitForSeconds(0.3f); Chup("radio_saulung_1");
                for (float w = 0; w < 3f && dangDoa; w += Time.deltaTime) yield return null;
                log.AppendLine($"  quay lại nhìn → cảnh {(dangDoa ? "CHƯA xong" : "đã xong (hù, tắt đèn, biến)")}");
            }
            tiLeSauLungKhiTat = gOld;
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
