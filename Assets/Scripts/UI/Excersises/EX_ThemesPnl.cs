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
    private LanguageSwitcher languageSwitcher;

    private ExDataLoader dataLoader;
    public GameObject themeButtonPrefab;
    public RectTransform themesContainer;

    private SectionManager tempSectionManager;

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


    //load panels with themes
    public void LoadThemes()
    {
        // Clear old panels (important when reloading)
        foreach (Transform child in themesContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < dataLoader.themes.theme.Length; i++)
        {
            GameObject themeButton = Instantiate(themeButtonPrefab, themesContainer.transform);

            string themeName = dataLoader.themes.theme[i].name;

            themeButton.name = themeName;

            // Ensure theme exists in DB
            dbUtils.EnsureThemeExists(themeName);

            // Ensure correct UI transform values
            RectTransform rt = themeButton.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            
            // Initialize button data            
            EX_ThemeBtn themeBtnComponent = themeButton.GetComponent<EX_ThemeBtn>();

            //load data into button component
            themeBtnComponent.themeName.text = GetTitle(dataLoader.themes.theme[i]);
            themeBtnComponent.themeDescription.text = GetDescription(dataLoader.themes.theme[i]);
            themeBtnComponent.sectionManager = dataLoader.themes.theme[i];
            //set icon
            themeBtnComponent.themeIcon.sprite = dataLoader.themes.theme[i].themeIcon;
            themeBtnComponent.themeIndex = i;

            //dificulty
            themeBtnComponent.themeDifficulty.text = dataLoader.themes.theme[i].themeDifficulty.ToString(); ;
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

    public string GetDescription(SectionManager theme)
    {
        if (theme == null || theme.themeDescription == null)
            return string.Empty;

        Locale locale = GetLocale();

        if (locale == null)
            return theme.themeDescription.en;

        switch (locale.Identifier.Code)
        {
            case "ru":
                return theme.themeDescription.ru;

            case "en":
            default:
                return theme.themeDescription.en;
        }
    }

    public string GetTitle(SectionManager theme)
    {
        if (theme == null || theme.themeName == null)
            return string.Empty;

        Locale locale = GetLocale();

        if (locale == null)
            return theme.themeName.en;

        switch (locale.Identifier.Code)
        {
            case "ru":
                return theme.themeName.ru;

            case "en":
            default:
                return theme.themeName.en;
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


