using UnityEngine;
using TMPro;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class MirrorManager : MonoBehaviour
{
    public GameObject welcome;
    public GameObject right;
    public GameObject Continue;
    private bool leftRaised = false;
    void Start()
    {
        welcome.SetActive(true);
        right.SetActive(false);
        Continue.SetActive(false);
    }

    public void raised(string side)
    {
        if (side == "left" && !leftRaised)
        {
            Debug.Log("received left");
            leftRaised = true;
            welcome.SetActive(false);
            right.SetActive(true);
        }
        if (leftRaised && side == "right")
        {
            right.SetActive(false);
            Continue.SetActive(true);
        }
    }
}
