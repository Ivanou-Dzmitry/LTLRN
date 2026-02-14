using System;
using UnityEngine;
using static ExGameLogic;

public abstract class QuestionBase : ScriptableObject
{
    public virtual void ApplyQuestionText(
        QuestionData data,
        TMPro.TMP_Text targetText)
    {
    }

    public virtual void ApplyAnswers(QuestionData data, ExQManager01 qData)
    {
        // do nothing by default
    }

    public virtual string[] GetAnswerColumns()
    {
        // do nothing by default
        return Array.Empty<string>();
    }


    [Header("Automatization")]
    public bool isAutomated;

    [Header("Category")]
    public DatabaseReference questionCategory;


    /*sys - its system languages (RU, EN),
     * lt - lithuanian, 
     * img - image based question, 
     * num - numeral based question, 
     * sound - sound based question*/
    public enum QuestionLang
    {
        SYS,
        LT,
        IMG,
        NUM,
        SOUND
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
        Text,
        Image, //image
        Input,
        Sound,
        Learn
    }

    [Header("Type")]
    [Tooltip("Type1 - question is text, Type2 - question is image, Type3 - with input field.")]
    public QuestionType questionType = QuestionType.Text;

    [Header("Question")]
    public DatabaseReference questionReference;

    [Header("Answers")]
    public DatabaseReference[] answerReferences;
    public DatabaseReference[] answerSecondWord;

    [Header("Sound")]
    public DatabaseReference[] soundReferences;

    [Header("Image")]
    public DatabaseReference[] questionImageFile;

    [Header("Image: Count")]
    public int imagesCount = -1;

    [Header("Image: Color")]
    public Color questionImageColor = Color.white;

    [Header("Correct Answer")]
    public int correctAnswerNumber = -1;

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

[System.Serializable]
public class DatabaseColumnReference
{
    public int recordID;
    public string tableName;      // e.g., "Numerals"
    public string columnName;     // e.g., "Word" or "Sound"    
}
