using LTLRN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;



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

    [Header("Feed")]
    public Button feedbackButton;

    [Header("DB")]
    public TMP_Text dbLog;
    private DBUtils dbUtils;

    //for user avatar
    public CarouselImages userAvatarCarousel;

    private const float NORMAL_SPEED = 1f;
    private const float SLOW_SPEED = 0.75f;

    public override void Initialize()
    {
        if (IsInitialized)
            return;

        soundButton.onClick.AddListener(SoundToggle);

        musicButton.onClick.AddListener(MusicToggle);

        if(feedbackButton != null)
            feedbackButton.onClick.AddListener(OpenFeedbackPage);

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
        dbUtils = GameObject.FindWithTag("DBUtils").GetComponent<DBUtils>();

        userAvatarCarousel.LoadAvatar();
        
        LoadData();

/*        if(dbUtils!=null && dbLog!=null)
            dbLog.text = "DB: " + dbUtils.CheckConnection();*/

        base.Open();
    }

    private void LoadData()
    {
        if(gameData == null)
            return;

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

        float vsliderval = gameData.saveData.soundSpeed;

        if (vsliderval == 1)
            soundSpeedSlider.value = 1;
        else
            soundSpeedSlider.value = 0;

        //sound speed
        //soundSpeedSlider.value = gameData.saveData.soundSpeed;

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
        if (gameData == null) return;        
       
        gameData.SaveToFile();
    }


    public void OnSoundSliderChanged()
    {
        if (gameData == null) return;

        gameData.saveData.soundVolume = soundSlider.value;
        soundManager.SetVolume("sound");

    }

    public void OnMuscSliderChange()
    {
        if (gameData == null) return;

        gameData.saveData.musicVolume = musicSlider.value;
        soundManager.SetVolume("music");
    }

    public void OnSpeedSliderChange()
    {
        if (gameData == null)
            return;

        gameData.saveData.soundSpeed = soundSpeedSlider.value;
        
        float speed = 0;

        if(soundSpeedSlider.value == 1)
        {
            speedBtnImg.sprite = imagesGallery.soundSpeedSprites[0];
            speed = NORMAL_SPEED;
        }
        else if(soundSpeedSlider.value == 0)
        {
            speedBtnImg.sprite = imagesGallery.soundSpeedSprites[1];
            speed = SLOW_SPEED;
        }

        gameData.saveData.soundSpeed = speed;
        gameData.SaveToFile();

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
    public void OpenFeedbackPage()
    {
        Application.OpenURL("https://forms.gle/ymN8Bg9UsPLd2tJX9");
    }


    private void OnDestroy()
    {
        //remove listeners
        soundButton.onClick.RemoveListener(SoundToggle);
        musicButton.onClick.RemoveListener(MusicToggle);        
        feedbackButton.onClick.RemoveListener(OpenFeedbackPage);
    }
}
