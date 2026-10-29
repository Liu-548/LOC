using System.Collections;
using UnityEngine;

// [29/10] Tay Khôi góc nhìn thứ nhất + tay thầy Đồng (Bad đường c). Model: LOC_Nguon/Tay (bản vẽ ThietKe_BanTay_v1, đã duyệt: áo xắn · tay thầy giữ nhẫn + móng út).
//  Luật (đặc tả tay mục 0–1): tay KHÔNG hiện khi đi lại; bấm E thì tay đưa vào từ mép dưới → chạm → rút ra. Không va chạm, không đổ bóng, không tự sáng.
//  Khung tay (phai/trai) là con của camera: gốc ở cổ tay, +Z = hướng ngón, +Y = mu bàn tay. Tư thế ngón lấy mẫu sẵn lúc dựng nhà (LocHouseBuilder_Tay).
public class LocTay : MonoBehaviour
{
    public static LocTay I;
    public Transform phai, trai;
    public Transform[] xuongPhai, xuongTrai;
    public Quaternion[] ttPhai, ttTrai;       // [tư thế × xương]
    public string[] tenTT;                    // Tha, Nam, Cam, Nhon, Xoe, Xin, Chap
    public GameObject nhang;                  // ba nén hương kẹp giữa hai lòng bàn tay (con của khung tay phải)
    public Transform tayThay;                 // tay phải thầy Đồng, đặt trong thế giới (ẩn tới Bad đường c)
    public Transform[] xuongThay; public Quaternion[] ttThay;
    public GameObject amTra;                  // ấm tay cầm ở cảnh rót trà ban ngày (con của khung tay phải)
    bool banPhai, banTrai;
    public static bool Ban => I && (I.banPhai || I.banTrai);

    const float VAO = 0.3f, RA = 0.3f;

    void Awake()
    {
        I = this;
        foreach (var t in new[] { phai, trai, tayThay }) if (t) t.gameObject.SetActive(false);
        if (nhang) nhang.SetActive(false); if (amTra) amTra.SetActive(false);
    }

    int TT(string ten) { int i = System.Array.IndexOf(tenTT, ten); return i < 0 ? 0 : i; }
    static void DatTT(Transform[] xs, Quaternion[] tt, int a, int b, float t)
    {
        int n = xs.Length;
        for (int i = 0; i < n; i++) if (xs[i]) xs[i].localRotation = Quaternion.Slerp(tt[a * n + i], tt[b * n + i], t);
    }
    static float Em(float t) => t * t * (3 - 2 * t);

    // tư thế khung tay (trong hệ camera) ngoài mép dưới khung hình
    static (Vector3, Quaternion) NgoaiKhung(bool laTrai) =>
        (new Vector3(laTrai ? -0.22f : 0.22f, -0.55f, 0.22f), Quaternion.LookRotation(new Vector3(0, 0.35f, 1f), Vector3.up));

    // tư thế khung tay để đầu ngón chạm điểm thế giới p (bám theo camera đang đứng). Xa quá thì dừng ở tầm với.
    (Vector3, Quaternion) Toi(Vector3 p, bool laTrai, float lui = 0.13f)
    {
        var cam = transform.parent;
        var l = cam.InverseTransformPoint(p);
        if (l.z < 0.34f) l.z = 0.34f;                               // không sát mắt (near clip 0,05 + biên)
        if (l.magnitude > 0.62f) l = l.normalized * 0.62f;          // tầm với của tay
        var vai = new Vector3(laTrai ? -0.18f : 0.18f, -0.32f, -0.02f);
        var dir = (l - vai).normalized;
        var mu = Vector3.ProjectOnPlane(Vector3.up, dir).normalized;
        mu = Quaternion.AngleAxis(laTrai ? 25f : -25f, dir) * mu;  // nghiêng nhẹ, ngón cái hơi ngửa lên — tay thả lỏng
        return (l - dir * lui, Quaternion.LookRotation(dir, mu));
    }

    // một tay: vào → giữ (đích có thể đổi theo khung hình) → ra. dich == null: giữ nguyên chỗ cuối.
    IEnumerator Lam(bool laTrai, System.Func<(Vector3, Quaternion)> dich, string tuThe, float giu, System.Func<float, Vector3> lac = null, System.Func<bool> thoi = null)
    {
        var k = laTrai ? trai : phai; var xs = laTrai ? xuongTrai : xuongPhai; var tt = laTrai ? ttTrai : ttPhai;
        if (!k) yield break;
        if (laTrai) banTrai = true; else banPhai = true;
        int a = TT("Tha"), b = TT(tuThe);
        var (p0, r0) = NgoaiKhung(laTrai);
        k.gameObject.SetActive(true);
        for (float t = 0; t < 1; t += Time.deltaTime / VAO)
        {
            var (p, r) = dich(); float e = Em(t);
            k.localPosition = Vector3.Lerp(p0, p, e); k.localRotation = Quaternion.Slerp(r0, r, e); DatTT(xs, tt, a, b, e);
            yield return null;
        }
        for (float t = 0; thoi != null ? !thoi() : t < giu; t += Time.deltaTime)   // thoi: giữ tới khi điều kiện đúng
        {
            var (p, r) = dich();
            k.localPosition = p + (lac != null ? lac(t) : Vector3.zero); k.localRotation = r; DatTT(xs, tt, a, b, 1);
            yield return null;
        }
        Vector3 p1 = k.localPosition; Quaternion r1 = k.localRotation;
        for (float t = 0; t < 1; t += Time.deltaTime / RA)
        {
            float e = Em(t);
            k.localPosition = Vector3.Lerp(p1, p0, e); k.localRotation = Quaternion.Slerp(r1, r0, e); DatTT(xs, tt, b, a, e);
            yield return null;
        }
        k.gameObject.SetActive(false);
        if (laTrai) banTrai = false; else banPhai = false;
    }

    // ───── API cho các cảnh
    // bấm E bình thường (cửa, công tắc, nhặt đồ…): tay phải với tới chỗ tia nhìn trúng rồi rút về. Đang bận cảnh khác thì thôi.
    public static void Cham(Vector3 diem)
    {
        if (!I || Ban || !I.isActiveAndEnabled) return;
        I.StartCoroutine(I.Lam(false, () => I.Toi(diem, false), "Nhon", 0.12f));
    }

    // hai tay cùng tới một điểm (thay khăn, kéo hũ, dán niêm…): tay trái lệch sang trái một chút
    public static IEnumerator HaiTay(Vector3 diem, float giu, string tuThe = "Cam", float rong = 0.07f)
    {
        if (!I) yield break;
        var cam = I.transform.parent;
        var r = I.StartCoroutine(I.Lam(false, () => I.Toi(diem + cam.right * rong, false), tuThe, giu));
        yield return I.Lam(true, () => I.Toi(diem - cam.right * rong, true), tuThe, giu);
        yield return r;
    }
    public static IEnumerator MotTay(Vector3 diem, float giu, string tuThe = "Nhon", bool laTrai = false)
    {
        if (!I) yield break;
        yield return I.Lam(laTrai, () => I.Toi(diem, laTrai), tuThe, giu);
    }

    // Đêm 1, bác bước tới nắm cánh tay: tay trái Khôi đưa ra trước, thả lỏng, giữ tới khi thoi() đúng. CangTayTrai = chỗ bác nắm (giữa cẳng tay, mặt trên).
    public static IEnumerator GiuTayTrai(System.Func<bool> thoi)
    {
        if (!I || !I.trai) yield break;
        yield return I.Lam(true, TayTraiGiu, "Tha", 0, null, thoi);
    }
    static (Vector3, Quaternion) TayTraiGiu() => (new Vector3(-0.09f, -0.17f, 0.46f), Quaternion.LookRotation(new Vector3(0.15f, -0.2f, 1f), new Vector3(-0.35f, 1f, 0f)));   // cẳng tay lọt khung (FOV dọc 70°)
    public static Vector3 CangTayTrai => I && I.trai ? I.trai.TransformPoint(new Vector3(0, 0.035f, -0.12f)) : Vector3.zero;

    // thắp hương: chắp tay kẹp ba nén hương trước mặt, vái ba lần
    public static IEnumerator ThapHuong(float giay)
    {
        if (!I) yield break;
        System.Func<float, Vector3> vai = t => new Vector3(0, -0.06f * Mathf.Max(0, Mathf.Sin(t / giay * 3 * Mathf.PI * 2 - Mathf.PI / 2) * 0.5f + 0.5f) * Mathf.Clamp01(t * 2), 0);   // ba lần vái
        if (I.nhang) I.nhang.SetActive(true);
        var r = I.StartCoroutine(I.Lam(false, () => Chap(false), "Chap", giay, vai));
        yield return I.Lam(true, () => Chap(true), "Chap", giay, vai);
        yield return r;
        if (I.nhang) I.nhang.SetActive(false);
    }

    // chắp tay trước mặt: hai lòng bàn tay áp nhau, ngón chỉ lên-ra trước, mu tay quay ra hai bên
    static (Vector3, Quaternion) Chap(bool laTrai) =>
        (new Vector3(laTrai ? -0.021f : 0.021f, -0.19f, 0.33f), Quaternion.LookRotation(new Vector3(0, 0.85f, 0.5f).normalized, laTrai ? Vector3.left : Vector3.right));

#if UNITY_EDITOR
    // thử trong Editor (menu LOC → Thử tay: chụp): đặt tay đứng yên ở giữa một việc rồi chụp từ camera người chơi
    public void ThuTinh(string viec, Vector3 diem)
    {
        I = this;
        foreach (var t in new[] { phai, trai }) if (t) t.gameObject.SetActive(false);
        if (nhang) nhang.SetActive(false); if (amTra) amTra.SetActive(false);
        void Dat(bool laTrai, (Vector3 p, Quaternion r) pr, string tt)
        {
            var k = laTrai ? trai : phai; if (!k) return;
            k.gameObject.SetActive(true); k.localPosition = pr.p; k.localRotation = pr.r;
            DatTT(laTrai ? xuongTrai : xuongPhai, laTrai ? ttTrai : ttPhai, TT(tt), TT(tt), 1);
        }
        var cam = transform.parent;
        switch (viec)
        {
            case "cham": Dat(false, Toi(diem, false), "Nhon"); break;
            case "hai": Dat(false, Toi(diem + cam.right * 0.07f, false), "Cam"); Dat(true, Toi(diem - cam.right * 0.07f, true), "Cam"); break;
            case "xoe": Dat(false, Toi(diem + cam.right * 0.04f, false), "Xoe"); Dat(true, Toi(diem - cam.right * 0.04f, true), "Xoe"); break;
            case "chap": Dat(false, Chap(false), "Chap"); Dat(true, Chap(true), "Chap"); if (nhang) nhang.SetActive(true); break;
            case "giu": Dat(true, TayTraiGiu(), "Tha"); break;
            case "tra": if (amTra) amTra.SetActive(true); var (p, r) = Toi(diem + Vector3.up * 0.16f + cam.right * 0.10f, false, 0.05f); Dat(false, (p, r * Quaternion.Euler(0, 0, 35f)), "Cam"); break;
        }
    }
    public void ThuTat() { foreach (var t in new[] { phai, trai }) if (t) t.gameObject.SetActive(false); if (nhang) nhang.SetActive(false); if (amTra) amTra.SetActive(false); }
#endif

    // cảnh ban ngày: tay phải cầm ấm rót vào chén trên bàn nước
    public static IEnumerator RotTra(Vector3 chen, float giay)
    {
        if (!I) yield break;
        if (I.amTra) I.amTra.SetActive(true);
        var cam = I.transform.parent;
        yield return I.Lam(false, () =>
        {
            var (p, r) = I.Toi(chen + Vector3.up * 0.16f + cam.right * 0.10f, false, 0.05f);
            return (p, r * Quaternion.Euler(0, 0, 35f));                 // nghiêng cổ tay: vòi ấm chúc xuống
        }, "Cam", giay);
        if (I.amTra) I.amTra.SetActive(false);
    }

    // Bad đường (c), sáng mùng một: một bàn tay qua cửa ngách cổng, ngửa ra; Khôi đưa cái hũ; tay khép lại, rút ra cùng cái hũ.
    //  cho: chỗ lòng bàn tay thầy (thế giới) · vao: hướng tay thò vào (từ ngoài đường vào sân) · hu: bản sao hình cái hũ (con của khung tay Khôi lúc đưa)
    public static IEnumerator TayThayNhanHu(Vector3 cho, Vector3 vao, GameObject hu)
    {
        if (!I || !I.tayThay) yield break;
        var k = I.tayThay; var xs = I.xuongThay; var tt = I.ttThay;
        vao.y = 0; vao.Normalize();
        var xoay = Quaternion.LookRotation(vao, Vector3.down);         // ngửa bàn tay: mu hướng xuống
        Vector3 ngoai = cho - vao * 0.45f, trong = cho - vao * 0.08f;
        k.gameObject.SetActive(true); DatTT(xs, tt, I.TT("Xin"), I.TT("Xin"), 1);
        for (float t = 0; t < 1; t += Time.deltaTime / 1.1f) { k.SetPositionAndRotation(Vector3.Lerp(ngoai, trong, Em(t)), xoay); yield return null; }
        yield return new WaitForSeconds(1.2f);
        if (hu)   // hai tay Khôi đưa cái hũ tới lòng bàn tay thầy
        {
            var cam = I.transform.parent; hu.SetActive(true);
            Vector3 dau = cam.TransformPoint(new Vector3(0, -0.30f, 0.40f)), cuoi = cho + Vector3.up * 0.03f;
            hu.transform.position = dau;
            float tDua = 0; bool xong = false;
            I.StartCoroutine(Dua());
            IEnumerator Dua()
            {
                for (; tDua < 1; tDua += Time.deltaTime / 1.3f) { hu.transform.position = Vector3.Lerp(dau, cuoi, Em(tDua)); yield return null; }
                hu.transform.position = cuoi; xong = true;
            }
            var r = I.StartCoroutine(I.Lam(false, () => I.Toi(hu.transform.position + cam.right * 0.11f + Vector3.up * 0.12f, false, 0.10f), "Cam", 1.6f));
            yield return I.Lam(true, () => I.Toi(hu.transform.position - cam.right * 0.11f + Vector3.up * 0.12f, true, 0.10f), "Cam", 1.6f);
            yield return r;
            while (!xong) yield return null;
            hu.transform.SetParent(k, true);
        }
        for (float t = 0; t < 1; t += Time.deltaTime / 0.5f) { DatTT(xs, tt, I.TT("Xin"), I.TT("Cam"), Em(t)); yield return null; }
        yield return new WaitForSeconds(0.5f);
        for (float t = 0; t < 1; t += Time.deltaTime / 1.2f) { k.position = Vector3.Lerp(trong, ngoai - vao * 0.4f, Em(t)); yield return null; }
        k.gameObject.SetActive(false);
    }
}
