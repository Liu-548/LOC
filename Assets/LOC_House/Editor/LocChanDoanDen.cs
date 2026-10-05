using System.Linq;
using UnityEditor;
using UnityEngine;

// [4/10] chẩn đoán: mọi renderer/đèn trong phòng Nhím + vật liệu, shader, tính chất phát sáng → LOC_KiemTra/Ma/DenPhongNhim.txt
public static class LocChanDoanDen
{
    [MenuItem("LOC/Chẩn đoán đèn phòng Nhím")]
    static void Run()
    {
        var b = new Bounds(new Vector3(5.4f, 5.2f, 10.45f), new Vector3(4.6f, 3.4f, 4.1f));
        var sb = new System.Text.StringBuilder();
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (b.Contains(l.transform.position)) sb.AppendLine($"ĐÈN {l.name} {l.type} I={l.intensity} bật={l.enabled}");
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (!b.Contains(r.bounds.center)) continue;
            foreach (var m in r.sharedMaterials.Where(x => x))
            {
                string em = "";
                if (m.HasProperty("_EmissionColor")) em += $" _EmissionColor={m.GetColor("_EmissionColor")} kw={m.IsKeywordEnabled("_EMISSION")}";
                if (m.HasProperty("emissiveFactor")) em += $" emissiveFactor={m.GetColor("emissiveFactor")}";
                if (m.HasProperty("_BaseColor")) em += $" base={m.GetColor("_BaseColor")}";
                sb.AppendLine($"{r.name} [{r.transform.parent?.name}] {m.name} ({m.shader.name}){em}");
            }
        }
        System.IO.Directory.CreateDirectory("LOC_KiemTra/Ma");
        System.IO.File.WriteAllText("LOC_KiemTra/Ma/DenPhongNhim.txt", sb.ToString());
    }
}
