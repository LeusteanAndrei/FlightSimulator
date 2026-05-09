using UnityEngine;

public class OrbitBurns : MonoBehaviour
{

    [SerializeField] Engine forwardEngine;
    [SerializeField] Engine upEngine;
    [SerializeField] RCSManager rcsManager;
    [SerializeField] GravitySource gravitySource;
    [SerializeField] GravitationalOrientation orientation;


    [Header("Controls")]
    [SerializeField] bool retrogradeBurn = false;
    [SerializeField] bool progradeBurn = false;
    [SerializeField] bool radialOutBurn = false;
    [SerializeField] bool radialInBurn = false;

    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 direction = Vector3.zero;

        if (retrogradeBurn)
        {
            direction = gravitySource.rigidBody.linearVelocity.normalized;
        }
        else if (progradeBurn)
        {
            
            direction = -gravitySource.rigidBody.linearVelocity.normalized;
        }
        else if (radialInBurn)
        {
            direction = gravitySource.totalForce.normalized;
        }
        else if (radialOutBurn)
        {
            direction = -gravitySource.totalForce.normalized;
        }
        if (direction != Vector3.zero)
        {
            rcsManager.RotateTowardsVector(direction);
            if ((orientation.Forward() - direction).magnitude < 0.01)
                forwardEngine.Activate();
            else
                forwardEngine.Deactivate();
        }
        else
        {
            //forwardEngine.Deactivate();x
        }
    }

    void Update()
    {
    }
}
