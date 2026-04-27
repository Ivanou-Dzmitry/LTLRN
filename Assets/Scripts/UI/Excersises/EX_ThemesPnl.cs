using LTLRN.UI;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

//panel with theme choosed

public class EX_ThemesPanel : Panel
{
    private GameData gameData;
    private DBUtils dbUtils;

    //for name display
    public TMP_Text userName;
       
    private ExDataLoader dataLoader;

    //prefab and container for themes
    public GameObject themeButtonPrefab;
    public RectTransform themesContainer;

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

        //get component (with safety check)
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

        // Find ExDataLoader (with safety check)
        dataLoader = GameObject.FindWithTag("ExDataLoader").GetComponent<ExDataLoader>();

        if (dataLoader != null)
        {
            //important - load themes after data is ready
            LoadThemes();

            // Set tempSectionManager to current sectionManager to ensure ApplySelectedTheme works correctly
            dataLoader.tempSectionManager = dataLoader.sectionManager;
        }
    }


    //load panels with themes IMPORTANT
    public void LoadThemes()
    {
        //load player name
        if(gameData == null)
            gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        //set player name at the top of panel
        if (gameData != null)
            userName.text = $", {gameData.saveData.playerName}!";        

        // Clear old panels (important when reloading)
        foreach (Transform child in themesContainer)
        {
            Destroy(child.gameObject);
        }

        // Loop through themes and create buttons
        for (int i = 0; i < dataLoader.themes.theme.Length; i++)
        {
            //prefab instance
            string themeName = dataLoader.themes.theme[i].name;
            GameObject themeButton = Instantiate(themeButtonPrefab, themesContainer.transform);
            
            //set button name
            themeButton.name = themeName;

            // Ensure theme exists in DB
            dbUtils.EnsureThemeExists(themeName);

            // Ensure correct UI transform values
            RectTransform rt = themeButton.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            
            // Initialize button data            
            EX_ThemeBtn themeBtnComponent = themeButton.GetComponent<EX_ThemeBtn>();

            //exit
            if (themeBtnComponent == null)
                return;

            //set current theme                        
            SectionManager currentTheme = dataLoader.themes.theme[i];

            //exit
            if (currentTheme == null)
                return;

            //load data into button component
            
            //localized theme name
            try
            {
                themeBtnComponent.themeName.text = currentTheme.themeName.GetLocalizedString();
            }
            catch
            {
                themeBtnComponent.themeName.text = "Loc not assigned yet";
            }
            
            //thame name in target lang (lern lang LT, LV etc)
            themeBtnComponent.themeNameLocal.text = currentTheme.themeNameTargetLang;

            //set theme            
            themeBtnComponent.sectionManager = currentTheme;
            
            //set theme button icon
            themeBtnComponent.themeIcon.sprite = dataLoader.themes.theme[i].themeIcon;
            
            //set index. For save
            themeBtnComponent.themeIndex = i;

            //load sections count
            int sectionsCount = currentTheme.sections.Length;

            //set valused for progress bar
            themeBtnComponent.themeProgressSlider.maxValue = sectionsCount;
            themeBtnComponent.themeProgressSlider.value = 0;

            //set sections count and questions count
            themeBtnComponent.sectionsCount.text = sectionsCount.ToString();

            //for questions count
            int qCount = 0;
            //int completeCount = 0;

            // Step 1 - try to get questions count - basic
            qCount = currentTheme.GetTotalQuestionCount();

            // Step 2 - try to get questions count - try bundle
            if (qCount == 0)
                qCount = currentTheme.GetBundleTotalQuestionCount();

            //set questions count (any)
            themeBtnComponent.questionsCount.text = qCount.ToString(); // bundle
            
            //initial values
            int completeSections = 0;
            int totalCompleteSections = 0;

            foreach (var section in currentTheme.sections)
            {
                if (!section.isBundle) continue;

                foreach (var bundleSection in section.bundleSections)
                {
                    totalCompleteSections++;

                    bool isComplete = dbUtils.GetSectionComplete(bundleSection.name);
                    if (isComplete)
                    {
                        completeSections++;
                    }
                }
            }

            //tasks cont
            themeBtnComponent.tasksCount.text = totalCompleteSections.ToString();

            //set slider max value
            themeBtnComponent.themeProgressSlider.maxValue = totalCompleteSections;
            //themeBtnComponent.themeProgressSlider.value = comp;

            //slider animator
            themeBtnComponent.themeProgressSlider.GetComponent<EX_SliderAnimator>().AnimateTo(completeSections, 0.5f);

            //fill slider with theme color
            Image fillImage = themeBtnComponent.themeProgressSlider.fillRect.GetComponent<Image>();

            //set slider color
            Color color = dataLoader.themes.theme[i].themeHeaderColor;
            color.a = 0.5f; // 50% opacity

            fillImage.color = color;

            //set color Gray if no sections, set button interactable
            if (sectionsCount == 0)
            {
                //get local description for WIP themes
                string wipTxt = LocalizationSettings.StringDatabase.GetLocalizedString("KELIAS_UI", "TopicWIPTxt");

                //set UI
                themeBtnComponent.topPnlImg.color = palette.Panel02;
                themeBtnComponent.button.interactable = false;
                themeBtnComponent.themeDescription.text = wipTxt;
            }
            else
            {
                themeBtnComponent.topPnlImg.color = dataLoader.themes.theme[i].themeHeaderColor;
                themeBtnComponent.button.interactable = true;
                //description
                //themeBtnComponent.themeDescription.text = currentTheme.GeThemetDescription(currentTheme, locale);
            }

            themeBtnComponent.UpdateUI();
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
    
}


