
// Custom shader inspector to make it more user-friendly
using UnityEngine;
using UnityEditor;

// Place this script in an Editor folder
public class OutlineShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        // Find properties
        MaterialProperty mainTex = FindProperty("_MainTex", properties);
        MaterialProperty outlineColor = FindProperty("_OutlineColor", properties);
        MaterialProperty outlineWidth = FindProperty("_OutlineWidth", properties);
        MaterialProperty useAlpha = FindProperty("_UseAlpha", properties);

        Material targetMat = materialEditor.target as Material;

        EditorGUILayout.LabelField("Sprite Outline Settings", EditorStyles.boldLabel);

        materialEditor.TexturePropertySingleLine(new GUIContent("Main Texture"), mainTex);

        EditorGUI.BeginChangeCheck();

        // Outline color picker
        Color color = EditorGUILayout.ColorField("Outline Color", outlineColor.colorValue);
        if (EditorGUI.EndChangeCheck())
        {
            outlineColor.colorValue = color;
        }

        // Outline width slider
        EditorGUI.BeginChangeCheck();
        float width = EditorGUILayout.Slider("Outline Width", outlineWidth.floatValue, 0, 10);
        if (EditorGUI.EndChangeCheck())
        {
            outlineWidth.floatValue = width;
        }

        // Use alpha toggle
        EditorGUI.BeginChangeCheck();
        bool useAlphaValue = EditorGUILayout.Toggle("Use Alpha Channel", useAlpha.floatValue > 0.5f);
        if (EditorGUI.EndChangeCheck())
        {
            useAlpha.floatValue = useAlphaValue ? 1.0f : 0.0f;
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This shader creates outlines around 2D sprites.\nAdjust the outline width and color as needed.", MessageType.Info);

        // Draw other properties
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Advanced Settings", EditorStyles.boldLabel);
        materialEditor.RenderQueueField();
        materialEditor.EnableInstancingField();
    }
}
