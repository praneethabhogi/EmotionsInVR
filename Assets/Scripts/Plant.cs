using UnityEngine;
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
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PlantState {
    Idle,
    WaitingForSpeech,
    AskingContinue,
    Dialogue,
    Finished
}

[System.Serializable]
public class Dialogue
{
    public List<string> StartLines;
    public List<string> EndLines;
}

public class Plant : MonoBehaviour
{
    public UnityEngine.UI.Button interactButton;
    public GameObject dialogueCanvas;
    public GameObject continueCanvas;

    public TMP_Text dialogueText;

    public List<Animator> plantAnimators;
    public List<string> animationNames;

    public SpeechToText STT;

    public InputActionReference X;
    public InputActionReference Y;
    // public InputActionReference LeftJoystick;
    // public InputActionReference RightJoystick;

    public List<Dialogue> AllDialogue;
    public List<string> finishedDialogue;
    private int stage = 0;

    [SerializeField] private List<float> maxSentences; // the number of sentence needed for a full bloom 
    
    private PlantState state = PlantState.Idle;
    private PlantState prevState;

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

    private bool received = false;

    

    private void Start()
    {
        if (interactButton != null) interactButton.onClick.AddListener(Interact);

        STT.OnSpeechFinished += HandleSpeech;
        Y.action.Enable();
        Y.action.performed += (ctx) => Continue();

        X.action.Enable();
        X.action.performed += (ctx) => Exit();

        if (plantAnimators.Count != 0)
        {
            plantAnimators[0].speed = 0;
            plantAnimators[0].Play(animationNames[0]);
        }
        
        dialogueCanvas.SetActive(false);
        
        continueCanvas.SetActive(false);
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
        if (AllDialogue.Count > stage) 
        {
            dialogueLines = AllDialogue[stage].StartLines;
            
        }
        else 
        {
            dialogueLines = finishedDialogue;
        }
        StartDialogue();
        interactButton.enabled = false; // not sure how many times we want to allow them to talk to each plant
        
    }

    public void Exit()
    {
        UnfreezePlayer();
        interactButton.enabled = true;
        continueCanvas.SetActive(false);
        dialogueCanvas.SetActive(false);
        received = false;
        prevState = PlantState.Idle;
        state = PlantState.Idle;
    }

    public void StartDialogue()
    {
        Debug.Log("PLANT starting dialogue");
        currentLine = 0;
        StartLine();
        dialogueCanvas.SetActive(true);
    }

    private void StartLine()
    {
        currentText = "";
        dialogueText.text = currentText;
        fullText = dialogueLines[currentLine];
        charInd = 0;
        isTyping = true;
        prevState = state; // keep track of state before dialogue, in case we trigger finished dialogue
        state = PlantState.Dialogue;
    }

    public void Continue()
    {
        if (state == PlantState.Dialogue)
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
                    if (received) // after STT
                    {
                        dialogueCanvas.SetActive(false);
                        stage++;

                        if (stage >= AllDialogue.Count)
                        {
                            state = PlantState.Finished;
                            STT.OnSpeechFinished -= HandleSpeech;
                            return;
                        }
                        state = PlantState.AskingContinue;
                        continueCanvas.SetActive(true);
                        return;
                    }
                    dialogueCanvas.SetActive(false);
                    if (prevState == PlantState.Finished) 
                    {
                        state = PlantState.Finished;
                        return;
                    }

                    if (stage < plantAnimators.Count &&
                        stage < animationNames.Count &&
                        stage < maxSentences.Count)
                    {
                        STT.Spawn(
                            plantAnimators[stage],
                            animationNames[stage],
                            maxSentences[stage]
                        );

                        state = PlantState.WaitingForSpeech;
                    }
                    // STT.Spawn(plantAnimators[stage], animationNames[stage], maxSentences[stage]);
                    // state = PlantState.WaitingForSpeech;
                }
            }
        }
        else if (state == PlantState.AskingContinue) // start next stage right away
        {
            continueCanvas.SetActive(false);
            if (stage >= AllDialogue.Count)
            {
                state = PlantState.Finished;
                return;
            }
            received = false;
            dialogueLines = AllDialogue[stage].StartLines;
            StartDialogue();
        }
    }

    public void HandleSpeech(string text)
    {
        if (state != PlantState.WaitingForSpeech) return;
        Debug.Log($"PLANT handling speech {text}");
        received = true;
        dialogueLines = AllDialogue[stage].EndLines;
        StartDialogue();
    }

    private void FreezePlayer()
    {
        playerController.SetActive(false);
        // LeftJoystick.action.Disable();
        // RightJoystick.action.Disable();
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
        // yield return new WaitUntil(() =>
        // {
        //     Vector2 left = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        //     Vector2 right = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        //     bool jumpHeld = OVRInput.Get(OVRInput.Button.One) || OVRInput.Get(OVRInput.Button.Three);
        //     return left.magnitude < 0.1f && right.magnitude < 0.1f && !jumpHeld;
        // });

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
        // LeftJoystick.action.Enable();
        // RightJoystick.action.Enable();
    }
}
