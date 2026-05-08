using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements; // Ensure this is included for UI Toolkit

[Serializable]
public class FontLanguageMap
{
    public SystemLanguage language;
    public FontAsset fontAsset; // Directly using UI Toolkit's FontAsset
}

[Serializable]
public class TMPFontLanguageMap
{
    public SystemLanguage language;
    public TMP_FontAsset fontAsset; // Directly using TextMeshPro's TMP_FontAsset
}

[CreateAssetMenu(fileName = "FontManager", menuName = "ScriptableObjects/FontManager", order = 1)]
public class FontManager : ScriptableObject, IDependentInitialization
{
    [SerializeField]
    private List<FontLanguageMap> fontMapUI = new List<FontLanguageMap>();

    [SerializeField]
    private List<TMPFontLanguageMap> TMPfontMapUI = new List<TMPFontLanguageMap>();

    [SerializeField]
    private List<FontLanguageMap> fontMapCard = new List<FontLanguageMap>();

    private Dictionary<SystemLanguage, FontAsset> runtimeFontMapUI = new Dictionary<SystemLanguage, FontAsset>();
    private Dictionary<SystemLanguage, TMP_FontAsset> runtimeTMPFontMapUI = new Dictionary<SystemLanguage, TMP_FontAsset>();
    private Dictionary<SystemLanguage, FontAsset> runtimeFontMapCard = new Dictionary<SystemLanguage, FontAsset>();
    private FontAsset currentFontUI;
    private TMP_FontAsset currentFontUITMP;
    private FontAsset currentFontCard;
    private StyleFontDefinition currentFontDefinitionUI;
    private StyleFontDefinition currentFontDefinitionCard;

    [SerializeField]
    private FontAsset defaultFontUI;

    [SerializeField]
    private TMP_FontAsset defaultFontUITMP;

    [SerializeField]
    private FontAsset defaultFontCard;

    private Dictionary<string, SystemLanguage> isoToSystemLanguageMap;

    public void Initialize()
    {
        // Initialize the font dictionary
        foreach (var map in fontMapUI)
        {
            if (!runtimeFontMapUI.ContainsKey(map.language))
            {
                runtimeFontMapUI.Add(map.language, map.fontAsset);
            }
        }

        foreach (var map in TMPfontMapUI)
        {
            if (!runtimeTMPFontMapUI.ContainsKey(map.language))
            {
                runtimeTMPFontMapUI.Add(map.language, map.fontAsset);
            }
        }

        foreach (var map in fontMapCard)
        {
            if (!runtimeFontMapCard.ContainsKey(map.language))
            {
                runtimeFontMapCard.Add(map.language, map.fontAsset);
            }
        }

        isoToSystemLanguageMap = new Dictionary<string, SystemLanguage>
        {
            {"en", SystemLanguage.English},
            {"ko", SystemLanguage.Korean},
            {"ja", SystemLanguage.Japanese},
            {"fr", SystemLanguage.French},
            {"de", SystemLanguage.German},
            {"zh", SystemLanguage.ChineseSimplified},
            {"zh-TW", SystemLanguage.ChineseTraditional},
            {"es", SystemLanguage.Spanish},
            {"it", SystemLanguage.Italian},
            {"pt", SystemLanguage.Portuguese},
            {"hi", SystemLanguage.Hindi},
            {"id", SystemLanguage.Indonesian},
            {"ru", SystemLanguage.Russian},
            {"tr", SystemLanguage.Turkish},
            {"vi", SystemLanguage.Vietnamese}
        };

        // Set initial font
        UpdateCurrentFont();
    }   

    void OnEnable()
    {
        // Subscribe to the locale change event
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    void OnDisable()
    {
        // Unsubscribe from the locale change event
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        UpdateCurrentFont();
    }

    private void UpdateCurrentFont()
    {
        if (LocalizationSettings.SelectedLocale != null)
        {
            string isoCode = LocalizationSettings.SelectedLocale.Identifier.CultureInfo.TwoLetterISOLanguageName.ToLower();
            SystemLanguage language = ConvertISOCodetoSystemLanguage(isoCode);
            currentFontUI = runtimeFontMapUI.ContainsKey(language) ? runtimeFontMapUI[language] : defaultFontUI;
            currentFontUITMP = runtimeTMPFontMapUI.ContainsKey(language) ? runtimeTMPFontMapUI[language] : defaultFontUITMP;
            currentFontCard = runtimeFontMapCard.ContainsKey(language) ? runtimeFontMapCard[language] : defaultFontCard;
        }
        else
        {
            currentFontUI = defaultFontUI;
            currentFontUITMP = defaultFontUITMP;
            currentFontCard = defaultFontCard;
        }

        currentFontDefinitionUI = new StyleFontDefinition(currentFontUI);
        currentFontDefinitionCard = new StyleFontDefinition(currentFontCard);
    }

    public FontAsset GetCurrentUIFont()
    {
        return currentFontUI;
    }

    public TMP_FontAsset GetCurrentUITMPFontAsset()
    {
        return currentFontUITMP;
    }

    public FontAsset GetCurrentCardFont()
    {
        return currentFontCard;
    }

    public StyleFontDefinition GetCurrentUIStyleFont()
    {
        return currentFontDefinitionUI;
    }

    public StyleFontDefinition GetCurrentCardStyleFont()
    {
        return currentFontDefinitionCard;
    }

    private SystemLanguage ConvertISOCodetoSystemLanguage(string isoCode)
    {
        if (isoToSystemLanguageMap == null)
        {
            Initialize();
        }

        if (isoToSystemLanguageMap.TryGetValue(isoCode, out SystemLanguage language))
        {
            return language;
        }
        else
        {
            return SystemLanguage.Unknown; // Fallback if no mapping is found
        }
    }
}
