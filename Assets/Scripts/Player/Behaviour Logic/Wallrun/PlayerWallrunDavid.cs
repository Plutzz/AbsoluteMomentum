using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wallrun-David", menuName = "Player Logic/Wallrun Logic/David")]
public class PlayerWallrunDavid : PlayerWallrunSOBase
{
    [Header("WallRun")]
    // [SerializeField] private LayerMask whatIsWall;
    // [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float wallRunSpeed = 20f;
    [SerializeField] private float wallRunForce = 1000f;
    [SerializeField] private float maxWallRunTime;
    private float wallRunSpeedx;
    private float wallRunSpeedy;
    private float wallRunTimer;


    public override void CheckTransitions()
    {
        base.CheckTransitions();
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFixedUpdateState()
    {
        stateMachine.WallCheck();

        //if(!stateMachine.WallRunning())
        {
            stateMachine.ChangeState(stateMachine.MovingState);
        }

        WallRun();
        base.DoFixedUpdateState();
    }

    public override void DoUpdateState()
    {
        MovementSpeedHandler();
        base.DoUpdateState();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }

    private void MovementSpeedHandler()
    {
        stateMachine.desiredMoveSpeed = wallRunSpeed;

        stateMachine.moveSpeed = stateMachine.desiredMoveSpeed;

        stateMachine.lastDesiredMoveSpeed = stateMachine.desiredMoveSpeed;
    }

    private void WallRun()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);


        //Vector3 wallNormal = stateMachine.wallRight ? stateMachine.rightSideWall.normal : stateMachine.leftSideWall.normal;
        //Vector3 wallForward = Vector3.Cross(wallNormal, stateMachine.playerObj.up);
        //Debug.Log(stateMachine.rightSideWall.normal);
        //if((stateMachine.playerObj.forward - wallForward).magnitude > (stateMachine.playerObj.forward - -wallForward).magnitude)
        {
            //wallForward = -wallForward;
        }


        //forward force
        //rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //rb.velocity = (wallForward * wallRunForce);

        //push to wall force
        //if(!(stateMachine.wallLeft && Input.GetKey(KeyCode.D)) && !(stateMachine.wallRight && Input.GetKey(KeyCode.A)))
        {
            //rb.AddForce(-wallNormal * 100, ForceMode.Force);
        }
    }
}
