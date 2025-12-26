using LTLRN.UI;
using System;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

//name: auth

public class AuthentificationMenu : Panel
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField usernameInput = null;
    [SerializeField] private TMP_InputField passwordInput = null;

    [Header("Buttons")]
    [SerializeField] private Button signinButton = null;
    [SerializeField] private Button signupButton = null; 
    [SerializeField] private Button anonymousButton = null;

    private ButtonImage signInBtn;
    private ButtonImage signUpBtn;

    private string userName = string.Empty;
    private string password = string.Empty;


    public override void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        //buttons
        signinButton.onClick.AddListener(SignIn);
        signupButton.onClick.AddListener(SignUp);
        anonymousButton.onClick.AddListener(AnonymousSignIn);

        usernameInput.onValueChanged.AddListener(OnUsernameChanged);
        passwordInput.onValueChanged.AddListener(OnPasswordChanged);

        base.Initialize();
    }

    public override void Open()
    {
        usernameInput.text = "";
        passwordInput.text = "";

        userName = usernameInput.text;
        password = passwordInput.text;

        signInBtn = signinButton.GetComponent<ButtonImage>();
        signUpBtn = signupButton.GetComponent<ButtonImage>();

        signInBtn.SetButtonColor(ButtonImage.ButtonColor.Disabled);
        signinButton.interactable = false;

        signUpBtn.SetButtonColor(ButtonImage.ButtonColor.Disabled);
        signupButton.interactable = false;

        base.Open();
    }



    private void AnonymousSignIn()
    {
        MenuManager.Singleton.SignInAnonymouslyAsync();
    }

    private void SignIn()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            return;

        try
        {            
            MenuManager.Singleton.SignInWithUsernameAndPasswordAsync(username, password);
        }
        catch (Exception exception)
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            string uiMessage = "Failed to sign in.\n Error: " + (exception.Message);
            panel.Open(ErrorMenu.Action.None, uiMessage, "OK");
        }


            /*        if (string.IsNullOrEmpty(username) == false && string.IsNullOrEmpty(password)== false)
                    {
                        MenuManager.Singleton.SignInWithUsernameAndPasswordAsync(username, password);
                    }    */
    }

    private void SignUp()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        Debug.Log($"SignUp attempt with username: {username} and password: {password}");

        if (string.IsNullOrEmpty(username) == false && string.IsNullOrEmpty(password) == false)
        {
            if(IsPasswordValid(password))
            {
                MenuManager.Singleton.SignUpWithUsernameAndPasswordAsync(username, password);
            }
            else
            {
                ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
                //Password must be 8-30 characters long and include at least one uppercase letter, one lowercase letter, one digit, and one symbol. Error8
                string errText = LocalizationSettings.StringDatabase.GetLocalizedString("LTLRN", "Error08");
                panel.Open(ErrorMenu.Action.None, errText, "OK");
            }
        }
    }

    private bool IsPasswordValid(string password)
    {
        if (password.Length < 8 || password.Length > 30)
        {
            return false;
        }

        bool hasUppercase = false;
        bool hasLowercase = false;
        bool hasDigit = false;
        bool hasSymbol = false;

        foreach (char c in password)
        {
            if (char.IsUpper(c))
            {
                hasUppercase = true;
            }

            else if (char.IsLower(c))
            {
                hasLowercase = true;
            }

            else if (char.IsDigit(c))
            {
                hasDigit = true;
            }

            else if (!char.IsLetterOrDigit(c))
            {
                hasSymbol = true;
            }
        }

        return hasUppercase && hasLowercase && hasDigit && hasSymbol;
    }

    private void OnUsernameChanged(string value)
    {
        userName = value;
        ValidateForm();
    }

    private void OnPasswordChanged(string value)
    {
        password = value;
        ValidateForm();
    }

    private void ValidateForm()
    {
        bool isValid =
            userName.Length >= 3 &&
            password.Length >= 8;

        signinButton.interactable = isValid;
        signupButton.interactable = isValid;

        var color = isValid
            ? ButtonImage.ButtonColor.Primary
            : ButtonImage.ButtonColor.Disabled;

        signInBtn.SetButtonColor(color);
        signUpBtn.SetButtonColor(color);
    }

}
