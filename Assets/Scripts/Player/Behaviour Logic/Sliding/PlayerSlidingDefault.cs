using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "Sliding-Default", menuName = "Player Logic/Sliding Logic/Default")]
public class PlayerSlidingDefault : PlayerSlidingSOBase
{
    [Header("Sliding")]
    [SerializeField] private float maxSlideTime;
    [SerializeField] private float maxSlideSpeed;
    [SerializeField] private float minimumSlideSpeed;
    [SerializeField] private float slideAcceleration;
    [SerializeField] private float slideDeceleration;
    private float acceleration;
    private Vector3 slideDirection;
    private bool reachedMaxSpeed;
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        rb.drag = 0;
        StartSlide();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoUpdateState()
    {
        GetInput();
        MovementSpeedHandler();
        base.DoUpdateState();
    }

    public override void DoFixedUpdateState()
    {

        SlidingMovement();
        SpeedControl();


        base.DoFixedUpdateState();
    }

    private void GetInput()
    {
        if(!stateMachine.crouching)
        {
            StopSlide();
        }
        if (playerInputActions.Player.Jump.ReadValue<float>() == 1f)
        {
            Jump();
        }
    }

    private void StartSlide()
    {
        //rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        stateMachine.desiredMoveSpeed = maxSlideSpeed;
        reachedMaxSpeed = false;
    }

    private void MovementSpeedHandler()
    {
        Debug.Log("Sliding Velocity: " + rb.velocity.magnitude);
        // sliding down a slope
        if (stateMachine.SlopeCheck() && rb.velocity.y < 0.1f)
        {
            reachedMaxSpeed = false;
            stateMachine.desiredMoveSpeed = maxSlideSpeed;
            acceleration = slideAcceleration;
        }
        // max speed hasn't been reached yet
        else if (!reachedMaxSpeed && (rb.velocity.magnitude <= maxSlideSpeed))
        {
            stateMachine.desiredMoveSpeed = maxSlideSpeed;
            acceleration = slideAcceleration;
        }
        else
        {
            reachedMaxSpeed = true;
            stateMachine.desiredMoveSpeed = minimumSlideSpeed;
            acceleration = slideDeceleration;
        }

        if (Mathf.Abs(stateMachine.desiredMoveSpeed - stateMachine.lastDesiredMoveSpeed) > 1f && stateMachine.moveSpeed != 0)
        {
            Debug.Log("Sliding Coroutine: " + acceleration);
            stateMachine.StopCoroutine(stateMachine.SmoothlyLerpMoveSpeed(acceleration));
            stateMachine.StartCoroutine(stateMachine.SmoothlyLerpMoveSpeed(acceleration));
        }

        stateMachine.lastDesiredMoveSpeed = stateMachine.desiredMoveSpeed;
    }
    
    private void SlidingMovement()
    {
        slideDirection = orientation.forward * inputVector.y + orientation.right * inputVector.x;


        //Sliding Normal
        if (!stateMachine.SlopeCheck() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(slideDirection.normalized * slideAcceleration, ForceMode.Force);
        }
        
        //Sliding down a slope
        else
        {
           rb.AddForce(stateMachine.GetSlopeMoveDirection(slideDirection) * slideAcceleration, ForceMode.Force);
        }


        if (rb.velocity.magnitude <= minimumSlideSpeed + 1f)
        {
            StopSlide();
        }

    }

    private void SpeedControl()
    {
        //exitingGround = timeOfLastJump + exitingGroundTimer > Time.time;



        // If the player is mid jump don't limit velocity
        // if (!readyToJump) return;

        // limit velocity on slope if player is not leaving the slope
        if (stateMachine.SlopeCheck())
        {
            if (rb.velocity.magnitude > stateMachine.moveSpeed)
                rb.velocity = rb.velocity.normalized * stateMachine.moveSpeed;
        }
        // limit velocity on ground
        else
        {
            Vector3 _flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (_flatVel.magnitude > stateMachine.moveSpeed)
            {
                Vector3 _limitedVelTarget = _flatVel.normalized * stateMachine.moveSpeed;
                rb.velocity = new Vector3(_limitedVelTarget.x, rb.velocity.y, _limitedVelTarget.z);
            }
        }


    }

    private void StopSlide()
    {
        stateMachine.ChangeState(stateMachine.MovingState);
    }
    

    private void Jump()
    {
        stateMachine.ChangeState(stateMachine.MovingState);
    }


}