using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

// TẠM THỜI (gộp main nhóm, 23/10): ghép các mảnh file lớn / sao chép / xoá / đo md5 theo lệnh trong LOC_Gop.txt (thư mục gốc project).
// Mỗi dòng: COPY|nguồn|đích   CAT|đích|mảnh1|mảnh2|...   DEL|đường dẫn   MD5|đường dẫn      (đường dẫn tương đối với thư mục gốc project)
// Kết quả: LOC_Gop_KetQua.txt. Xong việc có thể xoá file này.
public static class LocGopFile
{
    static string Md5(string p)
    {
        using var m = System.Security.Cryptography.MD5.Create(); using var s = File.OpenRead(p);
        return string.Concat(m.ComputeHash(s).Select(b => b.ToString("x2")));
    }
    [MenuItem("LOC/Gộp file (theo LOC_Gop.txt)")]
    public static void Chay()
    {
        var sb = new System.Text.StringBuilder();
        if (!File.Exists("LOC_Gop.txt")) { File.WriteAllText("LOC_Gop_KetQua.txt", "không có LOC_Gop.txt\n"); return; }
        foreach (var l in File.ReadAllLines("LOC_Gop.txt"))
        {
            var a = l.Trim().Split('|'); if (a[0].Length == 0) continue;
            try
            {
                switch (a[0])
                {
                    case "COPY":
                        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(a[2])));
                        File.Copy(a[1], a[2], true); sb.AppendLine($"OK COPY {a[1]} -> {a[2]} ({new FileInfo(a[2]).Length} B) md5={Md5(a[2])}"); break;
                    case "CAT":
                        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(a[1])));
                        using (var o = File.Create(a[1])) foreach (var p in a.Skip(2)) { using var i = File.OpenRead(p); i.CopyTo(o); }
                        sb.AppendLine($"OK CAT {a[1]} ({new FileInfo(a[1]).Length} B) md5={Md5(a[1])}"); break;
                    case "ZIP":   // ZIP|tệp.zip|thư mục đích : giải nén, ghi đè
                        {
                            int n = 0;
                            using (var z = System.IO.Compression.ZipFile.OpenRead(a[1]))
                                foreach (var e in z.Entries)
                                {
                                    if (e.FullName.EndsWith("/")) continue;
                                    var dst = Path.Combine(a[2], e.FullName);
                                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(dst)));
                                    using (var s = e.Open()) using (var o = File.Create(dst)) s.CopyTo(o); n++;
                                }
                            sb.AppendLine($"OK ZIP {a[1]} -> {a[2]} ({n} file)"); break;
                        }
                    case "DEL": File.Delete(a[1]); sb.AppendLine("OK DEL " + a[1]); break;
                    case "MD5": sb.AppendLine($"OK MD5 {a[1]} ({new FileInfo(a[1]).Length} B) {Md5(a[1])}"); break;
                    default: sb.AppendLine("??? " + l); break;
                }
            }
            catch (System.Exception e) { sb.AppendLine("LỖI " + l + " :: " + e.Message); }
        }
        File.Delete("LOC_Gop.txt");
        File.WriteAllText("LOC_Gop_KetQua.txt", sb.ToString());
    }
}
