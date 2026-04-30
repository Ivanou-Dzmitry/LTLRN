// SafeAreaFitter.cs — attach to your root panel RectTransform
using UnityEngine;

public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rt;
    private Rect lastSafeArea;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        Apply();
    }

    void Apply()
    {
        Rect safe = Screen.safeArea;

        if (safe == lastSafeArea) return;
        lastSafeArea = safe;

        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Debug.Log($"[SafeAreaFitter] Applied: min={anchorMin} max={anchorMax}");
    }
}
