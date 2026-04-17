using UnityEngine;

[ExecuteAlways] 
public class UniverseConstants : MonoBehaviour
{
    [SerializeField] private float _gravitationalConstant = 1f;
    [SerializeField] private float _fixedTimeStep = 0.02f;

    public static float gravitationalConstant = 1f;
    public static float G => gravitationalConstant;

    public static float fixedTimeStep = 0.02f;

    private void OnValidate()
    {
        UpdateStaticValues();
    }

    private void Awake()
    {
        UpdateStaticValues();
    }

    private void UpdateStaticValues()
    {
        gravitationalConstant = _gravitationalConstant;
        fixedTimeStep = _fixedTimeStep;
        Time.fixedDeltaTime = UniverseConstants.fixedTimeStep;
    }
}