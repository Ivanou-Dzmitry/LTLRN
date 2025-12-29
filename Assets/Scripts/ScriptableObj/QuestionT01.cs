using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "Scriptable Objects/QuestionT01")]
public class QuestionT01 : ScriptableObject
{
    public string uID;

    public enum QuestionLang
    {
        RU,
        BY,
        EN
    }

    [Header("lang")]
    public QuestionLang questionLang = QuestionLang.RU;

    public enum QuestionDifficulty
    {
        Simple,
        Medium,
        Hard
    }

    [Header("Difficulty")]
    public QuestionDifficulty questionDifficulty = QuestionDifficulty.Simple;

    public enum QuestionType
    {
        Type1,
        Type2,
        Type3
    }

    public enum CorrectAnswer
    {
        Element0 = 0,
        Element1 = 1,
        Element2 = 2,
        Element3 = 3
    }

    [Header("Type")]
    public QuestionType questionType = QuestionType.Type1;

    [Header("Description")]
    public string questionDescription;

    [Header("Question")]

    public bool isQuestionTextOnly = true; // Text or data from DB

    [Header("Question: text")]
    public string questionText; // Text of the question

    [Header("Question: Database References")]
    public DatabaseReference questionReference;  // References to DB records

    [Header("Illustration")]
    public string qSpriteFile;

    [Header("TEXT")]

    public bool isAnswerTextOnly = true; // Text or data from DB

    [Header("TEXT: Answer")]
    public string[] answerVariantsText;

    [Header("TEXT: Answer Database References")]
    public DatabaseReference[] answerReferences;  // References to DB records

    [Header("IMAGE")]

    [Header("IMAGE: Answer Database References")]
    public DatabaseReference[] answerImageReferences;

    [Header("SOUND")]

    [Header("SOUND: Answer Database References")]
    public DatabaseReference[] soundReferences;   // References to DB records

    [Header("Correct Answer")]
    public CorrectAnswer correctAnswerNumber = CorrectAnswer.Element0;

    [Header("Reward")]
    public int rewardAmount = 1;
}

[System.Serializable]
public class DatabaseReference
{
    public string tableName;      // e.g., "Numerals"
    public string columnName;     // e.g., "Word" or "Sound"    
    public int recordID;          // e.g., 1, 2, 3... read only
    public string value;            // e.g., "Five" or "five.mp3"
    public string whereColumn;    // e.g., "Digit"
    public string whereValue;     // e.g., "5"
}
