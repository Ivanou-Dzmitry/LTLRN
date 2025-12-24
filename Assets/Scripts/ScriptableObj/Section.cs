using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Section", menuName = "Scriptable Objects/Section")]
public class Section : ScriptableObject
{
    [Header("UI")]
    public int sectionNumber;
    public Sprite sectionIcon;
    public string sectionTitle;
    public string sectionDescription;
    public Color sectionHeaderColor;
    public bool isLiked;

    public Question[] questions;
}
