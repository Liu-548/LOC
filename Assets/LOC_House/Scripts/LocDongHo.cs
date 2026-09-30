using UnityEngine;

// Đồng hồ quả lắc (DongHoQuaLac.glb có sẵn node con QuaLac / KimGio / KimPhut).
// Quả lắc lắc ±bienDo° quanh điểm treo, chu kỳ chuKy giây. Kim mặc định đứng yên ở 8 giờ 20; bật kimChay để kim chạy theo giờ game.
public class LocDongHo : MonoBehaviour
{
    public Transform quaLac, kimGio, kimPhut;
    public float bienDo = 6f, chuKy = 1f;
    public bool kimChay = false;
    Quaternion gocLac, gocGio, gocPhut;

    void Awake()
    {
        if (!quaLac) quaLac = Tim(transform, "QuaLac");
        if (!kimGio) kimGio = Tim(transform, "KimGio");
        if (!kimPhut) kimPhut = Tim(transform, "KimPhut");
        if (quaLac) gocLac = quaLac.localRotation;
        if (kimGio) gocGio = kimGio.localRotation;
        if (kimPhut) gocPhut = kimPhut.localRotation;
    }

    void Update()
    {
        if (quaLac) quaLac.localRotation = gocLac * Quaternion.Euler(0, 0, bienDo * Mathf.Sin(Time.time * 2f * Mathf.PI / chuKy));
        if (!kimChay) return;
        float phut = Time.time / 60f;   // mỗi 60 giây thật = 1 phút trên mặt số
        if (kimPhut) kimPhut.localRotation = gocPhut * Quaternion.Euler(0, 0, -6f * phut);
        if (kimGio) kimGio.localRotation = gocGio * Quaternion.Euler(0, 0, -0.5f * phut);
    }

    static Transform Tim(Transform t, string ten)
    {
        foreach (var c in t.GetComponentsInChildren<Transform>(true)) if (c.name == ten) return c;
        return null;
    }
}
