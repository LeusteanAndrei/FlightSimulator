using UnityEngine;

[RequireComponent(typeof(GravitySource))]
public class OrbitScript : MonoBehaviour
{
    [SerializeField]    GravitySource parentPlanet;
    [SerializeField]public  GravitySource targetPlanet;

    [SerializeField] GravitationalOrientation orientation;
    [Header("Triggers")]
    [SerializeField] bool startOrbit = false;
    [SerializeField] bool maintainRoll = true;
    [SerializeField] bool mantainHeight = false;
    [SerializeField] bool enterOrbit = false;
    [Header("Orbit info")]
    [SerializeField] float orbitHeight = 10f;
    [SerializeField] public float pGain = 2f; 
    [SerializeField] public float dGain = 1f;
    [SerializeField] float threshold = 0.5f;


    [Header("Engines")]
    [SerializeField] Engine upEngine;
    [SerializeField] Engine forwardEngine;
    [SerializeField] RCSManager rcs;

    [SerializeField] GravitySource thisPlanet;

    private float lastError = 0f;

    private void Awake()
    {
        thisPlanet = GetComponent<GravitySource>();
    }

    public void Update()
    {
     
        //if(startOrbit == false)
        //{
        //    newOrbit = false;
        //    isOrbitting = false;
        //}
        //else
        //{
        //    if (isOrbitting == false)
        //    {
        //        newOrbit = true;
        //    }
        //    else
        //    {
        //        newOrbit = false;
        //    }
        //}
    }

    void MaintainOrientation()
    {
        if (orientation == null)
        {
            return;
        }

        Vector3 towardPlanet =
            (targetPlanet.transform.position - thisPlanet.transform.position).normalized;
        Vector3 up = -towardPlanet;
        Vector3 forwardOrient =
            Vector3.ProjectOnPlane(
                this.transform.forward,
                up
            ).normalized;

        if (forwardOrient.sqrMagnitude < 0.001f)
        {
            forwardOrient = Vector3.Cross(transform.right, up).normalized;
        }

        float targetAngle =
            orientation.GetRollAngle(forwardOrient, up);

        rcs.RotateTowardsVector(forwardOrient);

        // Apply roll correction
        rcs.SetRoll(targetAngle);

        //Vector3 forwardOrient = orientation.GetForwardOrientation();
        //float targetAngle = orientation.GetRollAngle(forwardOrient, -thisPlanet.totalForce.normalized);
        //rcs.RotateTowardsVector(forwardOrient);
        //rcs.SetRoll(targetAngle);

    }

    public void FixedUpdate()
    {
        if(StateManager.MaintainRotation)
        {
            MaintainOrientation();
        }

        if (startOrbit)
        {
            if(maintainRoll)
                MaintainOrientation();
            if (mantainHeight)
                MaintainHeight(orbitHeight);
            if (mantainHeight && enterOrbit)
                SetVelocity();
        }
        else
        {
            //upEngine.Deactivate();
            //forwardEngine.Deactivate();
            //rcs.StopAll();
        }
    }


    void SetVelocity()
    {

        thisPlanet.rigidBody.linearVelocity = GetNecessaryVelocity();
        EnteredOrbit();
        return;
    }
    void MaintainHeight(float desiredHeight)
    {
        float distance = Vector3.Distance(thisPlanet.transform.position, targetPlanet.transform.position);
        float error = desiredHeight - distance;

        float errorRateOfChange = (error - lastError) / Time.fixedDeltaTime;
        lastError = error; 

        if (error > threshold)
        {
            float calculatedStrength = (error * pGain) + (errorRateOfChange * dGain);

            upEngine.SetForce(calculatedStrength);

            upEngine.Activate();
        }
        else
        {
            upEngine.Deactivate();
        }
    }

    void EnteredOrbit()
    {
        mantainHeight = false;
        startOrbit = false;
        maintainRoll = false;
        enterOrbit = false;
        upEngine.Deactivate();
        forwardEngine.Deactivate();
        rcs.StopAll();
    }

    public Vector3 GetNecessaryVelocity()
    {
        if (targetPlanet == null)
        {
            Debug.Log("No Planet to orbit");
            return Vector3.zero;
        }
        Vector3 distanceVector = thisPlanet.transform.position - parentPlanet.transform.position;

        float distance = distanceVector.magnitude;
        float necessarySpeed = Mathf.Sqrt(UniverseConstants.gravitationalConstant * targetPlanet.Mass / distance);

        Vector3 orbitNormal = orientation.Up();
        Vector3 velocityDirection = Vector3.Cross(orbitNormal, distanceVector).normalized;
        Vector3 velocity = velocityDirection*necessarySpeed;
        return velocity;

    }

    public GravitySource Planet()
    { return thisPlanet; }

    public void ConfigurePassiveOrbit(GravitySource centerBody, Vector3 orbitNormal, bool clockwise)
    {
        if (centerBody == null)
        {
            return;
        }

        if (thisPlanet == null)
        {
            thisPlanet = GetComponent<GravitySource>();
        }

        parentPlanet = centerBody;
        targetPlanet = centerBody;
        startOrbit = false;
        maintainRoll = false;
        mantainHeight = false;
        enterOrbit = false;

        Vector3 distanceVector = thisPlanet.transform.position - centerBody.transform.position;
        float distance = Mathf.Max(0.001f, distanceVector.magnitude);

        Vector3 normal = orbitNormal;
        if (normal.sqrMagnitude < 0.0001f)
        {
            normal = Vector3.up;
        }
        normal.Normalize();

        Vector3 velocityDirection = Vector3.Cross(normal, distanceVector).normalized;
        if (velocityDirection.sqrMagnitude < 0.0001f)
        {
            velocityDirection = Vector3.Cross(Vector3.up, distanceVector).normalized;
            if (velocityDirection.sqrMagnitude < 0.0001f)
            {
                velocityDirection = Vector3.Cross(Vector3.right, distanceVector).normalized;
            }
        }

        if (!clockwise)
        {
            velocityDirection *= -1f;
        }

        float speed = Mathf.Sqrt(UniverseConstants.gravitationalConstant * centerBody.Mass / distance);
        Vector3 velocity = velocityDirection * speed;

        Vector3 centerVelocity = Vector3.zero;
        if (centerBody != null && centerBody.rigidBody != null)
        {
            centerVelocity = centerBody.rigidBody.linearVelocity;
        }

        velocity += centerVelocity;
        thisPlanet.SetStartVelocity(velocity);
    }
    
    public Vector3 GetStartOrbitVelocity()
    {
        return Vector3.zero;
        if (parentPlanet == null || thisPlanet == null) return Vector3.zero;

        Vector3 distanceVector = thisPlanet.transform.position - parentPlanet.transform.position;
        float distance = (distanceVector).magnitude;
        float speed = Mathf.Sqrt(UniverseConstants.gravitationalConstant * parentPlanet.Mass / distance);


        Vector3 orbitNormal = Vector3.up; 
        Vector3 velocityDir = Vector3.Cross(orbitNormal, distanceVector).normalized;
        Vector3 velocity = velocityDir * speed;
        return velocity;
    }


}

