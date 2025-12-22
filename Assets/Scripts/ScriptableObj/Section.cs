using UnityEngine;

[CreateAssetMenu(fileName = "Section", menuName = "Scriptable Objects/Section")]
public class Section : ScriptableObject
{
    public int sectionNumber;
    public Sprite sectionIcon;
    public string sectionTitle;
    public string sectionDescription;
    public Question[] questions;
}
