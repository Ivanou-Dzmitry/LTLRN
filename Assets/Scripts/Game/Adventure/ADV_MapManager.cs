using UnityEngine;

public class ADV_MapManager : MonoBehaviour
{
    public GameObject[] map;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadMap();
    }

    private void LoadMap()
    {
        GameObject currentMap = Instantiate(map[0], Vector3.zero, Quaternion.identity);
        currentMap.transform.parent = transform;
        currentMap.name = map[0].name;
    }

}
