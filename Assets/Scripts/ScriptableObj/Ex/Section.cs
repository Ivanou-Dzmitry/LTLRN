using UnityEngine;
using UnityEngine.Localization;
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

    [Header("Localization")]
    public LocalizedString sectionTitle;
    public LocalizedString sectionDescription;

    [Header("Info text")]
    public DatabaseReference sectionInfo;

    [Header("Sections")]
    public Section[] bundleSections;
    public Section[] bundleTests;
    public Section bundleExam;

    [Header("Bundle Question")]
    public bool isContainBundleQuestion;

    [Header("Type01 and Type02")]
    public QuestionBase[] questions;

}
