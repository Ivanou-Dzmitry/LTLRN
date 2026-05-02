using LTLRN.UI;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class EX_TestSelectorPnl : Panel
{
    private GameData gameData;
    //private ExDataLoader dataLoader;
    private DBUtils dbUtils;
    private ExGameLogic exGameLogic;

    private Section[] bundleSections;

    [Header("Buttons")]
    [SerializeField] private Button closePanelButton;
    [SerializeField] private Animator _animator;

    [Header("UI")]
    [SerializeField] private RectTransform testsRectTransform;
    [SerializeField] private GameObject testButonPrefab;
    public Sprite[] sectionTypeIcons;

    private const string ARROW_RIGHT = "\u2192";
    private enum LangList
    {
        LT,
        EN,
        RU
    }

    public override void Initialize()
    {
        closePanelButton.onClick.AddListener(ClosePanel);

        base.Initialize();
    }

    public override void Open()
    {
        base.Open();

        //get game data
        if (gameData == null)
            gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        if (exGameLogic == null)
            exGameLogic = GameObject.FindWithTag("ExGameLogic").GetComponent<ExGameLogic>();

        if (dbUtils == null)
            dbUtils = GameObject.FindWithTag("DBUtils").GetComponent<DBUtils>();

/*        if (dataLoader == null)
            dataLoader = GameObject.FindWithTag("ExDataLoader").GetComponent<ExDataLoader>();*/

       // SectionLoader(exGameLogic.currentSection.bundleSections);
    }

    public void SectionLoader(Section[] section) //, string bundleName, string bndlSecName)
    {
        // Clear old panels (important when reloading)
        foreach (Transform child in testsRectTransform)
        {
            Destroy(child.gameObject);
        }

        //set current section - root
        //bundleSectionName = bndlSecName;
        bundleSections = section;

        Debug.Log(section.Length);

        //get levels from db
        //string selLevels = dbUtils.GetSectionLevels(bndlSecName);

        // Normalize levels string
        //string normValueSelLevels = NormalizeLevels(selLevels);

        //UpdateAvailableLevels();

        //set values of selected levels based on db value
        //ApplyLevelsFromDB(normValueSelLevels);

        //ValidateSelectedLevels();

        //set selected levels of difficulty buttons
        //UpdateLevelButtonsUI();

        //button instances
        foreach (Section sec in bundleSections)
        {
            //string difficulty = sec.difficultyType.ToString();

            if (sec.sectionType == Section.SectionType.LearnType01)
                continue;

            GameObject sectionBtnObj = Instantiate(testButonPrefab, testsRectTransform);
            sectionBtnObj.name = sec.name;

            //get section button component
            SectionButton sectionButton = sectionBtnObj.GetComponent<SectionButton>();
            sectionButton.sectionName = sec;

            //apply section type setup skip Learn
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

            case Section.SectionType.LearnType01:
                break;

            case Section.SectionType.Image:
                SetupImageSection(sec, button);
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

        //string test = LocalizationSettings.StringDatabase.GetLocalizedString("KELIAS_UI", "TestTxt");

        //final text on button
        if (sec.sectionLanguage == Section.SectionLanguage.TARGET)
        {
            button.sectionText.text = $"{btnText02}{ARROW_RIGHT}{btnText01}";
        }
        else if (sec.sectionLanguage == Section.SectionLanguage.SYS)
        {
            button.sectionText.text = $"{btnText01}{ARROW_RIGHT}{btnText02}";
        }

        button.sectionIcon.sprite = sectionTypeIcons[4];
        //button.sectionIcon.gameObject.SetActive(false);

        SetProgressSlider(sec, button);
    }

    private void SetupImageSection(Section sec, SectionButton button)
    {
        button.sectionIcon.sprite = sectionTypeIcons[1]; //image icon
        button.sectionIcon.gameObject.SetActive(true); //show icon
        //button.sectionText.gameObject.SetActive(false);

        string text = LocalizationSettings.StringDatabase.GetLocalizedString("KELIAS_UI", "ImageTxt");
        string test = LocalizationSettings.StringDatabase.GetLocalizedString("KELIAS_UI", "TestTxt");

        //set button text
        button.sectionText.text = $"{test}\n{text}";

        //set difficulty text
        //button.sectionDifficulty.text = sec.difficultyType.ToString();

        SetProgressSlider(sec, button);
    }

    private void SetupSoundSection(Section sec, SectionButton button)
    {
        //button.sectionText.gameObject.SetActive(false);
        button.sectionIcon.gameObject.SetActive(true); //show icon
        button.sectionIcon.sprite = sectionTypeIcons[0]; //hear icon

        //set difficulty text
        //button.sectionDifficulty.text = sec.difficultyType.ToString();

        //string test = LocalizationSettings.StringDatabase.GetLocalizedString("KELIAS_UI", "TestTxt");
        string text = LocalizationSettings.StringDatabase.GetLocalizedString("KELIAS_UI", "ListenTxt");

        //set button text
        button.sectionText.text = $"{text}"; //{LangList.LT.ToString()}\n

        SetProgressSlider(sec, button);
    }

    private void SetupExamSection(Section sec, SectionButton button)
    {
        button.sectionText.gameObject.SetActive(true);
        button.sectionIcon.gameObject.SetActive(true);
        button.sectionIcon.sprite = sectionTypeIcons[2]; //exam icon

        string text = LocalizationSettings.StringDatabase.GetLocalizedString("KELIAS_UI", "ExamTxt");

        button.sectionText.text = $"{text}"; // {sec.difficultyType.ToString()}";
    }

    private void SetupInputSection(Section sec, SectionButton button)
    {
        ///

        
    }

    private void SetProgressSlider(Section sec, SectionButton button, bool complete = false)
    {
        int questionsCount = 10;//dataLoader.GetQuestionCount(sec);

        button.progressSlider.maxValue = questionsCount;

        int result = dbUtils.GetSectionResult(sec.name);

        //set slider value based on result or complete status
        if (complete)
        {
            //button.progressSlider.value = questionsCount;
            button.progressSlider.GetComponent<EX_SliderAnimator>().AnimateTo(questionsCount, 0.5f);
        }

        else
        {
            //button.progressSlider.value = result;
            button.progressSlider.GetComponent<EX_SliderAnimator>().AnimateTo(result, 0.5f);
        }
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

    private void ClosePanel()
    {
        _animator.SetBool("hidePanel", true);
        
        StartCoroutine(CloseAfterAnimation());
    }

    private IEnumerator CloseAfterAnimation()
    {
        yield return new WaitForSeconds(0.5f); // match animation length
        PanelManager.Close("testselector");
    }
}
