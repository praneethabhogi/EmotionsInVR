using System;
using UnityEngine;
using static Oculus.Avatar2.OvrAvatarEntity;

public class AvatarSpawner : MonoBehaviour
{
    [SerializeField] private GameObject metaAvatarObject;
    [SerializeField] private string presetNamePrefix = "";

    private void Start()
    {
        var entity = metaAvatarObject.GetComponent<SampleAvatarEntity>();
        if (entity != null)
        {
            string assetPath = $"{presetNamePrefix}{AvatarSelection.SelectedPresetIndex}";
            entity.ReloadAvatarManually(assetPath, AssetSource.Zip);
        }
    }
}