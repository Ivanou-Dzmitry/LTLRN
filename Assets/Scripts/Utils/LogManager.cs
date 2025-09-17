using System;
using System.IO;
using System.Text;
using UnityEngine;

public class LogManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private string gameLogFilePath;
    private string sysLogFilePath;

    private DateTime startTime;
    const string GAME_LOG_FILE_NAME = "ltlrn_log.csv";
    const string SYS_LOG_FILE_NAME = "ltlrn_syslog.csv";

    public string questionID;    

    void Start()
    {
        //log for game
        gameLogFilePath = Path.Combine(Application.persistentDataPath, GAME_LOG_FILE_NAME);
        
        //log for sys
        sysLogFilePath = Path.Combine(Application.persistentDataPath, SYS_LOG_FILE_NAME);

        startTime = DateTime.Now;

        questionID = "";

        if (!File.Exists(sysLogFilePath))
        {
            File.WriteAllText(sysLogFilePath, "Time, Message\n");
        }

        if (!File.Exists(gameLogFilePath))
        {
            File.WriteAllText(gameLogFilePath, "Duration(sec)\n");
            WriteSysLog("Game log file exist.");
        }
        else
        {
            WriteSysLog("Game log file NOT exist.");
        }
    }

    /// <summary>
    /// Write a line into the system log file.
    /// </summary>
    public void WriteSysLog(string message)
    {
        string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string line = $"{timeStamp},{message}";
        File.AppendAllText(sysLogFilePath, line + Environment.NewLine);
    }

    public void EndGameSession()
    {
        DateTime endTime = DateTime.Now;
        TimeSpan duration = endTime - startTime;
        int secondsOnly = duration.Seconds;

        //string to save
        string logEntry = $"{secondsOnly}, {questionID}";

        try
        {
            // Use FileStream with FileShare.ReadWrite to avoid file access conflicts
            using (FileStream fs = new FileStream(gameLogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
            {
                writer.WriteLine(logEntry);
            }

            string logText = $"Log file saved at: {gameLogFilePath}";

            Debug.Log(logText);
            WriteSysLog(logText);
        }
        catch (IOException ex)
        {
            string logText = $"Error writing to log file: {ex.Message}";
            Debug.LogError(logText);
            WriteSysLog(logText);
        }
    }

}
