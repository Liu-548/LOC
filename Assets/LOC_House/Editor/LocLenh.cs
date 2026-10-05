using UnityEditor;
using UnityEngine;

// Chạy menu LOC theo lệnh ghi vào file LOC_Lenh.txt ở thư mục gốc project (mỗi dòng 1 menu), kết quả ghi LOC_Lenh_KetQua.txt.
[InitializeOnLoad]
public static class LocLenh
{
    const string F = "LOC_Lenh.txt", KQ = "LOC_Lenh_KetQua.txt";
    static double next;
    static LocLenh()
    {
        EditorApplication.update += Tick;
        // [3/10] ghi lỗi biên dịch ra LOC_BienDich.txt (đọc được từ ngoài Unity)
        UnityEditor.Compilation.CompilationPipeline.assemblyCompilationFinished += (asm, msgs) =>
        {
            var sb = new System.Text.StringBuilder();
            foreach (var m in msgs) if (m.type == UnityEditor.Compilation.CompilerMessageType.Error) sb.AppendLine(m.message);
            System.IO.File.AppendAllText("LOC_BienDich.txt", $"{System.DateTime.Now:HH:mm:ss} {System.IO.Path.GetFileName(asm)} {(sb.Length == 0 ? "OK" : "LỖI")}\n{sb}");
        };
    }

    static void Tick()
    {
        if (EditorApplication.timeSinceStartup < next) return;
        next = EditorApplication.timeSinceStartup + 1.0;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!System.IO.File.Exists(F)) return;
        var lines = System.IO.File.ReadAllLines(F);
        System.IO.File.Delete(F);
        var sb = new System.Text.StringBuilder();
        foreach (var l in lines)
        {
            var m = l.Trim(); if (m.Length == 0) continue;
            bool ok = false;
            try { ok = EditorApplication.ExecuteMenuItem(m); } catch (System.Exception e) { sb.AppendLine("EXC " + e.Message); }
            sb.AppendLine($"{System.DateTime.Now:HH:mm:ss} {(ok ? "OK" : "KHÔNG THẤY MENU")} {m}");
        }
        System.IO.File.WriteAllText(KQ, sb.ToString());
    }
}
