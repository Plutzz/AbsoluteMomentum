using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wallrun-Vinh", menuName = "Player Logic/Wallrun Logic/WallRunVinh")]
public class PlayerWallrunVinh : PlayerWallrunSOBase
{
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallRunForce = 15f;
    [SerializeField] private float maxRunSpeed = 20f;
    [SerializeField] private float jumpUpForce = 7f;
    [SerializeField] private float jumpSideForce = 12f;
    [SerializeField] private float jumpCooldown = 0.25f;
    private bool exitingWall, readyToJump;
    public float exitWallTime;
    private float exitWallTimer;
    private bool isWallRunning;
    private bool sprinting, jumping;
    private bool isWallRight, isWallLeft;
    private RaycastHit wallRight, wallLeft;

    public override void Initialize(GameObject gameObject, PlayerStateMachine stateMachine, PlayerInputActions playerInputActions)
    {
        base.Initialize(gameObject, stateMachine, playerInputActions);
    }


    public override void DoUpdateState()
    {
        GetInput();
        WallDirection();
        rb.AddForce(Vector3.down, ForceMode.Force);

        if (exitingWall)
        {
            if (isWallRunning)
                DoExitLogic();
            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;
            if (exitWallTimer <= 0)
                exitingWall = false;
        }
        base.DoUpdateState();
    }

    public override void DoFixedUpdateState()
    {
        if (!exitingWall)
            WallRun();
        if (jumping)
        {
            if (readyToJump)
            {
                readyToJump = false;
                WallJump();
                stateMachine.timeOfLastJump = Time.time;
            }
        }
        base.DoFixedUpdateState();
    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        sprinting = playerInputActions.Player.Sprint.ReadValue<float>() == 1f;
        jumping = playerInputActions.Player.Jump.ReadValue<float>() == 1f;

        if (stateMachine.timeOfLastJump + jumpCooldown < Time.time)
            readyToJump = true;
    }

    public override void CheckTransitions()
    {
        // Player is not on wall, ground and moving
        if (!stateMachine.GroundedCheck() && !stateMachine.WallCheck() && inputVector != Vector2.zero) stateMachine.ChangeState(stateMachine.AirborneState);
        // Player is not on wall or ground and moving
        else if ((!stateMachine.WallCheck() || stateMachine.GroundedCheck()) && inputVector != Vector2.zero) stateMachine.ChangeState(stateMachine.MovingState);
        // Player is not on wall or on ground and not moving
        else if (inputVector == Vector2.zero && (stateMachine.GroundedCheck() || !stateMachine.WallCheck())) stateMachine.ChangeState(stateMachine.IdleState);

    }


    public override void DoEnterLogic()
    {
        rb.useGravity = false;
        isWallRunning = true;
        stateMachine.desiredMoveSpeed = maxRunSpeed;
        stateMachine.StartCoroutine(EnterWallRun());
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
        exitingWall = true;
        exitWallTimer = exitWallTime;
        Vector3 normal = isWallRight ? wallRight.normal : wallLeft.normal;
        Vector3 force = stateMachine.player.up * jumpUpForce + normal * jumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(force, ForceMode.Impulse);
    }

    private void WallRun()
    {
        rb.useGravity = false;
        Vector3 wallNormal = isWallRight ? wallRight.normal : wallLeft.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, stateMachine.playerObj.transform.up);

        // Debug Tools
        Debug.DrawRay(stateMachine.player.position, wallForward * 2f, Color.red);
        Debug.DrawRay(stateMachine.player.position, wallNormal * 2f, Color.blue);

        if ((stateMachine.playerObj.forward - wallForward).magnitude > (stateMachine.playerObj.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        if (!(isWallLeft && inputVector.x < 0) && !(isWallRight && inputVector.x > 0))
            rb.AddForce(-wallNormal * 200, ForceMode.Force);

        if (rb.velocity.magnitude <= maxRunSpeed)
        {
            // forward using w
            if (inputVector.y > 0 && (isWallRight || isWallLeft))
                rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
            else
            {
                rb.AddForce(-rb.velocity.normalized * 2f, ForceMode.Force); // Decrease speed when not inputting up
                rb.AddForce(Vector3.down, ForceMode.Force);
            }
        }
    }

    private void WallDirection()
    {
        isWallRight = Physics.Raycast(stateMachine.player.position, stateMachine.playerObj.right, out wallRight, 2f, wallLayer);
        isWallLeft = Physics.Raycast(stateMachine.player.position, -stateMachine.playerObj.right, out wallLeft, 2f, wallLayer);

    }

    private IEnumerator EnterWallRun()
    {
        Debug.Log("Enter Wallrun");
        float timeElapsed = 0f;
        float enterDuration = 0.2f;
        Vector3 initialVelocity = rb.velocity;
        Vector3 targetVelocity = new Vector3(rb.velocity.x, 5f, rb.velocity.z);

        while (timeElapsed < enterDuration)
        {
            rb.velocity = Vector3.Lerp(initialVelocity, targetVelocity, timeElapsed / enterDuration);
            timeElapsed += Time.deltaTime;

            yield return null;
        }
        rb.velocity = targetVelocity;
    }


}
