using Entities;
using UnityEngine;
using Lean.Localization;
using Utils;

public class LocalizationManager : ISingleton
{
    #region singleton
    
    private static LocalizationManager _instance;
    public static LocalizationManager Instance => _instance ?? (_instance = new LocalizationManager());

    #endregion
    
    #region factory

    
    #endregion
    
    #region private members

    private GameObject m_LocalizationObject;
   
    #endregion

    #region api

    public void Init()
    {
        if (m_LocalizationObject != null)
            return;
        m_LocalizationObject = new GameObject("Localization");
        Object.DontDestroyOnLoad(m_LocalizationObject);
        LeanLocalization localization = m_LocalizationObject.AddComponent<LeanLocalization>();
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
        englishCsv.transform.SetParent(m_LocalizationObject.transform);
        LeanLanguageCSV engCsv = englishCsv.AddComponent<LeanLanguageCSV>();
        engCsv.Source = Resources.Load<TextAsset>("texts/English");
        engCsv.Language = "English";
        
        GameObject russianCsv = new GameObject("russianCSV");
        russianCsv.transform.SetParent(m_LocalizationObject.transform);
        LeanLanguageCSV rusCsv = russianCsv.AddComponent<LeanLanguageCSV>();
        rusCsv.Source = Resources.Load<TextAsset>("texts/Russian");
        rusCsv.Language = "Russian";
        
        GameObject germanCsv = new GameObject("germanCSV");
        germanCsv.transform.SetParent(m_LocalizationObject.transform);
        LeanLanguageCSV gerCsv = germanCsv.AddComponent<LeanLanguageCSV>();
        gerCsv.Source = Resources.Load<TextAsset>("texts/German");
        gerCsv.Language = "German";
        
        GameObject spanishCsv = new GameObject("spanishCSV");
        spanishCsv.transform.SetParent(m_LocalizationObject.transform);
        LeanLanguageCSV spCsv = spanishCsv.AddComponent<LeanLanguageCSV>();
        spCsv.Source = Resources.Load<TextAsset>("texts/Spanish");
        spCsv.Language = "Spanish";
        
        GameObject portugalCsv = new GameObject("portugalCSV");
        portugalCsv.transform.SetParent(m_LocalizationObject.transform);
        LeanLanguageCSV portCsv = portugalCsv.AddComponent<LeanLanguageCSV>();
        portCsv.Source = Resources.Load<TextAsset>("texts/Portugal");
        portCsv.Language = "Portugal";
        
        localization.SetCurrentLanguage(SaveUtils.GetValue<Language>(SaveKey.SettingLanguage).ToString());    
    }
   
    #endregion
}
