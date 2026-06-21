using UnityEngine;
using UnityEngine.Localization;

/// <summary>
/// Attach to any GameObject with a Collider2D (trigger or solid). When the player
/// touches or enters it, shows a localized message in the shared ADV_InfoPanel.
/// </summary>
public class ADV_InfoTrigger : MonoBehaviour
{
    [Header("Info text localized")]
    public LocalizedString description;

    [Header("Duration")]
    [SerializeField] private int duration = 2;

    [Header("Behaviour")]
    [Tooltip("If true, the info only shows the first time the player touches this collider.")]
    [SerializeField] private bool triggerOnce = false;

    [SerializeField] private bool closeOnExit = false;

    private bool hasTriggered;

    // Works whether the collider on this GameObject is a trigger...
    private void OnTriggerEnter2D(Collider2D other)
    {
        TryShow(other.GetComponent<Player>());
    }

    // ...or a solid collider.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryShow(collision.collider.GetComponent<Player>());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TryClose(other.GetComponent<Player>());
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        TryClose(collision.collider.GetComponent<Player>());
    }

    private void TryClose(Player player)
    {
        if (!closeOnExit || player == null)
            return;

        // Closes immediately on exit — the panel's own duration/auto-hide timer is
        // bypassed entirely in this case.
        ADV_InfoPanel.Instance?.HideImmediately();
    }

    private void TryShow(Player player)
    {
        if (player == null)
            return;

        if (triggerOnce && hasTriggered)
            return;

        if (ADV_InfoPanel.Instance == null)
        {
            Debug.LogWarning($"[ADV_InfoTrigger] '{name}': ADV_InfoPanel.Instance not set yet.");
            return;
        }

        hasTriggered = true;

        // When closeOnExit is on, the timer shouldn't matter — the panel should stay
        // open until the player actually leaves the collider.
        ADV_InfoPanel.Instance.ShowInfo(description.GetLocalizedString(), duration, autoClose: !closeOnExit);
    }
}
