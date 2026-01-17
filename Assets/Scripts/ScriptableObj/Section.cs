using UnityEngine;
using static QuestionT01;
using static QuestionT02;

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
        A1,
        A2,
        B1,
        B2,
        C1,
        C2
    }

    //Description:
    //type1 - with next button
    //type2 - without next button. Result at the end.

    [Header("Type")]
    public SectionType sectionType = SectionType.Type1;

    [Header("Difficulty")]
    public DifficultyType difficultyType = DifficultyType.A1;

    [Header("UI")]
    public int sectionNumber;
    public Sprite sectionIcon;
    public Color sectionHeaderColor;

    [Header("Like")]
    [Tooltip("Auto. System.")]
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

    [Header("Info text")]
    public DatabaseReference sectionInfo;

    [Header("Type01 and Type02")]
    public QuestionBase[] questions;

    /*    public string GetSectionDifValue(DifficultyType difficulty)
        {
            switch (difficulty)
            {
                case DifficultyType.A1:
                    return "1";

                case DifficultyType.A2:
                    return "1";

                case DifficultyType.B1:
                    return "1";

                case DifficultyType.B2:
                    return "1";

                case DifficultyType.C1:
                    return "1";

                case DifficultyType.C2:
                    return "1";

                default:
                    return "1";
            }
        }*/
}
