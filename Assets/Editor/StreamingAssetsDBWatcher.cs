using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

// Automatically regenerates a .hash file next to every .db in StreamingAssets
// whenever those files are imported (saved / replaced).
// The hash file is used at runtime on Android to detect DB changes without
// downloading the full file.
public class StreamingAssetsDBWatcher : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string assetPath in importedAssets)
        {
            if (!assetPath.Contains("StreamingAssets"))
                continue;

            if (!assetPath.EndsWith(".db", System.StringComparison.OrdinalIgnoreCase))
                continue;

            string fullPath = Path.GetFullPath(assetPath);
            RegenerateHash(fullPath, assetPath);
        }
    }

    private static void RegenerateHash(string fullPath, string assetPath)
    {
        if (!File.Exists(fullPath))
            return;

        string hash = ComputeMD5(fullPath);
        string hashFilePath = fullPath + ".hash";
        File.WriteAllText(hashFilePath, hash);

        // Tell Unity about the new .hash file so it appears in Project window
        string hashAssetPath = assetPath + ".hash";
        AssetDatabase.ImportAsset(hashAssetPath, ImportAssetOptions.ForceUpdate);

        Debug.Log($"[DBWatcher] Hash updated for {Path.GetFileName(fullPath)}: {hash}");
    }

    private static string ComputeMD5(string filePath)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            byte[] hash = md5.ComputeHash(stream);
            return System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    // Menu item to regenerate all hashes manually if needed
    [MenuItem("Tools/Regenerate DB Hashes")]
    private static void RegenerateAllHashes()
    {
        string streamingAssetsPath = Application.dataPath + "/StreamingAssets";

        if (!Directory.Exists(streamingAssetsPath))
        {
            Debug.LogWarning("[DBWatcher] StreamingAssets folder not found");
            return;
        }

        string[] dbFiles = Directory.GetFiles(streamingAssetsPath, "*.db", SearchOption.AllDirectories);

        foreach (string fullPath in dbFiles)
        {
            string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "").Replace("\\", "/");
            RegenerateHash(fullPath, assetPath);
        }

        AssetDatabase.Refresh();
        Debug.Log($"[DBWatcher] Regenerated hashes for {dbFiles.Length} database(s)");
    }
}
