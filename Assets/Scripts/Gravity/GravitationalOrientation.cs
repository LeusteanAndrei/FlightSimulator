using System.Runtime.CompilerServices;
using UnityEngine;


public class GravitationalOrientation : MonoBehaviour
{
    [SerializeField] private GravitySource gravitySource;
    [SerializeField] private Rigidbody rb;
    [SerializeField] float rotationSpeed;

    [SerializeField] private Vector3 forward;

    private void Awake()
    {
        if ( rb == null )
            rb = GameInstance.Instance.SpaceShip().RigidBody();
        if(gravitySource == null)
            gravitySource = GameInstance.Instance.SpaceShip().GravitySource();
    }

    void Start()
    {
        
    }

    public Vector3 Up()
    {
        return rb.transform.TransformDirection(Vector3.up);
    }
    public Vector3 Forward()
    {
        return rb.transform.TransformDirection(Vector3.forward);
    }

    public Vector3 Right()
    {
        return rb.transform.TransformDirection(Vector3.right);
    }
    private void Update()
    {
        forward = Forward();
    }

    private void FixedUpdate()
    {
    }
}
