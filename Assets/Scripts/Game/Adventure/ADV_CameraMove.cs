using UnityEngine;

public class ADV_CameraMove : MonoBehaviour
{
    private GameData gameData;

    public Transform target;
    public float moveSmooth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get classes
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
        
        if(gameData!=null)
            transform.position = new Vector3(gameData.saveData.playerPosition.x, gameData.saveData.playerPosition.y, transform.position.z);
    }

    // Update is called once per frame
    void LateUpdate()
    {        
        if (transform.position != target.position)
        {
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSmooth);
        }
    }
}
