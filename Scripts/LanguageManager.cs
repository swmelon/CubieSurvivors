using System.Collections.Generic;
using UnityEngine;

public static class LanguageManager
{
    public static List<KeyValuePair<string, string>> Languages = new List<KeyValuePair<string, string>>()
    {
        new KeyValuePair<string, string>("en", "English"),
        new KeyValuePair<string, string>("ko", "한국어"),
        new KeyValuePair<string, string>("ja", "日本語"),
        new KeyValuePair<string, string>("zh-CN", "简体中文"),
        new KeyValuePair<string, string>("zh-TW", "繁體中文"),
        new KeyValuePair<string, string>("fr", "Français"),
        new KeyValuePair<string, string>("es", "Español"),
        new KeyValuePair<string, string>("de", "Deutsch"),
        new KeyValuePair<string, string>("vi", "Tiếng Việt"),
        new KeyValuePair<string, string>("id", "Bahasa Indonesia"),
        new KeyValuePair<string, string>("pt", "Português"),
        new KeyValuePair<string, string>("ru", "Русский"),
        new KeyValuePair<string, string>("it", "Italiano"),
        new KeyValuePair<string, string>("hi", "Hindi"),
        new KeyValuePair<string, string>("tr", "Türkçe")
    };

    public static bool TryChangeLanguage(string localeCode)
    {
        Debug.Log($"Changing language to: {localeCode}");

        // Example: Update the localization settings (if using Unity Localization package)
        var locale = UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        if (locale != null)
        {
            UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale = locale;
            Debug.Log($"Language successfully changed to {locale.Identifier.Code}");
            return true;
        }
        else
        {
            Debug.LogError($"Locale '{localeCode}' not found in the available locales.");
            return false;
        }
    }

    public static List<KeyValuePair<string, string>> GetSortedLanguages()
    {
        string userLocale = GetUserLocale(); // Replace with actual locale retrieval logic
        // Ensure the list is initialized and contains the expected data
        if (Languages == null || Languages.Count == 0)
        {
            Debug.LogError("Languages list is empty or not initialized!");
            return new List<KeyValuePair<string, string>>();
        }

        // 특별 처리: userLocale이 간체(zh-CN) 또는 번체(zh-TW)일 때
        if (userLocale == "zh-CN" || userLocale == "zh-TW")
        {
            var sortedLanguages = new List<KeyValuePair<string, string>>();

            // 현재 언어 추가
            foreach (var lang in Languages)
            {
                if (lang.Key == userLocale)
                {
                    sortedLanguages.Add(lang);
                    Debug.Log($"Added current language: {lang.Key} - {lang.Value}");
                }
            }

            // 다른 중국어 변형 추가 (간체 <-> 번체)
            foreach (var lang in Languages)
            {
                if ((userLocale == "zh-CN" && lang.Key == "zh-TW") ||
                    (userLocale == "zh-TW" && lang.Key == "zh-CN"))
                {
                    sortedLanguages.Add(lang);
                    Debug.Log($"Added other Chinese variant: {lang.Key} - {lang.Value}");
                }
            }

            // 나머지 언어 추가
            foreach (var lang in Languages)
            {
                if (lang.Key != "zh-CN" && lang.Key != "zh-TW")
                {
                    sortedLanguages.Add(lang);
                    Debug.Log($"Added other language: {lang.Key} - {lang.Value}");
                }
            }

            return sortedLanguages;
        }

        // 기본 정렬 로직 (간체나 번체가 아닌 경우)
        var defaultSortedLanguages = new List<KeyValuePair<string, string>>();

        // 현재 언어를 맨 위로
        foreach (var lang in Languages)
        {
            if (lang.Key == userLocale)
            {
                defaultSortedLanguages.Add(lang);
                Debug.Log($"Added current language: {lang.Key} - {lang.Value}");
            }
        }

        // 나머지 언어 추가
        foreach (var lang in Languages)
        {
            if (lang.Key != userLocale)
            {
                defaultSortedLanguages.Add(lang);
                Debug.Log($"Added other language: {lang.Key} - {lang.Value}");
            }
        }

        return defaultSortedLanguages;
    }



    private static string GetUserLocale()
    {
        // Get the system language
        var systemLanguage = Application.systemLanguage;

        // Map Unity's system language to locale codes (ISO 639-1)
        return systemLanguage switch
        {
            SystemLanguage.Korean => "ko",
            SystemLanguage.English => "en",
            SystemLanguage.Japanese => "ja",
            SystemLanguage.ChineseSimplified => "zh-CN",
            SystemLanguage.ChineseTraditional => "zh-TW",
            SystemLanguage.French => "fr",
            SystemLanguage.Spanish => "es",
            SystemLanguage.German => "de",
            SystemLanguage.Vietnamese => "vi",
            SystemLanguage.Indonesian => "id",
            SystemLanguage.Portuguese => "pt",
            SystemLanguage.Russian => "ru",
            SystemLanguage.Italian => "it",
            SystemLanguage.Hindi => "hi",
            SystemLanguage.Turkish => "tr",
            _ => "en" // Default to English if language is not mapped
        };
    }
}
