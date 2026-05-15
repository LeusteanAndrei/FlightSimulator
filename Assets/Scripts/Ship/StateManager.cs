using UnityEngine;

public class StateManager : MonoBehaviour
{

    public static bool ByUser = true;
    public static bool GoingTowards;
    public static bool MaintainRotation;

    public static PlanetScript ps;
    public static GravitySource gs;
    public Engine forward, back;
    public RCSManager manager;
    public OrbitScript orbit;
    public SelectedPlanet sp;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (StateManager.ps != null)
        {
            orbit.targetPlanet = gs;
            sp.Planet = ps;

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ByUser = !ByUser;
                GoingTowards = !GoingTowards;
                manager.StopAll();
                forward.Deactivate();
                back.Deactivate();
                forward.Unreverse();
                back.Unreverse();
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                MaintainRotation = !MaintainRotation;
                manager.StopAll();
            }
        }
    }
}
