using Meta.XR.BuildingBlocks.AIBlocks;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using static System.Net.Mime.MediaTypeNames;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif


public class SpeechToText : MonoBehaviour
{
    private Animator plantAnimator;
    private string animName;
    private float fullSentences;
    public event Action<string> OnSpeechFinished;
    public SpeechToTextAgent speechToText;
    public GameObject STTCanvas;
    public TMP_Text transcribed_text;
    public TMP_Text startStopText;
    public GameObject finishText;
    //public Button startButton;
    //public Button stopButton;
    //public Button doneButton;
    public TMP_Text timer;
    public InputActionReference X;
    public InputActionReference Y;



    public GameObject Player;

    private bool listening = false;
    private string fullTranscript = "";
    public float time = 120.0f;
    void Start()
    { 

        #if UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Debug.Log("[STT] Requesting microphone permission");
                Permission.RequestUserPermission(Permission.Microphone);
            }
            else
            {
                Debug.Log("[STT] Microphone permission already granted");
            }
        #endif
        //startButton.onClick.AddListener(listen);
        //stopButton.onClick.AddListener(stop);
        //doneButton.onClick.AddListener(done);
        speechToText.onTranscript.AddListener(OnTranscript);
        //stopButton.gameObject.SetActive(false);
        //doneButton.gameObject.SetActive(false);
        STTCanvas.SetActive(false);
        finishText.SetActive(false);

        X.action.Enable();
        X.action.performed += (ctx) => listenToggle();
        Y.action.Enable();
        Y.action.performed += (ctx) => done();
    }

    public void Spawn(Animator anim, string name, float sentences)
    {
        fullSentences = sentences;
        Debug.Log("[STT] Spawn");
        plantAnimator = anim;
        animName = name;
        Canvas canvas = STTCanvas.GetComponent<Canvas>();
        STTCanvas.SetActive(true);
    }

    private void Update()
    {
        if (listening)
        {
            if (time > 0)
            {
                time = Mathf.Clamp(time - Time.deltaTime, 0, time);
                timer.text = time.ToString("F2");
            } else
            {
                timer.text = "0.00";
                fullTranscript = "empty";
                done();
                //startButton.gameObject.SetActive(false);
            }
            
        }
    }

    public void listenToggle()
    {
        Debug.Log("[STT] triggered listen");
        if (!STTCanvas.activeInHierarchy) return;

        if (listening)
        {
            startStopText.text = "[X] Start";
            speechToText.StopNow();
            listening = false;
        }
        else
        {
            finishText.SetActive(true);
            startStopText.text = "[X] Pause";
            listening = true;
            speechToText.StartListening();
        }
        Debug.Log("[STT] listen");
        
        //stopButton.gameObject.SetActive(true);
        //startButton.gameObject.SetActive(false);
        //doneButton.gameObject.SetActive(true);
        //doneButton.enabled = false;
    }

    public void stop()
    {
        Debug.Log("[STT] stop");
        speechToText.StopNow();
        //stopButton.gameObject.SetActive(false);
        //startButton.gameObject.SetActive(true);
        listening = false;
        
        //doneButton.enabled = true;
        // can trigger something here to send transcript to llm
    }

    public void OnTranscript(string transcript)
    {
        Debug.Log("[STT] on transcript");
        if (fullTranscript == null || fullTranscript.Length == 0)
        {
            fullTranscript = transcript;
        } else
        {
            fullTranscript += "\n" + transcript;
        }
        transcribed_text.text = fullTranscript;

        // calculate bloom
        string stripped = Regex.Replace(transcript, @"\[.*?\]", "");
        string[] sentences = stripped.Split(new char[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

        int sentenceCount = sentences.Length;
        StartCoroutine(Bloom(sentenceCount));
        
    }

    public void restart()
    {
        Debug.Log("[STT] restart");
        stop();
        time = 120.0f;
        timer.text = "120.00";
        fullTranscript = "";
        transcribed_text.text = "Your transcript will appear here";
    }

    public void done()
    {
        string stripped = Regex.Replace(fullTranscript, @"\[.*?\]", "");
        if (STTCanvas.activeInHierarchy  && !string.IsNullOrWhiteSpace(stripped))
        {
            Debug.Log("[STT] done");
            STTCanvas.SetActive(false);

            string transcript = fullTranscript;

            restart();

            OnSpeechFinished?.Invoke(transcript);
        }
    }

    private float currentBloom = 0f;

    public IEnumerator Bloom(float amount)
    {
        amount = Mathf.Clamp01(amount/fullSentences);

        float targetBloom = Mathf.Clamp01(currentBloom + amount);

        Debug.Log($"Bloom: growing from {currentBloom} to {targetBloom}");

        plantAnimator.speed = 1f;
        plantAnimator.gameObject.SetActive(true);

        plantAnimator.Play(animName, 0, currentBloom);

        yield return null;
        yield return null;

        AnimatorStateInfo stateInfo = plantAnimator.GetCurrentAnimatorStateInfo(0);
        float clipLength = stateInfo.length;

        if (clipLength <= 0f)
        {
            Debug.LogWarning("Bloom: clip length is 0, check animator state name");
            yield break;
        }

        float elapsed = currentBloom;

        while (elapsed < targetBloom)
        {
            elapsed += Time.deltaTime / clipLength;
            elapsed = Mathf.Min(elapsed, targetBloom);

            plantAnimator.Play(animName, 0, elapsed);

            yield return null;
        }

        // Save the new total growth
        currentBloom = targetBloom;

        // Freeze at the new growth amount
        plantAnimator.speed = 0f;
        plantAnimator.Play(animName, 0, currentBloom);

        Debug.Log($"Bloom: total growth is now {currentBloom}");
    }

}
