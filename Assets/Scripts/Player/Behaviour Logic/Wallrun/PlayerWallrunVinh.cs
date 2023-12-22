using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wallrun-Vinh", menuName = "Player Logic/Wallrun Logic/WallRunVinh")]
public class PlayerWallrunVinh : PlayerWallrunSOBase
{
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private bool isWallRunning;
    [SerializeField] private float wallRunForce = 5f;
    [SerializeField] private float maxRunSpeed = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private bool sprinting, jumping;
    private bool isWallRight, isWallLeft;
    private Vector3 moveDirection;

    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
    }


    public override void DoUpdateState()
    {
        GetInput();
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
    }

    public override void CheckTransitions()
    {
        // Player is moving in the opposite direction of the wall
        if ((playerInputActions.Player.Movement.ReadValue<Vector2>() == Vector2.left && isWallLeft)
               || playerInputActions.Player.Movement.ReadValue<Vector2>() == Vector2.right && isWallRight)
            stateMachine.ChangeState(stateMachine.MovingState);
        // Player is not on wall or not moving
        else if (playerInputActions.Player.Movement.ReadValue<Vector2>() == Vector2.zero || !stateMachine.WallCheck()) stateMachine.ChangeState(stateMachine.IdleState);
    }


    public override void DoEnterLogic()
    {
        Debug.Log("Do Enter Logic");
        moveDirection = Vector3.zero;
        rb.useGravity = false;
        isWallRunning = true;
        float climbForce = 10f;
        rb.AddForce(Vector3.up * climbForce, ForceMode.Impulse);
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
        WallDirection();
        Vector3 horizontalDir = stateMachine.playerObj.right * inputVector.x;
        Vector3 verticalDir = stateMachine.playerObj.forward * inputVector.y;
        float hMultiplier = 2f;
        float vMultiplier = 0.25f;

        Vector3 wallRunVelo = (horizontalDir * hMultiplier + verticalDir * vMultiplier).normalized * 10f;
        rb.AddForce(wallRunVelo, ForceMode.Force);

        if (rb.velocity.magnitude <= maxRunSpeed)
        {

            //Make sure char sticks to wall
            if (isWallRight)
            {
                rb.AddForce(-stateMachine.playerObj.right * wallRunForce, ForceMode.Force);
            }
            else if (isWallLeft)
            {
                rb.AddForce(stateMachine.playerObj.right * wallRunForce, ForceMode.Force);
            }
        }
    }

    private void WallDirection()
    {
        isWallRight = Physics.Raycast(stateMachine.player.position, stateMachine.playerObj.right, out RaycastHit right, 2f, wallLayer);
        isWallLeft = Physics.Raycast(stateMachine.player.position, -stateMachine.playerObj.right, out RaycastHit left, 2f, wallLayer);

        // if (isWallRight)
        //     RotateToWall(right);
        // else if (isWallLeft)
        //     RotateToWall(left);

        //leave wall run
        if (!isWallLeft && !isWallRight)
        {
            DoExitLogic();
        }
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
