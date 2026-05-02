using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonImage : MonoBehaviour
{
    public enum ButtonColor
    {
        Primary,
        Secondary,
        Accent,
        Success,
        Warning,
        Error,
        Disabled,
        Transparent,
        PrimaryLight,
        SuccessLight,
        Gray3Dark,
        Gray2Light
    }

    public enum TextColor
    {
        Primary,
        Secondary,
        Black,
        White,
        Disabled
    }

    public enum ButtonState
    {
        Normal,
        Disabled,
        Selected
    }

    public enum ButtonAnimation
    {
        Idle,
        Scale        
    }

    [Header("ButtonState")]
    public ButtonState currentState = ButtonState.Normal;

    [Header("Palette")]
    [SerializeField] private UIColorPalette palette;

    [Header("Button Color")]
    public ButtonColor selectedButtonColor = ButtonColor.Primary;

    [Header("Button Icon")]
    public Image buttonIcon;

    [Header("Text Color")]
    public TextColor selectedTextColor = TextColor.Primary;

    [Header("Text")]
    public string buttonTextStr = string.Empty;
    public TMP_Text buttonText;

    private Button button;
    private Image buttonImage;

    [Header("Animator")]
    [SerializeField] private Animator btnAnimator;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        ApplyAll();
    }

    private void ApplyAll()
    {
        //ApplyButtonColor();
        ApplyButtonText();
    }

    private void ApplyButtonText()
    {
        if (buttonText == null || palette == null)
            return;

        if (!string.IsNullOrEmpty(buttonTextStr))
            buttonText.text = buttonTextStr;

        buttonText.color = GetTextColor(selectedTextColor);
    }

    private void ApplyButtonColor(ButtonColor color)
    {
        if (buttonImage == null || palette == null)
            return;

        //set disabled colors
        if (!button.interactable)
        {
            buttonImage.color = palette.Gray2Light;
            buttonText.color = palette.Gray2Light;
            return;
        }

        if(currentState == ButtonState.Normal)
        {
            buttonImage.color = GetButtonColor(color);
        }
        else if(currentState == ButtonState.Selected)
        {
            buttonImage.color = GetButtonColor(color);
        }
            
    }

    private Color GetButtonColor(ButtonColor color)
    {
        return color switch
        {
            ButtonColor.Primary => palette.Primary,
            ButtonColor.Secondary => palette.Secondary,
            ButtonColor.Accent => palette.Accent,
            ButtonColor.Success => palette.Success,
            ButtonColor.Warning => palette.Warning,
            ButtonColor.Error => palette.Error,
            ButtonColor.Disabled => palette.DisabledButton,
            ButtonColor.Transparent => palette.TransparentPanel,
            ButtonColor.PrimaryLight => palette.PrimaryLight,
            ButtonColor.SuccessLight => palette.SuccessLight,
            ButtonColor.Gray3Dark => palette.Gray3Dark,
            ButtonColor.Gray2Light => palette.Gray2Light,            
            _ => palette.Primary
        };
    }

    private Color GetTextColor(TextColor color)
    {
        return color switch
        {
            TextColor.Primary => palette.TextPrimary,
            TextColor.Secondary => palette.TextSecondary,
            TextColor.Black => palette.Accent,
            TextColor.White => palette.TextSecondary,
            TextColor.Disabled => palette.Gray2Light,
            _ => palette.TextPrimary
        };
    }

    // Runtime API
    public void SetButtonColor(ButtonColor color)
    {
        selectedButtonColor = color;
        ApplyButtonColor(color);
    }

    public void SetTextColor(TextColor color)
    {
        selectedTextColor = color;
        ApplyButtonText();
    }

    public void SetIconColor(ButtonColor color)
    {
        buttonIcon.color = GetButtonColor(color);        
    }

    public void SetText(string text)
    {
        buttonText.text = text;
        ApplyButtonText();
    }

    public void SetDisabled(bool disabled)
    {
        button.interactable = !disabled;
        currentState = disabled ? ButtonState.Disabled : ButtonState.Normal;

        if (disabled)
        {
            SetIconColor(ButtonColor.Gray2Light);
            SetTextColor(TextColor.Disabled);
        }
        else
        {
            SetIconColor(ButtonColor.Gray3Dark);
            SetTextColor(TextColor.Black);
        }
            

        //ApplyButtonColor();
    }

    public void SetSelected(bool selected)
    {
        currentState = selected ? ButtonState.Selected : ButtonState.Normal;
        //ApplyButtonColor();
    }

    public void RefreshState()
    {
        ApplyAll();
    }

    public void PlayAnimation(bool play, string triggerName)
    {
        if (btnAnimator != null)
        {
            if (play)
            {
                btnAnimator.enabled = true;
                btnAnimator.ResetTrigger(triggerName);
                btnAnimator.SetTrigger(triggerName);
            }
            else
            {
                btnAnimator.enabled = false;
            }                
        }
    }
}
