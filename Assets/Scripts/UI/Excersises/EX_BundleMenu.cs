using LTLRN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using System.Linq;

public class EX_BundleMenu : Panel
{
    private GameData gameData;

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

            SectionButton sectionButton = sectionBtnObj.GetComponent<SectionButton>();
            sectionButton.sectionName = sec;

            //set colors
            Image backColor = sectionBtnObj.GetComponent<Image>();
            backColor.color = sec.sectionHeaderColor;

            ApplySectionType(sec, sectionButton);
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

        if (sec.sectionLanguage == Section.SectionLanguage.TARGET)
        {            
            button.sectionText.text = $"{btnText02}{ARROW_RIGHT}{btnText01}";
        }
        else if(sec.sectionLanguage == Section.SectionLanguage.SYS)
        {         
            button.sectionText.text = $"{btnText01}{ARROW_RIGHT}{btnText02}";
        }

        button.sectionIcon.gameObject.SetActive(false);

        button.sectionDifficulty.text = sec.difficultyType.ToString();
    }

    private void SetupImageSection(Section sec, SectionButton button)
    {
        button.sectionIcon.sprite = sectionTypeIcons[1]; //image icon
        button.sectionIcon.gameObject.SetActive(true); //show icon
        button.sectionText.gameObject.SetActive(false);

        button.sectionDifficulty.text = sec.difficultyType.ToString();
    }

    private void SetupSoundSection(Section sec, SectionButton button)
    {
        button.sectionText.gameObject.SetActive(false);
        button.sectionIcon.gameObject.SetActive(true); //show icon
        button.sectionIcon.sprite = sectionTypeIcons[0]; //hear icon

        button.sectionDifficulty.text = sec.difficultyType.ToString();
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
        button.sectionText.gameObject.SetActive(false);

        button.sectionDifficulty.text = sec.difficultyType.ToString();
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


    private void OnDestroy()
    {
        exitButton.onClick.RemoveListener(OnExitClick);
    }
}
