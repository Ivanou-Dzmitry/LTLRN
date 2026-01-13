using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "Scriptable Objects/QuestionT01")]
public class QuestionT01 : ScriptableObject
{
    //public string uID;

    [Header("Automatization")]
    public bool isAutomated = false;

    public enum QuestionLang
    {
        SYS,
        LT,        
        IMG,
        NUM
    }

    [Header("lang")]
    [Tooltip("Optional field. System.")]
    public QuestionLang questionLang = QuestionLang.SYS;

    public enum QuestionDifficulty
    {
        Simple,
        Medium,
        Hard
    }

    [Header("Difficulty")]
    [Tooltip("Optional field. For backlog")]
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
    [Tooltip("Type1 - question is text, Type2 - question is image, Type3 - with input field.")]
    public QuestionType questionType = QuestionType.Type1;

    [Header("Category")]
    [Tooltip("This is the category of the question. Example: Transport, Animals, etc. Important for sound and images!")]
    public DatabaseReference questionCategory;

    [Header("Description")]
    [Tooltip("Optional field. For backlog")]
    public string questionDescription;

    [Header("Question")]
    // Text or data from DB
    [Tooltip("If TRUE - text for question get from text field, if FALSE - from database")]
    public bool isQuestionTextOnly = true;

    [Header("Q: text")]
    [Tooltip("Question text - string")]
    [TextArea] public string questionText; // Text of the question

    [Header("Q: DB")]
    [Tooltip("Question text - reference to string from database")]
    public DatabaseReference questionReference;  // References to DB records

    [Header("Image: Count")]
    public int imagesCount = 1;

    [Header("Image: File")]
    [Tooltip("Question image - sprite")]
    public Sprite questionImage;

    [Header("Image: DB")]
    [Tooltip("Question text - reference to string file name from database")]
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
    [Tooltip("Set correct answer.")]
    public CorrectAnswer correctAnswerNumber = CorrectAnswer.Element0;

    [Header("Reward")]
    [Tooltip("Optional. For backlog")]
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
