#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{
    private AudioDatas audioDatas;
    private string[] audioNames;

    private void OnEnable()
    {
        RefreshAudioNames();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawVisionBgmList(
            serializedObject.FindProperty("blueVisionBgmNames"),
            "Blue Vision Music");
        DrawVisionBgmList(
            serializedObject.FindProperty("redVisionBgmNames"),
            "Red Vision Music");
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("visionBgmFadeDuration"),
            new GUIContent("Fade Duration"));

        EditorGUILayout.Space();
        DrawPropertiesExcluding(
            serializedObject,
            "m_Script",
            "blueVisionBgmNames",
            "redVisionBgmNames",
            "visionBgmFadeDuration");

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawVisionBgmList(SerializedProperty listProperty, string label)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        if (audioNames.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "AudioDataListSO 中没有可选择的音乐。",
                MessageType.Warning);
        }

        for (int i = 0; i < listProperty.arraySize; i++)
        {
            SerializedProperty item = listProperty.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal();

            int currentIndex = FindAudioIndex(item.stringValue);
            int selectedIndex = EditorGUILayout.Popup($"Music {i + 1}", currentIndex, audioNames);
            if (audioNames.Length > 0 && selectedIndex >= 0)
                item.stringValue = audioNames[selectedIndex];

            if (GUILayout.Button("-", GUILayout.Width(24f)))
            {
                listProperty.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        using (new EditorGUI.DisabledScope(audioNames.Length == 0))
        {
            if (GUILayout.Button("+ Add Music"))
            {
                int newIndex = listProperty.arraySize;
                listProperty.InsertArrayElementAtIndex(newIndex);
                listProperty.GetArrayElementAtIndex(newIndex).stringValue = audioNames[0];
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void RefreshAudioNames()
    {
        audioDatas = Resources.Load<AudioDatas>("Data/AudioDataListSO");
        var names = new List<string>();

        if (audioDatas != null && audioDatas.audioDataList != null)
        {
            foreach (AudioData audioData in audioDatas.audioDataList)
            {
                if (audioData != null &&
                    !string.IsNullOrWhiteSpace(audioData.audioName) &&
                    !names.Contains(audioData.audioName))
                {
                    names.Add(audioData.audioName);
                }
            }
        }

        audioNames = names.ToArray();
    }

    private int FindAudioIndex(string audioName)
    {
        for (int i = 0; i < audioNames.Length; i++)
        {
            if (audioNames[i] == audioName)
                return i;
        }

        return 0;
    }
}
#endif
