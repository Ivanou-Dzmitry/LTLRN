using UnityEngine;

//no database reference for this type of question

[CreateAssetMenu(fileName = "Question", menuName = "Scriptable Objects/QuestionT02")]
public class QuestionT02 : ScriptableObject
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
    public string questionText; // Text of the question

    [Header("Illustration")]
    public string qSpriteFile;

    [Header("TEXT")]

    [Header("TEXT: Answer")]
    public string[] answerVariantsText;

    [Header("IMAGE")]

    [Header("Image data type")]
    public bool isImageTextOnly = true; //text or data from DB

    [Header("IMAGE: Answer")]
    public string[] answerVarSpriteName;

    [Header("SOUND")]

    [Header("SOUND: Text")]
    public string[] qSoundClipName;

    [Header("Correct Answer")]
    public CorrectAnswer correctAnswerNumber = CorrectAnswer.Element0;

    [Header("Reward")]
    public int rewardAmount = 1;
}
