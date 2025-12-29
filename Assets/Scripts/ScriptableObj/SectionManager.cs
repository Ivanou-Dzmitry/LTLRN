using UnityEngine;
using UnityEngine.UI;
using static Section;

[CreateAssetMenu(fileName = "SectionManager", menuName = "Scriptable Objects/SectionManager")]
public class SectionManager : ScriptableObject
{
    public enum ThemeDifficulty
    {
        Simple,
        Medium,
        Hard
    }

    [Header("Difficulty")]
    public ThemeDifficulty themeDifficulty = ThemeDifficulty.Simple;

    [Header("Additional data")]
    public Sprite themeIcon;
    public LocalizedText themeName;    
    public LocalizedText themeDescription;

    [Header("Theme Sections")]
    public Section[] sections;
    public int questionsCount = 10;


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
