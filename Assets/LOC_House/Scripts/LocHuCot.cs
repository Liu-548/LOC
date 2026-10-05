using System.Collections;
using UnityEngine;

// [3/10] Hũ cốt dưới gầm bàn thờ hầm. Đêm 1–2: "Đừng động vào." · Đêm 3: [E] Hé miệng hũ → LocMa.HeHu (bắt buộc, kịch bản v3.1 Đêm 3).
// [4/10] model mới (Models/HuCot/HuCot.fbx) + cảnh hé hũ: quỳ xuống → kéo hũ ra khỏi gầm → cúi sát → xé niêm, lật góc giấy dầu
//        → thấy lọn tóc buộc chỉ đỏ (dưới là mảnh xương lẫn tro) → đậy vội, góc niêm rách, giấy không khép hẳn → đứng dậy.
public class LocHuCot : MonoBehaviour
{
    public Transform than;    // gốc model (kéo ra khỏi gầm)
    public Transform goc;     // góc giấy dầu, gốc ở bản lề (tờ niêm phần trên góc là con của nó)
    public AudioClip keo, xe, sot;
    [Header("Thông số cảnh")]
    public float keoRa = 0.22f, gocMo = 150f, gocDay = 9f, cao = 0.30f, denManh = 1.7f;

    public string GoiY => LocMa.dem == 3 && !LocMa.huDaHe ? "[E] Hé miệng hũ" : "[E] Cái hũ";

    public void Bam()
    {
        if (LocMa.dem == 3 && !LocMa.huDaHe && LocMa.I) LocMa.I.HeHu(this);
        else LocFirstPerson.PhuDe(LocMa.huDaHe ? "Đậy lại rồi. Đừng động vào nữa." : "Đừng động vào.", 2f);
    }

    Quaternion gocNghi; bool coNghi;
    Vector3 Huong() { var d = goc ? goc.position - than.position : Vector3.forward; d.y = 0; return d.sqrMagnitude > 1e-6f ? d.normalized : Vector3.forward; }
    void DatGoc(float a)   // a độ: 0 đóng · dương = mép trước lật lên rồi gập ngược ra sau
    {
        if (!goc) return;
        if (!coNghi) { gocNghi = goc.localRotation; coNghi = true; }
        goc.localRotation = gocNghi;
        goc.rotation = Quaternion.AngleAxis(-a, Vector3.Cross(Vector3.up, Huong())) * goc.rotation;
    }
    static Quaternion Nhin(Vector3 tu, Vector3 toi) => Quaternion.LookRotation(toi - tu, Vector3.up);
    static float Em(float t) => t * t * (3 - 2 * t);
    static IEnumerator Bay(Transform c, Vector3 p, Quaternion r, float giay)
    {
        Vector3 p0 = c.position; Quaternion r0 = c.rotation;
        for (float t = 0; t < 1; t += Time.deltaTime / giay) { float e = Em(t); c.SetPositionAndRotation(Vector3.Lerp(p0, p, e), Quaternion.Slerp(r0, r, e)); yield return null; }
        c.SetPositionAndRotation(p, r);
    }

    public IEnumerator CanhHe(Transform cam)
    {
        var am = GetComponent<AudioSource>(); if (!am) { am = gameObject.AddComponent<AudioSource>(); am.playOnAwake = false; am.spatialBlend = 0; }
        if (!than) than = transform;
        var f = Huong(); var up = Vector3.up;
        Vector3 camLP = cam.localPosition; Quaternion camLR = cam.localRotation, r0 = cam.rotation; Vector3 p0 = cam.position;
        Vector3 day0 = than.position, day1 = day0 + f * keoRa;

        // 1. quỳ xuống trước gầm bàn thờ
        LocFirstPerson.PhuDe("Hé một góc thôi.", 2.2f);
        var p1 = day0 + f * 0.95f + up * 0.62f;
        yield return Bay(cam, p1, Nhin(p1, day0 + up * 0.18f), 1.2f);

        // 2. kéo hũ ra khỏi gầm — ba nhịp giật cục, khớp tiếng sành cạ nền
        if (keo) am.PlayOneShot(keo, 0.9f);
        float t0 = Time.time; var nhip = new[] { (0f, 0.32f), (0.45f, 0.8f), (0.95f, 1.22f) };
        for (int k = 0; k < 3; k++)
        {
            var a = Vector3.Lerp(day0, day1, k / 3f); var b = Vector3.Lerp(day0, day1, (k + 1) / 3f);
            while (Time.time - t0 < nhip[k].Item1) yield return null;
            float t;
            while ((t = (Time.time - t0 - nhip[k].Item1) / (nhip[k].Item2 - nhip[k].Item1)) < 1)
            {
                than.position = Vector3.Lerp(a, b, Em(t)) + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)) * 0.0015f;
                yield return null;
            }
            than.position = b;
        }

        // 3. cúi sát, nhìn chéo xuống miệng hũ; đèn hầm hắt xuống (đèn phụ yếu cho đọc được lòng hũ)
        var mieng = day1 + up * cao;
        var p2 = mieng + f * 0.27f + up * 0.30f;
        var den = new GameObject("Ham_DenPhu_HuCot").AddComponent<Light>();
        den.type = LightType.Point; den.range = 1.3f; den.color = new Color(1f, 0.72f, 0.45f); den.shadows = LightShadows.None; den.intensity = 0;
        den.transform.position = mieng + up * 0.26f + f * 0.20f;   // như ánh đèn hầm hắt xuống miệng hũ
        yield return Bay(cam, p2, Nhin(p2, mieng - f * 0.015f), 0.9f);
        for (float t = 0; t < 1; t += Time.deltaTime / 0.5f) { den.intensity = denManh * t; yield return null; }

        // 4. xé tờ niêm: góc giấy giật lên một chút
        if (xe) am.PlayOneShot(xe, 0.85f);
        for (float t = 0; t < 1; t += Time.deltaTime / 0.6f) { DatGoc(12f * Em(t) + Mathf.Sin(t * 40) * 1.2f * (1 - t)); yield return null; }
        yield return new WaitForSeconds(0.35f);

        // 5. lật hẳn góc giấy dầu gập ra sau — lòng hũ: lọn tóc buộc chỉ đỏ
        if (sot) am.PlayOneShot(sot, 0.8f);
        for (float t = 0; t < 1; t += Time.deltaTime / 1.1f) { DatGoc(Mathf.Lerp(12f, gocMo, Em(t))); yield return null; }
        DatGoc(gocMo);
        var p3 = Vector3.Lerp(p2, mieng, 0.22f);   // từ từ ghé sát
        StartCoroutine(Bay(cam, p3, Nhin(p3, mieng - f * 0.02f), 5.5f));
        yield return new WaitForSeconds(0.6f);
        LocFirstPerson.PhuDe("...Tóc.", 1.8f); yield return new WaitForSeconds(2f);
        LocFirstPerson.PhuDe("Buộc chỉ đỏ.", 1.8f); yield return new WaitForSeconds(2f);
        LocFirstPerson.PhuDe("Lễ giữ mạng. Lấy ít tóc.", 2.4f); yield return new WaitForSeconds(2.6f);

        // 6. đậy vội — góc niêm rách, giấy không khép hẳn
        if (sot) am.PlayOneShot(sot, 0.6f);
        for (float t = 0; t < 1; t += Time.deltaTime / 0.45f) { DatGoc(Mathf.Lerp(gocMo, gocDay, t * t)); yield return null; }
        DatGoc(gocDay);
        for (float t = 1; t > 0; t -= Time.deltaTime / 0.4f) { den.intensity = denManh * t; yield return null; }
        Destroy(den.gameObject);

        // 7. đứng dậy
        yield return Bay(cam, p0, r0, 0.9f);
        cam.localPosition = camLP; cam.localRotation = camLR;
    }
}
