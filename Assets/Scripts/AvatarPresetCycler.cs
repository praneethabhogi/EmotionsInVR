using Oculus.Avatar2;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Oculus.Avatar2.OvrAvatarEntity;
using System.Linq;

public static class AvatarSelection
{
    public static int SelectedPresetIndex = 0;
}

public enum PresetGender
{
    All, Female, Male
}
public class AvatarPresetCycler : MonoBehaviour
{
    [SerializeField] private GameObject metaAvatarObject;
    [SerializeField] private GameObject avatarPreviewB2;
    [SerializeField] private GameObject avatarPreviewB1;
    [SerializeField] private GameObject avatarPreviewF1;
    [SerializeField] private GameObject avatarPreviewF2;
    [SerializeField] private int presetCount = 33;
    [SerializeField] private string presetNamePrefix = "";
    [SerializeField] private string nextSceneName = "MainScene";

    [Header("UI (optional, wire up in Inspector or call methods directly)")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text indexText;
    [SerializeField] private TMP_Dropdown genderDropdown;
    [SerializeField] private PresetGender presetGender;

    private int selectedPresetIndex = 0;
    private SampleAvatarEntity _entity;
    private SampleAvatarEntity _entityB2;
    private SampleAvatarEntity _entityB1;
    private SampleAvatarEntity _entityF1;
    private SampleAvatarEntity _entityF2;
    //private bool _isLoading = false;

    List<int> AllPresets = Enumerable.Range(0, 33).ToList();
    private List<int> FemalePresets = new List<int> {0,2,4,5,8,9,10,12,15,16,18,21,22,24,25,30,31 };
    private List<int> MalePresets = new List<int> {1,3,6,7,11,13,14,17,19,20,21,23,26,27,28,29,30,32 };

    private List<int> currentPresets;
    private int currentListIndex = 0;

    private void Awake()
    {
        _entity = metaAvatarObject.GetComponent<SampleAvatarEntity>();
        _entityB2 = avatarPreviewB2.GetComponent<SampleAvatarEntity>();
        _entityB1 = avatarPreviewB1.GetComponent<SampleAvatarEntity>();
        _entityF1 = avatarPreviewF1.GetComponent<SampleAvatarEntity>();
        _entityF2 = avatarPreviewF2.GetComponent<SampleAvatarEntity>();

        if (nextButton != null) nextButton.onClick.AddListener(Next);
        if (previousButton != null) previousButton.onClick.AddListener(Previous);
        if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmAndContinue);
        if (genderDropdown != null) genderDropdown.onValueChanged.AddListener(OnGenderChanged);

        currentPresets = AllPresets;
        _entity.OnUserAvatarLoadedEvent.AddListener(OnAvatarLoaded);

        LoadCurrentPreset();

        //_entity.OnUserAvatarLoadedEvent.AddListener(OnAvatarLoaded);
        //LoadPresetAtIndex(selectedPresetIndex);
    }

    private void LoadCurrentPreset()
    {
        int preset = currentPresets[currentListIndex];
        LoadPreset(preset);
    }

    private void LoadPreset(int preset)
    {
        SetButtonsInteractable(false);
        if (confirmButton != null)
            confirmButton.interactable = false;

        _entity.ReloadAvatarManually($"{presetNamePrefix}{preset}", AssetSource.Zip);

        int prev1 = currentPresets[(currentListIndex - 1 + currentPresets.Count) % currentPresets.Count];
        int prev2 = currentPresets[(currentListIndex - 2 + currentPresets.Count) % currentPresets.Count];
        int next1 = currentPresets[(currentListIndex + 1) % currentPresets.Count];
        int next2 = currentPresets[(currentListIndex + 2) % currentPresets.Count];

        _entityB2.ReloadAvatarManually($"{presetNamePrefix}{prev2}", AssetSource.Zip);
        _entityB1.ReloadAvatarManually($"{presetNamePrefix}{prev1}", AssetSource.Zip);
        _entityF1.ReloadAvatarManually($"{presetNamePrefix}{next1}", AssetSource.Zip);
        _entityF2.ReloadAvatarManually($"{presetNamePrefix}{next2}", AssetSource.Zip);

        indexText.text = $"{currentListIndex + 1}/{currentPresets.Count}";
    }

    private void OnDestroy()
    {
        if (_entity != null)
            _entity.OnUserAvatarLoadedEvent.RemoveListener(OnAvatarLoaded);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (nextButton != null) nextButton.interactable = interactable;
        if (previousButton != null) previousButton.interactable = interactable;
        if (genderDropdown != null) genderDropdown.interactable = interactable;
        // Confirm intentionally stays disabled until a load completes at least once
    }

    private void OnAvatarLoaded(OvrAvatarEntity entity)
    {
        Debug.Log($"Preset {selectedPresetIndex} finished loading");
        SetButtonsInteractable(true);

        if (confirmButton != null) confirmButton.interactable = true;
    }

    public void Next()
    {
        //if (_isLoading) return;
        //selectedPresetIndex = (selectedPresetIndex + 1) % presetCount;
        //LoadPresetAtIndex(selectedPresetIndex);
        currentListIndex = (currentListIndex + 1) % currentPresets.Count;
        LoadCurrentPreset();
    }

    public void Previous()
    {
        //if (_isLoading) return;
        //selectedPresetIndex = (selectedPresetIndex - 1 + presetCount) % presetCount;
        //LoadPresetAtIndex(selectedPresetIndex);
        currentListIndex = (currentListIndex - 1 + currentPresets.Count) % currentPresets.Count;
        LoadCurrentPreset();
    }

    private void LoadPresetAtIndex(int index)
    {
        if (_entity == null)
        {
            Debug.LogWarning("AvatarPresetCycler: no SampleAvatarEntity found on metaAvatarObject");
            return;
        }

        //_isLoading = true;
        //Debug.Log($"Loading preset {index}");
        SetButtonsInteractable(false);
        if (confirmButton != null) confirmButton.interactable = false;

        _entity.ReloadAvatarManually($"{presetNamePrefix}{index}", AssetSource.Zip);
        _entityB2.ReloadAvatarManually($"{presetNamePrefix}{(index - 2 + presetCount) % presetCount}", AssetSource.Zip);
        _entityB1.ReloadAvatarManually($"{presetNamePrefix}{(index - 1 + presetCount) % presetCount}", AssetSource.Zip);
        _entityF1.ReloadAvatarManually($"{presetNamePrefix}{(index + 1) % presetCount}", AssetSource.Zip);
        _entityF2.ReloadAvatarManually($"{presetNamePrefix}{(index + 2) % presetCount}", AssetSource.Zip);

        //_isLoading = false;


        indexText.text = (index + 1) + "/" + presetCount;
    }

    public void ConfirmAndContinue()
    {
        //AvatarSelection.SelectedPresetIndex = selectedPresetIndex;
        AvatarSelection.SelectedPresetIndex = currentPresets[currentListIndex];
        SceneManager.LoadScene(nextSceneName);
    }
    public void OnGenderChanged(int value)
    {
        switch (value)
        {
            case 0:
                currentPresets = AllPresets;
                break;

            case 1:
                currentPresets = FemalePresets;
                break;

            case 2:
                currentPresets = MalePresets;
                break;
        }

        currentListIndex = 0;
        LoadCurrentPreset();
    }
}