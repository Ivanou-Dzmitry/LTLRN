using LTLRN.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Linq;

//panel with theme choosed

public class EX_ThemesPanel : Panel
{
    private GameData gameData;
    private DBUtils dbUtils;

    private ExDataLoader dataLoader;
    public GameObject themeButtonPrefab;
    public RectTransform themesContainer;

    public override void Open()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        base.Open();
    }

    private void Start()
    {
        StartCoroutine(WaitAndLoadData());
    }

    private IEnumerator WaitAndLoadData()
    {
        // Find DBUtils (with safety check)
        GameObject dbUtilsObj = GameObject.FindWithTag("DBUtils");

        if (dbUtilsObj == null)
        {
            Debug.LogError("DBUtils GameObject with tag 'DBUtils' not found!");
            yield break;
        }

        dbUtils = dbUtilsObj.GetComponent<DBUtils>();

        if (dbUtils == null)
        {
            Debug.LogError("DBUtils component not found on GameObject!");
            yield break;
        }

        // Wait for database to be ready
        while (!dbUtils.IsReady)
        {
            yield return null;
        }

        dataLoader = GameObject.FindWithTag("ExDataLoader").GetComponent<ExDataLoader>();

        if (dataLoader != null)
        {
            LoadThemes();

            dataLoader.tempSectionManager = dataLoader.sectionManager;
        }
    }


    //load panels with themes IMPORTANT
    public void LoadThemes()
    {
        // Clear old panels (important when reloading)
        foreach (Transform child in themesContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < dataLoader.themes.theme.Length; i++)
        {
            string themeName = dataLoader.themes.theme[i].name;
            GameObject themeButton = Instantiate(themeButtonPrefab, themesContainer.transform);
                     
            themeButton.name = themeName;

            // Ensure theme exists in DB
            dbUtils.EnsureThemeExists(themeName);

            // Ensure correct UI transform values
            RectTransform rt = themeButton.GetComponent<RectTransform>();

            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            
            // Initialize button data            
            EX_ThemeBtn themeBtnComponent = themeButton.GetComponent<EX_ThemeBtn>();

            Locale locale = GetLocale();
            SectionManager currentTheme = dataLoader.themes.theme[i];

            //load data into button component
            themeBtnComponent.themeName.text = currentTheme.GetThemeName(currentTheme, locale);
            themeBtnComponent.themeDescription.text = currentTheme.GeThemetDescription(currentTheme, locale);
            themeBtnComponent.sectionManager = dataLoader.themes.theme[i];
            
            //set icon
            themeBtnComponent.themeIcon.sprite = dataLoader.themes.theme[i].themeIcon;
            themeBtnComponent.themeIndex = i;

            //set color
            themeBtnComponent.topPnlImg.color = dataLoader.themes.theme[i].themeHeaderColor;

            //dificulty
            themeBtnComponent.themeDifSlider.value = currentTheme.GetThemeDifValue(currentTheme.themeDifficulty);
            //themeBtnComponent.themeDifficulty.text = dataLoader.themes.theme[i].themeDifficulty.ToString(); ;

            //load info
            int sectionsCount = currentTheme.sections.Length;
            themeBtnComponent.sectionsCount.text = sectionsCount.ToString();
            themeBtnComponent.questionsCount.text = currentTheme.GetTotalQuestionCount().ToString();

            //Complete slider
            int completeCount = dbUtils.GetCompleteSectionsCount();
            themeBtnComponent.themeProgressSlider.maxValue = sectionsCount;
            themeBtnComponent.themeProgressSlider.value = completeCount;
        }
    }

    //IMPORTANT - there we apply theme
    public void ApplySelectedTheme()
    {       
        if (dataLoader != null && dataLoader.tempSectionManager != null)
        {
            dataLoader.sectionManager = dataLoader.tempSectionManager;   
            gameData.saveData.selectedThemeIndex = dataLoader.tempThemeIndex;
            gameData.SaveToFile();
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

}


