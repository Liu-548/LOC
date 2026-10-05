using UnityEngine;

// ngọn lửa đèn dầu: cường độ nhấp nháy ngẫu nhiên quanh giá trị gốc
[RequireComponent(typeof(Light))]
public class LocLuaNhapNhay : MonoBehaviour
{
    Light l; float goc;
    void Awake() { l = GetComponent<Light>(); goc = l.intensity; }
    void Update() => l.intensity = goc * (0.75f + 0.5f * Mathf.PerlinNoise(Time.time * 7f, 0.3f));
}
