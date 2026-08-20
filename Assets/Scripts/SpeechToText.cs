using Meta.XR.BuildingBlocks.AIBlocks;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using static System.Net.Mime.MediaTypeNames;

public class SpeechToText : MonoBehaviour
{
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

    public void Spawn()
    {
        Debug.Log("[STT] Spawn");
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
                stop();
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

}
