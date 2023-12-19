using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wallrun-Vinh", menuName = "Player Logic/Wallrun Logic/WallRunVinh")]
public class PlayerWallrunTest : PlayerWallrunSOBase
{
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private bool isWallLeft, isWallRight, isWallRunning;
    [SerializeField] private float wallRunForce = 1f;
    [SerializeField] private float maxRunSpeed = 10f;
    [SerializeField] private float maxRunTime = 3f;
    [SerializeField] private float jumpForce = 10f;
    public float maxWallRunCameraTilt, wallRunCameraTilt;
    Vector3 direction = Vector3.zero;
    Vector3 groundNormal = Vector3.up;

    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
    }

    public override void CheckTransitions()
    {
        base.CheckTransitions();
    }

    public override void DoEnterLogic()
    {
        stateMachine.ChangeState(stateMachine.WallrunState);
        Debug.Log("Wallrun");
        StartWallrun();
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        StopWallrun();
        stateMachine.ChangeState(stateMachine.AirborneState);
        base.DoExitLogic();
    }

    public override void DoFixedUpdateState()
    {
        StartWallrun();
        base.DoFixedUpdateState();
    }

    public override void DoUpdateState()
    {
        WallRunInput();
        CheckForWall();
        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        base.ResetValues();

    }

    void StartWallrun()
    {
        rb.useGravity = false;
        isWallRunning = true;

        if (rb.velocity.magnitude <= maxRunSpeed) {
            rb.AddForce(Time.deltaTime * wallRunForce * stateMachine.orientation.forward);

            if (isWallRight)
                rb.AddForce(stateMachine.orientation.right * wallRunForce / 5f * Time.deltaTime);
            else
                rb.AddForce(-stateMachine.orientation.right * wallRunForce / 5f * Time.deltaTime);
        }
    }

    void StopWallrun()
    {
        rb.useGravity = true;
        isWallRunning = false;
        stateMachine.ChangeState(stateMachine.AirborneState);
    }


    void WallJump()
    {
        if (isWallRunning)
        {

            //normal jump
            if (isWallLeft && !Input.GetKey(KeyCode.D) || isWallRight && !Input.GetKey(KeyCode.A))
            {
                rb.AddForce(Vector2.up * jumpForce * 1.5f);
                rb.AddForce(stateMachine.orientation.forward * jumpForce * 0.5f);
            }

            //sidwards wallhop
            if (isWallRight || isWallLeft && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) rb.AddForce(-stateMachine.orientation.up * jumpForce * 1f);
            if (isWallRight && Input.GetKey(KeyCode.A)) rb.AddForce(-stateMachine.orientation.right * jumpForce * 3.2f);
            if (isWallLeft && Input.GetKey(KeyCode.D)) rb.AddForce(stateMachine.orientation.right * jumpForce * 3.2f);

            //Always add forward force
            rb.AddForce(stateMachine.orientation.forward * jumpForce * 1f);

            //reset jump
            //ResetValues();
        }

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(stateMachine.player.position, Vector3.forward);
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
        if ((Input.GetKey(KeyCode.D) && isWallRight) || (Input.GetKey(KeyCode.A) && isWallLeft)) StartWallrun();
        if (Input.GetKeyDown(KeyCode.Space) && isWallRunning) WallJump();
    }
    
    void CheckForWall() {
        isWallLeft = Physics.Raycast(stateMachine.player.position, -stateMachine.orientation.right, 1f, wallLayer);
        isWallRight = Physics.Raycast(stateMachine.player.position, stateMachine.playerObj.right, 1f, wallLayer);

        if (!isWallLeft && !isWallRight) StopWallrun();
    }

}
