using UnityEngine;
using UnityEngine.Localization;
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

    [Header("UI")]
    public Sprite themeIcon;
    public Color themeHeaderColor;


    [Header("Additional data")]
    public LocalizedText themeName;    
    public LocalizedText themeDescription;

    [Header("Theme Sections")]
    public Section[] sections;
    
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

    public float GetThemeDifValue(ThemeDifficulty difficulty)
    {
        switch (difficulty)
        {
            case ThemeDifficulty.Simple:
                return 1f;

            case ThemeDifficulty.Medium:
                return 2f;

            case ThemeDifficulty.Hard:
                return 3f;

            default:
                return 1f;
        }
    }

    public string GeThemetDescription(SectionManager theme, Locale locale)
    {
        if (theme == null || theme.themeDescription == null)
            return string.Empty;

        //Locale locale = GetLocale();

        if (locale == null)
            return theme.themeDescription.en;

        switch (locale.Identifier.Code)
        {
            case "ru":
                return theme.themeDescription.ru;

            case "en":
            default:
                return theme.themeDescription.en;
        }
    }

    public string GetThemeName(SectionManager theme, Locale locale)
    {
        if (theme == null || theme.themeName == null)
            return string.Empty;

        //Locale locale = GetLocale();

        if (locale == null)
            return theme.themeName.en;

        switch (locale.Identifier.Code)
        {
            case "ru":
                return theme.themeName.ru;

            case "en":
            default:
                return theme.themeName.en;
        }
    }

}
