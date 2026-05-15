using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Rigidbody shipRigidbody;

    [SerializeField]
    private TextMeshProUGUI velocityText;
    [SerializeField]
    private TextMeshProUGUI instructions;


    private void Update()
    {
        float currentVelocity = shipRigidbody.linearVelocity.magnitude;
        velocityText.text = "Velocity: " + currentVelocity.ToString("0.00");

        bool byUser = StateManager.ByUser;
        bool maintainRot = StateManager.MaintainRotation;
        instructions.text = "Back engine - Left click\r\nBottom engine - Right click\r\nRotate ship - Mouse\r\nMap - P\r\n";

        if (StateManager.ps != null)
        {
            instructions.text += "Selected planet - " + StateManager.ps.name + "\r\n";
            instructions.text += "Tab -";
            if (byUser)
                instructions.text += " Go towards \r\n";
            else
                instructions.text += " Control ship\r\n";
            instructions.text += "Q - ";
            if (maintainRot)
            {
                instructions.text += " Control Rotatione\r\n";
            }
            else
            {
                instructions.text += " Maintain Rotation\r\n";
            }
        }
        else
        {
            instructions.text += "Selected planet - None";
        }

    }

}
