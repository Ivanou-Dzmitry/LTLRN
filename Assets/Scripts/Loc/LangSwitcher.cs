using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;

//language switcher for multiple dropdowns

public class LanguageSwitcher : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private TMP_Dropdown langDropdownSet;

    private TMP_Dropdown[] dropdowns;
    private GameData gameData;
    private bool isChanging = false;

    private void Awake()
    {
        // Create array of non-null dropdowns only
        dropdowns = new[] { languageDropdown, langDropdownSet }.Where(d => d != null).ToArray();
    }

    private void Start()
    {
        if (dropdowns.Length > 0)
        {
            StartCoroutine(InitializeLanguageDropdown());
        }
    }

    private IEnumerator InitializeLanguageDropdown()
    {
        //Debug.Log("Initializing Language Dropdown...");

        yield return LocalizationSettings.InitializationOperation;

        // Create options list once
        var options = LocalizationSettings.AvailableLocales.Locales
            .Select(locale => new TMP_Dropdown.OptionData(locale.Identifier.Code.ToUpper()))
            .ToList();

        // Apply options to all dropdowns
        foreach (var dropdown in dropdowns)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
        }

        // Cache GameData
        gameData = GameObject.FindWithTag("GameData")?.GetComponent<GameData>();

        // Load saved language
        if (gameData != null && !string.IsNullOrEmpty(gameData.saveData.lang))
        {
            var savedLocale = LocalizationSettings.AvailableLocales.Locales
                .FirstOrDefault(l => l.Identifier.Code == gameData.saveData.lang);

            if (savedLocale != null)
            {
                LocalizationSettings.SelectedLocale = savedLocale;
            }
        }

        // Set current language index
        int currentIndex = LocalizationSettings.AvailableLocales.Locales
            .IndexOf(LocalizationSettings.SelectedLocale);

        // Apply to all dropdowns and add listeners
        foreach (var dropdown in dropdowns)
        {
            dropdown.SetValueWithoutNotify(currentIndex);
            dropdown.onValueChanged.AddListener(OnLanguageChanged);
        }
    }

    private void OnLanguageChanged(int index)
    {
        if (isChanging) return;

        isChanging = true;

        var selectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        LocalizationSettings.SelectedLocale = selectedLocale;

        // Save language
        if (gameData != null)
        {
            gameData.saveData.lang = selectedLocale.Identifier.Code;
            gameData.SaveToFile();
        }

        // Sync all other dropdowns
        foreach (var dropdown in dropdowns)
        {
            dropdown.SetValueWithoutNotify(index);
        }

        isChanging = false;
    }

    private void OnDestroy()
    {
        // Clean up listeners
        foreach (var dropdown in dropdowns)
        {
            dropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        }
    }
}