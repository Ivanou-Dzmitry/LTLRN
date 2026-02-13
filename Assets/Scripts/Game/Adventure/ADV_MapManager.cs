using UnityEngine;

public class ADV_MapManager : MonoBehaviour
{
    private GameData gameData;
    
    public GameObject[] map;
   
    [Header("Player")]
    [SerializeField] private GameObject player;
    private Player playerClass;

    private int currentMapIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (player != null)
            playerClass = player.GetComponent<Player>();

        LoadMap();
    }

    private void LoadMap()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        currentMapIndex = -1;

        if (gameData != null)
            currentMapIndex = gameData.saveData.currentMapIndex;

        if(currentMapIndex >= 0)
        {
            GameObject currentMap = Instantiate(map[currentMapIndex], Vector3.zero, Quaternion.identity);
            currentMap.transform.parent = transform;
            currentMap.name = map[currentMapIndex].name;
        }      
    }

    private void OnApplicationQuit()
    {
        gameData.saveData.currentMapIndex = currentMapIndex;
        gameData.SaveToFile();
    }

}
