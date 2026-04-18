using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

public abstract class ADV_Action : ScriptableObject
{
    public abstract void Execute();

    [Header("Localization")]
    public LocalizedString description;

    public async Task<string> GetDescriptionAsync(string fallback = "")
        => await description.GetLocalizedStringAsync().Task;

    public string GetDescriptionSync(string fallback = "")
        => description.GetLocalizedString();
}
