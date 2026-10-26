
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioIdentifier))]
[CanEditMultipleObjects]
public class AudioIdentifierEditor : Editor
{
    private SerializedProperty categoryProperty;
    private SerializedProperty uiEventProperty;
    private SerializedProperty sfxEventProperty;
    private SerializedProperty backgroundContextProperty;
    private SerializedProperty audioSourceProperty;

    private void OnEnable()
    {
        categoryProperty = serializedObject.FindProperty("category");
        uiEventProperty = serializedObject.FindProperty("uiEvent");
        sfxEventProperty = serializedObject.FindProperty("sfxEvent");
        backgroundContextProperty =
            serializedObject.FindProperty("backgroundContext");
        audioSourceProperty = serializedObject.FindProperty("audioSource");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField(
            "Identificación",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(categoryProperty);

        AudioCategory category =
            (AudioCategory)categoryProperty.enumValueIndex;

        switch (category)
        {
            case AudioCategory.UI:
                EditorGUILayout.PropertyField(
                    uiEventProperty,
                    new GUIContent("Evento UI")
                );
                break;

            case AudioCategory.SFX:
                EditorGUILayout.PropertyField(
                    sfxEventProperty,
                    new GUIContent("Evento SFX")
                );
                break;

            case AudioCategory.Background:
                EditorGUILayout.PropertyField(
                    backgroundContextProperty,
                    new GUIContent("Contexto de fondo")
                );
                break;
        }

        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField(
            "Audio",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            audioSourceProperty,
            new GUIContent("Audio Source")
        );

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(8);

        AudioIdentifier identifier = (AudioIdentifier)target;

        if (!identifier.IsValid)
        {
            EditorGUILayout.HelpBox(
                "Asigna un AudioClip al AudioSource y selecciona " +
                "un evento válido para registrar este identificador.",
                MessageType.Warning
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "El identificador tiene una configuración válida.",
                MessageType.Info
            );
        }
    }
}
