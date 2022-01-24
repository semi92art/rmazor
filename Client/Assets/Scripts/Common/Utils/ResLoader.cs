using System.IO;
using System.Xml.Linq;
using Common.Entities;
using UnityEditor;
using UnityEngine;

namespace Common.Utils
{
    public static class ResLoader
    {
        #region constants

        public const  string PrefabSetsLocalPath = "prefab_sets";
        private const string PrefabSetsPath      = "Assets/Resources/" + PrefabSetsLocalPath;
        
        #endregion

        #region API

        public static PrefabSetScriptableObject GetPrefabSet(string _PrefabSetName)
        {
            PrefabSetScriptableObject set = null;
            // FIXME скрываем ошибку "GetCount is not allowed to be called during serialization..."
            // https://docs.unity3d.com/Manual/script-Serialization-Errors.html
            Dbg.LogZone(ELogLevel.Nothing, () =>
            {
                set = Resources.Load<PrefabSetScriptableObject>($"{PrefabSetsLocalPath}/{_PrefabSetName}");
            });
            if (set == null)
                Dbg.LogError($"Prefab set with name {_PrefabSetName} does not exist");
            return set;
        }

        public static bool PrefabSetExist(string _PrefabSetName)
        {
            var set = Resources.Load<PrefabSetScriptableObject>($"{PrefabSetsLocalPath}/{_PrefabSetName}");
            return set != null;
        }

#if UNITY_EDITOR
        public static PrefabSetScriptableObject CreatePrefabSetIfNotExist(string _PrefabSetName)
        {
            if (PrefabSetExist(_PrefabSetName))
                return GetPrefabSet(_PrefabSetName);
            var set = ScriptableObject.CreateInstance<PrefabSetScriptableObject>();
            AssetDatabase.CreateAsset(set, $"{PrefabSetsPath}/{_PrefabSetName}.asset");
            AssetDatabase.SaveAssets();
            return set;
        }
#endif

        public static TextAsset GetText(string _Path)
        {
            return Resources.Load<TextAsset>(_Path);
        }

        #endregion
        
        public static XElement FromResources(string _Path)
        {
            var textAsset = Resources.Load<TextAsset>(_Path);
            using (var ms = new MemoryStream(textAsset.bytes))
                return XDocument.Load(ms).Element("data");
        }
    }
}