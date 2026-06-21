using UnityEngine;
using System;
using System.IO;

#if !UNITY_EDITOR_OSX
using Oculus.Avatar2; // Only compiles Meta classes when building for the Quest hardware
#endif

public class AvatarLabController : MonoBehaviour
{
    [Header("Avatar Targets")]
    public GameObject mockAvatarMesh;      // Drag your 'Mock_Avatar_Mesh' capsule here
    public GameObject metaAvatarObject;    // Drag your 'Meta_Avatar_Entity' here

    [Header("Participant Metadata")]
    public string participantID = "PARTICIPANT_001";
    private int selectedPresetIndex = 0;

    [Header("Reactive Emojis (Optional UI Feedback)")]
    public GameObject happyEmojiPrefab;
    public GameObject sadEmojiPrefab;
    public GameObject surprisedEmojiPrefab;
    public Transform emojiSpawnAnchor;     // Position hovering slightly above the avatar's head

    private void Start()
    {
        #if UNITY_EDITOR_OSX
        // On Mac, we keep the capsule visible and completely clear out the reference 
        // to prevent Meta's native pipeline from waking up during SetActive calls.
        if (mockAvatarMesh != null) mockAvatarMesh.SetActive(true);
        metaAvatarObject = null; 
        Debug.Log("Mac Editor Mode: Meta object reference safely zeroed out to prevent lifecycle crashes.");
        #else
        // On your teammate's Quest headset, the capsule hides and the true 3D avatar initializes smoothly.
        if (mockAvatarMesh != null) mockAvatarMesh.SetActive(false);
        if (metaAvatarObject != null) metaAvatarObject.SetActive(true);
        #endif
    }

    // --- 1. CHARACTER CUSTOMIZATION (CYCLE GENERIC LOOKS) ---
    public void CycleNextCharacterLook()
    {
        // Cycles safely through Meta's 32 default local on-disk asset choices
        selectedPresetIndex = (selectedPresetIndex + 1) % 32;
        Debug.Log($"Swapped avatar look to preset index slot: {selectedPresetIndex}");

        #if !UNITY_EDITOR_OSX
        // Feeds the index parameter directly into Meta's hardware rendering loop on her Quest
        var entity = metaAvatarObject.GetComponent<OvrAvatarEntity>();
        if (entity != null)
        {
            entity.SetSimplePresetIndex((uint)selectedPresetIndex);
        }
        #endif
    }

    // --- 2. EMOTION REACTION TRIGGERS ---
    // Link these public methods directly to your floating canvas buttons
    public void TriggerHappyExpression()
    {
        SpawnReactionEmoji(happyEmojiPrefab);
        Debug.Log($"Tracked Selection [ID: {participantID}]: Expression changed to Happy");
    }

    public void TriggerSadExpression()
    {
        SpawnReactionEmoji(sadEmojiPrefab);
        Debug.Log($"Tracked Selection [ID: {participantID}]: Expression changed to Sad");
    }

    public void TriggerSurprisedExpression()
    {
        SpawnReactionEmoji(surprisedEmojiPrefab);
        Debug.Log($"Tracked Selection [ID: {participantID}]: Expression changed to Surprised");
    }

    private void SpawnReactionEmoji(GameObject emojiPrefab)
    {
        if (emojiPrefab != null && emojiSpawnAnchor != null)
        {
            GameObject freshEmoji = Instantiate(emojiPrefab, emojiSpawnAnchor.position, Quaternion.identity);
            Destroy(freshEmoji, 3.0f); // Automatically cleans up and destroys the floating asset after 3 seconds
        }
    }

    // --- 3. EXPORT METADATA LOG DOCUMENTATION & SNAPSHOTS ---
    public void SaveSessionData()
    {
        string creationDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        
        // Establishes a strict scannable filename format using your required parameters
        string fileName = $"ID_{participantID}_Look_{selectedPresetIndex}_{creationDate}";
        
        // Maps paths perfectly for both your local Mac folders and her standalone Quest application data path
        string fullOutputPath = Path.Combine(Application.persistentDataPath, fileName);

        // A. Capture Viewport Snapshot image layout directly to storage
        ScreenCapture.CaptureScreenshot(fullOutputPath + ".png");

        // B. Compile highly structured text logging document tracking metadata variables
        string sessionMetadataText = $"Participant Reference Identifier: {participantID}\n" +
                                     $"Creation Datetime Stamp: {DateTime.Now.ToString("F")}\n" +
                                     $"Selected Generic Preset Index: {selectedPresetIndex}";

        File.WriteAllText(fullOutputPath + ".txt", sessionMetadataText);
        
        Debug.Log($"Successfully tracked data session! Saved files to destination cache: {Application.persistentDataPath}");
    }
}