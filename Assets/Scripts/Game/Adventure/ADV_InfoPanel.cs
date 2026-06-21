using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ADV_InfoPanel : MonoBehaviour
{
    // GameObject.FindWithTag can't find this panel while it's hidden (SetActive(false)
    // at the end of every show), so callers like ADV_InfoTrigger use this singleton
    // instead — Awake() runs once regardless of later visibility toggles.
    public static ADV_InfoPanel Instance { get; private set; }

    [Header("General")]
    [SerializeField] private GameObject infoPanel;
    public bool isAutoClose = true;

    [Header("Text")]
    [SerializeField] private TMP_Text infoPanelText;

    [Header("Settings")]
    [SerializeField] private int timeToShowInfoPanel = 2;
    [SerializeField] private float hideDuration = 0.5f;
    [SerializeField] private float panelHeight = 340;
    [SerializeField] private Canvas canvasRoot;

    [Header("Button")]
    [SerializeField] private Button closeInfoPanel;

    private Coroutine infoRoutine;
    private Animator _animator;
    private Image panelImage;

    private void Awake()
    {
        Instance = this;

        closeInfoPanel.onClick.AddListener(CloseInfoPanel);

        if (canvasRoot == null)
            canvasRoot = infoPanel.GetComponentInParent<Canvas>();

        // infoPanel is this script's own GameObject — never SetActive(false) it, that
        // would disable this component and break StartCoroutine on every later show.
        // Visibility is instead controlled via alpha (already animated) + raycastTarget.
        panelImage = infoPanel.GetComponent<Image>();
        _animator  = infoPanel.GetComponent<Animator>();
    }

    /// <summary>
    /// Grows the panel by the top safe-area inset (notch/status bar) so its background
    /// bleeds under it, while the content (anchored to the bottom of the panel) stays
    /// fully visible. Called right before showing — Screen.safeArea can report a stale
    /// value for the first frames after scene load, so we recompute on each show instead
    /// of caching it once in Awake.
    /// </summary>
    private void ApplyPanelHeight()
    {
        if (canvasRoot == null)
            return;

        RectTransform rt = infoPanel.GetComponent<RectTransform>();
        if (rt == null)
            return;

        float scaleFactor = canvasRoot.scaleFactor;

        Rect safeArea = Screen.safeArea;
        float topInsetPx = Screen.height - (safeArea.y + safeArea.height);
        float topInset   = topInsetPx / scaleFactor;

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight + topInset);
    }

    public void ShowInfo(string text, int duration=2, bool autoClose=true)
    {
        if (string.IsNullOrEmpty(text))
            return;

        ApplyPanelHeight();

        if (infoRoutine != null)
            StopCoroutine(infoRoutine);

        timeToShowInfoPanel = duration;
        isAutoClose = autoClose;

        // Undo whatever the previous hide left behind, and play the appear animation
        // (dlg_pnl_idle -> dlg_pnl_in). Without this the Animator's default state would
        // be dlg_pnl_in itself, popping the panel up immediately on scene load.
        panelImage.raycastTarget = true;
        _animator.SetBool("closePanel", false);
        _animator.SetBool("openPanel", true);

        infoRoutine = StartCoroutine(ShowInfoRoutine(text));
    }

    private IEnumerator ShowInfoRoutine(string text)
    {
        // --- TYPEWRITER ---
        infoPanelText.text = text;
        infoPanelText.ForceMeshUpdate();
        infoPanelText.maxVisibleCharacters = 0;

        float charDelay = 0.03f; // speed (lower = faster)

        int totalChars = infoPanelText.textInfo.characterCount;

        for (int i = 0; i <= totalChars; i++)
        {
            infoPanelText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(charDelay);
        }

        // Stay open until CloseInfoPanel()/HideImmediately() is called externally
        // (e.g. the close button, or ADV_InfoTrigger's closeOnExit) — skip the
        // timed auto-hide below entirely.
        if (!isAutoClose)
        {
            infoRoutine = null;
            yield break;
        }

        Color color = panelImage.color; // cache initial color

        yield return new WaitForSeconds(timeToShowInfoPanel);

        hideDuration = 0.5f;
        float time = 0f;

        while (time < hideDuration)
        {
            time += Time.deltaTime;

            float t = time / hideDuration;
            t = 1f - Mathf.Pow(1f - t, 2f); // ease-out

            color.a = Mathf.Lerp(1f, 0f, t); // change ONLY alpha
            panelImage.color = color;

            yield return null;
        }

        //restore color
        color.a = 1f;
        panelImage.color = color;

        //run animation and hide
        _animator.SetBool("openPanel", false);
        _animator.SetBool("closePanel", true);
        yield return new WaitForSeconds(1f);

        panelImage.raycastTarget = false;
    }


    private void CloseInfoPanel()
    {
        HideImmediately();
    }

    /// <summary>
    /// Cancels any in-progress show and hides instantly, with no fade animation.
    /// Use this instead of SetActive(false) — the panel's GameObject must stay active
    /// for ShowInfo()'s coroutine to be able to run on the next call.
    /// </summary>
    public void HideImmediately()
    {
        if (infoRoutine != null)
        {
            StopCoroutine(infoRoutine);
            infoRoutine = null;
        }

        // Reset alpha to 1 (not 0!) — ShowInfo() never resets it on its own, it
        // expects the panel to already be at full alpha and relies on the Animator's
        // dlg_pnl_in clip for the visual appear effect. Leaving it at 0 here meant the
        // next ShowInfo() call would play the "open" animation on a fully transparent
        // image — looking exactly like it never showed up at all.
        Color color = panelImage.color;
        color.a = 1f;
        panelImage.color = color;
        panelImage.raycastTarget = false;

        // closePanel=true (not false!) — this is what actually drives the wired
        // dlg_pnl_in -> dlg_pnl_out -> dlg_pnl_idle transition. Setting both bools to
        // false leaves the Animator stuck in whatever state it was in (often dlg_pnl_in),
        // and if that clip animates alpha itself, it fights our color.a = 0 every frame,
        // making the panel appear to never actually close.
        _animator.SetBool("openPanel", false);
        _animator.SetBool("closePanel", true);
    }
}
