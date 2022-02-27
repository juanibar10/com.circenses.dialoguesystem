using System;
using UnityEditor;
using UnityEngine;

namespace DialogueSystem.Editor
{
    public enum Window
    {
        Scene,
        Game,
        Inspector,
        Project,
        Hierarchy,
        Console
    }

    public class DialogueSystemSettings : ScriptableObject
    {
        public const string DialogueSettingsPath = "Packages/com.circenses.dialoguesystem/Editor/DialogueSystemSettings.asset";

        [Header("Dock to Window")]
        public Window selectedWindowEnum;
        public Type selectedWindow
        {
            get => GetWindow(selectedWindowEnum);
            private set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                selectedWindow = GetWindow(selectedWindowEnum);
            }
        }

        [Header("Auto Save")]
        public bool autoSaveDafault;
        
        [Header("Last File")]
        public DialogueContainer lastOpenFile;

        private static DialogueSystemSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<DialogueSystemSettings>(DialogueSettingsPath);
            if (settings != null) return settings;

            
            settings.autoSaveDafault = true;
            settings.lastOpenFile = null;
            settings.selectedWindowEnum = Window.Scene;
            settings.selectedWindow = GetWindow(settings.selectedWindowEnum);
            
            settings = CreateInstance<DialogueSystemSettings>();
            AssetDatabase.CreateAsset(settings, DialogueSettingsPath);
            AssetDatabase.SaveAssets();

            return settings;
        }

        private static Type GetWindow(Window window)
        {
            return window switch
            {
                Window.Scene => Type.GetType("UnityEditor.SceneView,UnityEditor.dll"),
                Window.Game => Type.GetType("UnityEditor.GameView,UnityEditor.dll"),
                Window.Inspector => Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll"),
                Window.Project => Type.GetType("UnityEditor.ProjectBrowser,UnityEditor.dll"),
                Window.Hierarchy => Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor.dll"),
                Window.Console => Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll"),
                _ => typeof(SceneView)
            };
        }
        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    internal static class DialogueSystemSettingsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new SettingsProvider("Project/DialogueSystemIMGUISettings", SettingsScope.Project)
            {
                label = "Dialogue System",
                guiHandler = (_) =>
                {
                    var settings = DialogueSystemSettings.GetSerializedSettings();
                    settings.Update();

                    EditorGUILayout.PropertyField(settings.FindProperty("autoSaveDafault"),
                        new GUIContent("Default value"));
                    EditorGUILayout.PropertyField(settings.FindProperty("selectedWindowEnum"),
                        new GUIContent("Window"));
                    EditorGUILayout.PropertyField(settings.FindProperty("lastOpenFile"),
                        new GUIContent("File"));

                    settings.ApplyModifiedProperties();
                }
            };

            return provider;
        }
    }
}
