using LTLRN.UI;
using NUnit.Framework.Internal;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class EX_BundleMenu : Panel
{
    private GameData gameData;
    private ExDataLoader dataLoader;
    private DBUtils dbUtils;

    [Header("Buttons")]
    public Button exitButton;

    [Header("UI")]
    public TMP_Text headerText;
    public Sprite[] sectionTypeIcons;

    private Section[] bundleSections;

    public GameObject sectionButonPrefab;
    public RectTransform sectionsRectTransform;

    private const string ARROW_RIGHT = "\u2192";

    private enum LangList
    {
        LT,
        EN,
        RU
    }

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        //buttons
        if(exitButton != null)
            exitButton.onClick.AddListener(OnExitClick);

        base.Initialize();
    }

    public override void Open()
    {
        base.Open();

        //get game data
        if (gameData == null)
            gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        //run animation
        ButtonImage exitBtn = exitButton.GetComponent<ButtonImage>();
        exitBtn.PlayAnimation(true, ButtonImage.ButtonAnimation.Scale.ToString());

        if (dataLoader == null)
            dataLoader = GameObject.FindWithTag("ExDataLoader").GetComponent<ExDataLoader>();

        dbUtils = GameObject.FindWithTag("DBUtils").GetComponent<DBUtils>();
    }   

    private void OnExitClick()
    {
        PanelManager.CloseAll();
        PanelManager.Open("exmain");
    }

    public void SectionLoader(Section[] section, string bundleName)
    {
        // Clear old panels (important when reloading)
        foreach (Transform child in sectionsRectTransform)
        {
            Destroy(child.gameObject);
        }

        bundleSections = section;
        headerText.text = bundleName;

        //button instances
        foreach (Section sec in bundleSections)
        {
            GameObject sectionBtnObj = Instantiate(sectionButonPrefab, sectionsRectTransform);
            sectionBtnObj.name = sec.name;

            //get section button component
            SectionButton sectionButton = sectionBtnObj.GetComponent<SectionButton>();
            sectionButton.sectionName = sec;

            //apply section type setup
            ApplySectionType(sec, sectionButton);

            //fill eith theme color
            Image fillImage = sectionButton.progressSlider.fillRect.GetComponent<Image>();
            fillImage.color = sec.sectionHeaderColor;
        }        
    }

    private void ApplySectionType(Section sec, SectionButton button)
    {
        switch (sec.sectionType)
        {
            case Section.SectionType.Text:
                SetupTextSection(sec, button);
                break;

            case Section.SectionType.Image:
                SetupImageSection(sec, button);
                break;

            case Section.SectionType.LearnType01:
                SetupLearnSection(sec, button);
                break;

            case Section.SectionType.Sound:
                SetupSoundSection(sec, button);
                break;

            case Section.SectionType.Input:
                SetupInputSection(sec, button);
                break;
            case Section.SectionType.Exam:
                SetupExamSection(sec, button);
                break;
        }
    }

    private void SetupTextSection(Section sec, SectionButton button)
    {
        Locale locale = GetLocale();        
        string btnText01 = locale.Identifier.Code.ToUpperInvariant();

        string btnText02 = string.Empty;
        btnText02 = LangList.LT.ToString();

        string test = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "TestTxt");

        //final text on button
        if (sec.sectionLanguage == Section.SectionLanguage.TARGET)
        {            
            button.sectionText.text = $"{test}\n{btnText02}{ARROW_RIGHT}{btnText01}";
        }
        else if(sec.sectionLanguage == Section.SectionLanguage.SYS)
        {         
            button.sectionText.text = $"{test}\n{btnText01}{ARROW_RIGHT}{btnText02}";
        }

        button.sectionIcon.sprite = sectionTypeIcons[4];
        //button.sectionIcon.gameObject.SetActive(false);

        //set difficulty text
        button.sectionDifficulty.text = sec.difficultyType.ToString();

        SetProgressSlider(sec, button);
    }

    private void SetupImageSection(Section sec, SectionButton button)
    {
        button.sectionIcon.sprite = sectionTypeIcons[1]; //image icon
        button.sectionIcon.gameObject.SetActive(true); //show icon
        //button.sectionText.gameObject.SetActive(false);

        string text = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "ImageTxt");
        string test = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "TestTxt");

        //set button text
        button.sectionText.text = $"{test}\n{text}";

        //set difficulty text
        button.sectionDifficulty.text = sec.difficultyType.ToString();

        SetProgressSlider(sec, button);
    }

    private void SetupSoundSection(Section sec, SectionButton button)
    {
        //button.sectionText.gameObject.SetActive(false);
        button.sectionIcon.gameObject.SetActive(true); //show icon
        button.sectionIcon.sprite = sectionTypeIcons[0]; //hear icon

        //set difficulty text
        button.sectionDifficulty.text = sec.difficultyType.ToString();

        string test = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "TestTxt");
        string text = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "ListenTxt");

        //set button text
        button.sectionText.text = $"{LangList.LT.ToString()}\n{text}";

        SetProgressSlider(sec, button);
    }

    private void SetupInputSection(Section sec, SectionButton button)
    {
        ///

        button.sectionDifficulty.text = sec.difficultyType.ToString();
    }

    private void SetupLearnSection(Section sec, SectionButton button)
    {
        button.sectionIcon.sprite = sectionTypeIcons[3]; //image icon
        button.sectionIcon.gameObject.SetActive(true); //show icon
        //button.sectionText.gameObject.SetActive(false);

        string text = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "LearnTxt");

        button.sectionText.text = text;

        button.sectionDifficulty.text = sec.difficultyType.ToString();

        bool complete = dbUtils.GetSectionComplete(sec.name);
        if (complete)
        {
            SetProgressSlider(sec, button, true);
        }
    }

    private void SetupExamSection(Section sec, SectionButton button)
    {
        button.sectionText.gameObject.SetActive(false);
        button.sectionIcon.gameObject.SetActive(true);
        button.sectionIcon.sprite = sectionTypeIcons[2]; //exam icon

        button.sectionDifficulty.text = sec.difficultyType.ToString();
    }

    private Locale GetLocale()
    {
        string savedLang = gameData.saveData.lang.ToLower();

        Locale locale = LocalizationSettings.AvailableLocales.Locales
            .FirstOrDefault(l => l.Identifier.Code == savedLang);

        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;

        return locale;
    }

    private void SetProgressSlider(Section sec, SectionButton button, bool complete=false)
    {
        int questionsCount = dataLoader.GetQuestionCount(sec);

        button.progressSlider.maxValue = questionsCount;

        int result = dbUtils.GetSectionResult(sec.name);

        //set slider value based on result or complete status
        if (complete)
            button.progressSlider.value = questionsCount;
        else
            button.progressSlider.value = result;
    }


    private void OnDestroy()
    {
        exitButton.onClick.RemoveListener(OnExitClick);
    }
}
