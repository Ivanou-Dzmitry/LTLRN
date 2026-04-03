using UnityEngine;

public class ADV_InteractionManager : MonoBehaviour
{
    public string GetInteraction(Collider2D collider)
    {
        ADV_Interaction interaction = collider.GetComponent<ADV_Interaction>();

        if (interaction == null)
            return "null";

        return interaction.interactionType.ToString();
    }
}
