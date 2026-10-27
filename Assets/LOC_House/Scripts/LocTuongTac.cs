using UnityEngine;

// [6/10] Vật tương tác chung: nhìn vào rồi bấm E. LocFirstPerson hỏi GoiY (rỗng = không hiện, không bấm được) rồi gọi Bam().
public abstract class LocTuongTac : MonoBehaviour
{
    public abstract string GoiY { get; }
    public abstract void Bam();
    // [27/10 chơi thử] điểm tia nhìn trúng có tính là nhìn vào vật không (mặc định: có). Vật to như bàn thờ vong thu hẹp vùng bấm.
    public virtual bool NhanTai(Vector3 diem) => true;
}
