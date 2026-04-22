using UnityEngine;

public class GameInstance : MonoBehaviour
{
    public static GameInstance Instance { get; private set; }

    [SerializeField] private string spaceShipTagName = "Player";
    private ShipScriptManager spaceShip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        spaceShip = GameObject.FindWithTag(spaceShipTagName).GetComponent<ShipScriptManager>();
    }
    void Start()
    {
        
    }

    public ShipScriptManager SpaceShip()
    {
        return spaceShip;
    }
    void Update()
    {
        
    }
}
