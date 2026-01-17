using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

//language switcher for multiple dropdowns


//important - its columns names from DB
public enum Languages
{
    RU,
    EN
}

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

    public Locale GetLocale()
    {
        if(gameData == null)
            gameData = GameObject.FindWithTag("GameData")?.GetComponent<GameData>();

        string savedLang = gameData.saveData.lang.ToLower();

        Locale locale = LocalizationSettings.AvailableLocales.Locales
            .FirstOrDefault(l => l.Identifier.Code == savedLang);

        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;

        return locale;
    }

    public static Languages GetLanguageFromLocale(Locale locale)
    {
        if (locale == null)
            return Languages.EN; // default fallback

        string code = locale.Identifier.Code.ToLower(); // usually "ru", "en", etc.

        switch (code)
        {
            case "ru": return Languages.RU;
            case "en": return Languages.EN;
            default:
                Debug.LogWarning($"Unknown locale code '{code}', defaulting to EN");
                return Languages.EN;
        }
    }
}