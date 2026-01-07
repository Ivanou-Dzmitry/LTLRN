using LTLRN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//name: login

public class SignOutMenu : Panel
{
    [SerializeField] private Button logoutButton = null;
    [SerializeField] private TMP_Text nameText = null;

    private GameData gameData;

    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        logoutButton.onClick.AddListener(SignOut);
        base.Initialize();
    }

    public override void Open()
    {
        //UpdatePlayerNameUI();
        base.Open();        
    }

    private void SignOut()
    {
        MenuManager.Singleton.SignOut();
    }

    public void UpdatePlayerNameUI(string playerName)
    {        
/*        if(playerName.Length == 0)
            playerName = AuthenticationService.Instance.PlayerName;*/

        //Debug.Log("Player Name: " + playerName);        

        nameText.text = playerName;       
    }

}