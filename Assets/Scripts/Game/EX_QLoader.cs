using UnityEngine;

public class QuestionLoader : MonoBehaviour
{
    // Load all data from database references
/*    public static QuestionData LoadQuestionData(Question question)
    {
        QuestionData data = new QuestionData();

        // Load answer texts
        data.answerVariantsText = new string[question.answerReferences.Length];
        for (int i = 0; i < question.answerReferences.Length; i++)
        {
            data.answerVariantsText[i] = DBUtils.Instance.ResolveReference(question.answerReferences[i]);
        }

        // Load sound clips
        data.qSoundClipName = new string[question.soundReferences.Length];
        for (int i = 0; i < question.soundReferences.Length; i++)
        {
            data.qSoundClipName[i] = DBUtils.Instance.ResolveReference(question.soundReferences[i]);
        }

        // Copy other data
        data.questionText = question.questionText;
        data.qSpriteFile = question.qSpriteFile;
        data.correctAnswerNumber = (int)question.correctAnswerNumber;

        return data;
    }*/
}

[System.Serializable]
public class QuestionData1
{
/*    public string questionText;
    public string[] answerVariantsText;
    public string[] qSoundClipName;
    public string qSpriteFile;
    public int correctAnswerNumber;*/
}
