using System.Collections;
using UnityEngine;
using TMPro;

public class GardenerDialogueTrigger : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueCanvas;
    public TextMeshProUGUI dialogueText;

    [Header("Settings")]
    [TextArea(3, 5)]
    public string gardenerSpeech = "You don't have to do anything yet. But if something good happened today — even something small — you could tell me about it. That's all.";
    
    public float displayDuration = 8f;

    private bool hasSpoken = false;

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.name.Contains("Camera")) && !hasSpoken)
        {
            StartCoroutine(ShowDialogue());
        }
    }

    private IEnumerator ShowDialogue()
    {
        hasSpoken = true;

        if (dialogueText != null)
        {
            dialogueText.text = gardenerSpeech;
        }

        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(true);
        }

        yield return new WaitForSeconds(displayDuration);

        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(false);
        }
    }
}