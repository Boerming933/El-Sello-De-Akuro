using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueBattle : MonoBehaviour
{
    //public GameObject indicadorInteraccion;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text NPCName;
    public string nameTxt;
    public GameObject e;
    public GameObject hud;
    [SerializeField, TextArea(4, 8)] private string[] dialogueLines;

    private bool isPlayerInRange = true;
    private bool didDialogueStart;
    private int lineIndex;
    public float typingSpeed = 0.06f;

    private AudioSource audioSource;
    [SerializeField] private AudioClip NPCVoice;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }


    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!didDialogueStart)
            {
                StartDialogue();

            }
            else if (dialogueText.text == dialogueLines[lineIndex])
            {
                NextDialogueLine();
            }
            else
            {
                StopAllCoroutines();
                dialogueText.text = dialogueLines[lineIndex];
            }
        }
    }

    private void StartDialogue()
    {
        didDialogueStart = true;
        dialoguePanel.SetActive(true);
        hud.SetActive(false);
        e.SetActive(false);
        NPCName.text = nameTxt;
        lineIndex = 0;
        Time.timeScale = 0f;
        //audioSource.PlayOneShot(NPCVoice);          //Borrar esta wea cuando se use el script en otros NPCs XD
        StartCoroutine(ShowLine());
    }

    private void NextDialogueLine()
    {
        lineIndex++;

        if (lineIndex < dialogueLines.Length)
        {
            StartCoroutine(ShowLine());
        }
        else
        {
            dialoguePanel.SetActive(false);
            didDialogueStart = false;
            hud.SetActive(true);
            Time.timeScale = 1f;
        }
    }

    private IEnumerator ShowLine()
    {
        dialogueText.text = string.Empty;
        foreach (char letter in dialogueLines[lineIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }

   
}
