using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorGUILayout;

namespace LeTai.TrueShadow.Editor
{
[CanEditMultipleObjects]
[CustomEditor(typeof(TrueShadow))]
public class TrueShadowEditor : UnityEditor.Editor
{
    EditorProperty sizeProp;
    EditorProperty angleProp;
    EditorProperty distanceProp;
    EditorProperty colorProp;
    EditorProperty blendModeProp;
    EditorProperty colorBleedModeProp;
    EditorProperty shadowAsSiblingProp;
    EditorProperty cutoutProp;
    EditorProperty bakedProp;

    GUIContent procrastinateLabel;

    static bool showExperimental;
    static bool showAdvanced;

    void OnEnable()
    {
        sizeProp            = new EditorProperty(serializedObject, nameof(TrueShadow.Size));
        angleProp           = new EditorProperty(serializedObject, nameof(TrueShadow.OffsetAngle));
        distanceProp        = new EditorProperty(serializedObject, nameof(TrueShadow.OffsetDistance));
        colorProp           = new EditorProperty(serializedObject, nameof(TrueShadow.Color));
        blendModeProp       = new EditorProperty(serializedObject, nameof(TrueShadow.BlendMode));
        shadowAsSiblingProp = new EditorProperty(serializedObject, nameof(TrueShadow.ShadowAsSibling));
        cutoutProp          = new EditorProperty(serializedObject, nameof(TrueShadow.Cutout));
        bakedProp           = new EditorProperty(serializedObject, nameof(TrueShadow.Baked));
        colorBleedModeProp  = new EditorProperty(serializedObject, nameof(TrueShadow.ColorBleedMode));

        if (EditorPrefs.GetBool("LeTai_TrueShadow_" + nameof(showExperimental)))
        {
            showExperimental = EditorPrefs.GetBool("LeTai_TrueShadow_" + nameof(showExperimental), false);
            showAdvanced     = EditorPrefs.GetBool("LeTai_TrueShadow_" + nameof(showAdvanced),     false);
        }

        procrastinateLabel = new GUIContent("Procrastinate", "A bug that is too fun to fix");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        sizeProp.Draw();
        angleProp.Draw();
        distanceProp.Draw();
        colorProp.Draw();
        blendModeProp.Draw();

        using (var change = new EditorGUI.ChangeCheckScope())
        {
            Space();
            showExperimental = Foldout(showExperimental, "Experimental Settings", true);
            using (new EditorGUI.IndentLevelScope())
                if (showExperimental)
                {
                    shadowAsSiblingProp.Draw();

                    if (((TrueShadow) serializedObject.targetObject).ShadowAsSibling)
                        cutoutProp.Draw();

                    // bakedProp.Draw();
                }

            showAdvanced = Foldout(showAdvanced, "Advanced Settings", true);
            using (new EditorGUI.IndentLevelScope())
                if (showAdvanced)
                {
                    colorBleedModeProp.Draw();

                    if (KnobPropertyDrawer.procrastinationMode)
                    {
                        var rot = GUI.matrix;
                        GUI.matrix                             =  Matrix4x4.identity;
                        KnobPropertyDrawer.procrastinationMode ^= Toggle("Be Productive", false);
                        GUI.matrix                             =  rot;
                    }
                    else
                    {
                        KnobPropertyDrawer.procrastinationMode |= Toggle(procrastinateLabel, false);
                    }
                }

            if (change.changed)
            {
                EditorPrefs.SetBool("LeTai_TrueShadow_" + nameof(showExperimental), showExperimental);
                EditorPrefs.SetBool("LeTai_TrueShadow_" + nameof(showAdvanced),     showAdvanced);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
}
