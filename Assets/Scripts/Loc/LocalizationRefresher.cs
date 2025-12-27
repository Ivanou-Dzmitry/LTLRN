using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Components;
using System.Collections;

public class LocalizationRefresher : MonoBehaviour
{
    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

        // Wait for localization to be ready before refreshing
        StartCoroutine(RefreshAfterDelay());
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(UnityEngine.Localization.Locale newLocale)
    {
        StartCoroutine(RefreshAfterDelay());
    }

    private IEnumerator RefreshAfterDelay()
    {
        // Wait for localization to initialize
        yield return LocalizationSettings.InitializationOperation;

        // Wait one more frame to ensure everything is loaded
        yield return null;

        RefreshLocalization();
    }

    private void RefreshLocalization()
    {
        var localizedStrings = GetComponentsInChildren<LocalizeStringEvent>(true);

        Debug.Log($"Found {localizedStrings.Length} localized components");

        foreach (var localizedString in localizedStrings)
        {
            // Only refresh if the component is enabled
            if (localizedString.enabled)
            {
                localizedString.RefreshString();
            }
        }
    }
}