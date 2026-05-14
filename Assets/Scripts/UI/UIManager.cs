using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Rigidbody shipRigidbody;

    [SerializeField]
    private TextMeshProUGUI velocityText;

    private void Update()
    {
        float currentVelocity = shipRigidbody.linearVelocity.magnitude;
        velocityText.text = "Velocity: " + currentVelocity.ToString("0.00");
    }

}
