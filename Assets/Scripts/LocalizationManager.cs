using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Localization;

public class LocalizationManager : MonoBehaviour, ISingleton
{
    #region singleton
    
    private static LocalizationManager _instance;
    public static LocalizationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject("Localization Manager");
                _instance = obj.AddComponent<LocalizationManager>();
                Init(obj);
            }
            return _instance;
        }
    }
    
    #endregion
    
    #region factory

    
    #endregion
    
    #region private members

   

    #endregion

    #region api
     private static void Init(GameObject _LocalizationObject)
    {
        LeanLocalization localization = _LocalizationObject.AddComponent<LeanLocalization>();
        //add languages in list
        string[] cultres= new string[2]{"en", "en-GB"};
        localization.AddLanguage("English", cultres);
        cultres= new string[2]{"ru", "ru-RUS"};
        localization.AddLanguage("Russian", cultres);
        cultres= new string[2]{"ger", "ger-GER"};
        localization.AddLanguage("German", cultres);
        cultres= new string[2]{"sp", "sp-SP"};
        localization.AddLanguage("Spanish", cultres);
        cultres= new string[2]{"por", "por-POR"};
        localization.AddLanguage("Portugal", cultres);
        
        //Create readers from localization files
        GameObject englishCSV = new GameObject("englishCSV");
        englishCSV.transform.SetParent(_LocalizationObject.transform);
        LeanLanguageCSV engCSV = englishCSV.AddComponent<LeanLanguageCSV>();
        engCSV.Source = Resources.Load<TextAsset>("texts/English");
        engCSV.Language = "English";
        
        GameObject russianCSV = new GameObject("russianCSV");
        russianCSV.transform.SetParent(_LocalizationObject.transform);
        LeanLanguageCSV rusCSV = russianCSV.AddComponent<LeanLanguageCSV>();
        rusCSV.Source = Resources.Load<TextAsset>("texts/Russian");
        rusCSV.Language = "Russian";
        
        GameObject germanCSV = new GameObject("germanCSV");
        germanCSV.transform.SetParent(_LocalizationObject.transform);
        LeanLanguageCSV gerCSV = germanCSV.AddComponent<LeanLanguageCSV>();
        gerCSV.Source = Resources.Load<TextAsset>("texts/German");
        gerCSV.Language = "German";
        
        GameObject spanishCSV = new GameObject("spanishCSV");
        spanishCSV.transform.SetParent(_LocalizationObject.transform);
        LeanLanguageCSV spCSV = spanishCSV.AddComponent<LeanLanguageCSV>();
        spCSV.Source = Resources.Load<TextAsset>("texts/Spanish");
        spCSV.Language = "Spanish";
        
        GameObject portugalCSV = new GameObject("portugalCSV");
        portugalCSV.transform.SetParent(_LocalizationObject.transform);
        LeanLanguageCSV portCSV = portugalCSV.AddComponent<LeanLanguageCSV>();
        portCSV.Source = Resources.Load<TextAsset>("texts/Portugal");
        portCSV.Language = "Portugal";
        
        localization.SetCurrentLanguage("English");
    }
   
    #endregion
}
