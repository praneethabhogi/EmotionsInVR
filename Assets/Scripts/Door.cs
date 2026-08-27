using UnityEngine;

public class Door : MonoBehaviour
{
    public Animator anim;
    private bool unlocked = false;
    private bool opened = false;

    void Start()
    {
        
    }

    public void UnlockDoor() 
    {
        unlocked = true;
    }

    public void OpenDoor()
    {
        if (unlocked) anim.Play("doorOpen");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OpenDoor();
        }
    }
}
