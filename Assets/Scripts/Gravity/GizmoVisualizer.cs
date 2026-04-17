using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


class SimulateBody
{
    public Vector3 position;
    public float mass;
    public Vector3 totalForce;
    public Vector3 velocity;

    public SimulateBody(GravitySource gravitySource)
    {
        mass = gravitySource.Mass;
        velocity = gravitySource.Velocity;
        position = gravitySource.transform.position;
        totalForce = gravitySource.totalForce;
    }



    public Vector3 GetGravitationalForceFor(SimulateBody other)
    {
        Vector3 thisPos = position;
        Vector3 otherPos = other.position;
        Vector3 distanceVector = thisPos - otherPos;
        if (distanceVector == Vector3.zero)
            return Vector3.zero;


        float massThis = mass;
        float massOther = other.mass;

        float distanceSquared = distanceVector.sqrMagnitude;


        Vector3 forceDirection = distanceVector.normalized;
        float forceMagnitude = UniverseConstants.gravitationalConstant * massThis * massOther / distanceSquared;

        return forceDirection * forceMagnitude;
    }
}


[RequireComponent(typeof(GravityManager))]
public class GizmoVisualizer : MonoBehaviour
{
    [SerializeField] int stepCount = 100;
    [SerializeField] bool realTime = false;
    [SerializeField] bool useAllSources = true;
    [SerializeField] List<GravitySource> sources = new List<GravitySource>();
    List<List<Vector3>> positions = new List<List<Vector3>>();

    void FixedUpdate()
    {
        if (realTime)
            RecomputeSimulation();
    }
    public void RecomputeSimulation()
    {
        GravityManager gravityManager = GetComponent<GravityManager>();
        if (!gravityManager)
        {
            Debug.Log("Gravity manager is null");
            return;
        }

        if (useAllSources == true)
            sources = gravityManager.GetSources(); 
        List<SimulateBody> simulateBodyList = 
                sources.Select(x => new SimulateBody(x)).ToList();

        positions = new List<List<Vector3>>();
        for (int k = 0; k < simulateBodyList.Count; k++)
        {
            positions.Add(new List<Vector3>());
        }

        for (int k=0;k<stepCount;k++)
        {

            for (int i = 0; i < simulateBodyList.Count; i++)
                positions[i].Add(simulateBodyList[i].position);
            SimulateStep(simulateBodyList);
        }
    }

    void SimulateStep(List<SimulateBody> simulateBodies)
    {
        foreach (var body in simulateBodies)
            body.totalForce = Vector3.zero;

        for(int i=0;i<simulateBodies.Count;i++)
        {
            for(int j=0;j<simulateBodies.Count;j++)
            {
                if(i!=j)
                {
                    SimulateBody other = simulateBodies[j];
                    SimulateBody source = simulateBodies[i];
                    Vector3 gravForce = source.GetGravitationalForceFor(other);
                    other.totalForce += gravForce;
                }
            }
        }
        foreach ( var source in simulateBodies)
        {
            source.velocity += (source.totalForce / source.mass) * UniverseConstants.fixedTimeStep;
            source.position += source.velocity * UniverseConstants.fixedTimeStep;
        }
    }



    private void OnDrawGizmos()
    {
        if (positions == null) return;
        for(int i = 0; i < positions.Count; i++)
        {
            var path = positions[i];
            for (int j = 1; j < path.Count; j++)
            {
                Gizmos.DrawLine(path[j - 1], path[j]);
            }
        }
    }
}
