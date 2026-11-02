using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

// [31/10] Build bản Windows chỉ gồm cảnh LOC_NhaLoc (Build Settings trước đây chỉ có SampleScene → .exe mở cảnh mẫu).
// Kết quả + lỗi ghi LOC_Build_KetQua.txt; bản build ở Build/LOC/ (không đưa lên git).
public static class LocBuild
{
    const string Canh = "Assets/Scenes/LOC_NhaLoc.unity";
    const string CanhMo = "Assets/Scenes/LOC_GiacMo_Dem2.unity";   // [02/11] cảnh riêng của giấc mơ cuối Đêm 2 (LocGiacMo nạp chồng lúc vào mơ)

    [MenuItem("LOC/Build thử (Windows .exe)")]
    public static void Build()
    {
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Canh, true), new EditorBuildSettingsScene(CanhMo, true) };
        var r = BuildPipeline.BuildPlayer(new[] { Canh, CanhMo }, "Build/LOC/LOC.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        var loi = r.steps.SelectMany(s => s.messages).Where(m => m.type is UnityEngine.LogType.Error or UnityEngine.LogType.Exception).Select(m => m.content);
        System.IO.File.WriteAllText("LOC_Build_KetQua.txt",
            $"{System.DateTime.Now:dd/MM HH:mm} {r.summary.result} · {r.summary.totalErrors} lỗi · {r.summary.totalWarnings} cảnh báo · {r.summary.totalSize / 1048576} MB · {r.summary.totalTime:mm\\:ss}\n" + string.Join("\n", loi));
    }
}
