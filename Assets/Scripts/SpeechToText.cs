using TMPro;
using UnityEngine;
using Meta.XR.BuildingBlocks.AIBlocks;
using UnityEngine.UI;
using System;

public class SpeechToText : MonoBehaviour
{
    public event Action<string> OnSpeechFinished;
    public SpeechToTextAgent speechToText;
    public GameObject STTCanvas;
    public TMP_Text transcribed_text;
    public Button startButton;
    public Button stopButton;
    public Button doneButton;
    public TMP_Text timer;

    public GameObject Player;

    private bool listening = false;
    private string fullTranscript;
    public float time = 120.0f;
    void Start()
    { 
        startButton.onClick.AddListener(listen);
        stopButton.onClick.AddListener(stop);
        doneButton.onClick.AddListener(done);
        speechToText.onTranscript.AddListener(OnTranscript);
        stopButton.gameObject.SetActive(false);
        doneButton.gameObject.SetActive(false);
        STTCanvas.SetActive(false);
    }

    public void Spawn()
    {
        Transform cam = Camera.main.transform;
        Vector3 spawnPos = cam.position + cam.forward;
        Quaternion spawnRot = Quaternion.LookRotation(cam.forward);

        STTCanvas.transform.position = spawnPos;
        STTCanvas.transform.rotation = spawnRot;
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
                startButton.gameObject.SetActive(false);
            }
            
        }
    }

    public void UpdatePosition()
    {

    }

    public void listen()
    {
        listening = true;
        speechToText.StartListening();
        stopButton.gameObject.SetActive(true);
        startButton.gameObject.SetActive(false);
        doneButton.gameObject.SetActive(true);
        doneButton.enabled = false;
    }

    public void stop()
    {
        speechToText.StopNow();
        stopButton.gameObject.SetActive(false);
        startButton.gameObject.SetActive(true);
        listening = false;
        doneButton.enabled = true;
        // can trigger something here to send transcript to llm
    }

    public void OnTranscript(string transcript)
    {
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
        stop();
        time = 120.0f;
        fullTranscript = "";
        transcribed_text.text = "Your transcript will appear here";
    }

    public void done()
    {
        OnSpeechFinished?.Invoke(fullTranscript);
        restart();
        STTCanvas.SetActive(false);
    }

}
