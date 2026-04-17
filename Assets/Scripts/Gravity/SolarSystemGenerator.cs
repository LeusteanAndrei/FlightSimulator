using UnityEngine;



public class PlanetData
{
    public string Name { get; set; } 
    public float RadiusPercentage{ get; set; } // relative to earth
    public float MassPercentage { get; set; } // relative to earth
    public float DistanceFromSun { get; set; } // in Au where 1 au = 149 597 871 kilometers 

    public Vector3 Velocity { get; set; }
}

public class PlanetInstance
{
    public PlanetData data;
    public GameObject obj;
}

public class SolarSystemGenerator : MonoBehaviour
{
    [SerializeField] GameObject planetPrefab;
    [SerializeField] float earthScale = 1.0f;
    [SerializeField] float earthMass = 1.0f;
    [SerializeField] float AUtoUnity = 10f;

    [SerializeField] public PlanetData[] planets = new PlanetData[]
{
    new PlanetData {
        Name = "Mercury",
        RadiusPercentage = 0.383f,
        MassPercentage = 0.0553f,
        DistanceFromSun = 0.387f,
        Velocity = new Vector3(0, 0, 47f)
    },

    new PlanetData {
        Name = "Venus",
        RadiusPercentage = 0.949f,
        MassPercentage = 0.815f,
        DistanceFromSun = 0.723f,
        Velocity = new Vector3(0, 0, 35f)
    },

    new PlanetData {
        Name = "Earth",
        RadiusPercentage = 1.0f,
        MassPercentage = 1.0f,
        DistanceFromSun = 1.0f,
        Velocity = new Vector3(0, 0, 29.8f)
    },

    new PlanetData {
        Name = "Mars",
        RadiusPercentage = 0.532f,
        MassPercentage = 0.107f,
        DistanceFromSun = 1.524f,
        Velocity = new Vector3(0, 0, 24f)
    },

    new PlanetData {
        Name = "Jupiter",
        RadiusPercentage = 10.97f,
        MassPercentage = 317.8f,
        DistanceFromSun = 5.204f,
        Velocity = new Vector3(0, 0, 13f)
    },

    new PlanetData {
        Name = "Saturn",
        RadiusPercentage = 9.14f,
        MassPercentage = 95.2f,
        DistanceFromSun = 9.583f,
        Velocity = new Vector3(0, 0, 9.7f)
    },

    new PlanetData {
        Name = "Uranus",
        RadiusPercentage = 4.01f,
        MassPercentage = 14.5f,
        DistanceFromSun = 19.218f,
        Velocity = new Vector3(0, 0, 6.8f)
    },

    new PlanetData {
        Name = "Neptune",
        RadiusPercentage = 3.88f,
        MassPercentage = 17.1f,
        DistanceFromSun = 30.07f,
        Velocity = new Vector3(0, 0, 5.4f)
    },

    new PlanetData {
        Name = "Sun",
        RadiusPercentage = 109.1f,
        MassPercentage = 332950f,
        DistanceFromSun = 0.0f,
        Velocity = Vector3.zero
    }
};

    private PlanetInstance[] instances = null;
    private float lastAU;

    void Start()
    {
        //SpawnPlanets();
        //ApplyScaling();
        lastAU = AUtoUnity;
    }

    void Update()
    {
        if (!Mathf.Approximately(lastAU, AUtoUnity))
        {
            ApplyScaling();
            lastAU = AUtoUnity;
        }
    }

    public void SpawnPlanets()
    {
        if (instances != null)
        {
            for (int i = 0; i < instances.Length; i++)
            {
                if (instances[i] != null && instances[i].obj != null)
                {
                    Destroy(instances[i].obj);
                }
            }
        }
        instances = new PlanetInstance[planets.Length];

        for (int i = 0; i < planets.Length; i++)
        {
            var planet = planets[i];

            GameObject obj = Instantiate(planetPrefab);
            obj.name = planet.Name;

            instances[i] = new PlanetInstance
            {
                data = planet,
                obj = obj
            };
        }
    }

    public void ApplyScaling()
    {
        foreach (var p in instances)
        {
            float distance = p.data.DistanceFromSun * AUtoUnity;

            p.obj.transform.position = new Vector3(distance, 0f, 0f);
            p.obj.transform.localScale = Vector3.one * p.data.RadiusPercentage * earthScale;

            GravitySource gs = p.obj.GetComponent<GravitySource>();
            if(gs)
            {
                gs.SetMass(earthMass * p.data.MassPercentage);
                gs.SetStartVelocity(p.data.Velocity);
            }

        }
    }
}