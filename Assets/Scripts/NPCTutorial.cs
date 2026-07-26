using Oculus.Avatar2;
using Oculus.Interaction.Input;
using Oculus.Interaction.Locomotion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum TutorialState
{
    Idle,
    Dialogue,
    WaitingForSpeech,
    AskingRetry,
    Finished
}
public class NPCTutorial : MonoBehaviour
{
    public GameObject dialogueCanvas;
    public GameObject retryCanvas;
    public TMP_Text dialogueText;
    public Animator plantAnimator;
    public SpeechToText STT;

    public InputActionReference X;
    public InputActionReference Y;
    public InputActionReference LeftJoystick;
    public InputActionReference RightJoystick;

    [SerializeField] private List<string> StartingLines;
    [SerializeField] private List<string> CompletedLines;
    [SerializeField] private List<string> RetryLines;

    private bool received = false;

    [Header("Player")]
    public GameObject playerController;
    public Rigidbody PlayerBody;
    public Transform player;
    public Transform npc;

    private List<string> dialogueLines;
    private int currentLine;
    private string currentText;
    private string fullText;
    private int charInd;
    private bool isTyping = false;
    

    private TutorialState state = TutorialState.Idle;
    void Start()
    {
        STT.OnSpeechFinished += HandleSpeech;
        Y.action.Enable();
        Y.action.performed += (ctx) => Continue();

        X.action.Enable();
        X.action.performed += (ctx) => restart();

        plantAnimator.gameObject.SetActive(false);
        dialogueCanvas.SetActive(false);
        retryCanvas.SetActive(false);
    }

    private void OnDestroy()
    {
        STT.OnSpeechFinished -= HandleSpeech;
    }

    void Update()
    {
        if (isTyping)
        {
            if (charInd < fullText.Length)
            {
                currentText = fullText.Substring(0,charInd);
                dialogueText.text = currentText;
                charInd += 1;
            }
            else
            {
                isTyping = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (state == TutorialState.Idle)
            {
                FreezePlayer();
                dialogueLines = StartingLines;
                StartDialogue();
            }
        }
    }

    public void StartDialogue()
    {
        Debug.Log($"TUTORIAL starting dialogue lines {dialogueLines.Count}");
        currentLine = 0;
        StartLine();
        dialogueCanvas.SetActive(true);
    }

    private void StartLine()
    {
        Debug.Log($"TUTORIAL starting line {currentLine}");
        currentText = "";
        dialogueText.text = currentText;
        fullText = dialogueLines[currentLine];
        charInd = 0;
        isTyping = true;
        state = TutorialState.Dialogue;
    }

    public void Continue()
    {
        if (state == TutorialState.Dialogue)
        {
            Debug.Log($"TUTORIAL continue dialogue {dialogueLines.Count}");
            if (isTyping)
            {
                currentText = fullText;
                dialogueText.text = currentText;
                isTyping = false;
            }
            else
            {
                currentLine += 1;
                if (currentLine < dialogueLines.Count)
                {
                    StartLine();
                }
                else // finished last line in dialogue
                {
                    Debug.Log($"TUTORIAL continuing but ended at {dialogueLines.Count}");
                    if (received)
                    {
                        dialogueCanvas.SetActive(false);
                        state = TutorialState.AskingRetry;
                        retryCanvas.SetActive(true);
                        return;
                    }
                    dialogueCanvas.SetActive(false);
                    STT.Spawn();
                    state = TutorialState.WaitingForSpeech;
                }
            }    
        }
        else if (state == TutorialState.AskingRetry)
        {
            Debug.Log($"TUTORIAL continuing no retry");
            retryCanvas.SetActive(false);
            UnfreezePlayer();
            state = TutorialState.Finished;
        }
    }

    public void restart()
    {
        if (state == TutorialState.AskingRetry)
        {
            Reset();
        }
    }

    public void Reset()
    {
        retryCanvas.SetActive(false);
        UnfreezePlayer();
        Debug.Log($"TUTORIAL reset");
        plantAnimator.gameObject.SetActive(false);
        state = TutorialState.Idle;
        dialogueCanvas.SetActive(false);
        received = false;
    }

    public void Bloom()
    {
        plantAnimator.gameObject.SetActive(true);
        plantAnimator.Play("tutorial_grow");
    }

    public void HandleSpeech(string text)
    {
        Debug.Log($"TUTORIAL handling speech {text}");
        string stripped = Regex.Replace(text, @"\[.*?\]", "");
        if (!string.IsNullOrWhiteSpace(stripped))
        {
            dialogueLines = CompletedLines;
            Bloom();
        }
        else
        {
            dialogueLines = RetryLines;
        }
        received = true;
        StartDialogue();
        state = TutorialState.Dialogue;
    }

    private void FreezePlayer()
    {

        Vector3 direction = npc.position - player.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            player.rotation = Quaternion.LookRotation(direction);
            player.rotation = player.rotation * Quaternion.Euler(direction);
        }
        playerController.SetActive(false);
        LeftJoystick.action.Disable();
        RightJoystick.action.Disable();
    }

    private void UnfreezePlayer()
    {
        StartCoroutine(UnfreezeNextFrame());
    }
    private IEnumerator UnfreezeNextFrame()
    {
        Y.action.Disable();
        X.action.Disable();
        

        if (PlayerBody != null)
        {
            PlayerBody.linearVelocity = Vector3.zero;
            PlayerBody.angularVelocity = Vector3.zero;
        }

        yield return null;

        playerController.SetActive(true);

        Y.action.Enable();
        X.action.Enable();
        LeftJoystick.action.Enable();
        RightJoystick.action.Enable();
    }
}
