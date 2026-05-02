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
}
