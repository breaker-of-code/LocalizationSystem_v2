using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class LocalizationEditorWindow : EditorWindow
{
    private string newLanguageName = "";
    private string newKey = "";
    private Dictionary<string, string> translations = new Dictionary<string, string>();
    private string[] languages = new string[] { };
    private int selectedTab = 0;
    private string searchKey = "";
    private Dictionary<string, string> searchTranslations = new Dictionary<string, string>();
    private bool noLocalizationFound = false;

    [MenuItem("LocalizationEditor/Editor")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationEditorWindow>("Localization Editor");
    }

    private void OnGUI()
    {
        selectedTab = GUILayout.Toolbar(selectedTab, new string[] { "Add Language", "Manage Languages", "Update Translations" });

        GUILayout.Space(10);

        if (selectedTab == 0)
        {
            DrawAddLanguageTab();
        }
        else if (selectedTab == 1)
        {
            DrawManageLanguagesTab();
        }
        else if (selectedTab == 2)
        {
            DrawUpdateTranslationsTab();
        }
    }

    private void DrawAddLanguageTab()
    {
        GUILayout.Label("Add New Language", EditorStyles.boldLabel);

        newLanguageName = EditorGUILayout.TextField("Language Name", newLanguageName);

        if (GUILayout.Button("Add Language"))
        {
            if (!string.IsNullOrEmpty(newLanguageName))
            {
                CreateLocalizationFolder();
                CreateLanguageScriptableObject(newLanguageName);
                newLanguageName = string.Empty;
                GUI.FocusControl(null);
            }
        }

        GUILayout.Space(20);

        GUILayout.Label("Add New Localization", EditorStyles.boldLabel);

        newKey = EditorGUILayout.TextField("Localization Key", newKey);

        languages = GetAllLanguageNames();
        foreach (var language in languages)
        {
            if (!translations.ContainsKey(language))
            {
                translations[language] = "";
            }
            translations[language] = EditorGUILayout.TextField($"{language} Translation", translations[language]);
        }

        if (GUILayout.Button("Add Localization"))
        {
            AddLocalizationToAllLanguages(newKey, translations);
            newKey = ""; 
            ClearTranslations(); 
        }
    }

    private void DrawManageLanguagesTab()
    {
        GUILayout.Label("Manage Languages", EditorStyles.boldLabel);

        languages = GetAllLanguageNames();
        foreach (var language in languages)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(language, GUILayout.Width(200));

            if (GUILayout.Button("Delete", GUILayout.Width(50)))
            {
                if (EditorUtility.DisplayDialog("Delete Language",
                    $"Are you sure you want to delete the language '{language}'? This action cannot be undone.",
                    "Yes", "No"))
                {
                    DeleteLanguage(language);
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    private void DrawUpdateTranslationsTab()
    {
        GUILayout.Label("Update Translations", EditorStyles.boldLabel);

        searchKey = EditorGUILayout.TextField("Search Key", searchKey);

        if (GUILayout.Button("Search"))
        {
            FetchTranslations(searchKey);
        }

        GUILayout.Space(20);

        if (noLocalizationFound)
        {
            GUILayout.Label("No Localization found", EditorStyles.boldLabel);
        }
        else
        {
            foreach (var language in languages)
            {
                if (searchTranslations.ContainsKey(language))
                {
                    searchTranslations[language] = EditorGUILayout.TextField($"{language} Translation", searchTranslations[language]);
                }
            }

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save"))
            {
                UpdateTranslations(searchKey, searchTranslations);
                searchKey = "";
                searchTranslations.Clear();
            }

            if (GUILayout.Button("Cancel"))
            {
                searchKey = "";
                searchTranslations.Clear();
            }
            GUILayout.EndHorizontal();
        }
    }

    private void CreateLocalizationFolder()
    {
        string localizationFolderPath = "Assets/Localization";

        if (!Directory.Exists(localizationFolderPath))
        {
            Directory.CreateDirectory(localizationFolderPath);
            AssetDatabase.Refresh();
            Debug.Log("Created Localization folder in Assets.");
        }
    }

    private void CreateLanguageScriptableObject(string languageName)
    {
        if (string.IsNullOrEmpty(languageName))
        {
            Debug.LogWarning("Language name cannot be empty.");
            return;
        }

        string path = $"Assets/Localization/{languageName}Localization.asset";

        if (File.Exists(path))
        {
            Debug.LogWarning($"Localization scriptable object for language '{languageName}' already exists.");
            return;
        }

        LocalizationScriptableObject newLanguage = ScriptableObject.CreateInstance<LocalizationScriptableObject>();
        AssetDatabase.CreateAsset(newLanguage, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Created new localization scriptable object for language: {languageName}");
    }

    private string[] GetAllLanguageNames()
    {
        string[] guids = AssetDatabase.FindAssets("t:LocalizationScriptableObject");
        string[] names = new string[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            LocalizationScriptableObject obj = AssetDatabase.LoadAssetAtPath<LocalizationScriptableObject>(path);
            names[i] = obj.name.Replace("Localization", "");
        }

        return names;
    }

    private void AddLocalizationToAllLanguages(string key, Dictionary<string, string> translations)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("Localization key cannot be empty.");
            return;
        }

        foreach (var language in translations.Keys)
        {
            string translation = translations[language];

            if (string.IsNullOrEmpty(translation))
                continue;

            string path = $"Assets/Localization/{language}Localization.asset";
            LocalizationScriptableObject languageSO = AssetDatabase.LoadAssetAtPath<LocalizationScriptableObject>(path);

            if (languageSO != null)
            {
                languageSO.AddEntry(key, translation);
                EditorUtility.SetDirty(languageSO);
                AssetDatabase.SaveAssets();
                Debug.Log($"Added localization for key '{key}' in language '{language}'.");
            }
            else
            {
                Debug.LogError($"Localization scriptable object for language '{language}' not found.");
            }
        }
    }

    private void ClearTranslations()
    {
        foreach (var language in languages)
        {
            translations[language] = "";
        }
    }

    private void DeleteLanguage(string languageName)
    {
        string path = $"Assets/Localization/{languageName}Localization.asset";
        if (File.Exists(path))
        {
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();
            Debug.Log($"Deleted localization scriptable object for language: {languageName}");
        }
        else
        {
            Debug.LogError($"Localization scriptable object for language '{languageName}' not found.");
        }
    }

    private void FetchTranslations(string key)
    {
        searchTranslations.Clear();
        languages = GetAllLanguageNames();
        noLocalizationFound = true;

        foreach (var language in languages)
        {
            string path = $"Assets/Localization/{language}Localization.asset";
            LocalizationScriptableObject languageSO = AssetDatabase.LoadAssetAtPath<LocalizationScriptableObject>(path);

            if (languageSO != null)
            {
                LocalizationEntry entry = languageSO.entries.Find(e => e.key == key);
                if (entry != null)
                {
                    searchTranslations[language] = entry.translation;
                    noLocalizationFound = false;
                }
            }
        }
    }

    private void UpdateTranslations(string key, Dictionary<string, string> translations)
    {
        foreach (var language in translations.Keys)
        {
            string translation = translations[language];
            string path = $"Assets/Localization/{language}Localization.asset";
            LocalizationScriptableObject languageSO = AssetDatabase.LoadAssetAtPath<LocalizationScriptableObject>(path);

            if (languageSO != null)
            {
                LocalizationEntry entry = languageSO.entries.Find(e => e.key == key);
                if (entry != null)
                {
                    entry.translation = translation;
                }
                else
                {
                    languageSO.AddEntry(key, translation);
                }
                EditorUtility.SetDirty(languageSO);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Updated translations for key '{key}'.");
    }
}
