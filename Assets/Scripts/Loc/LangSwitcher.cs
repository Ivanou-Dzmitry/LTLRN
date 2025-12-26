using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

public class LanguageSwitcher : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;
    private bool isChanging = false;

    private GameData gameData;      

    void Start()
    {       
        StartCoroutine(InitializeLanguageDropdown());
    }

    private System.Collections.IEnumerator InitializeLanguageDropdown()
    {
        yield return LocalizationSettings.InitializationOperation;

        // Fill dropdown with available locales
        languageDropdown.ClearOptions();

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            languageDropdown.options.Add(new TMP_Dropdown.OptionData(locale.Identifier.Code.ToUpper()));
        }

        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        // Load saved language or use current
        if (!string.IsNullOrEmpty(gameData.saveData.lang))
        {
            // Find the locale by code
            var savedLocale = LocalizationSettings.AvailableLocales.Locales
                .FirstOrDefault(l => l.Identifier.Code == gameData.saveData.lang);

            if (savedLocale != null)
            {
                LocalizationSettings.SelectedLocale = savedLocale;
            }
        }

        // Set dropdown to current language
        var currentLocale = LocalizationSettings.SelectedLocale;
        int currentIndex = LocalizationSettings.AvailableLocales.Locales.IndexOf(currentLocale);
        languageDropdown.value = currentIndex;
        languageDropdown.RefreshShownValue();

        // Add listener for changes
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnLanguageChanged(int index)
    {
        if (isChanging) return;
        isChanging = true;

        var selectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        LocalizationSettings.SelectedLocale = selectedLocale;

        gameData.saveData.lang = selectedLocale.Identifier.Code;
        gameData.SaveToFile();

        isChanging = false;
    }
}
