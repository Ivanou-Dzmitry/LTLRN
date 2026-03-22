using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

//language switcher for multiple dropdowns


//important - its columns names from DB
public enum Languages
{
    RU,
    EN
}

public class LanguageSwitcher : MonoBehaviour
{
    [SerializeField] private Button[] langButtons;
    private GameData gameData;

    private void Start()
    {
        StartCoroutine(InitializeLanguageButtons());
    }

    private IEnumerator InitializeLanguageButtons()
    {
        yield return LocalizationSettings.InitializationOperation;

        gameData = GameObject.FindWithTag("GameData")?.GetComponent<GameData>();

        // Load saved language or fallback
        string savedLang = gameData?.saveData.lang;

        if (!string.IsNullOrEmpty(savedLang))
        {
            SetLanguage(savedLang.ToUpper());
        }
        else
        {
            SetLanguage("EN"); // default
        }

        // Hook buttons
        foreach (var btn in langButtons)
        {
            var btnImage = btn.GetComponent<ButtonImage>();
            string langCode = btnImage.buttonTextStr.ToUpper();

            btn.onClick.AddListener(() => OnLanguageButtonClicked(langCode));
        }

        UpdateButtonsUI();
    }

    private void OnLanguageButtonClicked(string langCode)
    {
        SetLanguage(langCode);
        UpdateButtonsUI();
    }

    private void SetLanguage(string langCode)
    {
        var locale = LocalizationSettings.AvailableLocales.Locales
            .FirstOrDefault(l => l.Identifier.Code.ToUpper() == langCode);

        if (locale == null)
        {
            Debug.LogWarning($"Locale not found: {langCode}");
            return;
        }

        LocalizationSettings.SelectedLocale = locale;

        // Save
        if (gameData != null)
        {
            gameData.saveData.lang = locale.Identifier.Code;
            gameData.SaveToFile();
        }
    }

    private void UpdateButtonsUI()
    {
        string currentLang = LocalizationSettings.SelectedLocale.Identifier.Code.ToUpper();

        foreach (var btn in langButtons)
        {
            var btnImage = btn.GetComponent<ButtonImage>();
            string btnLang = btnImage.buttonTextStr.ToUpper();

            bool isSelected = btnLang == currentLang;

            btnImage.SetSelected(isSelected);
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