using UnityEngine;

// [02/11] Bộ texture PBR (D2_*) cho bản "Thực tế". Asset Assets/LOC_House/Resources/LocThucTeBo.asset do Editor/LocThucTeBoTao.cs tự tạo.
// Giữ tham chiếu để texture được đóng vào bản build (texture gốc nằm ngoài thư mục Resources).
public class LocThucTeBo : ScriptableObject
{
    public string[] lop;          // vd: "xm", "gach", "go"
    public Texture2D[] a, n, m;   // albedo · normal · (R metallic, G AO, A smoothness)

    public bool Lay(string l, out Texture2D A, out Texture2D N, out Texture2D M)
    {
        A = N = M = null; if (lop == null) return false;
        int i = System.Array.IndexOf(lop, l); if (i < 0) return false;
        A = a[i]; N = n[i]; M = m[i]; return true;
    }
}
