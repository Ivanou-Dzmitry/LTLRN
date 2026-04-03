using UnityEngine;

public class ADV_Interaction : MonoBehaviour
{
    public enum InteractionType
    {
        None,
        Collectible,
        Enemy
    }

    public InteractionType interactionType;
}
