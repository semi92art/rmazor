using Entities;
using Lean.Localization;
using UnityEngine;
using Utils;

public class LocalizationManager : IInit
{
    #region singleton
    
    private static LocalizationManager _instance;
    public static LocalizationManager Instance => _instance ?? (_instance = new LocalizationManager());

    #endregion

    #region nonpublic members

    private GameObject m_localizationObject;

    #endregion

    #region api

    public event NoArgsHandler Initialized;

    public void Init()
    {
        if (m_localizationObject != null)
            return;
        m_localizationObject = new GameObject("Localization");
        Object.DontDestroyOnLoad(m_localizationObject);
        var localization = m_localizationObject.AddComponent<LeanLocalization>();
        //add languages in list
        string[] cultres = {"en", "en-GB"};
        localization.AddLanguage("English", cultres);
        cultres = new [] {"ru", "ru-RUS"};
        localization.AddLanguage("Russian", cultres);
        cultres = new [] {"ger", "ger-GER"};
        localization.AddLanguage("German", cultres);
        cultres = new [] {"sp", "sp-SP"};
        localization.AddLanguage("Spanish", cultres);
        cultres = new []{"por", "por-POR"};
        localization.AddLanguage("Portugal", cultres);
        
        //Create readers from localization files
        GameObject englishCsv = new GameObject("englishCSV");
        englishCsv.transform.SetParent(m_localizationObject.transform);
        LeanLanguageCSV engCsv = englishCsv.AddComponent<LeanLanguageCSV>();
        engCsv.Source = Resources.Load<TextAsset>("Texts/English");
        engCsv.Language = "English";
        
        GameObject russianCsv = new GameObject("russianCSV");
        russianCsv.transform.SetParent(m_localizationObject.transform);
        LeanLanguageCSV rusCsv = russianCsv.AddComponent<LeanLanguageCSV>();
        rusCsv.Source = Resources.Load<TextAsset>("Texts/Russian");
        rusCsv.Language = "Russian";
        
        GameObject germanCsv = new GameObject("germanCSV");
        germanCsv.transform.SetParent(m_localizationObject.transform);
        LeanLanguageCSV gerCsv = germanCsv.AddComponent<LeanLanguageCSV>();
        gerCsv.Source = Resources.Load<TextAsset>("Texts/German");
        gerCsv.Language = "German";
        
        GameObject spanishCsv = new GameObject("spanishCSV");
        spanishCsv.transform.SetParent(m_localizationObject.transform);
        LeanLanguageCSV spCsv = spanishCsv.AddComponent<LeanLanguageCSV>();
        spCsv.Source = Resources.Load<TextAsset>("Texts/Spanish");
        spCsv.Language = "Spanish";
        
        GameObject portugalCsv = new GameObject("portugalCSV");
        portugalCsv.transform.SetParent(m_localizationObject.transform);
        LeanLanguageCSV portCsv = portugalCsv.AddComponent<LeanLanguageCSV>();
        portCsv.Source = Resources.Load<TextAsset>("Texts/Portugal");
        portCsv.Language = "Portugal";
        
        localization.SetCurrentLanguage(SaveUtils.GetValue<Language>(SaveKey.SettingLanguage).ToString());
        
        Initialized?.Invoke();
    }
   
    #endregion
}
