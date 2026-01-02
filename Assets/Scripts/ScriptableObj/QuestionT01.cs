using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "Scriptable Objects/QuestionT01")]
public class QuestionT01 : ScriptableObject
{
    public string uID;

    public enum QuestionLang
    {
        RU,
        LT,
        EN,
        Image,
        Number
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
        Type2, //image
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
    [Tooltip("Type1 - question is text, Type2 - question is image")]
    public QuestionType questionType = QuestionType.Type1;

    [Header("Category")]
    [Tooltip("This is the category of the question. Example: Transport, Animals, etc.")]
    public DatabaseReference questionCategory;

    [Header("Description")]
    public string questionDescription;

    [Header("Question")]

    // Text or data from DB
    public bool isQuestionTextOnly = true;

    [Header("Q: text")]
    [TextArea] public string questionText; // Text of the question

    [Header("Q: DB")]
    public DatabaseReference questionReference;  // References to DB records

    [Header("Image: File")]
    public Sprite questionImage;

    [Header("Image: DB")]
    public DatabaseReference[] questionImageFile;

    [Header("Image: Color")]
    public Color questionImageColor = Color.white;

    [Header("TEXT")]
    public bool isAnswerTextOnly = true; // Text or data from DB

    [Header("TEXT: Answer from text")]
    [TextArea] public string[] answerVariantsText; //text

    [Header("TEXT: Answer from DB")]
    public DatabaseReference[] answerReferences;  // References to DB records

    public DatabaseReference[] answerSecondWord;  // References to 2nd word

    [Header("IMAGE")]

    [Header("IMAGE: Answer Database References")]
    public DatabaseReference[] answerImageReferences;

    [Header("SOUND")]

    [Header("SOUND: from DB")]
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
