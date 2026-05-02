using UnityEngine;

public class CameraControl : MonoBehaviour
{
     public float sensitivity = 2f;
    float rotationX;
    float rotationY;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        rotationX += Input.GetAxis("Mouse X") * sensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * sensitivity;

        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);

    }
}
