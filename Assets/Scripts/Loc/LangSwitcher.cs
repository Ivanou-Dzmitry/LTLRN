using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

public class LanguageSwitcher : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private TMP_Dropdown langDropdownSet;
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
        if(languageDropdown != null )
            languageDropdown.ClearOptions();
        
        if( langDropdownSet != null )
            langDropdownSet.ClearOptions();

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (languageDropdown != null)
                languageDropdown.options.Add(new TMP_Dropdown.OptionData(locale.Identifier.Code.ToUpper()));
        }

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            if (langDropdownSet != null)
                langDropdownSet.options.Add(new TMP_Dropdown.OptionData(locale.Identifier.Code.ToUpper()));
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

        if (languageDropdown != null)
        {
            languageDropdown.value = currentIndex;
            languageDropdown.RefreshShownValue();
            // Add listener for changes
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        if(langDropdownSet != null)
        {
            langDropdownSet.value = currentIndex;
            langDropdownSet.RefreshShownValue();
            langDropdownSet.onValueChanged.AddListener(OnLanguageChanged);
        }

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
