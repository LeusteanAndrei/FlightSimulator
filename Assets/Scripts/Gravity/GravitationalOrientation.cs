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
    public Vector3 GetForwardOrientation()
    {
        Vector3 gravityDir = gravitySource.totalForce;

        if (gravityDir.sqrMagnitude < 0.0001f)
            return Forward();

        gravityDir.Normalize();

        Vector3 up = -gravityDir;

        Vector3 currentForward = Forward();

        Vector3 projectedForward = Vector3.ProjectOnPlane(currentForward, up);

        if (projectedForward.sqrMagnitude < 0.0001f)
        {
            projectedForward = Vector3.Cross(Right(), up);
        }

        return projectedForward.normalized;
    }

    public float GetRollAngle(Vector3 forwardDirection, Vector3 upDirection)
    {
        Vector3 projectedUp = Vector3.ProjectOnPlane(upDirection, forwardDirection);

        if (projectedUp.sqrMagnitude < 0.0001f)
            return 0f; 

        projectedUp.Normalize();

        Vector3 referenceUp = Vector3.ProjectOnPlane(Vector3.up, forwardDirection);
        if (referenceUp.sqrMagnitude < 0.0001f)
            referenceUp = Vector3.ProjectOnPlane(Vector3.right, forwardDirection);
        referenceUp.Normalize();
        return Vector3.SignedAngle(referenceUp, projectedUp, forwardDirection);
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

    private void OnDrawGizmos()
    {
        if (rb == null)
            return;

        Transform t = rb.transform;

        float length = 2f; // axis length

        // Forward (Z) - Blue
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(t.position, t.position + t.forward * length);

        // Up (Y) - Green
        Gizmos.color = Color.green;
        Gizmos.DrawLine(t.position, t.position + t.up * length);

        // Right (X) - Red
        Gizmos.color = Color.red;
        Gizmos.DrawLine(t.position, t.position + t.right * length);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(t.position, t.position+ GetForwardOrientation()* length);
    }
}
