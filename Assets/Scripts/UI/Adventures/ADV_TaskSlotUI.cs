using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ADV_TaskSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private TMP_Text descriptionText;

    public void SetupTask(ADV_Condition taskDef)
    {        
        descriptionText.text = taskDef.description.GetLocalizedString();

        if (taskDef.IsMet())
        {
            icon.sprite = sprites[1];
        }
        else
        {
            icon.sprite = sprites[0];
        }
    }
}
