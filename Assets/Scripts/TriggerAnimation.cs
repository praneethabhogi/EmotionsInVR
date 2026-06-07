using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerAnimation : MonoBehaviour
{
    public InputActionReference action;
    void Start()
    {
        action.action.Enable();
        action.action.performed += (ctx) => enabled = !enabled;
    }
}
