using UnityEngine;
using LTLRN.UI;
using UnityEngine.UI;

public class SettingsPanel : Panel
{
    private GameData gameData;

    [Header("Data")]
    [SerializeField] private UIImagesGallery imagesGallery;

    [Header("Avatar UI")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    private int currentIndex;

    public override void Initialize()
    {
        if (IsInitialized)
            return;

        prevButton.onClick.AddListener(PreviousAvatar);
        nextButton.onClick.AddListener(NextAvatar);

        base.Initialize();
    }

    public override void Open()
    {
        LoadAvatar();
        base.Open();
    }

    private void LoadAvatar()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        if (gameData == null || imagesGallery == null || imagesGallery.userAvatar.Length == 0)
            return;

        currentIndex = Mathf.Clamp(
            gameData.saveData.playerIconIndex,
            0,
            imagesGallery.userAvatar.Length - 1
        );

        ApplyAvatar();
    }

    private void ApplyAvatar()
    {
        avatarImage.sprite = imagesGallery.userAvatar[currentIndex];
        gameData.saveData.playerIconIndex = currentIndex;
    }

    private void NextAvatar()
    {
        currentIndex++;
        if (currentIndex >= imagesGallery.userAvatar.Length)
            currentIndex = 0;

        ApplyAvatar();
    }

    private void PreviousAvatar()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = imagesGallery.userAvatar.Length - 1;

        ApplyAvatar();
    }


    public void SaveSettings()
    {        
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();
       
        gameData.SaveToFile();
    }

}
