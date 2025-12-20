using UnityEngine.SceneManagement;
using LTLRN.UI;
using UnityEngine.UI;

public class MainPanel : Panel
{
    public Button mode1Button;
    public Button mode2Button;

    public override void Initialize()
    {
        if (IsInitialized)
            return;

        mode1Button.onClick.AddListener(OpenAdventureMenu);
        mode2Button.onClick.AddListener(OpenExercisesMenu);

        base.Initialize();
    }

    private void OpenAdventureMenu()
    {
        SceneManager.LoadScene("AdventureMenu");
    }

    private void OpenExercisesMenu()
    {
        SceneManager.LoadScene("ExscersisesMenu");
    }
}
