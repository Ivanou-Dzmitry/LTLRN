using UnityEngine;
using static QuestionT01;
using static QuestionT02;

[CreateAssetMenu(fileName = "Section", menuName = "Scriptable Objects/Section")]
public class Section : ScriptableObject
{
    [Header("Bundle")]
    public bool isBundle;
    public enum SectionType
    {
        Text,
        Image,
        Sound,
        Input,
        Exam,
        Bundle,
        LearnType01
    }

    //sys - system language, target - target learning language
    public enum SectionLanguage
    {
        NONE,
        SYS,
        TARGET,
        BUNDLE
    }

    public enum DifficultyType
    {
        A0,
        A1,
        A2,
        B1,
        B2,
        C1,
        C2,
        Bundle
    }

    //Description:
    //type1 - with next button
    //type2 - without next button. Result at the end.

    [Header("Type")]
    public SectionType sectionType = SectionType.Text;

    [Header("Language")]
    public SectionLanguage sectionLanguage = SectionLanguage.NONE;

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



    public Section[] bundleSections;

    [Header("Type01 and Type02")]
    public QuestionBase[] questions;

}
