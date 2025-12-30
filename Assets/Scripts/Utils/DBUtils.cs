using SQLite;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DBUtils : MonoBehaviour
{
    private static DBUtils _instance;
    public static DBUtils Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DBUtils>(); // Updated

                if (_instance == null)
                {
                    Debug.LogError("DBUtils instance not found in scene!");
                }
            }
            return _instance;
        }
    }

    private string dbPath;
    private bool isInitialized = false;
    private const string dbName = "ltlrn01.db";


    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        StartCoroutine(InitializeDatabase());
    }

    private void CreateTablesIfNeeded()
    {
        if (!isInitialized) return;

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                // Create Sections table if it doesn't exist
                connection.CreateTable<SectionDB>();
                connection.CreateTable<ThemesDB>();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error creating tables: {ex.Message}");
        }
    }

    private IEnumerator InitializeDatabase()
    {
        string sourceDbPath;

#if UNITY_ANDROID
        // Android: Copy from APK to writable location
        sourceDbPath = Path.Combine(Application.streamingAssetsPath, dbName);
        dbPath = Path.Combine(Application.persistentDataPath, dbName);
#elif UNITY_EDITOR
    // Editor: Copy from StreamingAssets to persistentDataPath for testing
    sourceDbPath = Path.Combine(Application.dataPath, "StreamingAssets", dbName);
    dbPath = Path.Combine(Application.persistentDataPath, dbName);
#else
    // PC Build: Copy to persistentDataPath
    sourceDbPath = Path.Combine(Application.streamingAssetsPath, dbName);
    dbPath = Path.Combine(Application.persistentDataPath, dbName);
#endif

        // Always copy database to writable location if not exists
        if (!File.Exists(dbPath))
        {
            Debug.Log($"Copying database from {sourceDbPath} to {dbPath}");

#if UNITY_ANDROID
            UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(sourceDbPath);
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
#else
        // PC/Editor: Direct file copy
        if (File.Exists(sourceDbPath))
        {
            File.Copy(sourceDbPath, dbPath);
            Debug.Log("Database copied successfully");
        }
        else
        {
            Debug.LogError($"Source database not found: {sourceDbPath}");
            yield break;
        }
        yield return null;
#endif
        }
        else
        {
            Debug.Log("Database already exists at: " + dbPath);
        }

        Debug.Log("Final Database Path: " + dbPath);
        string result = CheckConnection();
        
        //Debug.Log(result);

        CreateTablesIfNeeded();
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
       //Debug.Log($"Database file size: {fileInfo.Length} bytes");

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                var result = connection.ExecuteScalar<int>("SELECT 1");
                isInitialized = true;
                Debug.Log("Connection ok");
                return "ok";
            }
        }
        catch (System.Exception ex)
        {
            string error = $"SQLite connection failed: {ex.Message}";
            Debug.LogError(error);
            isInitialized = false;
            return error;
        }
    }

    //section
    public void EnsureSectionExists(string sectionName)
    {
        if (!isInitialized)
        {
            Debug.LogError("Database not initialized! Wait for initialization to complete.");
            return;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                // Check if exists
                var existing = connection.Table<SectionDB>()
                    .Where(s => s.Name == sectionName)
                    .FirstOrDefault();

                // If not exists, create it
                if (existing == null)
                {
                    var newSection = new SectionDB
                    {
                        Name = sectionName,
                        QDone = 0,
                        Liked = "false",
                        Time = "00:00"
                    };

                    int rowsAffected = connection.Insert(newSection);
                }
                else
                {
                    //Debug.Log($"Section already exists: {sectionName} (ID: {existing.ID})");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error ensuring section exists: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
        }
    }

    //theme
    public void EnsureThemeExists(string themeName)
    {
        if (!isInitialized)
        {
            Debug.LogError("Database not initialized! Wait for initialization to complete.");
            return;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                // Check if exists
                var existing = connection.Table<ThemesDB>()
                    .Where(t => t.Name == themeName)
                    .FirstOrDefault();

                // If not exists, create it
                if (existing == null)
                {
                    var newTheme = new ThemesDB
                    {
                        Name = themeName
                    };

                    connection.Insert(newTheme);                    
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error ensuring theme exists: {ex.Message}");
        }
    }

    public void UpdateSectionLiked(string sectionName, bool isLiked)
    {
        if (!isInitialized)
        {
            Debug.LogError("Database not initialized!");
            return;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                var section = connection.Table<SectionDB>()
                    .Where(s => s.Name == sectionName)
                    .FirstOrDefault();

                if (section != null)
                {
                    section.Liked = isLiked ? "true" : "false";
                    connection.Update(section);
                    //Debug.Log($"Updated {sectionName} - Liked: {section.Liked}");
                }
                else
                {
                    Debug.LogWarning($"Section not found: {sectionName}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error updating section liked status: {ex.Message}");
        }
    }

    public bool GetSectionLikedStatus(string sectionName)
    {
        if (!isInitialized)
        {
            Debug.LogError("Database not initialized!");
            return false;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                var section = connection.Table<SectionDB>()
                    .Where(s => s.Name == sectionName)
                    .FirstOrDefault();

                if (section != null)
                {
                    return section.Liked == "true";
                }
                else
                {
                    Debug.LogWarning($"Section not found: {sectionName}");
                    return false;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error getting section liked status: {ex.Message}");
            return false;
        }
    }

    public int GetSectionProgress(string sectionName)
    {
        if (!isInitialized)
        {
            Debug.LogError("Database not initialized!");
            return 0;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                var section = connection.Table<SectionDB>()
                    .Where(s => s.Name == sectionName)
                    .FirstOrDefault();

                if (section != null)
                {
                    return section.QDone;
                }
                else
                {
                    Debug.LogWarning($"Section not found: {sectionName}");
                    return 0;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error getting section liked status: {ex.Message}");
            return 0;
        }
    }

    // Generic method to get value from any table
    public string GetValue(string tableName, string columnName, int recordID)
    {
        if (!isInitialized)
        {
            Debug.LogError("Database not initialized!");
            return null;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                string query = $"SELECT {columnName} FROM {tableName} WHERE ID = ?";
                var result = connection.ExecuteScalar<string>(query, recordID);
                return result;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error getting value from {tableName}.{columnName} (ID={recordID}): {ex.Message}");
            return null;
        }
    }

    // Get value by WHERE clause
    public string GetValueWhere(string tableName, string columnName, string whereColumn, string whereValue)
    {
        if (!isInitialized)
        {
            Debug.LogError("Database not initialized!");
            return null;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                string query = $"SELECT {columnName} FROM {tableName} WHERE {whereColumn} = ?";
                var result = connection.ExecuteScalar<string>(query, whereValue);
                return result;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error: {ex.Message}");
            return null;
        }
    }

    // Get full record from Numerals table
    public NumeralDB GetNumeral(int id)
    {
        if (!isInitialized) return null;

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                return connection.Table<NumeralDB>()
                    .Where(n => n.ID == id)
                    .FirstOrDefault();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error getting numeral: {ex.Message}");
            return null;
        }
    }

    // Get numeral by digit
    public NumeralDB GetNumeralByDigit(int digit)
    {
        if (!isInitialized) return null;

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                return connection.Table<NumeralDB>()
                    .Where(n => n.Digit == digit)
                    .FirstOrDefault();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error getting numeral by digit: {ex.Message}");
            return null;
        }
    }

    // Get random numerals
    public List<NumeralDB> GetRandomNumerals(int count)
    {
        if (!isInitialized) return new List<NumeralDB>();

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                var all = connection.Table<NumeralDB>().ToList();
                return all.OrderBy(x => System.Guid.NewGuid()).Take(count).ToList();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error getting random numerals: {ex.Message}");
            return new List<NumeralDB>();
        }
    }

    // Resolve database reference
    public string ResolveReference(DatabaseReference dbRef)
    {
        if (dbRef == null) return null;

        // If using ID
        if (dbRef.recordID > 0)
        {
            return GetValue(dbRef.tableName, dbRef.columnName, dbRef.recordID);
        }
        // If using WHERE clause
        else if (!string.IsNullOrEmpty(dbRef.whereColumn) && !string.IsNullOrEmpty(dbRef.whereValue))
        {
            return GetValueWhere(dbRef.tableName, dbRef.columnName, dbRef.whereColumn, dbRef.whereValue);
        }

        return null;
    }

    public bool IsReady => isInitialized;
}