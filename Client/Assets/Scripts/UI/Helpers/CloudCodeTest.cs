using System.Threading.Tasks;
using Common;
using Unity.Services.CloudCode;
using UnityEngine;

namespace UI.Helpers
{
    public class CloudCodeTest : MonoBehaviour
    {
    }
    
    #if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(CloudCodeTest))]
    public class CloudCodeTestEditor : UnityEditor.Editor
    {
        private CloudCodeTest o;

        private void OnEnable()
        {
            o = target as CloudCodeTest;
        }

        public override async void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("TestScript"))
            {
                var res = await TestScript();
                Dbg.Log(res);
            }
        }

        private static async Task<string> TestScript()
        {
            var result = await CloudCode.CallEndpointAsync("test_script",  null);
            return result;
        }
    }
    
    #endif
}