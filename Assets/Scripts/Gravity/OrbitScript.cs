using UnityEngine;

[RequireComponent(typeof(GravitySource))]
public class OrbitScript : MonoBehaviour
{
    [SerializeField] GravitySource parentPlanet;
    GravitySource thisPlanet;

    private void Awake()
    {
        thisPlanet = GetComponent<GravitySource>();
    }
    public GravitySource Planet()
    { return thisPlanet; }
    public Vector3 GetStartOrbitVelocity()
    {
        if(parentPlanet == null || thisPlanet == null) return Vector3.zero;

        Vector3 distanceVector = thisPlanet.transform.position - parentPlanet.transform.position;
        float distance = (distanceVector).magnitude;
        float speed = Mathf.Sqrt(UniverseConstants.gravitationalConstant * parentPlanet.Mass / distance);


        Vector3 orbitNormal = Vector3.up; 
        Vector3 velocityDir = Vector3.Cross(orbitNormal, distanceVector).normalized;
        Vector3 velocity = velocityDir * speed;
        return velocity;
    }


    

}

