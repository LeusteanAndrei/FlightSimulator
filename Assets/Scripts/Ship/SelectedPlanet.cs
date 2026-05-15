using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SelectedPlanet : MonoBehaviour
{
    public PlanetScript Planet;
    public bool goTowards;

    public RCSManager rcsManager;
    public GravitationalOrientation orientation;
    public Engine forwardEngine;
    public Rigidbody rb;

    public float maxDist = 5000f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
            if (StateManager.GoingTowards)
            {
                Vector3 dir = -this.transform.position + Planet.transform.position;
                float distance = dir.magnitude;

                dir = dir.normalized;
                rcsManager.RotateTowardsVector(dir);
                if (Vector3.Dot(dir, orientation.Forward()) > 0.99f)
                {
                    if (distance < maxDist + Planet.shapeSettings.planetRadius)
                    {

                        float speedInDir = Vector3.Dot(this.rb.linearVelocity, dir);
                        if (speedInDir > 10.0)
                        {
                        //forwardEngine.reverse = true;
                        forwardEngine.Reverse();
                            forwardEngine.Activate();
                        }
                        else if (speedInDir < -10.0f)
                        {
                        forwardEngine.Unreverse();
                            //forwardEngine.reverse = false;
                            forwardEngine.Activate();
                        }
                        else
                        {
                        forwardEngine.Unreverse();
                            //forwardEngine.reverse = false;
                            forwardEngine.Deactivate();
                        }

                    }
                    else
                        forwardEngine.Activate();

                }
                else
                {
                    forwardEngine.Deactivate();
                }
            }
    }
}
