using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour, IPointerDownHandler
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        public string line;
    }

    [Header("Drag your dialogue .txt file here")]
    public TextAsset dialogueFile;

    public DialogueLine[] dialogueLines;
    public Image left;
    public Image right;
    public Image dialogBox;

    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;

    private string currentLine = "";
    private float charDisplayInterval = 0.02f;
    private float charDisplayTimer = 0f;

    public int currentLineIndex = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var rawLines = dialogueFile.text.Split('\n');
        dialogueLines = new DialogueLine[rawLines.Length];

        for (int i = 0; i < rawLines.Length; i++)
        {
            var parts = rawLines[i].Split(": ");
            if (parts.Length >= 2)
            {
                dialogueLines[i] = new DialogueLine
                {
                    speaker = parts[0].Trim(),
                    line = parts[1].Trim()
                };
            }
        }

        AdvanceDialogue();
    }

    void Update()
    {
        if (currentLineIndex < dialogueLines.Length)
        {
            if (dialogueText.text.Length < currentLine.Length)
            {
                charDisplayTimer += Time.deltaTime;
                if (charDisplayTimer >= charDisplayInterval)
                {
                    dialogueText.SetText(currentLine.Substring(0, dialogueText.text.Length + 1));
                    charDisplayTimer = 0f;
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (dialogueText.text.Length < currentLine.Length)
        {
            dialogueText.SetText(currentLine);
        }
        else
        {
            AdvanceDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        currentLineIndex++;
        if (currentLineIndex < dialogueLines.Length)
        {
            charDisplayTimer = 0f;
            dialogueText.SetText("");
            var line = dialogueLines[currentLineIndex];
            if (line.speaker == "%name%")
            {
                left.color = Color.white;
                right.color = Color.gray;
            }
            else if (line.speaker == "Scene")
            {
                left.color = Color.gray;
                right.color = Color.gray;
            }
            else
            {
                left.color = Color.gray;
                right.color = Color.white;
            }

            speakerNameText.SetText(TextFormatting.Instance.GetText(line.speaker));
            currentLine = TextFormatting.Instance.GetText(line.line);
        }
        else
        {
            // End of dialogue
            Debug.Log("End of dialogue");
            left.enabled = false;
            right.enabled = false;
            dialogBox.enabled = false;
            speakerNameText.enabled = false;
            dialogueText.enabled = false;
        }
    }
}
