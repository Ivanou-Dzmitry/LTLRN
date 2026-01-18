using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class QImage01 : MonoBehaviour
{
    public GameObject imageCountBlock;
    public TMP_Text imagesCountText;

    private void Start()
    {
        Animator animator = GetComponent<Animator>();
        animator.SetFloat("animSpeed", Random.Range(0.5f, 2.0f));
        animator.Play("ScaleUpDown01", 0, Random.value);
    }


    public void ShowImagesCountText(string text)
    {
        if(text != null)
            imagesCountText.text = text;
    }

    private void OnDestroy()
    {
        imagesCountText.text = string.Empty;
    }

}
