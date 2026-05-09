using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Camera Settings")]
    public float mouseSensitivity = 5f;

    [Header("Output Data")]
    [Tooltip("This vector represents the current forward direction of the camera.")]
    public Vector3 savedForwardDirection;
    public RCSManager rcsManager;
    public GravitationalOrientation shipOrientation;
    bool rollRight = false;
    bool rollLeft = false;

    private float pitch = 0f; 
    private float yaw = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        Vector3 angles = transform.eulerAngles;
        pitch = angles.x;
        yaw = angles.y;
        savedForwardDirection = shipOrientation.Forward();  
    }
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Debug.Log(scroll);
        if (scroll == 0)
        {
            rollLeft = false;
            rollRight = false;
        }
        else if (scroll < 0)
        {
            rollLeft = true;
            rollRight = false;
        }
        else if (scroll > 0)
        {
            rollLeft = false;
            rollRight = true;
        }
            float mouseX =
                    Input.GetAxis("Mouse X") *
                    mouseSensitivity *
                    Time.deltaTime;

        float mouseY =
            Input.GetAxis("Mouse Y") *
            mouseSensitivity *
            Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, -90f, 90f);

        Quaternion yawRot =
            Quaternion.AngleAxis(
                yaw,
                shipOrientation.Up()
            );

        Quaternion pitchRot =
            Quaternion.AngleAxis(
                pitch,
                shipOrientation.Right()
            );

        Quaternion targetRot =
            yawRot * pitchRot;

        savedForwardDirection =
            targetRot * shipOrientation.Forward();

        Debug.DrawRay(
            transform.position,
            savedForwardDirection * 10f,
            Color.green
        );
    }
    float WrapAngle(float angle)
    {
        angle %= 360f;

        if (angle > 180f)
            angle -= 360f;

        if (angle < -180f)
            angle += 360f;

        return angle;
    }
    private void FixedUpdate()
    {
        if (rollRight)
        {
            rcsManager.SetRoll(
                WrapAngle(rcsManager.currentRollAngle + 5.0f)
            ); 
        }
        else if (rollLeft)
        {
            rcsManager.SetRoll(
                WrapAngle(rcsManager.currentRollAngle - 5.0f)
            );
        }
        else
            rcsManager.zAxisStop();


        rcsManager.RotateTowardsVector(savedForwardDirection.normalized); 
        yaw = Mathf.Lerp(yaw, 0f, Time.deltaTime * 2f);
        pitch = Mathf.Lerp(pitch, 0f, Time.deltaTime * 2f);
    }
}
