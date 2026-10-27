using System.Collections;
using UnityEngine;

// [21/10] NGƯỜI DÌ — người họ hàng trông nhà, chỉ có ở Đêm 1 (kịch bản v3.2 C1 "Người họ hàng").
//  Đứng chờ ở phòng khách, người chơi lại gần (≤ 3,5 m) hoặc đang nói thì quay người về phía người chơi.
//  LocCotTruyen gọi Noi(true/false) theo từng câu thoại, rồi Ve(): đi ra cổng, qua cổng thì khép cổng lại, khuất tầm nhìn thì biến mất.
public class LocDi : MonoBehaviour
{
    public static LocDi I;
    public Animator anim;
    public Vector3 huong0 = Vector3.forward;   // hướng mặt model khi rotation = identity (đo lúc dựng)
    public Vector3[] duongVe;
    public int diemCong = 2;
    public float tocDoDi = 1.1f;
    public bool daVe;
    static readonly string[] tenCong = { "CanhCong_Trai (mở)", "CanhCong_Phai (mở)" };
    bool dangDi, dangNoi; string clip = "";
    Transform nguoiChoi;

    void Awake() { I = this; }

    // [27/10 chơi thử] "chân không chạm đất": đường đi dựng sẵn nội suy thẳng giữa các điểm (qua bậc hiên, ngưỡng cửa thì lơ lửng/lún),
    //  và tư thế clip có thể nhấc gốc. Mỗi khung sau hoạt cảnh: đo bàn chân thấp nhất so với sàn ngay dưới, kéo cả người xuống/lên cho chạm.
    Transform[] banChan; Collider[] cuaToi;
    void LateUpdate()
    {
        if (banChan == null)
        {
            var l = new System.Collections.Generic.List<Transform>();
            foreach (var t in GetComponentsInChildren<Transform>(true)) if (t.name == "Toes_L" || t.name == "Toes_R" || t.name == "Foot_L" || t.name == "Foot_R") l.Add(t);
            banChan = l.ToArray(); cuaToi = GetComponentsInChildren<Collider>(true);
        }
        if (banChan.Length == 0) return;
        float thap = float.MaxValue; foreach (var b in banChan) if (b) thap = Mathf.Min(thap, b.position.y);
        var goc = transform.position + Vector3.up * 0.6f;
        var hits = Physics.RaycastAll(goc, Vector3.down, 1.6f, ~0, QueryTriggerInteraction.Ignore);
        float san = float.MinValue;
        foreach (var h in hits) { bool cua = false; foreach (var c in cuaToi) if (h.collider == c) { cua = true; break; } if (!cua && h.point.y > san) san = h.point.y; }
        if (san == float.MinValue) return;
        float lech = san + 0.02f - thap;   // mũi/gót chân cách sàn 2 cm
        if (Mathf.Abs(lech) > 0.004f && Mathf.Abs(lech) < 0.35f) transform.position += Vector3.up * Mathf.Clamp(lech, -0.6f * Time.deltaTime - 0.01f, 0.6f * Time.deltaTime + 0.01f);
    }
    void Start()
    {
        nguoiChoi = FindAnyObjectByType<LocFirstPerson>()?.transform;
        if (LocTui.Co("ho_hang_ve")) { daVe = true; gameObject.SetActive(false); return; }   // chơi lại đầu đêm sau đoạn này
        Clip("Dung_Cho", 0f);
    }

    void Clip(string ten, float fade = 0.3f) { if (anim && clip != ten) { clip = ten; anim.CrossFadeInFixedTime(ten, fade); } }

    void Quay(Vector3 toi, float doMoiGiay)
    {
        var d = toi - transform.position; d.y = 0; if (d.sqrMagnitude < 1e-4f) return;
        var muon = Quaternion.Euler(0, Vector3.SignedAngle(huong0, d, Vector3.up), 0);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, muon, doMoiGiay * Time.deltaTime);
    }

    void Update()
    {
        if (dangDi || !nguoiChoi) return;
        if (dangNoi || (nguoiChoi.position - transform.position).sqrMagnitude < 3.5f * 3.5f) Quay(nguoiChoi.position, 120f);
    }

    public void Noi(bool noi) { dangNoi = noi; if (!dangDi) Clip(noi ? "Dung_Noi" : "Dung_Cho"); }

    public void Ve() { if (!dangDi && gameObject.activeInHierarchy) StartCoroutine(DiVe()); }

    IEnumerator DiVe()
    {
        dangDi = true; dangNoi = false;
        var col = GetComponent<Collider>(); if (col) col.enabled = false;
        // quay về phía cửa trước rồi mới bước
        for (float t = 0; t < 0.6f; t += Time.deltaTime) { Quay(duongVe[0], 240f); yield return null; }
        Clip("Di_Bo", 0.25f);
        for (int i = 0; i < duongVe.Length; i++)
        {
            if (i == diemCong) foreach (var n in tenCong) GameObject.Find(n)?.GetComponent<LocDoor>()?.DatNgay(true);
            var dich = duongVe[i];
            while (new Vector2(dich.x - transform.position.x, dich.z - transform.position.z).sqrMagnitude > 0.04f)
            {
                Quay(dich, 300f);
                var p0 = transform.position;   // đi theo mặt phẳng, độ cao do LateUpdate bám sàn
                transform.position = Vector3.MoveTowards(p0, new Vector3(dich.x, p0.y, dich.z), tocDoDi * Time.deltaTime);
                if (i > diemCong + 0 && Khuat()) { XongVe(); yield break; }
                yield return null;
            }
            if (i == diemCong + 1) foreach (var n in tenCong) GameObject.Find(n)?.GetComponent<LocDoor>()?.DatNgay(false);   // ra khỏi cổng thì khép lại
        }
        XongVe();
    }

    // đã ra khỏi cổng: người chơi không nhìn thấy, hoặc đã xa
    bool Khuat()
    {
        var cam = Camera.main; if (!cam) return true;
        if ((cam.transform.position - transform.position).sqrMagnitude > 14f * 14f) return true;
        var v = cam.WorldToViewportPoint(transform.position + Vector3.up * 0.9f);
        return v.z < 0 || v.x < -0.1f || v.x > 1.1f || v.y < -0.1f || v.y > 1.1f;
    }

    void XongVe()
    {
        foreach (var n in tenCong) GameObject.Find(n)?.GetComponent<LocDoor>()?.DatNgay(false);
        daVe = true; gameObject.SetActive(false);
    }
}
