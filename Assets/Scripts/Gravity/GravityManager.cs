using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class GravityManager : MonoBehaviour
{

    public static GravityManager Instance { get; private set; }

    public bool runSimulation = false;
    List<GravitySource> gravitySources = new List<GravitySource>();


    private void Awake()
    {
        if(Instance != null && Instance!=this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Time.fixedDeltaTime = UniverseConstants.fixedTimeStep;

    }


    private void FixedUpdate()
    {

        if (runSimulation)
        {
            UpdateGravity(gravitySources);
        }
        else
        {
            foreach (var source in gravitySources)
                source.Freeze();
        }
    }


    public void UpdateGravity(List<GravitySource> gravitySources )
    {
        foreach (var source in gravitySources)
            source.totalForce = Vector3.zero;

        foreach (var source in gravitySources)
        {
            source.Unfreeze();
            foreach (var target in gravitySources)
            {
                if (target != source)
                {
                    source.Attract(target);
                }
            }
        }

        foreach (var source in gravitySources)
        {
            source.UpdateVelocity();
        }
        
    }

    public void Register(GravitySource gravitySource)
    {
        gravitySources.Add(gravitySource);
    }

    public void Deregister(GravitySource gravitySource)
    {
        gravitySources.Remove(gravitySource);
    }

    public List<GravitySource> GetSources() { return gravitySources; }
    private void OnDrawGizmos()
    {
    }

}
