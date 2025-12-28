using LTLRN.UI;
using UnityEngine;
using System.Collections;

//panel with theme choosed

public class EX_ThemesPanel : Panel
{
    private GameData gameData;
    private DBUtils dbUtils;

    private ExDataLoader dataLoader;
    public GameObject themeButtonPrefab;
    public RectTransform themesContainer;

    private SectionManager tempSectionManager;

    public override void Open()
    {
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
            themeBtnComponent.themeName.text = dataLoader.themes.theme[i].themeName;
            themeBtnComponent.themeDescription.text = dataLoader.themes.theme[i].themeDescription;
            themeBtnComponent.sectionManager = dataLoader.themes.theme[i];

            themeBtnComponent.themeIndex = i;
        }
    }

    //IMPORTANT - there we apply theme
    public void ApplySelectedTheme()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        if (dataLoader != null && dataLoader.tempSectionManager != null)
        {
            dataLoader.sectionManager = dataLoader.tempSectionManager;   
            gameData.saveData.selectedThemeIndex = dataLoader.tempThemeIndex;
            gameData.SaveToFile();
        }
    }

}


