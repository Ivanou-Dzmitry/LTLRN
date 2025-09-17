using UnityEngine;

[CreateAssetMenu(fileName = "Question", menuName = "Scriptable Objects/Question")]
public class Question : ScriptableObject
{

    public string uID;


    public enum QuestionLang
    {
        Rus,
        Bel,
        Eng
    }

    [Header("lang")]
    public QuestionLang questionLang = QuestionLang.Rus;

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

    [Header("Type")]
    public QuestionType questionType = QuestionType.Type1;

    [Header("Answer")]
    public string[] answerVariantsText;
    public Sprite[] answerVariantsSprite;
    public int correctAnswerNumber;

    [Header("Question")]
    public string questionText;
    public Sprite questionSprite;

    [Header("Reward")]
    public int rewardAmount;
}
