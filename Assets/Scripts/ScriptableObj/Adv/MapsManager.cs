using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "MapsManager", menuName = "Scriptable Objects/MapsManager")]
public class MapsManager : ScriptableObject
{
    [Header("World Maps")]
    public Map[] maps;
    public GameObject worldMap;
}
