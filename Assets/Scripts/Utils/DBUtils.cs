using UnityEngine;
using SQLite;
using System.IO;
using System.Collections;

public class DBUtils : MonoBehaviour
{
    private string dbPath;
    private const string dbName = "ltlrn01.db";

    private void Start()
    {
        StartCoroutine(InitializeDatabase());
    }

    private IEnumerator InitializeDatabase()
    {
#if UNITY_ANDROID
        // On Android, copy from StreamingAssets to persistentDataPath
        dbPath = Path.Combine(Application.persistentDataPath, dbName);

        if (!File.Exists(dbPath))
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, dbName);
            Debug.Log($"Copying database from {sourcePath} to {dbPath}");

            UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(sourcePath);
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(dbPath, www.downloadHandler.data);
                Debug.Log("Database copied successfully");
            }
            else
            {
                Debug.LogError($"Failed to copy database: {www.error}");
                yield break;
            }
        }
        else
        {
            Debug.Log("Database already exists at: " + dbPath);
        }
#elif UNITY_EDITOR
        // In Unity Editor, use Assets/StreamingAssets
        dbPath = Path.Combine(Application.dataPath, "StreamingAssets", dbName);
        yield return null;
#else
        // On PC build, use StreamingAssets next to executable
        dbPath = Path.Combine(Application.streamingAssetsPath, dbName);
        yield return null;
#endif

        Debug.Log("Final Database Path: " + dbPath);
        string result = CheckConnection();
        Debug.Log(result);
    }

    public string CheckConnection()
    {
        if (!File.Exists(dbPath))
        {
            string error = $"Database file not found: {dbPath}";
            Debug.LogError(error);
            return error;
        }

        FileInfo fileInfo = new FileInfo(dbPath);
        Debug.Log($"Database file size: {fileInfo.Length} bytes");

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                var result = connection.ExecuteScalar<int>("SELECT 1");
                string success = $"Connection ok, result: {result}, Size: {fileInfo.Length} bytes";
                Debug.Log(success);
                return success;
            }
        }
        catch (System.Exception ex)
        {
            string error = $"SQLite connection failed: {ex.Message}";
            Debug.LogError(error);
            return error;
        }
    }
}