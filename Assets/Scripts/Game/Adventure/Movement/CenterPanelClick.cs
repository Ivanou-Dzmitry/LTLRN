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
        float now = Time.unscaledTime;
        bool isDoubleClick = (now - _lastClickTime) <= doubleClickThreshold;
        _lastClickTime = now;

        Vector3 worldPos = _cam.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0f;

        clickMarker.transform.position = worldPos;
        clickMarker.SetActive(true);

        player.SetTarget(worldPos, isDoubleClick);
    }

    public void HideMarker()
    {
        clickMarker.SetActive(false);
    }
}
