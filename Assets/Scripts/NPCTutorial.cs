using Oculus.Avatar2;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum TutorialState
{
    Idle,
    Dialogue,
    WaitingForSpeech,
    Finished
}
public class NPCTutorial : MonoBehaviour
{
    public GameObject dialogueCanvas;
    public TMP_Text dialogueText;
    public Animator plantAnimator;
    public SpeechToText STT;

    public InputActionReference action;

    [SerializeField] private List<string> dialogueLines;
    private int currentLine;
    private TutorialState state = TutorialState.Idle;
    void Start()
    {
        STT.OnSpeechFinished += HandleSpeech;
        action.action.Enable();
        action.action.performed += (ctx) => Continue();
        plantAnimator.gameObject.SetActive(false);
        dialogueCanvas.SetActive(false);
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (state == TutorialState.Idle)
                StartDialogue();
        }
    }

    public void StartDialogue()
    {
        state = TutorialState.Dialogue;
        currentLine = 0;
        dialogueText.text = dialogueLines[currentLine];
        dialogueCanvas.SetActive(true);
    }

    public void Continue()
    {
        if (state == TutorialState.Dialogue)
        {
            dialogueCanvas.SetActive(false);
            STT.Spawn();
        } else if (state == TutorialState.Finished)
        {
            dialogueCanvas.SetActive(false);
        }
        
        //if (!dialogueCanvas.activeInHierarchy) return;
        //currentLine += 1;
        //if (currentLine < dialogueLines.Count)
        //{
        //    dialogueText.text = dialogueLines.ElementAt(currentLine);
        //}
        //else
        //{
        //    dialogueCanvas.SetActive(false);
        //}
    }

    public void Bloom()
    {
        plantAnimator.gameObject.SetActive(true);
        plantAnimator.Play("tutorial_grow");
    }

    public void HandleSpeech(string text)
    {
        dialogueText.text = dialogueLines[1];
        dialogueCanvas.SetActive(true);
        Bloom();
        state = TutorialState.Finished;
    }
}
