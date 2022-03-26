using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Common;
using Common.Constants;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.SceneManagement;

namespace Utils.Editor
{
    public static class BuildSettingsUtils
    {
        [MenuItem("Tools/Build Settings/Set default build scene list")]
        public static void AddDefaultScenesToBuild()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"Assets/Scenes/{SceneNames.Preload}.unity", true),
                new EditorBuildSettingsScene($"Assets/Scenes/{SceneNames.Level}.unity", true)
            };
        }

        [MenuItem("Tools/Build Settings/Add current scene in build")]
        public static void AddOpenedSceneToBuild()
        {
            string path = SceneManager.GetActiveScene().path;
            EditorBuildSettings.scenes = new[] {new EditorBuildSettingsScene(path, true)};
        }

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget _Target, string _PathToBuild)
        {
            if (_Target != BuildTarget.iOS)
                return;
            const string infoPlistFileName = "Info.plist";
            string infoPlistPath = Path.Combine(_PathToBuild, infoPlistFileName);
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
            AddIronSourceElementsToPlist(dict);
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
            var keyElements = _Dict.Elements("key");
            if (keyElements.Any(_El => _El.Value == "SKAdNetworkItems"))
                return;
            _Dict.Add(new XElement("key", "SKAdNetworkItems"));
            var arrayEl = new XElement("array");
            var dictEl =  new XElement("dict");
            var stringEl = new XElement("string", "su67r6k2v3.skadnetwork");
            dictEl.Add(stringEl);
            arrayEl.Add(dictEl);
            _Dict.Add(arrayEl);
        }
    }
}