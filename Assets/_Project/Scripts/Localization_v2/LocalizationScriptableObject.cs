using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LocalizationEntry
{
    public string key;
    public string translation;
}

[CreateAssetMenu(fileName = "NewLocalization", menuName = "Localization/Language")]
public class LocalizationScriptableObject : ScriptableObject
{
    public List<LocalizationEntry> entries = new List<LocalizationEntry>();

    public void AddEntry(string key, string translation)
    {
        LocalizationEntry entry = entries.Find(e => e.key == key);
        if (entry != null)
        {
            entry.translation = translation;
        }
        else
        {
            entries.Add(new LocalizationEntry { key = key, translation = translation });
        }
    }

    public List<LocalizationEntry> SearchEntries(string searchTerm)
    {
        return entries.FindAll(e => e.key.Contains(searchTerm) || e.translation.Contains(searchTerm));
    }
}