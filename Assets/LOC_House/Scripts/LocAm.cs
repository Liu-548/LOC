using System.Collections.Generic;
using UnityEngine;

// [1/11] Âm hiệu ứng dùng chung (BUG-009 "không có tiếng bước chân, cửa, công tắc"): file trong Resources/LOC_AmThanh
//  (tổng hợp bằng LOC_Nguon/AmThanh/tao_am_0111.py — thay file cùng tên bằng âm thu thật là xong, không đổi code).
public static class LocAm
{
    static readonly Dictionary<string, AudioClip> kho = new();
    static AudioClip Lay(string ten)
    {
        if (!kho.TryGetValue(ten, out var c)) kho[ten] = c = Resources.Load<AudioClip>("LOC_AmThanh/" + ten);
        return c;
    }
    public static void Phat(string ten, Vector3 p, float vol = 1f, float caoDo = 1f)
    {
        var c = Lay(ten); if (!c) return;
        var g = new GameObject("_Am_" + ten); g.transform.position = p;
        var a = g.AddComponent<AudioSource>(); a.clip = c; a.volume = vol; a.pitch = caoDo; a.spatialBlend = 1f;
        a.minDistance = 0.8f; a.maxDistance = 14f; a.rolloffMode = AudioRolloffMode.Logarithmic; a.Play();
        Object.Destroy(g, c.length / Mathf.Max(0.1f, caoDo) + 0.1f);
    }
    static int truoc = -1;
    public static void BuocChan(Vector3 p, float vol)
    {
        // [13/11] tiếng bước mới: 6 bước dép trên nền (freesound footsteps-rug-slippers-slow-pace) · bản gạch cũ BuocChan_Gach_0..3 vẫn để trong thư mục
        int i = Random.Range(0, 6); if (i == truoc) i = (i + 1) % 6; truoc = i;
        Phat("BuocChan_Dep_" + i, p, vol, Random.Range(0.94f, 1.06f));
    }
}
