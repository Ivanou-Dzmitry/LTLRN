using System.Collections.Generic;
using UnityEngine;

public class EX_QGen : MonoBehaviour
{
    private QuestionBase currentQuestion;


    public List<QuestionBase> QuestionGenerator(QuestionBase question)
    {
        Debug.Log(question.name);

        if (!question.isQuestionBundle)
            return null;

        int count = question.questionReferences.Length;
        List<QuestionBase> newList = new List<QuestionBase>(count);

        for (int i = 0; i < count; i++)
        {
            DatabaseReference dbRef = question.questionReferences[i];

            // Create a clone
            QuestionBase tempQ = Instantiate(question);

            tempQ.isQuestionBundle = false;
            tempQ.questionReference = dbRef;

            newList.Add(tempQ);
        }

        return newList;
    }

}
