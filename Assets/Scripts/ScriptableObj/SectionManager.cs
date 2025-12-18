using UnityEngine;

[CreateAssetMenu(fileName = "SectionManager", menuName = "Scriptable Objects/SectionManager")]
public class SectionManager : ScriptableObject
{
    public Section[] sections;
    public int questionPerSection = 10;
}
