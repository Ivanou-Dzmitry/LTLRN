using UnityEngine;

[CreateAssetMenu(fileName = "Map", menuName = "Scriptable Objects/Map")]
public class Map : ScriptableObject
{
    public GameObject mapPrefab;
    public string mapID;

    [Header("Map Info")]
    public bool showMapInfo;
    public string mapInfo;    
}
