using SQLite;
using System;
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

    //main database
    private string dbPath;
    private bool isInitialized = false;
    private const string dbName = "ltlrn01.db"; 

    //game data
    private string dbDataPath;
    private bool isDataInitialized = false;
    private const string dbDataName = "keliasdata.db"; 

    private const int DB_VERSION = 1; // Increment this when you update the database
    private const string VERSION_KEY = "database_version";

    //loading IMAGES
    private const string IMAGES_ROOT = "Images";
    private readonly Dictionary<string, Dictionary<string, Sprite>> spriteCache
        = new Dictionary<string, Dictionary<string, Sprite>>();

    private int correctAnswerIndex = -1;

    //for sound
    private enum SoundColumn
    {
        Sound,
        Sound01,
        Sound02
    }

    private enum ImageColumn
    {
        Image,
        Image01,
        Image02
    }

    //for words
    private enum WordColumn
    {
        NomSing,
        NomPlur
    }

    public struct QuestionParams
    {
        public string TableName;
        public string ColumnName;
        public int RecordID;
    }

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

    //check game data table
    private bool CreateGameDataTables()
    {
        if (!isDataInitialized)
        {
            Debug.LogWarning("CreateGameDataTables skipped: data not initialized");
            return false;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath))
            {
                // Create tables if it doesn't exist
                connection.CreateTable<SectionDB>();
                connection.CreateTable<ThemesDB>();
                connection.CreateTable<QuestionsDataDB>();
            }

            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error creating game data tables: {ex.Message}");
            return false;
        }
    }


    private IEnumerator InitializeDatabase()
    {
        // Initialize main database
        yield return InitializeSingleDatabase(dbName, DB_VERSION, VERSION_KEY,
            (path) => dbPath = path);

        // Initialize data database
        yield return InitializeSingleDatabase(dbDataName, DB_VERSION, VERSION_KEY,
            (path) => dbDataPath = path);

        // Check connections
        Debug.Log("Final Main Database Path: " + dbPath);
        isInitialized = CheckConnection(dbPath);
        Debug.Log("DB1 Connection Result: " + isInitialized);

        Debug.Log("Final Data Database Path: " + dbDataPath);
        isDataInitialized = CheckConnection(dbDataPath);
        Debug.Log("DB2 Connection Result: " + isDataInitialized);

        // Create tables
        bool res = CreateGameDataTables();
        Debug.Log("DB2 create tables: " + res);
    }


    private IEnumerator InitializeSingleDatabase(string databaseName, int targetVersion,
    string versionKey, System.Action<string> setPath)
    {
        string sourceDbPath = GetSourcePath(databaseName);
        string destDbPath = GetDestinationPath(databaseName);

        // Set the path using callback
        setPath(destDbPath);

        // Check if database needs update
        int savedVersion = PlayerPrefs.GetInt(versionKey, 0);
        bool needsUpdate = savedVersion < targetVersion || !File.Exists(destDbPath);

        if (needsUpdate)
        {
            Debug.Log($"Updating {databaseName} from version {savedVersion} to {targetVersion}");

            // Delete old database if exists
            if (File.Exists(destDbPath))
            {
                File.Delete(destDbPath);
                Debug.Log($"Old {databaseName} deleted");
            }

            // Copy database
            yield return CopyDatabase(sourceDbPath, destDbPath, databaseName, versionKey, targetVersion);
        }
        else
        {
            Debug.Log($"{databaseName} is up to date (version {savedVersion})");
            yield return null;
        }
    }

    private string GetDestinationPath(string databaseName)
    {
        return Path.Combine(Application.persistentDataPath, databaseName);
    }

    private string GetSourcePath(string databaseName)
    {
#if UNITY_ANDROID
        return Path.Combine(Application.streamingAssetsPath, databaseName);
#elif UNITY_EDITOR
        return Path.Combine(Application.dataPath, "StreamingAssets", databaseName);
#else
        return Path.Combine(Application.streamingAssetsPath, databaseName);
#endif
    }

    private IEnumerator CopyDatabase(string sourceDbPath, string destDbPath,
    string databaseName, string versionKey, int targetVersion)
    {
#if UNITY_ANDROID
        yield return CopyDatabaseAndroid(sourceDbPath, destDbPath, databaseName, versionKey, targetVersion);
#else
        yield return CopyDatabaseStandalone(sourceDbPath, destDbPath, databaseName, versionKey, targetVersion);
#endif
    }

#if UNITY_ANDROID
    private IEnumerator CopyDatabaseAndroid(string sourceDbPath, string destDbPath,
        string databaseName, string versionKey, int targetVersion)
    {
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(sourceDbPath);
        yield return www.SendWebRequest();

        if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(destDbPath, www.downloadHandler.data);
            PlayerPrefs.SetInt(versionKey, targetVersion);
            PlayerPrefs.Save();
            Debug.Log($"{databaseName} updated successfully");
        }
        else
        {
            Debug.LogError($"Failed to copy {databaseName}: {www.error}");
        }
    }
#else
    private IEnumerator CopyDatabaseStandalone(string sourceDbPath, string destDbPath, 
        string databaseName, string versionKey, int targetVersion)
    {
        if (File.Exists(sourceDbPath))
        {
            File.Copy(sourceDbPath, destDbPath, true);
            PlayerPrefs.SetInt(versionKey, targetVersion);
            PlayerPrefs.Save();
            Debug.Log($"{databaseName} updated successfully");
        }
        else
        {
            Debug.LogError($"Source database not found: {sourceDbPath}");
        }
        
        yield return null;
    }
#endif


    public bool CheckConnection(string path)
    {
        if (!File.Exists(path))
        {
            string error = $"Database file not found: {path}";
            Debug.LogError(error);
            return false;
        }

        //check size
        FileInfo fileInfo = new FileInfo(path);
        Debug.Log($"Database file size: {fileInfo.Length} bytes");

        try
        {
            using (var connection = new SQLiteConnection(path))
            {
                var result = connection.ExecuteScalar<int>("SELECT 1");
                Debug.Log($"Connection with: {path} OK");
                return true;
            }
        }
        catch (System.Exception ex)
        {
            string error = $"SQLite connection failed: {ex.Message}";
            Debug.LogError($"{error}, Path: {path}");
            return false;
        }
    }

    //section DATA table
    public void EnsureSectionExists(string sectionName)
    {
        if (!isDataInitialized)
        {
            Debug.LogError("Database not initialized! Wait for initialization to complete.");
            return;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath)) //data table path
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

    //theme DATA Table
    public void EnsureThemeExists(string themeName)
    {
        if (!isDataInitialized)
        {
            Debug.LogError("Database not initialized! Wait for initialization to complete.");
            return;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath))
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

    //COMPLETE COUNT - DATA table
    public int GetCompleteSectionsCount()
    {
        if (!isDataInitialized)
        {
            Debug.LogError("Database not initialized!");
            return 0;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath))
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


    //PROGRESS - Data table
    public int GetSectionProgress(string sectionName)
    {
        if (!isDataInitialized)
        {
            Debug.LogError("Database not initialized!");
            return 0;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath))
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

    //set progress - data table
    public void SetSectionProgress(string sectionName, int value)
    {
        if (!isDataInitialized)
        {
            Debug.LogError("Database not initialized!");
            return;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath))
            {
                var section = connection.Table<SectionDB>()
                    .Where(s => s.Name == sectionName)
                    .FirstOrDefault();

                if (section != null)
                {
                    section.QDone = value;
                    connection.Update(section);
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


    //RESULT - Data table
    public int GetSectionResult(string sectionName)
    {
        if (!isDataInitialized)
        {
            Debug.LogError("Database not initialized!");
            return 0;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath))
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

    //set progress - data table
    public void SetSectionResult(string sectionName, int value)
    {
        if (!isDataInitialized)
        {
            Debug.LogError("DATA: Database not initialized!");
            return;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath))
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
                    Debug.LogWarning($"DATA: Section not found: {sectionName}");
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
        if (!isDataInitialized)
        {
            Debug.LogError("DATA: Database not initialized!");
            return;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath))
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

    //get time - data table
    public float GetSectionTime(string sectionName)
    {
        if (!isDataInitialized)
        {
            Debug.LogError("Database not initialized!");
            return 0;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath))
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

    //LIKES - Data table
    public void UpdateSectionLiked(string sectionName, bool isLiked)
    {
        if (!isDataInitialized)
        {
            Debug.LogError("Database not initialized!");
            return;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath))
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

    //get likes - data table
    public bool GetSectionLikedStatus(string sectionName)
    {
        if (!isDataInitialized)
        {
            Debug.LogError("Database not initialized!");
            return false;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath))
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

    //COMPLETE - data table
    public void SetSectionComplete(string sectionName, bool isComplete)
    {
        if (!isDataInitialized)
        {
            Debug.LogError("Database not initialized!");
            return;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath))
            {
                var section = connection.Table<SectionDB>()
                    .Where(s => s.Name == sectionName)
                    .FirstOrDefault();

                if (section != null)
                {
                    section.Complete = isComplete ? "true" : "false";
                    connection.Update(section);                    
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

    //get complete status - data table
    public bool GetSectionComplete(string sectionName)
    {
        if (!isDataInitialized)
        {
            Debug.LogError("Database not initialized!");
            return false;
        }

        try
        {
            using (var connection = new SQLiteConnection(dbDataPath))
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
            var result = $"DB error: {columnName}:{recordID}";
            return result;
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

    public string ResolveLangReference(DatabaseReference dbRef, string lang)
    {
        if (dbRef == null) return null;

        // If using ID
        if (dbRef.recordID > 0)
        {
            return GetValue(dbRef.tableName, lang, dbRef.recordID);
        }

        return null;
    }

    public QuestionParams? GetQuestionParams(DatabaseReference dbRef)
    {
        if (dbRef == null || dbRef.recordID <= 0)
            return null;

        return new QuestionParams
        {
            TableName = dbRef.tableName,
            ColumnName = dbRef.columnName,
            RecordID = dbRef.recordID
        };
    }

    class SingleValueRow
    {
        public string Value { get; set; }
    }

    public string[] AutoResolveReference(
        string tableName,
        string columnName,
        int recordID,
        string qLang,
        string sysLang,
        bool firstWord,
        int totalCount = 4
        )
    {
        if (!isInitialized)
            return Array.Empty<string>();

        //importnant! get correct language column. If question lt - then sys lang
        if (qLang == "LT")
        {
            if(ColumnExists(tableName, sysLang))
                columnName = sysLang;
        }                           

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                var results = new List<string>();

                // correct answer
                string correct = connection.ExecuteScalar<string>(
                    $"SELECT {columnName} FROM {tableName} WHERE ID = ?", recordID);

                if (!string.IsNullOrWhiteSpace(correct))
                    results.Add(correct);

                int remaining = totalCount - results.Count;
                if (remaining <= 0)
                    return results.ToArray();

                // random answers
                var randomQuery = $@"
                SELECT {columnName} AS Value
                FROM {tableName}
                WHERE ID != ?
                  AND {columnName} IS NOT NULL
                  AND {columnName} != ''
                ORDER BY RANDOM()
                LIMIT ?";

                var randomRows = connection.Query<SingleValueRow>(
                    randomQuery, recordID, remaining);

                foreach (var row in randomRows)
                {
                    results.Add(row.Value);
                }

                //random answers
                ShuffleList(results);

                // find correct index AFTER shuffle only for first word, because second connected to first
                if (firstWord)
                {
                    correctAnswerIndex = -1;
                    correctAnswerIndex = results.IndexOf(correct);
                    Debug.Log($"correctIndex = {correctAnswerIndex}");
                }                                   

                return results.ToArray();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"AutoResolveReference failed: {ex.Message}");
            return Array.Empty<string>();
        }
    }

    public int GetCorrectIndex()
    {
        return correctAnswerIndex;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private class TableColumnInfo
    {
        public int cid { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int notnull { get; set; }
        public string dflt_value { get; set; }
        public int pk { get; set; }
    }


    private bool ColumnExists(string tableName, string columnName)
    {
        using (var connection = new SQLiteConnection(dbPath))
        {
            // get table schema
            var query = $"PRAGMA table_info([{tableName}]);";
            var columns = connection.Query<TableColumnInfo>(query);
            return columns.Any(c => c.name == columnName);
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

    //get sound based on column name
    public string GetSound(string tableName, string nomSingValue, string columnName = null)
    {
        if (!isInitialized)
        {
            Debug.LogError("Database not initialized!");
            return null;
        }

        //return if no value
        if (string.IsNullOrEmpty(nomSingValue))
            return null;

        //for custom columns
        string customColumnName = string.Empty;

        //custom or default
        if (columnName != null && columnName != WordColumn.NomSing.ToString())
            customColumnName = columnName;
        else
            customColumnName = WordColumn.NomSing.ToString();

        try
            {
                using (var connection = new SQLiteConnection(dbPath))
                {
                    //request sound by nomSing
                    string query = $"SELECT {SoundColumn.Sound} FROM [{tableName}] WHERE {customColumnName} = ?";

                    var result = connection.ExecuteScalar<string>(query, nomSingValue);
                    return result;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error getting sound: {ex.Message}");
                return null;
            }
    }


    // Get Image by NomSing value
    public string GetImage(string tableName, string nomSingValue, string columnName = null)
    {
        if (!isInitialized)
        {
            Debug.LogError("Database not initialized!");
            return null;
        }

        if (string.IsNullOrEmpty(nomSingValue))
            return null;


        //for custom columns
        string customColumnName = string.Empty;

        //custom or default
        if (columnName != null && columnName != WordColumn.NomSing.ToString())
            customColumnName = columnName;
        else
            customColumnName = WordColumn.NomSing.ToString();

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                string query = $"SELECT {ImageColumn.Image} FROM [{tableName}] WHERE {customColumnName} = ?";
                var result = connection.ExecuteScalar<string>(query, nomSingValue);
                return result;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error getting image: {ex.Message}");
            return null;
        }
    }

    public int GetImagesCount(string tableName, string value, string columnName = null)
    {
        if (!isInitialized)
        {
            Debug.LogError("Database not initialized!");
            return -1;
        }

        if (string.IsNullOrEmpty(value))
            return -1;


        //for custom columns
        string customColumnName = string.Empty;

        //custom or default
        if (columnName != null && columnName != WordColumn.NomSing.ToString())
            customColumnName = columnName;
        else
            customColumnName = WordColumn.NomSing.ToString();

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                string query = $"SELECT Number FROM [{tableName}] WHERE {customColumnName} = ?";
                var result = connection.ExecuteScalar<int>(query, value);
                return result;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error getting image: {ex.Message}");
            return -1;
        }
    }

    public string[] GetSecondWord(
        string tableName,
        string[] firstWords,
        string firstColumnName,
        string secondColumnName
    )
    {
        if (!isInitialized)
        {
            Debug.LogError("Database not initialized!");
            return Array.Empty<string>();
        }

        if (firstWords == null || firstWords.Length == 0)
            return Array.Empty<string>();

        try
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                string placeholders = string.Join(
                    ",",
                    Enumerable.Repeat("?", firstWords.Length)
                );

                string query = $@"
                SELECT [{firstColumnName}] AS FirstValue,
                       [{secondColumnName}] AS SecondValue
                FROM [{tableName}]
                WHERE [{firstColumnName}] IN ({placeholders})
            ";

                var rows = connection.Query<RowPair>(query, firstWords);

                // map first to second
                var map = rows.ToDictionary(r => r.FirstValue, r => r.SecondValue);

                // preserve order
                string[] result = new string[firstWords.Length];
                for (int i = 0; i < firstWords.Length; i++)
                {
                    map.TryGetValue(firstWords[i], out result[i]);
                }

                return result;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"GetSecondWord failed: {ex.Message}");
            return Array.Empty<string>();
        }
    }


    private class RowPair
    {
        public string FirstValue { get; set; }
        public string SecondValue { get; set; }
    }



    public bool IsReady => isInitialized && isDataInitialized;
}