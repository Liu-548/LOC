using UnityEngine;

// Công tắc điện bật/tắt được. Nhấn E khi nhìn vào cụm công tắc (LocFirstPerson) hoặc gọi Toggle() từ script game.
// dens: các đèn nối với công tắc này (có thể để trống — chỉ lật phím). Phím = con tên "Phim" của cụm (lật ±7°).
public class LocCongTac : MonoBehaviour
{
    public string ten = "công tắc";
    public Light[] dens;
    public GameObject[] hienKhiBat;   // vật chỉ hiện khi bật (bóng sáng…)
    public bool bat = true;

    Transform[] phim;

    void Awake()
    {
        var l = new System.Collections.Generic.List<Transform>();
        foreach (var t in GetComponentsInChildren<Transform>(true)) if (t.name == "Phim") l.Add(t);
        phim = l.ToArray();
        Ap();
    }

    public void Toggle() { bat = !bat; Ap(); }
    public void Dat(bool b) { bat = b; Ap(); }

    void Ap()
    {
        if (dens != null) foreach (var d in dens) if (d) d.enabled = bat;
        if (hienKhiBat != null) foreach (var g in hienKhiBat) if (g) g.SetActive(bat);
        if (phim != null) foreach (var p in phim) if (p) p.localRotation = Quaternion.Euler(bat ? -7f : 7f, 0, 0);
    }
}
