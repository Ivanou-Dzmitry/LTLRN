using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "Map", menuName = "Scriptable Objects/Map")]
public class Map : ScriptableObject
{
    [Header("Map main")]
    public GameObject mapPrefab;
    public string mapID;

    [Header("Map Information")]
    public bool showMapInfo;

    [Header("Localization Map Info")]
    public LocalizedString mapInfo;

    [Header("Localization Map Description")]
    public LocalizedString description;

    [Header("Rooms on map")]
    public GameObject[] roomPrefabs;

    [Header("Localization Room Description")]
    public LocalizedString[] roomDescription;

    [Header("Event on map")]
    public ADV_GameEvent onMapEvents;
}
