using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EX_ProgressButton : MonoBehaviour
{
    public Button button;
    public RectTransform buttonRT;
    public Image background;
    public Image progressBar;
    public UIColorPalette palette;
    private float targetWidth;
    private Coroutine progressRoutine;
    public event Action OnProgressClicked;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        button.onClick.AddListener(OnClicked);
    }

    void Start()
    {              
        //set color
        progressBar.color = palette.SuccessLight;
        background.color = palette.Primary;
    }

    public void StartProgress(float growthTime)
    {
        RectTransform rt = progressBar.rectTransform;

        // Store the original width from the layout / prefab
        targetWidth = buttonRT.sizeDelta.x;

        // Optional: set start width
        rt.sizeDelta = new Vector2(50f, rt.sizeDelta.y);

        progressRoutine = StartCoroutine(GrowWidth(rt, targetWidth, growthTime));
    }


    private IEnumerator GrowWidth(RectTransform rt, float targetWidth, float growthTime)
    {
        float elapsed = 0f;

        while (elapsed < growthTime)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / growthTime;
            float newWidth = Mathf.Lerp(0f, targetWidth, t);

            rt.sizeDelta = new Vector2(newWidth, rt.sizeDelta.y);
            yield return null;
        }

        // Snap to exact width
        rt.sizeDelta = new Vector2(targetWidth, rt.sizeDelta.y);
    }

    public void OnClicked()
    {
        // Stop visual progress
        if (progressRoutine != null)
            StopCoroutine(progressRoutine);

        // Notify owner
        OnProgressClicked?.Invoke();
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClicked);
    }

}
