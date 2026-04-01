using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(GravitySource))]
public class PlayerScript : MonoBehaviour
{
    [SerializeField] GravitySource thisSource;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] InputAction playerControls;


    Rigidbody rb;

    Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

    }
    private void OnEnable()
    {
        playerControls.Enable();
    }
    private void OnDisable()
    {
        playerControls.Disable();
    }

    void SetupOrientation()
    {
        Vector3 gravityDirection = thisSource.totalForce.normalized;
        Vector3 desiredUp = -gravityDirection;

        transform.up = Vector3.Slerp(transform.up, desiredUp, rotationSpeed * Time.fixedDeltaTime);
    }

    void HandleMovement(Vector2 input)
    {
        Vector3 moveDirection = transform.forward * input.y + transform.right * input.x;
        moveDirection = moveDirection.normalized * moveSpeed;

        Vector3 verticalVelocity = Vector3.Project(rb.linearVelocity, transform.up);
        rb.linearVelocity = verticalVelocity + moveDirection;
    }

    void Update()
    {
        SetupOrientation();
        moveInput = playerControls.ReadValue<Vector2>();
        HandleMovement(moveInput);
    }
}