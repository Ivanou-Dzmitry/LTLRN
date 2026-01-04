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

    private const int DB_VERSION = 1; // Increment this when you update the database
    private const string VERSION_KEY = "database_version";

    //loading IMAGES
    private const string IMAGES_ROOT = "Images";
    private readonly Dictionary<string, Dictionary<string, Sprite>> spriteCache
        = new Dictionary<string, Dictionary<string, Sprite>>();


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
        sourceDbPath = Path.Combine(Application.streamingAssetsPath, dbName);
        dbPath = Path.Combine(Application.persistentDataPath, dbName);
#elif UNITY_EDITOR
    sourceDbPath = Path.Combine(Application.dataPath, "StreamingAssets", dbName);
    dbPath = Path.Combine(Application.persistentDataPath, dbName);
#else
    sourceDbPath = Path.Combine(Application.streamingAssetsPath, dbName);
    dbPath = Path.Combine(Application.persistentDataPath, dbName);
#endif

        // Check if database needs update
        int savedVersion = PlayerPrefs.GetInt(VERSION_KEY, 0);
        bool needsUpdate = savedVersion < DB_VERSION || !File.Exists(dbPath);

        if (needsUpdate)
        {
            Debug.Log($"Updating database from version {savedVersion} to {DB_VERSION}");

            // Delete old database if exists
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
                Debug.Log("Old database deleted");
            }

#if UNITY_ANDROID
            UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(sourceDbPath);
            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(dbPath, www.downloadHandler.data);
                PlayerPrefs.SetInt(VERSION_KEY, DB_VERSION);
                PlayerPrefs.Save();
                Debug.Log("Database updated successfully");
            }
            else
            {
                Debug.LogError($"Failed to copy database: {www.error}");
                yield break;
            }
#else
        if (File.Exists(sourceDbPath))
        {
            File.Copy(sourceDbPath, dbPath, true); // true = overwrite
            PlayerPrefs.SetInt(VERSION_KEY, DB_VERSION);
            PlayerPrefs.Save();
            Debug.Log("Database updated successfully");
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
            Debug.Log("Database is up to date");
        }

        Debug.Log("Final Database Path: " + dbPath);
        string result = CheckConnection();
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
                        Time = 0
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

    //COMPLETE COUNT
    public int GetCompleteSectionsCount()
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
                int count = connection.Table<SectionDB>()
                    .Where(s => s.Complete == "true")
                    .Count();

                return count;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error counting complete sections: {ex.Message}");
            return 0;
        }
    }


    //PROGRESS
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

    public void SetSectionProgress(string sectionName, int value)
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
                    section.QDone = value;
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
            Debug.LogError($"Error updating progress: {ex.Message}");
        }
    }


    //RESULR
    public int GetSectionResult(string sectionName)
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
                    return section.QCorrect;
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

    public void SetSectionResult(string sectionName, int value)
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
                    section.QCorrect = value;
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
            Debug.LogError($"Error updating progress: {ex.Message}");
        }
    }


    //TIME
    public void SetSectionTime(string sectionName, float value)
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
                    section.Time = value;
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
            Debug.LogError($"Error updating progress: {ex.Message}");
        }
    }

    public float GetSectionTime(string sectionName)
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
                    return section.Time;
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

    //LIKES
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

    //COMPLETE
    public void SetSectionComplete(string sectionName, bool isComplete)
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
                    section.Complete = isComplete ? "true" : "false";
                    connection.Update(section);
                    //Debug.Log($"Updated {sectionName} - Complete: {section.Liked}");
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

    public bool GetSectionComplete(string sectionName)
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
                    return section.Complete == "true";
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

    public void ResetAllSections()
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
                string query = @"
                UPDATE Sections 
                SET QDone = 0, 
                    QCorrect = 0, 
                    Liked = 'false', 
                    Time = 0.0, 
                    Complete = 'false'
            ";

                int rowsAffected = connection.Execute(query);
                Debug.Log($"Reset {rowsAffected} sections");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error resetting sections: {ex.Message}");
        }
    }

    public Sprite LoadSpriteByName(string folder, string imageName)
    {
        if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(imageName))
            return null;

        imageName = Path.GetFileNameWithoutExtension(imageName);

        if (!spriteCache.ContainsKey(folder))
        {
            LoadSpriteFolder(folder);
        }

        if (spriteCache[folder].TryGetValue(imageName, out Sprite sprite))
        {
            return sprite;
        }

        Debug.LogWarning($"Sprite not found: {folder}/{imageName}");
        return null;
    }

    private void LoadSpriteFolder(string folder)
    {
        if (spriteCache.ContainsKey(folder))
            return;

        string path = $"{IMAGES_ROOT}/{folder}";
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);

        var dict = new Dictionary<string, Sprite>();

        foreach (var sprite in sprites)
        {
            if (!dict.ContainsKey(sprite.name))
                dict.Add(sprite.name, sprite);
        }

        spriteCache.Add(folder, dict);
    }

    public bool IsReady => isInitialized;
}