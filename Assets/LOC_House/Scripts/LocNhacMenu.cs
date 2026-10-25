using UnityEngine;

// [25/10] Nhạc menu chính (file người dùng đưa: Resources/LOC_MaNu/Nhac_Menu, vòng lặp 32 s).
//  Phát khi màn cảnh báo / menu chính / chọn đêm / ending / cài đặt đang phủ màn hình TRƯỚC khi vào game;
//  bấm vào game thì mờ dần 1,5 s rồi tắt. Không phát ở menu tạm dừng trong game.
//  Tự gắn khi nạp cảnh — không cần sửa scene. Âm lượng: amLuong.
public class LocNhacMenu : MonoBehaviour
{
    public float amLuong = 0.75f;
    AudioSource a;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void DangKy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= KhiNap;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += KhiNap;
    }
    static void KhiNap(UnityEngine.SceneManagement.Scene s, UnityEngine.SceneManagement.LoadSceneMode m)
    {
        if (FindAnyObjectByType<LocNhacMenu>()) return;
        new GameObject("LOC_NhacMenu").AddComponent<LocNhacMenu>();
    }

    void Awake()
    {
        var c = Resources.Load<AudioClip>("LOC_MaNu/Nhac_Menu"); if (!c) { enabled = false; return; }
        a = gameObject.AddComponent<AudioSource>(); a.clip = c; a.loop = true; a.spatialBlend = 0; a.playOnAwake = false; a.volume = 0;
        a.ignoreListenerPause = true;   // menu có thể dừng AudioListener — nhạc menu vẫn chạy
    }

    void Update()
    {
        if (!a) return;
        bool menu = LocMenu.OMenuChinh;
        float dich = menu ? amLuong : 0f;
        a.volume = Mathf.MoveTowards(a.volume, dich, Time.unscaledDeltaTime * amLuong / (menu ? 2f : 1.5f));
        if (a.volume > 0 && !a.isPlaying) a.Play();
        if (a.volume <= 0 && a.isPlaying) a.Stop();
    }
}
