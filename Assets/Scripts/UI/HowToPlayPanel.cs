using UnityEngine;
using LTLRN.UI;
using UnityEngine.UI;

public class HowToPlayPanel : Panel
{
    public Button openSiteButton;

    public override void Initialize()
    {
        if (IsInitialized)
            return;

        if (openSiteButton != null)
            openSiteButton.onClick.AddListener(OpenSite);

        base.Initialize();
    }

    public void OpenSite()
    {
        Application.OpenURL("https://forms.gle/ymN8Bg9UsPLd2tJX9");
    }
}
