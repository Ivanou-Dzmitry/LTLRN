using System.Text;

public static class AccentFormatter
{
    // accent color in TMP hex — change to match your theme
    private const string ACCENT_COLOR = "#E24B4A";        // red
    // private const string ACCENT_COLOR = "#1D9E75";    // teal
    // private const string ACCENT_COLOR = "#BA7517";    // amber

    public static string FormatAccents(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            char current = input[i];

            bool isAccent =
                current == '\u0300' ||
                current == '\u0301' ||
                current == '\u0303';

            if (isAccent) continue;

            bool nextIsAccent = i < input.Length - 1 && IsAccentChar(input[i + 1]);

            if (nextIsAccent)
            {
                // colored + bold + underline — clearly visible on any background
                sb.Append($"<color={ACCENT_COLOR}><b><u>{current}</u></b></color>");
            }
            else
            {
                sb.Append(current);
            }
        }

        return sb.ToString();
    }

    private static bool IsAccentChar(char c) =>
        c == '\u0300' || c == '\u0301' || c == '\u0303';
}
