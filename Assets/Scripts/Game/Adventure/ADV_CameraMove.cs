using UnityEngine;

public class ADV_CameraMove : MonoBehaviour
{
    private GameData gameData;
    private GameLogic gameLogic;

    public Transform target;
    public float moveSmooth;

    public Vector2 maxPosition;
    public Vector2 minPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //get classes
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
        gameLogic = GameObject.FindWithTag("ADVGameLogic").GetComponent<GameLogic>();

        if (gameData!=null)
            transform.position = new Vector3(gameData.saveData.playerPosition.x, gameData.saveData.playerPosition.y, transform.position.z);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //move cam only in play mode
        if (gameLogic.gameState == GameLogic.GameState.Pause)
            return;

        if (transform.position != target.position)
        {
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

            // Clamp the target position within the defined boundaries
            targetPosition.x = Mathf.Clamp(targetPosition.x, minPosition.x, maxPosition.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minPosition.y, maxPosition.y);

            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSmooth);
        }
    }
}
