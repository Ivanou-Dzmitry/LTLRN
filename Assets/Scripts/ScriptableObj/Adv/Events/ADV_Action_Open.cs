using UnityEngine;

[CreateAssetMenu(menuName = "ADV/Actions/Open Door")]
public class ADV_Action_OpenDoor : ADV_Action
{
    public GameObject door;

    public override void Execute()
    {
        if (door != null)
            door.SetActive(false); // или door.Open()
    }
}
