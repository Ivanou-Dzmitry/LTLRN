using System;
using System.IO;
using System.Text;
using UnityEngine;

public class LogManager : MonoBehaviour
{
    public static LogManager log;

    private string gameLogFilePath;
    private string sysLogFilePath;

    private DateTime startTime;

    private const string GAME_LOG_FILE_NAME = "ltlrn_log.csv";
    private const string SYS_LOG_FILE_NAME  = "ltlrn_syslog.csv";

    public string QuestionID { get; set; } = "";

    private void Awake()
    {
        if (log == null)
        {
            DontDestroyOnLoad(gameObject);
            log = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Subscribe here so crash messages are captured even before Start().
        Application.logMessageReceived += OnUnityLogMessage;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= OnUnityLogMessage;
    }

    void Start()
    {
        gameLogFilePath = Path.Combine(Application.persistentDataPath, GAME_LOG_FILE_NAME);
        sysLogFilePath  = Path.Combine(Application.persistentDataPath, SYS_LOG_FILE_NAME);

        startTime  = DateTime.Now;
        QuestionID = "";

        EnsureFileWithHeader(sysLogFilePath,  "Time,Message");
        EnsureFileWithHeader(gameLogFilePath, "Duration(sec),QuestionID");
    }

    /// <summary>Writes a timestamped entry to the system log.</summary>
    public void WriteSysLog(string message)
    {
        try
        {
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string line = $"{timeStamp},{EscapeCsvField(message)}";
            File.AppendAllText(sysLogFilePath, line + Environment.NewLine);
        }
        catch (Exception ex)
        {
            // Last-resort: can't write to log, at least surface it in the console.
            Debug.LogError($"LogManager: Failed to write sys log — {ex.Message}");
        }
    }

    /// <summary>Records total session duration and current question ID to the game log.</summary>
    public void EndGameSession()
    {
        DateTime endTime  = DateTime.Now;
        TimeSpan duration = endTime - startTime;

        // TotalSeconds gives the full duration, not just the seconds component (0-59).
        int totalSeconds = (int)duration.TotalSeconds;

        string logEntry = $"{totalSeconds},{EscapeCsvField(QuestionID)}";

        try
        {
            using var fs     = new FileStream(gameLogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(fs, Encoding.UTF8);
            writer.WriteLine(logEntry);

            WriteSysLog($"Session ended. Duration: {totalSeconds}s, QuestionID: {QuestionID}");
        }
        catch (IOException ex)
        {
            string msg = $"Error writing game log: {ex.Message}";
            Debug.LogError(msg);
            WriteSysLog(msg);
        }
    }

    /// <summary>
    /// Captures Unity errors and exceptions (including uncaught ones) and writes them to the sys log.
    /// </summary>
    private void OnUnityLogMessage(string message, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            string entry = $"[{type}] {message}";
            if (!string.IsNullOrEmpty(stackTrace))
                entry += $" | {stackTrace.Replace(Environment.NewLine, " ")}";

            WriteSysLog(entry);
        }
    }

    private void EnsureFileWithHeader(string path, string header)
    {
        if (!File.Exists(path))
        {
            try
            {
                File.WriteAllText(path, header + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.LogError($"LogManager: Could not create log file '{path}' — {ex.Message}");
            }
        }
    }

    /// <summary>Wraps a CSV field in quotes if it contains a comma, quote, or newline.</summary>
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return field;

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            return $"\"{field.Replace("\"", "\"\"")}\"";

        return field;
    }
}
