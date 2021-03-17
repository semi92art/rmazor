using System;
using System.Linq;
using Extensions;
using UnityEditor;
using UnityEngine;

namespace Games.RazorMaze.Prot.Editor
{
    [CustomEditor(typeof(MazeProtItem)), CanEditMultipleObjects]
    public class MazeProtItemEditor : UnityEditor.Editor
    {
        private readonly Array m_Types = Enum.GetValues(typeof(PrototypingItemType));
        private MazeProtItem[] targetsCopy;
        private void OnEnable()
        {
            targetsCopy = targets.Cast<MazeProtItem>().ToArray();
        }

        private void OnSceneGUI()
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(20, 20, 150, 300));
        
            EditorGUILayout.BeginVertical();
            if (targetsCopy.Length == 1)
            {
                foreach (var t in m_Types)
                {
                    var targ = targetsCopy[0];
                    if (targ.Type == (PrototypingItemType)t)
                    {
                        GUI.color = Color.green;
                        GUI.backgroundColor = Color.gray;
                    }
                    else
                    {
                        GUI.color = Color.white;
                        GUI.backgroundColor = Color.gray;
                    }
                
                    if (GUILayout.Button(t.ToString().WithSpaces()))
                        targ.Type = (PrototypingItemType) t;
                }
            }
            else
            {
                var targ = targetsCopy[0];
                bool allEquals = targetsCopy.All(_T => _T.Type == targ.Type);

                if (allEquals)
                {
                    foreach (var t in m_Types)
                    {
                        if (targ.Type == (PrototypingItemType)t)
                        {
                            GUI.color = Color.green;
                            GUI.backgroundColor = Color.gray;
                        }
                        else
                        {
                            GUI.color = Color.white;
                            GUI.backgroundColor = Color.gray;
                        }
                        if (!GUILayout.Button(t.ToString().WithSpaces())) 
                            continue;
                        foreach (var tg in targetsCopy)
                            tg.Type = (PrototypingItemType) t;
                    }   
                }
                else
                {
                    GUI.color = Color.white;
                    GUI.backgroundColor = Color.gray;
                    foreach (var t in m_Types)
                    {
                        if (!GUILayout.Button(t.ToString().WithSpaces())) 
                            continue;
                        foreach (var tg in targetsCopy)
                            tg.Type = (PrototypingItemType) t;
                    }   
                }
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            Handles.EndGUI();
        }
    }
}