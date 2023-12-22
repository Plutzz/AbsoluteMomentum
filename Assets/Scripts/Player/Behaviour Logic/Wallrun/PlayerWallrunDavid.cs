using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wallrun-David", menuName = "Player Logic/Wallrun Logic/David")]
public class PlayerWallrunDavid : PlayerWallrunSOBase
{
    [Header("WallRun")]
    // [SerializeField] private LayerMask whatIsWall;
    // [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float wallRunSpeed;
    [SerializeField] private float wallRunForce = 200;
    [SerializeField] private float maxWallRunTime;
    private float wallRunTimer;

    // [Header("Detection")]
    // [SerializeField] private float wallCheckDistance;
    // [SerializeField] private float minHeight;
    // private RaycastHit leftSideWall;
    // private RaycastHit rightSideWall;
    // private bool wallLeft;
    // private bool wallRight;


    public override void CheckTransitions()
    {
        base.CheckTransitions();
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        rb.useGravity = false;
        Debug.Log("gravity = false");
        wallRunSpeed = stateMachine.desiredMoveSpeed;
    }

    public override void DoExitLogic()
    {
        //rb.useGravity = true;
        base.DoExitLogic();
    }

    public override void DoFixedUpdateState()
    {
        if(stateMachine.WallRunning())
        {
            WallRun();
        } else 
        {
            stateMachine.ChangeState(stateMachine.MovingState);
        }
        Debug.Log("wallrunning");
        base.DoFixedUpdateState();

        //if(NotOnWall)
        //{
        //    StopWallrun();
        //}
    }

    public override void DoUpdateState()
    {
        base.DoUpdateState();
        stateMachine.WallCheck();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    private void WallRun()
    {
        rb.useGravity = false;
        Debug.Log("gravity = false");
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);


        Vector3 wallNormal = stateMachine.wallRight ? stateMachine.rightSideWall.normal : stateMachine.leftSideWall.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, stateMachine.player.up);

        if((stateMachine.orientation.forward - wallForward).magnitude > (stateMachine.orientation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        //forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //push to wall force
        if(!(stateMachine.wallLeft && Input.GetKey(KeyCode.A)) && !(stateMachine.wallRight && Input.GetKey(KeyCode.D)))
        {
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
        }
    }

    // private void CheckForWall()
    // {
    //     wallRight = Physics.Raycast(gameObject.transform.position, gameObject.transform.right, out rightSideWall, wallCheckDistance, whatIsWall);
    //     wallLeft = Physics.Raycast(gameObject.transform.position, -gameObject.transform.right, out leftSideWall, wallCheckDistance, whatIsWall);
    // }

    // private bool AboveGround()
    // {
    //     return !Physics.Raycast(gameObject.transform.position, Vector3.down, minJumpHeight, whatIsGround);
    // }
}
