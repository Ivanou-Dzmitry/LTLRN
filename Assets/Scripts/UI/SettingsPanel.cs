using UnityEngine;
using LTLRN.UI;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : Panel
{
    private GameData gameData;
    private SoundManager soundManager => SoundManager.soundManager;

    [Header("Data")]
    [SerializeField] private UIImagesGallery imagesGallery;

    public TMP_Text playerNameText;

    [Header("Sound")]
    public Button soundButton;
    public Slider soundSlider;
    private Image soundBtnImg;

    [Header("Sound Speed")]
    public Button soundSpeedButton;
    public Slider soundSpeedSlider;
    private Image speedBtnImg;

    [Header("Music")]
    public Button musicButton;
    public Slider musicSlider;
    private Image musicBtnImg;

    //for user avatar
    public CarouselImages userAvatarCarousel;

    public override void Initialize()
    {
        if (IsInitialized)
            return;

        soundButton.onClick.AddListener(SoundToggle);

        musicButton.onClick.AddListener(MusicToggle);

        base.Initialize();
    }

    private void SoundToggle()
    {
        soundManager.SoundToggle();
        SoundButtonPress();
    }

    private void MusicToggle()
    {
        soundManager.MusicToggle();
        MusicButtonPress();
    }

    public override void Open()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        userAvatarCarousel.LoadAvatar();
        LoadData();
        base.Open();
    }

    private void LoadData()
    {
        soundBtnImg = soundButton.GetComponent<ButtonImage>().buttonIcon;
        musicBtnImg = musicButton.GetComponent<ButtonImage>().buttonIcon;
        speedBtnImg = soundSpeedButton.GetComponent<ButtonImage>().buttonIcon;

        //player
        playerNameText.text = gameData.saveData.playerName;

        //sound and music
        soundSlider.value = gameData.saveData.soundVolume;
        
        bool soundToggle = gameData.saveData.soundToggle;        

        if (soundToggle)
            soundBtnImg.sprite = imagesGallery.soundSprites[0];
        else
            soundBtnImg.sprite = imagesGallery.soundSprites[1];

        musicSlider.value = gameData.saveData.musicVolume;

        //music toggle
        bool musicToggle = gameData.saveData.musicToggle;

        if (musicToggle)
            musicBtnImg.sprite = imagesGallery.musicSprites[0];
        else
            musicBtnImg.sprite = imagesGallery.musicSprites[1];

        //sound speed
        soundSpeedSlider.value = gameData.saveData.soundSpeed;

        if (soundSpeedSlider.value == 1)
        {
            speedBtnImg.sprite = imagesGallery.soundSpeedSprites[0];
        }
        else if (soundSpeedSlider.value == 0)
        {
            speedBtnImg.sprite = imagesGallery.soundSpeedSprites[1];
        }
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

    public void OnSpeedSliderChange()
    {
        gameData.saveData.soundSpeed = soundSpeedSlider.value;
        
        float speed = 0;

        if(soundSpeedSlider.value == 1)
        {
            speedBtnImg.sprite = imagesGallery.soundSpeedSprites[0];
            speed = 1f;
        }
        else if(soundSpeedSlider.value == 0)
        {
            speedBtnImg.sprite = imagesGallery.soundSpeedSprites[1];
            speed = 0.5f;
        }

        soundManager.SetSoundSpeed(speed);
    }

    public void SoundButtonPress()
    {
        bool toggle = gameData.saveData.soundToggle;

        if (toggle)
        {
            soundBtnImg.sprite = imagesGallery.soundSprites[0];
        }
        else
        {
            soundBtnImg.sprite = imagesGallery.soundSprites[1];
        }
    }

    public void MusicButtonPress()
    {
        bool toggle = gameData.saveData.musicToggle;

        if (toggle)
        {
            musicBtnImg.sprite = imagesGallery.musicSprites[0];
        }
        else
        {
            musicBtnImg.sprite = imagesGallery.musicSprites[1];
        }
    }

}
