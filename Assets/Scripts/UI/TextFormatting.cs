using UnityEngine;

public class TextFormatting
{

    public static TextFormatting Instance { get; private set; } = new TextFormatting();

    private string name = "";

    private TextFormatting()
    {
        name = PlayerPrefs.GetString("PlayerName", "William Arthur");
    }

    public string GetText(string text)
    {
        // Format to replace names
        text = text.Replace("%name%", name);

        // More formatting can be added here
        return text;
    }
}
