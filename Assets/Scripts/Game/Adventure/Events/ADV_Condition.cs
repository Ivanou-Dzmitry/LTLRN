using UnityEngine;
using UnityEngine.Localization;
using System.Threading.Tasks;


public abstract class ADV_Condition : ScriptableObject
{
    public abstract bool IsMet();

    [Header("Localization")]
    public LocalizedString description;

    public async Task<string> GetDescriptionAsync(string fallback = "")
        => await description.GetLocalizedStringAsync().Task;

    public string GetDescriptionSync(string fallback = "")
        => description.GetLocalizedString();

}
