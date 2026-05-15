using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class Engine : MonoBehaviour
{

    public float strength = 5f;
    public float maxStrength = 10f;
    public bool useEngine = true;
    [SerializeField] KeyCode key;
    [SerializeField] KeyCode modifier = KeyCode.None;
    [SerializeField] private Vector3 orientation;
    [SerializeField] private Vector3 engineForce;

    [SerializeField] private GravitationalOrientation gravOrientation = null;
    [SerializeField] private Rigidbody rb = null;

    [SerializeField] private ParticleSystem normalEffect;
    [SerializeField] private ParticleSystem reverseEffect;
    private ParticleSystem engineEffect;
    [SerializeField] bool useKeyboard = false;

    [SerializeField] bool activated = false;

    [SerializeField] private bool reverse = false;

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
        if (reverse)
            engineForce *= -1;
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
    public void Reverse()
    {
        reverse = true;
        engineEffect = reverseEffect;
        normalEffect.Stop();

    }
    public void Unreverse()
    {
        reverseEffect.Stop();
        reverse = false;
        engineEffect = normalEffect;
    }
    void Update()
    {
        if (StateManager.ByUser == true)
        {
            if(Input.GetKey(KeyCode.LeftControl))
            {
                Reverse();
            }
            else
            {
                Unreverse();
            }
            if (Input.GetKey(key))
            {
                if (modifier == KeyCode.None || Input.GetKey(modifier))
                    Activate();
            }
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
