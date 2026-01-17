using UnityEngine;
using static ExGameLogic;

[CreateAssetMenu(fileName = "QuestionT01", menuName = "Scriptable Objects/QuestionT01")]
public class QuestionT01 : QuestionBase
{
    public override void ApplyQuestionText(QuestionData data, TMPro.TMP_Text targetText)
    {
        if (isQuestionTextOnly)
        {
            targetText.text = questionText;
        }
        else
        {
            targetText.text = data.questionText;
        }
    }

    public override void ApplyAnswers(QuestionData data, ExQManager01 qData)
    {
        if (isAnswerTextOnly)
            qData.SetAnswers(data.answerFirstWord);        
        else
            qData.SetAnswers(data.answerFirstWord, data.answerSecondWord);

    }

    public enum CorrectAnswer
    {
        Element0 = 0,
        Element1 = 1,
        Element2 = 2,
        Element3 = 3
    }

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

    [Header("Image: File")]
    [Tooltip("Question image - sprite")]
    public Sprite questionImage;

    [Header("TEXT")]
    public bool isAnswerTextOnly = true; // Text or data from DB

    [Header("TEXT: Answer from text")]
    [TextArea] public string[] answerVariantsText; //text

    [Header("IMAGE")]

    [Header("IMAGE: Answer Database References")]
    public DatabaseReference[] answerImageReferences;

}


