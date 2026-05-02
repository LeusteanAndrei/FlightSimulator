using UnityEngine;

public class RCSManager : MonoBehaviour
{

    /*
     * The naming of each Thruster Represents tha position on the spaceship therefore when activated they will produce a force opposite to their position.
     * example: 
     *      the left back thruster over the y axis is situated on the left side of the ship in the back. 
     *      upon activation it will shoot out a force which presses on the left back side of the ship towwards the ship
     *      meaning it will "rotate" the ship to the right ( clockwise ) taking as pivot the left back position of the thruster
     *      
     *    Use case:
     *    
     *       if i wanted to rotate the ship over say the z axis i look at the z axis thrusters.
     *    
     *       there are four:
     *          rightUp -> rotates the ship counter clockwise around the z axis with the right up position as a pivot
     *          leftUp -> rotates the ship clockwise around the z axis with the left up position as a pivot
     *          leftDown -> rotates the ship counter clockwise around the z axis with the left down position as a pivot
     *          rightDown -> rotates the ship clockwise around the z axis with the right down position as a pivot
     *    
     *       therefore if i just wanted to rotate the ship clockwise around the z axis , keeping it in its place i would activate simultaneously:
     *           leftUp and rightDown thrusters
     *       analogue for couunter clockwise :
     *           leftDowna and rightUp thrusters
     *       
     *    It's the same logic for each thruster
     *      
     *   
     * 
     * 
     * */


    /**
     * All of these notations take into account the viewere watches the ship projected onto the other 2 axis and pointed towards the positive horizontal axis looking at the top of the ship
     */
    [Header("Rotation around the y axis")]
    [SerializeField] ThrustEngine yLeftBackTruster; // counter clockwise ( toward the left )
    [SerializeField] ThrustEngine yRightBackTruster; //  clockwise ( towards the right )
    [SerializeField] ThrustEngine yLeftFrontTruster; // clockwise ( towards the right )
    [SerializeField] ThrustEngine yRightFrontTruster; // counter clockwiser ( towards the left )

    [Header("Rotation around the x axis")]
    [SerializeField] ThrustEngine xFrontUpThruster; // clockwise ( forward )
    [SerializeField] ThrustEngine xFrontDownThruster; // counter clockwise ( backward ) 
    [SerializeField] ThrustEngine xBackUpThruster; // counter clockwise ( backward )
    [SerializeField] ThrustEngine xBackDownThruster; // clockwise ( forward )

    [Header("Rotation around the z azis")]
    [SerializeField] ThrustEngine zRightUpThruster; // counter clockwise
    [SerializeField] ThrustEngine zRightDownThruster; // clockwise
    [SerializeField] ThrustEngine zLeftUpThruster; // clockwise
    [SerializeField] ThrustEngine zLeftDownThruster; // counter clockwise

    [Header("Others")]
    [SerializeField] Rigidbody shipRigidBody;
    [SerializeField] Vector3 LookTowards;
    [SerializeField] bool lookTowardVector;
    [SerializeField] bool unityRotate;
    [SerializeField] float threshold = 0.1f;

    GravitationalOrientation gravitationalOrientation;
    void Start()
    {
        gravitationalOrientation = GetComponent<GravitationalOrientation>();
        if (shipRigidBody == null)
            GameInstance.Instance.SpaceShip().RigidBody();
    }

    public void xAxisStop()
    {
        xFrontDownThruster.Stop();
        xBackUpThruster.Stop();
        xFrontUpThruster.Stop();
        xBackDownThruster.Stop();
    }
    public void yAxisStop()
    {
        yLeftBackTruster.Stop();
        yLeftFrontTruster.Stop();
        yRightBackTruster.Stop();
        yRightFrontTruster.Stop();
    }

    public void zAxisStop()
    {
        zLeftDownThruster.Stop();
        zLeftUpThruster.Stop();
        zRightDownThruster.Stop();
        zRightUpThruster.Stop();
    }

    public void RotateRight()
    {
        yRightBackTruster.Fire();
        yLeftFrontTruster.Fire();
    }
    public void RotateLeft()
    {
        yLeftBackTruster.Fire();
        yRightFrontTruster.Fire();
    }

    public void RotateForward()
    {
        xFrontUpThruster.Fire();
        xBackDownThruster.Fire();
    }
    public void RotateBackwards()
    {
        xFrontDownThruster.Fire();
        xBackUpThruster.Fire();
    }
    public void RotateSidewaysClockwise()
    {
        zRightDownThruster.Fire();
        zLeftUpThruster.Fire();
    }

    public void RotateSidewaysCounterClockwise()
    {
        zLeftDownThruster.Fire();
        zRightUpThruster.Fire();
    }

    public void StopAll()
    {
        yAxisStop();
        xAxisStop();
        zAxisStop();
    }

    public void RotateTowardsVector(Vector3 newOrientation)
    {
        if (newOrientation.sqrMagnitude < 0.0001f)
            return;

        Vector3 target = newOrientation.normalized;

        Vector3 forward = transform.forward;

        float currentY = Vector3.Dot(forward, gravitationalOrientation.Up());
        float targetY = Vector3.Dot(target, gravitationalOrientation.Up());
        float error = currentY - targetY;
        if(error > threshold)
            RotateForward();   
        else if (error < -threshold)
            RotateBackwards(); 
        else
            xAxisStop();

        float currentX = Vector3.Dot(forward, gravitationalOrientation.Right());
        float targetX = Vector3.Dot(target, gravitationalOrientation.Right());
        error  = currentX - targetX;
        if (error > threshold)
            RotateLeft();
        else if (error < -threshold)
            RotateRight();
        else
            yAxisStop();

        //float currentZ = Vector3.Dot(forward, Vector3.forward);
        //float targetZ = Vector3.Dot(target, Vector3.forward);
        //error = currentZ - targetZ;

        //if (error > threshold)
        //    RotateSidewaysClockwise();
        //else if (error < -threshold)
        //    RotateSidewaysCounterClockwise();
        //else
        //    zAxisStop();
    }

    void Update()
    {
    }
    private void FixedUpdate()
    {
        if (lookTowardVector)
            RotateTowardsVector(LookTowards);
        //else if(!UseKeyboard)
            //StopAll();
        if (unityRotate)
            shipRigidBody.MoveRotation(Quaternion.RotateTowards(
            shipRigidBody.rotation,
            Quaternion.LookRotation(LookTowards),
            10.0f * Time.fixedDeltaTime
        ));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(shipRigidBody.position, LookTowards);
        Vector3 Threshold = new Vector3(threshold, threshold, threshold);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(shipRigidBody.position, LookTowards - Threshold);
        Gizmos.DrawRay(shipRigidBody.position, LookTowards +Threshold);

    }
}
