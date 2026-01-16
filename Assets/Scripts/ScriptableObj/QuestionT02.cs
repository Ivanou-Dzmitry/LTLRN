using UnityEngine;
using static ExGameLogic;

//no database reference for this type of question

[CreateAssetMenu(fileName = "QuestionT02", menuName = "Scriptable Objects/QuestionT02")]
public class QuestionT02 : QuestionBase
{
    //public string uID;
/*    public enum QuestionLang
    {
        SYS,
        LT,
        IMG,
        NUM
    }

    [Header("lang")]
    [Tooltip("Optional field. System.")]
    public QuestionLang questionLang = QuestionLang.SYS;*/
    
/*    public enum QuestionDifficulty
    {
        Simple,
        Medium,
        Hard
    }

    [Header("Difficulty")]
    [Tooltip("Optional field. For backlog")]
    public QuestionDifficulty questionDifficulty = QuestionDifficulty.Simple;*/

    [Header("Description")]
    [Tooltip("Optional field. For backlog")]
    public string questionDescription;

    /*   public enum QuestionType
       {
           Type1,
           Type2,
           Type3
       }

       [Header("Type")]
       public QuestionType questionType = QuestionType.Type1;*/

    public enum CorrectAnswer
       {
           Element0 = 0,
           Element1 = 1,
           Element2 = 2,
           Element3 = 3
       }

    /*    [Header("Q: DB")]
        [Tooltip("Question text - reference to string from database")]
        public DatabaseReference questionReference;  // References to DB records*/

/*    [Header("Reward")]
    public int rewardAmount = 1;*/
}
