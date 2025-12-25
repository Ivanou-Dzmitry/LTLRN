using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "Scriptable Objects/Question")]
public class Question : ScriptableObject
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
    public string questionText;

    [Header("Illustration")]
    public Sprite questionSprite;

    [Header("Answer")]
    public string[] answerVariantsText;
    public Sprite[] answerVariantsSprite;
    public CorrectAnswer correctAnswerNumber = CorrectAnswer.Element0;

    [Header("Sound")]
    public string[] qSoundClipName;

    [Header("Reward")]
    public int rewardAmount;
}
