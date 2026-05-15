using UnityEngine;

public class ShipScriptManager : MonoBehaviour
{

    [SerializeField] GameObject SpaceShipMesh;
    [SerializeField] GameObject ReactionControlSystem;

    public Rigidbody RigidBody()
    {
        return SpaceShipMesh.GetComponent<Rigidbody>();
    }

    public GravitationalOrientation Orientation()
    {
        return ReactionControlSystem.GetComponent<GravitationalOrientation>();
    }

    public GravitySource GravitySource()
    {
        return SpaceShipMesh.GetComponent<GravitySource>();
    }
    [SerializeField]
    public float effectiveCollisionMass = 100f;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody == null) return;

        Vector3 impulse = collision.impulse;

        float scale = 0.1f / effectiveCollisionMass;

        Vector3 reducedImpulse = impulse * scale;

        SpaceShipMesh.GetComponent<Rigidbody>().AddForce(-reducedImpulse, ForceMode.Impulse);
        SpaceShipMesh.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
}
