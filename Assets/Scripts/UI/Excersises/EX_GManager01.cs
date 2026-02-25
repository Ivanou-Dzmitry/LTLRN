using UnityEngine;

public class EX_GManager01 : MonoBehaviour
{
    private SoundManager soundManager;
    private ExGameLogic exGameLogic;
    private GameData gameData;

    [Header("Palette")]
    [SerializeField] private UIColorPalette palette;


    private void Start()
    {
        //get game logic
        exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();
        soundManager = GameObject.FindWithTag("SoundManager").GetComponent<SoundManager>();
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
    }
}
