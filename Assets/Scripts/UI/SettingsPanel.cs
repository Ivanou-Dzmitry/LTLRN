using UnityEngine;
using LTLRN.UI;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : Panel
{
    private GameData gameData;
    private SoundManager soundManager => SoundManager.Instance;

    [Header("Data")]
    [SerializeField] private UIImagesGallery imagesGallery;

    [Header("Avatar UI")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    public TMP_Text playerNameText;

    [Header("Sound")]
    public Button soundButton;
    public Slider soundSlider;
    public Sprite[] soundButtonSprites;

    [Header("Music")]
    public Button musicButton;
    public Slider musicSlider;
    public Sprite[] musicButtonSprites;

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
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        LoadAvatar();
        LoadData();
        base.Open();
    }

    private void LoadData()
    {
        //player
        playerNameText.text = gameData.saveData.playerName;

        //sound and music
        soundSlider.value = gameData.saveData.soundVolume;
        
        bool soundToggle = gameData.saveData.soundToggle;

        if (soundToggle)
            soundButton.image.sprite = soundButtonSprites[0];
        else
            soundButton.image.sprite = soundButtonSprites[1];

        musicSlider.value = gameData.saveData.musicVolume;

        bool musicToggle = gameData.saveData.musicToggle;

        if (musicToggle)
            musicButton.image.sprite = musicButtonSprites[0];
        else
            musicButton.image.sprite = musicButtonSprites[1];
    }



    private void LoadAvatar()
    {        
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


    public void OnSoundSliderChanged()
    {
        gameData.saveData.soundVolume = soundSlider.value;
        soundManager.SetVolume("sound");

    }

    public void OnMuscSliderChange()
    {
        gameData.saveData.musicVolume = musicSlider.value;
        soundManager.SetVolume("music");
    }

    public void SoundButtonPress()
    {
        bool toggle = gameData.saveData.soundToggle;

        if (toggle)
        {
            soundButton.image.sprite = soundButtonSprites[0];
        }
        else
        {
            soundButton.image.sprite = soundButtonSprites[1];
        }
    }

    public void MusicButtonPress()
    {
        bool toggle = gameData.saveData.musicToggle;

        if (toggle)
        {
            musicButton.image.sprite = musicButtonSprites[0];
        }
        else
        {
            musicButton.image.sprite = musicButtonSprites[1];
        }
    }

}
