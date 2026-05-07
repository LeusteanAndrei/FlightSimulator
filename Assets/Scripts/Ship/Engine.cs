using Unity.VisualScripting;
using UnityEngine;

public class Engine : MonoBehaviour
{

    public float strength = 5f;
    public float maxStrength = 10f;
    public bool useEngine = true;
    [SerializeField] KeyCode key;
    [SerializeField] private Vector3 orientation;
    [SerializeField] private Vector3 engineForce;

    [SerializeField] private GravitationalOrientation gravOrientation = null;
    [SerializeField] private Rigidbody rb = null;

    [SerializeField] private ParticleSystem engineEffect;
    [SerializeField] bool useKeyboard = false;

    [SerializeField] bool activated = false;


    void Start()
    {
        if(rb == null)
            rb = GameInstance.Instance.SpaceShip().RigidBody();
        if(gravOrientation == null)
            gravOrientation = GameInstance.Instance.SpaceShip().Orientation();

    }


    private void FixedUpdate()
    {
        if (!useEngine)
            return;
        CheckEngine();
        
    }


    public void SetForce(float force)
    {
        this.strength = Mathf.Clamp(force, 0, maxStrength);
    }

    public Vector3 GetEngineForward()
    {
        return orientation.z * gravOrientation.Forward() + orientation.y * gravOrientation.Up() + orientation.x * gravOrientation.Right();  
    }

    void CheckEngine()
    {
        if (!IsActive()) return;
        Vector3 engineForward = GetEngineForward();
        engineForce = engineForward * strength ;
        rb.AddForce( engineForce );
    }
    public void Activate()
    {
        activated = true;
    }
    public void Deactivate()
    {
        activated = false; 
    }

    public bool IsActive()
    {
        return activated;
    }

    void Update()
    {
        if (useKeyboard == true)
        {
            if (Input.GetKey(key))
                Activate();
            else
                Deactivate();
        }
        if (engineEffect != null)
        {
            if (IsActive() && !engineEffect.isPlaying)
                engineEffect.Play();
            else if (!IsActive() && engineEffect.isPlaying)
                engineEffect.Stop();
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, GetEngineForward());
    }
}
