using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wallrun-Vinh", menuName = "Player Logic/Wallrun Logic/WallRunVinh")]
public class PlayerWallrunVinh : PlayerWallrunSOBase
{
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private bool isWallRunning;
    [SerializeField] private float wallRunForce = 1f;
    [SerializeField] private float maxRunSpeed = 10f;
    [SerializeField] private float maxRunTime = 3f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private bool sprinting, jumping;

    public float maxWallRunCameraTilt, wallRunCameraTilt;
    Vector3 direction = Vector3.zero;
    Vector3 groundNormal = Vector3.up;

    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
    }


    public override void DoUpdateState()
    {
        Debug.Log(isWallRunning);
        GetInput();
        base.DoUpdateState();
    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        sprinting = playerInputActions.Player.Sprint.ReadValue<float>() == 1f;
        jumping = playerInputActions.Player.Jump.ReadValue<float>() == 1f;
    }

    public override void CheckTransitions()
    {
        if (stateMachine.WallCheck()) return;

        // Player is grounded and moving
        if (playerInputActions.Player.Movement.ReadValue<Vector2>() != Vector2.zero && !stateMachine.WallCheck()) stateMachine.ChangeState(stateMachine.MovingState);
        // Player is grounded and not moving
        else if (playerInputActions.Player.Movement.ReadValue<Vector2>() == Vector2.zero || !stateMachine.WallCheck()) stateMachine.ChangeState(stateMachine.IdleState);
        // Player is jumping off the wall
        else if (playerInputActions.Player.Jump.ReadValue<float>() == 1f || !sprinting) stateMachine.ChangeState(stateMachine.AirborneState);
    }


    public override void DoEnterLogic()
    {
        rb.useGravity = false;
        isWallRunning = true;
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        rb.useGravity = true;
        isWallRunning = false;
        base.DoExitLogic();
    }

    public override void DoFixedUpdateState()
    {
        base.DoFixedUpdateState();
    }


    public override void ResetValues()
    {
        base.ResetValues();
    }

    void WallJump()
    {
        //if (isWallRunning)
        //{

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
        //}

    }

    Vector3 RotateToWall(Vector3 direction, Vector3 wallNormal)
    {
        Vector3 rotDir = Vector3.ProjectOnPlane(wallNormal, Vector3.up);
        Quaternion rotation = Quaternion.AngleAxis(-90f, Vector3.up);
        rotDir = rotation * rotDir;
        float angle = -Vector3.Angle(Vector3.up, wallNormal);
        rotation = Quaternion.AngleAxis(angle, rotDir);
        direction = rotation * direction;
        return direction;
    }

    void WallRunInput()
    {
        //if ((Input.GetKey(KeyCode.D) && stateMachine.WallCheck()) || (Input.GetKey(KeyCode.A) && stateMachine.WallCheck())) StartWallrun();
        //if (Input.GetKeyDown(KeyCode.Space) && isWallRunning) WallJump();
        //if (Input.GetKey(KeyCode.S) && isWallRunning) DoExitLogic();

    }

}
