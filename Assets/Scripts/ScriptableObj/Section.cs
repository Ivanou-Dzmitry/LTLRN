using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static QuestionT01;

[CreateAssetMenu(fileName = "Section", menuName = "Scriptable Objects/Section")]
public class Section : ScriptableObject
{
    public enum SectionType
    {
        Type1,
        Type2,
        Type3
    }

    public enum DifficultyType
    {
        Simple,
        Medium,
        Hard
    }

    //Description:
    //type1 - with next button
    //type2 - without next button. Result at the end.

    [Header("Type")]
    public SectionType sectionType = SectionType.Type1;

    [Header("Difficulty")]
    public DifficultyType difficultyType = DifficultyType.Simple;

    [Header("UI")]
    public int sectionNumber;
    public Sprite sectionIcon;
    public Color sectionHeaderColor;
    public bool isLiked;

    [Header("Title")]
    public LocalizedText sectionTitle;

    [System.Serializable]
    public class LocalizedText
    {
        [TextArea] public string ru;
        [TextArea] public string en;
    }

    [Header("Description")]
    public LocalizedText sectionDescription;


    public QuestionT01[] questions;

    public float GetSectionDifValue(DifficultyType difficulty)
    {
        switch (difficulty)
        {
            case DifficultyType.Simple:
                return 1f;

            case DifficultyType.Medium:
                return 2f;

            case DifficultyType.Hard:
                return 3f;

            default:
                return 1f;
        }
    }
}
