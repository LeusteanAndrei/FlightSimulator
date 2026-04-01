using Unity.VisualScripting;
using UnityEngine;

public class GravitySource : MonoBehaviour
{

    [SerializeField] private float _mass;
    public float Mass => _mass;

    [SerializeField] private Vector3 _startVelocity = Vector3.zero;
    [SerializeField] private Vector3 _currentVelocity;
    public Vector3 Velocity => _currentVelocity;

    [SerializeField] public Vector3 totalForce = Vector3.zero;
    [SerializeField] public bool attractOthers = true;
    

   

    [SerializeField] Rigidbody _rigidBody = null;
    public Rigidbody rigidBody => _rigidBody;

    [SerializeField] private bool frozen = false;

    public void Awake()
    {
        if (rigidBody == null)
            _rigidBody = GetComponent<Rigidbody>();
        rigidBody.mass = _mass;



    }

    private void Start()
    {
        GravityManager.Instance.Register(this);
        SetupInitialVelocity();
    }


    private void Update()
    {
        if(!frozen)
            this._currentVelocity = rigidBody.linearVelocity;
    }


    private void SetupInitialVelocity()
    {


        OrbitScript orbitScript = GetComponent<OrbitScript>();
        if (orbitScript!=null && orbitScript.enabled)
        {
            this.SetStartVelocity(orbitScript.GetStartOrbitVelocity());
        }
        else
            this.SetStartVelocity(_startVelocity);
    }

    /**
     * Computes the force with wich this Objects attracts the other Object
     */
    public Vector3 GetGravitationalForceFor(GravitySource other)
    {


        Vector3 thisPos = transform.position;
        Vector3 otherPos = other.transform.position;
        Vector3 distanceVector = thisPos - otherPos;
        if (distanceVector == Vector3.zero)
            return Vector3.zero;
        

        float massThis = _mass;
        float massOther = other._mass;

        float distanceSquared = distanceVector.sqrMagnitude;


        Vector3 forceDirection = distanceVector.normalized;
        float forceMagnitude = UniverseConstants.gravitationalConstant * massThis * massOther / distanceSquared;

        return forceDirection * forceMagnitude;
    }

    public Vector3 GetAccelerationFor(GravitySource other)
    {
        return GetGravitationalForceFor(other) / other.Mass;
    }

    public void Attract(GravitySource other)
    {
        if(!attractOthers) { return; }
        Vector3 gravForce= GetGravitationalForceFor(other);
        other.totalForce += gravForce;
        //other._currentVelocity += GetAccelerationFor(other) * UniverseConstants.fixedTimeStep;
    }

    public void UpdateVelocity()
    {
        rigidBody.linearVelocity += (totalForce /this.Mass)*UniverseConstants.fixedTimeStep;
    }

    public void SetStartVelocity(Vector3 _velocity)
    {
        _startVelocity = _velocity;
        rigidBody.linearVelocity = _velocity;
        _currentVelocity = _velocity;
    }

    public void Freeze()
    {
        if (frozen) return;
        _currentVelocity = rigidBody.linearVelocity;
        rigidBody.linearVelocity = Vector3.zero;
        frozen = true;

    }

    public void Unfreeze()
    {
        if (!frozen) return;
        rigidBody.linearVelocity = _currentVelocity;
        frozen = false;
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float length = 2.0f;
        Vector3 direction = totalForce.normalized;


        Vector3 start = transform.position;
        Vector3 end = start + direction.normalized * length;
        
        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(end, 0.05f);
    }


    void DrawArrowHead(Vector3 position, Vector3 direction)
    {
        float headAngle = 20f; float headLength = 0.2f;
        // Calculate rotation for arrowhead lines
        Quaternion rightRotation = Quaternion.Euler(0, 0, headAngle);
        Quaternion leftRotation = Quaternion.Euler(0, 0, -headAngle);

        Vector3 right = rightRotation * direction * headLength;
        Vector3 left = leftRotation * direction * headLength;

        Gizmos.DrawLine(position, position + right);
        Gizmos.DrawLine(position, position + left);
    }

    public void OnDisable()
    {
        GravityManager.Instance.Deregister(this);
    }
}
