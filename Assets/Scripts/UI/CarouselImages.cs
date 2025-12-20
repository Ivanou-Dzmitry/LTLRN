using UnityEngine;
using UnityEngine.UI;

public class CarouselImages : MonoBehaviour
{
    private GameData gameData;

    [Header("Data")]
    [SerializeField] private UIImagesGallery imagesGallery;

    [Header("Avatar UI")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    private int currentIndex;

    private void Start()
    {
        prevButton.onClick.AddListener(PreviousAvatar);
        nextButton.onClick.AddListener(NextAvatar);
    }

    public void LoadAvatar()
    {
        gameData = GameObject.FindWithTag("GameData").GetComponent<GameData>();

        if (gameData == null || imagesGallery == null || imagesGallery.userAvatar.Length == 0)
            return;

        currentIndex = Mathf.Clamp(
            gameData.saveData.playerIconIndex,
            0,
            imagesGallery.userAvatar.Length - 1
        );

        ApplyAvatar();
    }

    private void ApplyAvatar()
    {
        avatarImage.sprite = imagesGallery.userAvatar[currentIndex];
        gameData.saveData.playerIconIndex = currentIndex;
    }

    private void NextAvatar()
    {
        currentIndex++;
        if (currentIndex >= imagesGallery.userAvatar.Length)
            currentIndex = 0;

        ApplyAvatar();
    }

    private void PreviousAvatar()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = imagesGallery.userAvatar.Length - 1;

        ApplyAvatar();
    }
}
