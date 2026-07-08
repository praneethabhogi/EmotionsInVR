using Oculus.Avatar2;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Oculus.Avatar2.OvrAvatarEntity;

public static class AvatarSelection
{
    public static int SelectedPresetIndex = 0;
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

    private int selectedPresetIndex = 0;
    private SampleAvatarEntity _entity;
    private SampleAvatarEntity _entityB2;
    private SampleAvatarEntity _entityB1;
    private SampleAvatarEntity _entityF1;
    private SampleAvatarEntity _entityF2;
    //private bool _isLoading = false;

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
    }

    private void Start()
    {
        _entity.OnUserAvatarLoadedEvent.AddListener(OnAvatarLoaded);
        LoadPresetAtIndex(selectedPresetIndex);
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
        selectedPresetIndex = (selectedPresetIndex + 1) % presetCount;
        LoadPresetAtIndex(selectedPresetIndex);
    }

    public void Previous()
    {
        //if (_isLoading) return;
        selectedPresetIndex = (selectedPresetIndex - 1 + presetCount) % presetCount;
        LoadPresetAtIndex(selectedPresetIndex);
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
        AvatarSelection.SelectedPresetIndex = selectedPresetIndex;
        SceneManager.LoadScene(nextSceneName);
    }
}