using UnityEngine;

public class BodyMono : MonoBehaviour
{
    public bool isTouching;
    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Ground")) {
            isTouching = true;
        }
    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Ground")) {
            isTouching = false;
        }
    }
}
