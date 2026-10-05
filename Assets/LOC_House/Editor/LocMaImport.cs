using UnityEditor;

// [4/10] model con ma phải đọc được đỉnh (bài tự kiểm đo đè tường từ đỉnh lưới) → bật Read/Write cho Models/Ma/*.fbx
public class LocMaImport : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        if (!assetPath.Replace('\\', '/').Contains("LOC_House/Models/Ma/")) return;
        var mi = (ModelImporter)assetImporter;
        mi.isReadable = true;
        mi.addCollider = false;
    }

    [MenuItem("LOC/Nạp lại model con ma")]
    static void NapLai()
    {
        foreach (var t in new[] { "LoDau", "Tay", "LanCan", "VanNguoi", "NgoiXom", "BoTran" })
            AssetDatabase.ImportAsset($"Assets/LOC_House/Models/Ma/Ma_{t}.fbx", ImportAssetOptions.ForceUpdate);
    }
}
