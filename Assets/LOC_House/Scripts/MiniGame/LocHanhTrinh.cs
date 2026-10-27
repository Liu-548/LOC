using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Hành trình từ đầu tới cuối — tự tạo khi vào cảnh có LocFirstPerson, không cần kéo vào scene.
//  · Ráp xong chén đêm 1, 2 (LocNghiThuc.DaXong): màn đen, chữ chuyển đêm, sang đêm sau, đưa người chơi về chỗ xuất phát.
//  · Ráp xong chén đêm 3: màn kết "HẾT", bấm R chơi lại từ đầu (xoá túi đồ, về đêm 1).
public class LocHanhTrinh : MonoBehaviour
{
    LocFirstPerson fp;
    Vector3 viTriDau, huongDau;
    bool dangChuyen, choR;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Tao()
    {
        if (Object.FindAnyObjectByType<LocFirstPerson>() == null) return;
        new GameObject("LocHanhTrinh").AddComponent<LocHanhTrinh>();
    }

    void Start()
    {
        fp = Object.FindAnyObjectByType<LocFirstPerson>();
        if (fp) { viTriDau = fp.transform.position; huongDau = fp.transform.forward; }
        LocNghiThuc.DaXong += KhiNghiThucXong;
    }

    void OnDestroy() { LocNghiThuc.DaXong -= KhiNghiThucXong; }

    void KhiNghiThucXong(int dem)
    {
        if (dangChuyen) return;
        StartCoroutine(dem >= 3 ? Ket() : ChuyenDem(dem + 1));
    }

    static IEnumerator Den(float tu, float den, float giay)
    {
        for (float t = 0; t < 1f; t += Time.deltaTime / giay) { LocFirstPerson.den = Mathf.Lerp(tu, den, t); yield return null; }
        LocFirstPerson.den = den;
    }

    IEnumerator ChuyenDem(int demMoi)
    {
        dangChuyen = true; LocFirstPerson.khoa = true; LocMa.tamDung = true;
        yield return Den(LocFirstPerson.den, 1f, 1.2f);
        LocFirstPerson.chuGiua = "Hết đêm thứ " + Chu(demMoi - 1) + ".";
        yield return new WaitForSeconds(2.2f);
        LocFirstPerson.chuGiua = "Đêm thứ " + Chu(demMoi) + ".";
        LocMa.SangDem(demMoi);
        if (fp) fp.DatTuThe(viTriDau, viTriDau + huongDau * 3f + Vector3.up * 1.52f);
        yield return new WaitForSeconds(2.2f);
        LocFirstPerson.chuGiua = null;
        LocMa.tamDung = false;
        yield return Den(1f, 0f, 1.2f);
        LocFirstPerson.khoa = false; dangChuyen = false;
    }

    IEnumerator Ket()
    {
        dangChuyen = true; LocFirstPerson.khoa = true; LocMa.tamDung = true;
        yield return Den(LocFirstPerson.den, 1f, 2f);
        LocFirstPerson.chuGiua = "Chiếc chén liền lại.";
        yield return new WaitForSeconds(3f);
        LocFirstPerson.chuGiua = "Trời sáng. Nhà im.";
        yield return new WaitForSeconds(3f);
        LocFirstPerson.chuGiua = "HẾT\n\n\n\n[R]  Chơi lại từ đầu";
        choR = true;
    }

    static string Chu(int d) { return d == 1 ? "nhất" : d == 2 ? "hai" : "ba"; }

    void Update()
    {
        if (!choR) return;
        var k = Keyboard.current;
        if (k == null || !k.rKey.wasPressedThisFrame) return;
        choR = false;
        LocTui.XoaHet(); LocMa.BatDauLai();
        LocFirstPerson.khoa = false; LocFirstPerson.den = 0; LocFirstPerson.chuGiua = null; LocMa.tamDung = false;
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(SceneManager.GetActiveScene().path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#endif
    }
}
