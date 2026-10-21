using UnityEditor;

public static class LocChanDoanNhayMenu
{
    [MenuItem("LOC/Chẩn đoán nháy (Play)")]
    static void Chay()
    {
        EditorPrefs.SetBool("LOC_ChanDoanNhay", true);
        if (!EditorPrefs.HasKey("LOC_RIB")) EditorPrefs.SetBool("LOC_RIB", PlayerSettings.runInBackground);   // LocNhimImport trả lại khi thoát Play
        PlayerSettings.runInBackground = true;
        EditorApplication.isPlaying = true;
    }
}
