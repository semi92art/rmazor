using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Common;
using mazing.common.Runtime;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.CrashReporting;

public class PrePostProcessBuild : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int  callbackOrder => 0;

    [InitializeOnLoadMethod]
    public static void InitOnLoad()
    {
        CrashReportingSettings.enabled = false;
    }
    
    public void OnPreprocessBuild(BuildReport _Report)
    {
        CrashReportingSettings.enabled = true;
    }
    
    public void OnPostprocessBuild(BuildReport _Report)
    {
        CrashReportingSettings.enabled = false;
        if (_Report.summary.platform != BuildTarget.iOS)
            return;
        const string infoPlistFileName = "Info.plist";
        string infoPlistPath = Path.Combine(_Report.summary.outputPath, infoPlistFileName);
        if (!File.Exists(infoPlistPath))
        {
            Dbg.LogWarning($"{infoPlistFileName} does not exist.");
            return;
        }
        var xDoc = XDocument.Load(infoPlistPath);
        var infoPlistXel = xDoc.Element("plist");
        var dictEls = infoPlistXel?.Elements("dict").ToList() ?? new List<XElement>();
        var dict = dictEls.FirstOrDefault();
        if (dict == null)
        {
            Dbg.LogWarning($"Element \"dict\" not found in {infoPlistFileName}");
            return;
        }
        AddAdMobElementsToPlist(dict);
        // AddIronSourceElementsToPlist(dict);
        xDoc.Save(infoPlistPath);
    }

    private static void AddAdMobElementsToPlist(XContainer _Dict)
    {
        var keyElements = _Dict.Elements("key");
        if (keyElements.Any(_El => _El.Value == "GADIsAdManagerApp"))
            return;
        _Dict.Add(new XElement("key", "GADIsAdManagerApp"));
        _Dict.Add(new XElement("true"));
    }

    private static void AddIronSourceElementsToPlist(XContainer _Dict)
    {
        var adNetworksArray = _Dict
            .Elements("array")
            .FirstOrDefault(_El => _El.Elements("dict").Any());
        if (adNetworksArray == null)
        {
            Dbg.LogError("Failed to add ironsource dependencies");
            return;
        }
        var dictEl =  new XElement("dict");
        var keyEl = new XElement("key", "SKAdNetworkIdentifier");
        var stringEl = new XElement("string", "su67r6k2v3.skadnetwork");
        dictEl.Add(keyEl);
        dictEl.Add(stringEl);
        adNetworksArray.Add(dictEl);
    }
}
