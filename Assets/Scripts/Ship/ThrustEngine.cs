using UnityEngine;

public class ThrustEngine : MonoBehaviour
{

    [SerializeField] Rigidbody rigidbody;
    [SerializeField] float force = .01f;
    [SerializeField] KeyCode key;
    [SerializeField] bool alwaysShow = false;
    [Header("Particle system")]
    [SerializeField] ParticleSystem airThruster;
    bool fire;
    private void Awake()
    {
    }
    void Start()
    {
        if (rigidbody == null)
            rigidbody = GameInstance.Instance.SpaceShip().RigidBody();
        if (airThruster == null)
            airThruster = GetComponentInChildren<ParticleSystem>();
    }
    public void Fire()
    {
        fire = true;
    }
    public void Stop()
    {
        fire = false;
    }
    public bool IsFiring()
    {
        return fire;
    }

    void Update()
    {
        if (Input.GetKey(key))
            Fire();
        else if (Input.GetKeyUp(key))
            Stop();

        if (airThruster != null)
        {
            if (IsFiring() && !airThruster.isPlaying)
                airThruster.Play();
            else if (!IsFiring() && airThruster.isPlaying)
                airThruster.Stop();
        }
    }
    private void FixedUpdate()
    {
      if(IsFiring())
        {
            FireThruster();
        }
    }
    public void FireThruster()
    {
        Vector3 direction = -transform.forward;
        rigidbody.AddForceAtPosition(direction * force, transform.position);
    }
    private void OnDrawGizmos()
    {
        if (!IsFiring() && ! alwaysShow) return;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}
