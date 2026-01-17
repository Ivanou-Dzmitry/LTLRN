using System;
using UnityEngine;
using static ExGameLogic;

//no database reference for this type of question

[CreateAssetMenu(fileName = "QuestionT02", menuName = "Scriptable Objects/QuestionT02")]
public class QuestionT02 : QuestionBase
{
    //text only from base
    public override void ApplyQuestionText(QuestionData data, TMPro.TMP_Text targetText)
    {        
        targetText.text = data.questionText;
    }

    public override void ApplyAnswers(QuestionData data, ExQManager01 qData)
    {
        qData.SetAnswers(data.answerFirstWord, data.answerSecondWord);
    }

    public override string[] GetAnswerColumns()
    {
        //Debug.Log($"{answerColumn}, {answerColumn.Length}");

        if (answerColumn == null || answerColumn.Length == 0)
            return Array.Empty<string>();

        var columns = new string[answerColumn.Length];

        for (int i = 0; i < answerColumn.Length; i++)
            columns[i] = answerColumn[i].columnName;

        return columns;
    }

    //where answers saved
    [Header("Answer Word Columns")]
    public DatabaseColumnReference[] answerColumn;    

    [Header("Description")]
    [Tooltip("Optional field. For backlog")]
    public string questionDescription;

    public enum CorrectAnswer
       {
           Element0 = 0,
           Element1 = 1,
           Element2 = 2,
           Element3 = 3
       }
}
