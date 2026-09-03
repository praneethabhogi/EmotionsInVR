using Oculus.Avatar2;
using Oculus.Interaction.Input;
using Oculus.Interaction.Locomotion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    public Animator gardenerAnimator;
    private static readonly int GestureHash = Animator.StringToHash("Gesture");
    public Door door;
    public SpeechToText STT;

    public InputActionReference X;
    public InputActionReference Y;
    public InputActionReference LeftJoystick;
    public InputActionReference RightJoystick;

    [SerializeField] private List<string> StartingLines;
    [SerializeField] private List<string> FinishedLines;
    [SerializeField] private List<string> ContinueLines;

    private bool received = false;
    private bool continuing = false;

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

    private float maxSentences = 5.0f;


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
            if (charInd <= fullText.Length)
            {
                currentText = fullText.Substring(0, charInd);
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
        state = TutorialState.Dialogue;
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
                        if (continuing)
                        { // ending tutorial
                            if (gardenerAnimator != null) gardenerAnimator.Play("Gesture");
                            UnfreezePlayer();
                            state = TutorialState.Finished;
                            door.UnlockDoor();
                            return;
                        }
                        else
                        {
                            state = TutorialState.AskingRetry;
                            retryCanvas.SetActive(true);
                            return;
                        }

                    }
                    dialogueCanvas.SetActive(false);
                    STT.Spawn(plantAnimator, "tutorial_grow", 5.0f);
                    state = TutorialState.WaitingForSpeech;
                }
            }
        }
        else if (state == TutorialState.AskingRetry)
        {
            Debug.Log($"TUTORIAL continuing no retry");
            retryCanvas.SetActive(false);

            dialogueLines = ContinueLines;
            continuing = true;
            if (gardenerAnimator != null) gardenerAnimator.Play("Gesture");
            StartDialogue();
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
        dialogueText.text = "";
        received = false;
    }

    //public void Bloom()
    //{
    //    plantAnimator.gameObject.SetActive(true);
    //    plantAnimator.Play("tutorial_grow");
    //}

    public IEnumerator Bloom(float amount, string animName)
    {
        amount = Mathf.Clamp01(amount);
        Debug.Log($"Bloom: targeting {amount} normalized time");

        plantAnimator.speed = 1f; // ensure speed is normal if previously frozen
        plantAnimator.gameObject.SetActive(true);
        plantAnimator.Play(animName, 0, 0f);

        yield return null;
        yield return null;

        AnimatorStateInfo stateInfo = plantAnimator.GetCurrentAnimatorStateInfo(0);
        float clipLength = stateInfo.length;

        if (clipLength <= 0f)
        {
            Debug.LogWarning("Bloom: clip length is 0, check animator state name");
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < amount)
        {
            elapsed += (Time.deltaTime / clipLength);
            elapsed = Mathf.Min(elapsed, amount);
            plantAnimator.Play(animName, 0, elapsed);
            yield return null;
        }

        // Stop the animator from advancing any further
        plantAnimator.speed = 0f;
        plantAnimator.Play(animName, 0, amount);

        Debug.Log($"Bloom: frozen at normalized time {amount}");

    }

    public void HandleSpeech(string text)
    {
        if (state != TutorialState.WaitingForSpeech) return;
        Debug.Log($"TUTORIAL handling speech {text}");
        string stripped = Regex.Replace(text, @"\[.*?\]", "");
        string[] sentences = stripped.Split(new char[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

        int sentenceCount = sentences.Length;
        dialogueLines = FinishedLines;
        plantAnimator.gameObject.SetActive(true);
        Debug.Log(sentenceCount / maxSentences);
        // StartCoroutine(Bloom(sentenceCount/maxSentences, "tutorial_grow"));
        STT.OnSpeechFinished -= HandleSpeech;

        received = true;
        StartDialogue();
    }

    private void FreezePlayer()
    {

        //Vector3 direction = player.position - npc.position;
        //direction.y = 0;
        //if (direction != Vector3.zero)
        //{
        //    player.rotation = Quaternion.LookRotation(direction);
        //    player.rotation = player.rotation * Quaternion.Euler(direction);
        //}
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

        // Wait until all relevant inputs are physically released
        yield return new WaitUntil(() =>
        {
            Vector2 left = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            Vector2 right = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            bool jumpHeld = OVRInput.Get(OVRInput.Button.One) || OVRInput.Get(OVRInput.Button.Three);
            return left.magnitude < 0.1f && right.magnitude < 0.1f && !jumpHeld;
        });

        // Extra frame after inputs are neutral to let OVR flush
        yield return null;
        yield return null;

        playerController.SetActive(true);

        if (PlayerBody != null)
        {
            PlayerBody.linearVelocity = Vector3.zero;
            PlayerBody.angularVelocity = Vector3.zero;
        }

        Y.action.Enable();
        X.action.Enable();
        LeftJoystick.action.Enable();
        RightJoystick.action.Enable();
    }
}