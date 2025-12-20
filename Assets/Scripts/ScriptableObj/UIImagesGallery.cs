using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(
    fileName = "UIImagesGallery",
    menuName = "UI/Images Gallery"
)]
public class UIImagesGallery : ScriptableObject
{
    public Sprite[] userAvatar;

    public Sprite[] soundSprites;

    public Sprite[] musicSprites;

    public Sprite[] soundSpeedSprites;
}
