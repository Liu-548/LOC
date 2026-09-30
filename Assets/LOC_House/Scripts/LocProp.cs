using UnityEngine;

// Đánh dấu đồ đạc do LocHouseBuilder đặt, để công cụ "LOC → Rà soát" biết món nào được phép thả/đẩy.
//  San    = đồ đứng trên sàn hoặc đặt lên mặt đồ khác → được thả chạm mặt đỡ + gỡ chồng lấn
//  Treo   = đồ treo tường/trần → không đụng tới
//  CoDinh = đặt đúng toạ độ có chủ đích (hầm, cửa cuốn, rạp, trụ cổng…) → không đụng tới
//  Cua    = cánh cửa → không đụng tới, không làm mặt đỡ
public class LocProp : MonoBehaviour
{
    public enum Kieu { San, Treo, CoDinh, Cua }
    public Kieu kieu = Kieu.San;
}
