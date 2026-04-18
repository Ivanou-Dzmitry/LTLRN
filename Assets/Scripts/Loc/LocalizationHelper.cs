using UnityEngine;
using UnityEngine.Localization.Settings;

public static class LocalizationHelper
{
    public static string GetSafe(string table, string key, string fallback = "")
    {
        // 1. invalid key
        if (string.IsNullOrWhiteSpace(key))
            return fallback;

        // 2. try get localized string
        string result = LocalizationSettings.StringDatabase.GetLocalizedString(table, key);

        // 3. fallback if missing or empty
        if (string.IsNullOrEmpty(result) || result == key)
        {
            Debug.LogWarning($"[Localization] Missing key: '{key}' in table: '{table}'");
            return fallback;
        }

        return result;
    }
}
