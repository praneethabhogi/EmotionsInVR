using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
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
using UnityEngine.UI;


public class PlantDialogue : MonoBehaviour
{
    public UnityEngine.UI.Button button;
    public GameObject dialogueCanvas;
    public GameObject retryCanvas;
    public TMP_Text dialogueText;
    public Animator plantAnimator;
    public string animationName;
    public SpeechToText STT;

    private InputActionReference X;
    private InputActionReference Y;
    private InputActionReference LeftJoystick;
    private InputActionReference RightJoystick;

    [SerializeField] private List<string> StartingLines;
    [SerializeField] private List<string> CompletedLines;
    [SerializeField] private List<string> RetryLines;
    [SerializeField] private float maxSentences; // the number of sentence needed for a full bloom 

    private bool received = false;

    [Header("Player")]
    public GameObject playerController;
    public Rigidbody PlayerBody;
    public Transform player;
    public Transform plant;

    private List<string> dialogueLines;
    private int currentLine;
    private string currentText;
    private string fullText;
    private int charInd;
    private bool isTyping = false;

    private TutorialState state = TutorialState.Idle;
    void Start()
    {
        
        if (button != null)
        {
            button.onClick.AddListener(Interact);
        }

        STT.OnSpeechFinished += HandleSpeech;
        Y.action.Enable();
        Y.action.performed += (ctx) => Continue();

        X.action.Enable();
        X.action.performed += (ctx) => restart();

        if (plantAnimator != null)
        {
            plantAnimator.speed = 0;
            plantAnimator.Play(animationName);
        }
        //plantAnimator.gameObject.SetActive(false);
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

    private void Interact()
    {
        Debug.Log($"PLANT interact");
        FreezePlayer();
        dialogueLines = StartingLines;
        StartDialogue();
        button.enabled = false; // not sure how many times we want to allow them to talk to each plant
    }

    public void StartDialogue()
    {
        Debug.Log($"PLANT starting dialogue ");
        currentLine = 0;
        StartLine();
        dialogueCanvas.SetActive(true);
    }

    private void StartLine()
    {
        Debug.Log($"PLANT starting line {currentLine}");
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
                    if (received)
                    {
                        dialogueCanvas.SetActive(false);
                        state = TutorialState.AskingRetry;
                        retryCanvas.SetActive(true);
                        return;
                    }
                    dialogueCanvas.SetActive(false);
                    STT.Spawn(plantAnimator, animationName, maxSentences);
                    state = TutorialState.WaitingForSpeech;
                }
            }
        }
        else if (state == TutorialState.AskingRetry)
        {
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
        Debug.Log($"PLANT restarting");
        retryCanvas.SetActive(false);
        UnfreezePlayer();
        //plantAnimator.gameObject.SetActive(false);
        plantAnimator.speed = 0;
        plantAnimator.Play(animationName);
        state = TutorialState.Idle;
        dialogueCanvas.SetActive(false);
        received = false;
        button.enabled = true;
    }

    public IEnumerator Bloom(float amount, string animName)
    {
        amount = Mathf.Clamp01(amount);

        plantAnimator.speed = 1f; // ensure speed is normal if previously frozen
        plantAnimator.gameObject.SetActive(true);
        plantAnimator.Play(animName, 0, 0f);

        yield return null;
        yield return null;

        AnimatorStateInfo stateInfo = plantAnimator.GetCurrentAnimatorStateInfo(0);
        float clipLength = stateInfo.length;

        if (clipLength <= 0f)
        {
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
    }

    public void HandleSpeech(string text)
    {
        if (state != TutorialState.WaitingForSpeech) return;
        Debug.Log($"PLANT handling speech: {text}");
        string stripped = Regex.Replace(text, @"\[.*?\]", "");
        if (!string.IsNullOrWhiteSpace(stripped))
        {
            string[] sentences = stripped.Split(new char[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            int sentenceCount = sentences.Length;
            // add speech length threshold here to determine bloom
            dialogueLines = CompletedLines;
            //Bloom();
            Debug.Log(sentenceCount / maxSentences);
            StartCoroutine(Bloom(sentenceCount / maxSentences, animationName));
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
