using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Voice.Dictation;

public class Dictation : MonoBehaviour
{
    public AppDictationExperience speechToText;
    public TMP_Text transcribed_text;
    public Button startButton;
    public Button stopButton;
    public TMP_Text timer;

    private bool listening = false;
    private string lastTranscript;
    public float time = 120.0f;
    void Start()
    {
        startButton.onClick.AddListener(listen);
        stopButton.onClick.AddListener(stop);

        speechToText.DictationEvents.OnFullTranscription.AddListener(OnTranscript);
        speechToText.DictationEvents.OnPartialTranscription.AddListener(OnPartial);

        stopButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (listening)
        {
            if (time > 0)
            {
                time = Mathf.Clamp(time - Time.deltaTime, 0, time);
                timer.text = time.ToString("F2");
            }
            else
            {
                timer.text = "0.00";
                stop();
                startButton.gameObject.SetActive(false);
            }

        }
    }

    public void listen()
    {
        listening = true;
        speechToText.Activate();
        Debug.Log(speechToText.Active);
        stopButton.gameObject.SetActive(true);
        startButton.gameObject.SetActive(false);
    }

    public void stop()
    {
        speechToText.Deactivate();
        stopButton.gameObject.SetActive(false);
        startButton.gameObject.SetActive(true);
        listening = false;
        // can trigger something here to send transcript to llm
    }

    public void OnPartial(string transcript)
    {
        if (lastTranscript == null || lastTranscript.Length == 0)
        {
            transcribed_text.text = transcript;
        }
        else
        {
            transcribed_text.text =  lastTranscript + "\n" + transcript;
        }
    }

    public void OnTranscript(string transcript)
    {
        if (lastTranscript == null || lastTranscript.Length == 0)
        {
            lastTranscript = transcript;
        }
        else
        {
            lastTranscript += "\n" + transcript;
        }
        transcribed_text.text = lastTranscript;

    }

    public void restart()
    {
        stop();
        time = 120.0f;
        lastTranscript = "";
        transcribed_text.text = "Your transcript will appear here";
    }

}