using UnityEngine;

// [02/11] đèn phụ đi theo một đèn khác: bật / tắt / cường độ (công tắc, mất điện, chập chờn của LocMa)
public class LocDenTheo : MonoBehaviour
{
    Light goc, l; float tiLe;
    public void Gan(Light theo, Light minh) { goc = theo; l = minh; tiLe = theo.intensity > 0 ? minh.intensity / theo.intensity : 0.3f; }
    void LateUpdate()
    {
        if (!l) return;
        bool bat = goc && goc.enabled && goc.gameObject.activeInHierarchy;
        l.enabled = bat;
        if (bat) l.intensity = goc.intensity * tiLe;
    }
}
