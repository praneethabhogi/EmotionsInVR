using UnityEngine;

public class MirrorMovement : MonoBehaviour
{
    public Transform PlayerTarget;
    public Transform Mirror;
    [Header("Movement Scaling")]
    public float fullMovementDistance = 1f;

    void Update()
    {
        Vector3 localPlayer = Mirror.InverseTransformPoint(PlayerTarget.position);

        float distance = Mathf.Abs(localPlayer.z);
        float scale = Mathf.Clamp01(distance / fullMovementDistance);

        Vector3 reflectedLocal = new Vector3(localPlayer.x * scale,transform.localPosition.y,-localPlayer.z * scale);

        transform.position = Mirror.TransformPoint(reflectedLocal);

        Vector3 lookAtLocal = new Vector3( -localPlayer.x * scale, transform.localPosition.y, localPlayer.z * scale);

        Vector3 lookAtWorld = Mirror.TransformPoint(lookAtLocal);
        transform.LookAt(lookAtWorld);
    }
}
