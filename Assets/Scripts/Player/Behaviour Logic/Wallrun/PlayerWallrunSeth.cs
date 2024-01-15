using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wallrun-Seth", menuName = "Player Logic/Wallrun Logic/WallRunSeth")]
public class PlayerWallrunSeth : PlayerWallrunSOBase
{
    [SerializeField] private LayerMask wallLayer;
    
    [SerializeField] private float jumpCooldown = 0.25f;


    [SerializeField] private float horizontalSpeedDecreaseMultiplier;
    
    [SerializeField] private float idealVerticalVelocity;
    [SerializeField] private float verticalSpeedConvergentMultiplier;
    
    [SerializeField] private float initialVelocityMultiplier;

    [SerializeField] private float minimumVelocityToWallRun;
    [SerializeField] private float slowTimeBeforeDropping;

    [SerializeField] private float jumpLagTime;
    private float jumpLagTimer;

    [Header("Upward Wall Jump")]
    [SerializeField] private float maximumDotToTrigger_UWJ;
    [SerializeField] private float outwardMultiplier_UWJ;
    [SerializeField] private float upwardMultiplier_UWJ;
    [SerializeField] private float previousVelocityOverallMultiplier_UWJ;

    [Header("Outward Wall Jump")]
    [SerializeField] private float minimumDotToTrigger_OWJ;
    [SerializeField] private float outwardMultiplier_OWJ;
    [SerializeField] private float upwardMultiplier_OWJ;
    [SerializeField] private float previousVelocityOverallMultiplier_OWJ;

    [Header("Forward Wall Jump")]
    [SerializeField] private float outwardMultiplier_FWJ;
    [SerializeField] private float upwardMultiplier_FWJ;
    [SerializeField] private float previousVelocityOverallMultiplier_FWJ;


    private float dropTimer;

    private Vector3 initialVelocity;
    
    private bool readyToJump;
    private bool jumping;
    private bool jumped;
        
    private bool isWallRight, isWallLeft;
    private RaycastHit wallRight, wallLeft;
    private RaycastHit activeWall;
    

    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
    }


    public override void DoUpdateState()
    {
        GetInput();
        WallDirection();

        base.DoUpdateState();

        if (jumped == true) {
            jumpLagTimer += Time.deltaTime;
            if (jumpLagTimer > jumpLagTime) {
                WallJump();
                jumpLagTimer = 0;
            }

        }
    }

    public override void DoFixedUpdateState()
    {
        if (jumped) {
            return;
        }

        if (jumping && readyToJump)
        {
            
            readyToJump = false;
            jumped = true;
            //WallJump();
            
            stateMachine.timeOfLastJump = Time.time;
            return;
        }

        Vector3 projectedVector = Vector3.ProjectOnPlane(rb.velocity, activeWall.normal);

        Vector3 velocityVector = new Vector3(projectedVector.x, rb.velocity.y, projectedVector.z);
        if (!rb.useGravity) {
            velocityVector.y += (idealVerticalVelocity - velocityVector.y) * verticalSpeedConvergentMultiplier * Time.deltaTime;
        }

        //Slowing down the speed over time while on wall
        //Debug.Log("Current velocity vector: " + velocityVector);
        velocityVector -= new Vector3(velocityVector.x, 0, velocityVector.z) * horizontalSpeedDecreaseMultiplier * Time.deltaTime;
        //Debug.Log("New velocity vector: " + velocityVector);

        rb.velocity = velocityVector;

        //If the player is too slow they will drop from the wall after a brief grace period
        if (rb.velocity.magnitude < minimumVelocityToWallRun)
        {
            dropTimer += Time.deltaTime;
        }
        else {
            dropTimer = 0;
        }

        base.DoFixedUpdateState();
    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        //sprinting = playerInputActions.Player.Sprint.ReadValue<float>() == 1f;
        jumping = playerInputActions.Player.Jump.ReadValue<float>() == 1f;

        if (Input.GetKey(KeyCode.W))
        {
            rb.useGravity = false;
        }
        else {
            rb.useGravity = true;
        }

        if (stateMachine.timeOfLastJump + jumpCooldown < Time.time)
             readyToJump = true;
    }

    public override void CheckTransitions()
    {
        if (stateMachine.GroundedCheck()) {
            if (inputVector == Vector2.zero) {
                stateMachine.ChangeState(stateMachine.IdleState);
            }
            else {
                stateMachine.ChangeState(stateMachine.MovingState);
            }
        }

        if (!WallDirection() || (dropTimer > slowTimeBeforeDropping)) {
            stateMachine.ChangeState(stateMachine.AirborneState);
        }

        
    }


    public override void DoEnterLogic()
    {
        initialVelocity = rb.velocity;
        jumped = false;
        jumpLagTimer = 0;
        dropTimer = 0;

        WallDirection();

        Vector3 wallNormal = activeWall.normal;
        Vector3 projectedVector = Vector3.ProjectOnPlane(initialVelocity, wallNormal);

        //Maintain full speed but stuck to wall
        rb.velocity = projectedVector.normalized * initialVelocity.magnitude * initialVelocityMultiplier;

        
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        rb.useGravity = true;
        base.DoExitLogic();
    }


    public override void ResetValues()
    {
        base.ResetValues();
    }

    private void WallJump()
    {

        //I am very sorry for this but it was the best way I could think of

        //Essentially checking if the key being pressed in the direction of the wall, and if so jump up

        //Else if a key is being pressed jump out

        //else jump forward

        if ((Input.GetKey(KeyCode.A) && activeWall.point == wallLeft.point) || (Input.GetKey(KeyCode.D) && activeWall.point == wallRight.point))
        {
            //Upward Wall Jump
            rb.velocity = (activeWall.normal * outwardMultiplier_UWJ + Vector3.up * upwardMultiplier_UWJ) * (1 + (rb.velocity.magnitude * previousVelocityOverallMultiplier_UWJ));
            return;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            //Outward Wall Jump
            rb.velocity = (activeWall.normal * outwardMultiplier_OWJ + Vector3.up * upwardMultiplier_OWJ) * (1 + (rb.velocity.magnitude * previousVelocityOverallMultiplier_OWJ));
            return;
        }
        else {
            //Forward Wall Jump
            rb.velocity = rb.velocity * previousVelocityOverallMultiplier_FWJ + activeWall.normal * outwardMultiplier_FWJ + Vector3.up * upwardMultiplier_FWJ;
            return;
        }


        
/*

        //Temp values, will probably include current speed to allow for good speed tech

        //Player is looking away from wall - Give them a big jump in the direction of wall
        if (Vector3.Dot(activeWall.normal, stateMachine.playerObj.forward) > minimumDotToTrigger_OWJ)
        {
            Debug.Log("Wall jump out");
            rb.velocity = (activeWall.normal * outwardMultiplier_OWJ + Vector3.up * upwardMultiplier_OWJ) * (1 + (rb.velocity.magnitude * previousVelocityOverallMultiplier_OWJ));
            return;
        }

        
        //Player is looking in - Give them a large vertical jump and away from wall
        if (Vector3.Dot(activeWall.normal, stateMachine.playerObj.forward) < maximumDotToTrigger_UWJ)
        {
            Debug.Log("Wall jump up");
            rb.velocity = (activeWall.normal * outwardMultiplier_UWJ + Vector3.up * upwardMultiplier_UWJ) * (1 + (rb.velocity.magnitude * previousVelocityOverallMultiplier_UWJ));
            return;
        }

        //Player is looking forward - Give them a boost in their current direction and away from wall
        Debug.Log("Wall jump forward");
        rb.velocity = rb.velocity * previousVelocityOverallMultiplier_FWJ + activeWall.normal * outwardMultiplier_FWJ + Vector3.up * upwardMultiplier_FWJ;
        */

    }

    IEnumerator WallJumpDelay() {
        Vector3 storedVelocity = rb.velocity;
        rb.velocity = Vector3.zero;
        yield return new WaitForSeconds(jumpLagTime);
        WallJump();
    }
   

    private bool WallDirection()
    {
        //isWallRight = Physics.Raycast(stateMachine.player.position, stateMachine.playerObj.right, out wallRight, 2f, wallLayer);
        //isWallLeft = Physics.Raycast(stateMachine.player.position, -stateMachine.playerObj.right, out wallLeft, 2f, wallLayer);

        float raycastDistance = 1.5f; // Adjust this distance based on your needs
        float raycastAngle = 10f;   // Adjust the angle of rotation based on your needs

        // Rotate right ray direction
        Vector3 rightRayDirection = Quaternion.Euler(0, -raycastAngle, 0) * stateMachine.playerObj.right;
        isWallRight = Physics.Raycast(stateMachine.player.position, rightRayDirection, out wallRight, raycastDistance, wallLayer);

        // Rotate left ray direction
        Vector3 leftRayDirection = Quaternion.Euler(0, raycastAngle, 0) * -stateMachine.playerObj.right;
        isWallLeft = Physics.Raycast(stateMachine.player.position, leftRayDirection, out wallLeft, raycastDistance, wallLayer);

        Debug.DrawRay(stateMachine.player.position, leftRayDirection * raycastDistance, Color.blue);
        Debug.DrawRay(stateMachine.player.position, rightRayDirection * raycastDistance, Color.blue);


        if (isWallRight == true)
        {
            activeWall = wallRight;
        }
        else if (isWallLeft == true)
        {
            activeWall = wallLeft;
        }
        else {
            //activeWall = null;
        }

        return (isWallRight || isWallLeft);

        
    }



   


}
