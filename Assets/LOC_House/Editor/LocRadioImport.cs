using UnityEditor;
using UnityEngine;

// [23/10] nhạc radio (5 phút, stereo): phát luồng (Streaming) cho nhẹ bộ nhớ, nén Vorbis
public class LocRadioImport : AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        if (!assetPath.Contains("LOC_Radio_Nhac")) return;
        var ai = (AudioImporter)assetImporter;
        var s = ai.defaultSampleSettings;
        s.loadType = AudioClipLoadType.Streaming; s.compressionFormat = AudioCompressionFormat.Vorbis; s.quality = 0.7f;
        ai.defaultSampleSettings = s;
        ai.forceToMono = false; ai.loadInBackground = true;
    }
}
