using UnityEngine;
using SQLite;
using System.IO;

public class DBUtils : MonoBehaviour
{
    private string dbPath;

    private void Start()
    {
        dbPath = Path.Combine(Application.dataPath, "StreamingAssets", "ltlrn01.db");
        
        Debug.Log("Database Path: " + dbPath);
        CheckConnection();
    }

    public string CheckConnection()
    {
        string message;

        if (!File.Exists(dbPath))
        {
            message = $"Database file not found: {dbPath}";
            Debug.LogError(message);
            return message;
        }

        FileInfo fileInfo = new FileInfo(dbPath);
        //Debug.Log($"Database file size: {fileInfo.Length} bytes");

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                var result = connection.ExecuteScalar<int>("SELECT 1");
                message = $"Connection ok, result: {result}, {fileInfo.Length}bytes";
                Debug.Log(message);
                return message;
            }
        }
        catch (System.Exception ex)
        {
            message = $"SQLite connection failed: {ex.Message}";
            Debug.LogError(message);
            return message;
        }
    }
}