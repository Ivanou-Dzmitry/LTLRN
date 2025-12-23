using LTLRN.UI;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//panel with theme choosed

public class EX_ThemesPanel : Panel
{
    private GameData gameData;

    private ExDataLoader dataLoader;
    public GameObject themeButtonPrefab;
    public RectTransform themesContainer;

    private SectionManager tempSectionManager;

    public override void Open()
    {
        dataLoader = GameObject.FindWithTag("ExDataLoader").GetComponent<ExDataLoader>();

        if (dataLoader != null)
        {
            LoadThemes();

            dataLoader.tempSectionManager = dataLoader.sectionManager;
        }

        base.Open();
    }

    //load panels with themes
    public void LoadThemes()
    {
        // Clear old panels (important when reloading)
        foreach (Transform child in themesContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < dataLoader.themes.Length; i++)
        {
            GameObject themeButton = Instantiate(themeButtonPrefab, themesContainer.transform);
            themeButton.name = "ThemeButton_" + i.ToString();
            
            // Ensure correct UI transform values
            RectTransform rt = themeButton.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            // Initialize button data
            
            EX_ThemeBtn themeBtnComponent = themeButton.GetComponent<EX_ThemeBtn>();

            //load data into button component
            themeBtnComponent.themeName.text = dataLoader.themes[i].themeName;
            themeBtnComponent.themeDescription.text = dataLoader.themes[i].themeDescription;
            themeBtnComponent.sectionManager = dataLoader.themes[i];

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


