using UnityEngine;

// [6/10] Vật tương tác chung: nhìn vào rồi bấm E. LocFirstPerson hỏi GoiY (rỗng = không hiện, không bấm được) rồi gọi Bam().
public abstract class LocTuongTac : MonoBehaviour
{
    public abstract string GoiY { get; }
    public abstract void Bam();
}
