using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IconEntry
{
    public string id;
    public Sprite icon;
}

public class ADV_IconsManager : MonoBehaviour
{
    [SerializeField] private List<IconEntry> icons;

    private Dictionary<string, Sprite> iconMap;

    private void Awake()
    {
        iconMap = new Dictionary<string, Sprite>();

        foreach (var entry in icons)
        {
            if (!iconMap.ContainsKey(entry.id))
            {
                iconMap.Add(entry.id, entry.icon);
            }
            else
            {
                Debug.LogWarning($"Duplicate icon id: {entry.id}");
            }
        }
    }

    public Sprite GetIcon(string id)
    {
        if (iconMap.TryGetValue(id, out var icon))
            return icon;

        Debug.LogWarning($"Icon not found: {id}");
        return null;
    }
}
