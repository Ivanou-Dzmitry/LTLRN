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

    //Description:
    //type1 - with next button
    //type2 - without next button. Result at the end.

    [Header("Type")]
    public SectionType sectionType = SectionType.Type1;

    [Header("UI")]
    public int sectionNumber;
    public Sprite sectionIcon;
    public string sectionTitle;
    public string sectionDescription;
    public Color sectionHeaderColor;
    public bool isLiked;



    public QuestionT01[] questions;
}
