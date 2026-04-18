using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "Map", menuName = "Scriptable Objects/Map")]
public class Map : ScriptableObject
{
    public GameObject mapPrefab;
    public string mapID;

    [Header("Map Info")]
    public bool showMapInfo;

    [Header("Localization Map Info")]
    public LocalizedString mapInfo;

    [Header("Localization Description")]
    public LocalizedString description;

    [Header("Event")]
    public ADV_GameEvent onMapEvents;
}
