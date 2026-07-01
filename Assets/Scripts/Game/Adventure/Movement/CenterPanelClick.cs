using UnityEngine;
using UnityEngine.EventSystems;

public class CenterPanelClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Player player;
    [SerializeField] private GameObject clickMarker;

    [Tooltip("Maximum seconds between two clicks to count as a double-click.")]
    [SerializeField] private float doubleClickThreshold = 0.3f;

    private Camera _cam;
    private float _lastClickTime = -1f;

    private void Awake()
    {
        _cam = Camera.main;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector3 worldPos = _cam.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0f;

        // Log every collider found at this point so we can see what's blocking/interfering.
        var allHits = Physics2D.OverlapPointAll(worldPos);
        if (allHits.Length > 0)
        {
            var names = new System.Text.StringBuilder("[CenterPanelClick] overlaps at ")
                .Append(worldPos).Append(": ");
            foreach (var h in allHits)
                names.Append(h.gameObject.name).Append('(').Append(h.GetType().Name).Append(") ");
            Debug.Log(names);
        }
        else
        {
            Debug.Log($"[CenterPanelClick] no overlaps at {worldPos} → moving to target");
        }

        foreach (var hit in allHits)
        {
            var btn = hit.GetComponent<ADV_OnScreenButton>();
            if (btn != null)
            {
                Debug.Log($"[CenterPanelClick] → ADV_OnScreenButton found on '{hit.gameObject.name}', invoking onClick");
                btn.onClick?.Invoke();
                return;
            }
        }

        float now = Time.unscaledTime;
        bool isDoubleClick = (now - _lastClickTime) <= doubleClickThreshold;
        _lastClickTime = now;

        bool moved = player.SetTarget(worldPos, isDoubleClick);

        Debug.Log($"[CenterPanelClick] → SetTarget({worldPos}) = {moved}");

        clickMarker.transform.position = worldPos;
        clickMarker.SetActive(moved);
    }

    public void HideMarker()
    {
        clickMarker.SetActive(false);
    }
}
