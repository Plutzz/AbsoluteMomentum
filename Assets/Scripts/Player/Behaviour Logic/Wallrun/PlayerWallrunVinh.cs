using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wallrun-Vinh", menuName = "Player Logic/Wallrun Logic/WallRunVinh")]
public class PlayerWallrunVinh : PlayerWallrunSOBase
{
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallRunForce = 15f;
    [SerializeField] private float maxRunSpeed = 20f;
    [SerializeField] private float wallClimbSpeed = 20f;
    [SerializeField] private float jumpForce = 10f;
    private bool isWallRunning;
    private bool sprinting, jumping, crouching;
    private bool isWallRight, isWallLeft;
    private bool reachedMaxSpeed;
    private RaycastHit wallRight, wallLeft;

    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
    }


    public override void DoUpdateState()
    {
        GetInput();
        WallDirection();
        // MovementSpeedHandler();
        base.DoUpdateState();
    }

    public override void DoFixedUpdateState()
    {
        WallRun();
        // SpeedControl();
        WallJump();

        base.DoFixedUpdateState();
    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        sprinting = playerInputActions.Player.Sprint.ReadValue<float>() == 1f;
        jumping = playerInputActions.Player.Jump.ReadValue<float>() == 1f;
        crouching = playerInputActions.Player.Crouch.ReadValue<float>() == 1f;
    }

    public override void CheckTransitions()
    {
        // Player is moving in the opposite direction of the wall
        // if ((playerInputActions.Player.Movement.ReadValue<Vector2>() == Vector2.left && isWallLeft)
        //        || playerInputActions.Player.Movement.ReadValue<Vector2>() == Vector2.right && isWallRight)
        //     stateMachine.ChangeState(stateMachine.MovingState);
        // Player is not on wall or not moving
        if (!stateMachine.WallCheck() && inputVector != Vector2.zero) stateMachine.ChangeState(stateMachine.MovingState);
        else if (inputVector == Vector2.zero && (stateMachine.GroundedCheck() || !stateMachine.WallCheck())) stateMachine.ChangeState(stateMachine.IdleState);
    }


    public override void DoEnterLogic()
    {
        rb.useGravity = false;
        isWallRunning = true;
        stateMachine.desiredMoveSpeed = maxRunSpeed;
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        rb.useGravity = true;
        isWallRunning = false;
        base.DoExitLogic();
    }


    public override void ResetValues()
    {
        base.ResetValues();
    }

    private void WallJump()
    {
        // if (isWallRunning)
        // {

        //    //normal jump
        //    if (isWallLeft && !Input.GetKey(KeyCode.D) || isWallRight && !Input.GetKey(KeyCode.A))
        //    {
        //        rb.AddForce(Vector2.up * jumpForce * 1.5f);
        //        rb.AddForce(stateMachine.orientation.forward * jumpForce * 0.5f);
        //    }

        //    //sidwards wallhop
        //    if (isWallRight || isWallLeft && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) rb.AddForce(-stateMachine.orientation.up * jumpForce * 1f);
        //    if (isWallRight && Input.GetKey(KeyCode.A)) rb.AddForce(-stateMachine.orientation.right * jumpForce * 3.2f);
        //    if (isWallLeft && Input.GetKey(KeyCode.D)) rb.AddForce(stateMachine.orientation.right * jumpForce * 3.2f);

        //    //Always add forward force
        //    rb.AddForce(stateMachine.orientation.forward * jumpForce * 1f);

        //    //reset jump
        //    //ResetValues();
        // }

    }
    private void SpeedControl()
    {
        // limit velocity on wall
        if (rb.velocity.magnitude > maxRunSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxRunSpeed;
        }
    }
    private void WallRun()
    {
        rb.useGravity = false;
        Vector3 wallNormal = isWallRight ? wallRight.normal : wallLeft.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, stateMachine.playerObj.transform.up);

        if ((stateMachine.playerObj.forward - wallForward).magnitude > (stateMachine.playerObj.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        if (rb.velocity.magnitude <= maxRunSpeed)
        {
            if (inputVector.x > 0 && isWallRight || inputVector.x < 0 && isWallLeft)
                rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
            // up using w
            if (inputVector.y > 0)
                rb.AddForce(stateMachine.orientation.up * wallClimbSpeed);
            else
                rb.AddForce(-rb.velocity.normalized * 2f); // Decrease speed when not inputting up
        }
        if (!(isWallLeft && inputVector.x < 0) && !(isWallRight && inputVector.x > 0))
            rb.AddForce(-wallNormal * 200, ForceMode.Force);
    }

    private void WallDirection()
    {
        isWallRight = Physics.Raycast(stateMachine.player.position, stateMachine.playerObj.right, out wallRight, 2f, wallLayer);
        isWallLeft = Physics.Raycast(stateMachine.player.position, -stateMachine.playerObj.right, out wallLeft, 2f, wallLayer);

    }
    void RotateToWall(RaycastHit hit)
    {
        Vector3 directionAlongWall = -hit.normal;
        Quaternion rotation = Quaternion.LookRotation(directionAlongWall, Vector3.up);
        Vector3 eulerRotation = rotation.eulerAngles;
        eulerRotation.z += 45;
        stateMachine.player.rotation = Quaternion.Euler(eulerRotation);
    }
}
