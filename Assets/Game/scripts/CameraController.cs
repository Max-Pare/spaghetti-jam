using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera cam = null;
    GameObject player = null;
    public float mouseSens = 28f;
    private void Awake()
    {
        cam = this.GetComponent<Camera>();
        player = GameObject.FindGameObjectWithTag("Player");
        mouseSens *= 10f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        MoveCamera();
    }

    float yRot = 0f;
    float xRot = 0f;
    void MoveCamera()
    {
        yRot += Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime * -1f;
        xRot += Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime * 1f;
        yRot = Mathf.Clamp(yRot, -89.5f, 89.5f);
        this.transform.eulerAngles = new Vector3(yRot, this.transform.eulerAngles.y, 0.0f);
        player.transform.eulerAngles = new Vector3(0f, xRot, 0f);
    }
}
