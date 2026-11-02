using UnityEngine;

// [03/11] bóng đi theo đèn dù ai bật / tắt đèn: cảnh của LocNhim nháy thẳng Den_DenNgu_Nhim (enabled), cốt truyện tắt đèn phòng…
//  trước đây chỉ bấm công tắc mới đổi vật liệu bóng → đèn tắt / nháy mà bóng vẫn sáng đều.
public class LocBongTheoDen : MonoBehaviour
{
    Light l; bool cu;
    public void Gan(Light d, bool on) { l = d; cu = on; }
    void LateUpdate()
    {
        if (!l) return;
        bool on = l.enabled && l.gameObject.activeInHierarchy;
        if (on == cu) return;
        cu = on; LocCongTac.ApBong(l, on);
    }
}
