using UnityEngine;

public class ControllerCheck : MonoBehaviour
{
    public MirrorManager manager;
    public string side;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit: " + other.name + " Tag: " + other.tag);

        if (other.CompareTag("RaiseArmCollider"))
        {
            Debug.Log("collided");
            manager.raised(side);
        }
    }
}
