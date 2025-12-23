using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SectionManager", menuName = "Scriptable Objects/SectionManager")]
public class SectionManager : ScriptableObject
{
    [Header("Additional data")]
    public string themeName;
    public Sprite themeIcon;
    public string themeDescription;

    [Header("Theme Sections")]
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
