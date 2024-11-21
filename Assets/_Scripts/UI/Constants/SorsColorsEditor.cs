using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;

[CustomEditor(typeof(SorsColors))]
[System.Serializable]
public class SorsColorsEditor : Editor
{
    GUISkin customSkin;
    protected static float foldoutItemSpace = 2;
    protected static float foldoutTopSpace = 5;
    protected static float foldoutBottomSpace = 2;
    protected static bool showColors = true;
    protected static bool showTypes = true;
    protected static bool showStats = true;
    protected static bool showHighlights = true;

    void OnEnable()
    {
        customSkin = (GUISkin)Resources.Load("SorsCustomEditorSkin");
    }

    public override void OnInspectorGUI()
    {
        // Foldout style
        GUIStyle foldoutStyle = customSkin.FindStyle("UIM Foldout");

        // UIM Header
        EditorDrawer.DrawHeader(customSkin, "UIM Header", 8);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        #region Players       
        var player = serializedObject.FindProperty("player");
        var opponent = serializedObject.FindProperty("opponent");

        GUILayout.Space(foldoutTopSpace);
        GUILayout.BeginHorizontal();
        showColors = EditorGUILayout.Foldout(showColors, "Players", true, foldoutStyle);
        showColors = GUILayout.Toggle(showColors, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));
        GUILayout.EndHorizontal();
        GUILayout.Space(foldoutBottomSpace);

        if (showColors)
        {
            EditorDrawer.DrawProperty(player, customSkin, "Player");
            EditorDrawer.DrawProperty(opponent, customSkin, "Opponent");
        }
        #endregion

        #region Card Types       
        var creature = serializedObject.FindProperty("creature");
        var technology = serializedObject.FindProperty("technology");
        var money = serializedObject.FindProperty("money");

        GUILayout.Space(foldoutTopSpace);
        GUILayout.BeginHorizontal();
        showTypes = EditorGUILayout.Foldout(showTypes, "Card Types", true, foldoutStyle);
        showTypes = GUILayout.Toggle(showTypes, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));
        GUILayout.EndHorizontal();
        GUILayout.Space(foldoutBottomSpace);

        if (showTypes)
        {
            EditorDrawer.DrawProperty(creature, customSkin, "Creature");
            EditorDrawer.DrawProperty(technology, customSkin, "Technology");
            EditorDrawer.DrawProperty(money, customSkin, "Money");
        }
        #endregion

        #region Card Stats       
        var cost = serializedObject.FindProperty("costValue");
        var attack = serializedObject.FindProperty("attackValue");
        var health = serializedObject.FindProperty("healthValue");
        var points = serializedObject.FindProperty("pointsValue");

        GUILayout.Space(foldoutTopSpace);
        GUILayout.BeginHorizontal();
        showStats = EditorGUILayout.Foldout(showStats, "Card Stats", true, foldoutStyle);
        showStats = GUILayout.Toggle(showStats, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));
        GUILayout.EndHorizontal();
        GUILayout.Space(foldoutBottomSpace);

        if (showStats)
        {
            EditorDrawer.DrawProperty(cost, customSkin, "Cost");
            EditorDrawer.DrawProperty(attack, customSkin, "Attack");
            EditorDrawer.DrawProperty(health, customSkin, "Health");
            EditorDrawer.DrawProperty(points, customSkin, "Points");
        }
        #endregion

        #region Highlights
        var defaultHighlight = serializedObject.FindProperty("defaultHighlight");
        var interactionPositiveHighlight = serializedObject.FindProperty("interactionPositiveHighlight");
        var InteractionNegativeHighlight = serializedObject.FindProperty("InteractionNegativeHighlight");
        var targetHighlight = serializedObject.FindProperty("targetHighlight");
        var triggerHighlight = serializedObject.FindProperty("triggerHighlight");
        var abilityHighlight = serializedObject.FindProperty("abilityHighlight");

        GUILayout.Space(foldoutTopSpace);
        GUILayout.BeginHorizontal();
        showHighlights = EditorGUILayout.Foldout(showHighlights, "Highlights", true, foldoutStyle);
        showHighlights = GUILayout.Toggle(showHighlights, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));
        GUILayout.EndHorizontal();
        GUILayout.Space(foldoutBottomSpace);

        if (showHighlights)
        {
            EditorDrawer.DrawProperty(defaultHighlight, customSkin, "Default Highlight");
            EditorDrawer.DrawProperty(interactionPositiveHighlight, customSkin, "Interaction Positive Highlight");
            EditorDrawer.DrawProperty(InteractionNegativeHighlight, customSkin, "Interaction Negative Highlight");
            EditorDrawer.DrawProperty(targetHighlight, customSkin, "Target Highlight");
            EditorDrawer.DrawProperty(triggerHighlight, customSkin, "Trigger Highlight");
            EditorDrawer.DrawProperty(abilityHighlight, customSkin, "Ability Highlight");
        }

        #endregion


        GUILayout.EndVertical();
        GUILayout.Space(foldoutItemSpace);

        // Apply changes
        serializedObject.ApplyModifiedProperties();
        Repaint();

        // Reset to defaults button
        // GUILayout.Space(12);
        // GUILayout.BeginHorizontal();

        if (GUILayout.Button(new GUIContent("Reset to defaults")))
            ResetToDefaults();

    }

    void ResetToDefaults()
    {
        if (! EditorUtility.DisplayDialog("Reset to defaults", "Are you sure you want to reset to default?", "Yes", "Cancel")) return;
        
        try
        {
            Preset defaultPreset = Resources.Load<Preset>("SorsDefaultColors");
            defaultPreset.ApplyTo(Resources.Load("Sors Colors"));
            Selection.activeObject = null;
            Debug.Log("<b>[Sors Colors]</b> Resetting is successful.");
        }

        catch { Debug.LogWarning("<b>[Sors Colors]</b> Resetting failed. Default preset seems to be missing"); }
        
    }
}