using UnityEngine;

[CreateAssetMenu(fileName = "SectionManager", menuName = "Scriptable Objects/SectionManager")]
public class SectionManager : ScriptableObject
{
    public string themeName;
    public string themeDescriptions;
    public Section[] sections;
    public int questionPerSection = 10;


    public int GetTotalQuestionCount()
    {
        int totalQuestions = 0;

        foreach (var section in sections)
        {
            if (section != null && section.questions != null)
                totalQuestions += section.questions.Length;
        }
        return totalQuestions;
    }

}
