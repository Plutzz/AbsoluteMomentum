using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

[CreateAssetMenu(fileName = "Sliding-Default", menuName = "Player Logic/Sliding Logic/Default")]
public class PlayerSlidingDefault : PlayerSlidingSOBase
{
    [Header("Sliding")]
    [SerializeField] private float maxSlideTime;
    [SerializeField] private float maxSlideSpeed;
    [SerializeField] private float minimumSlideSpeed;
    [SerializeField] private float slideAcceleration;
    [SerializeField] private float slideAccelerationSlope;
    [SerializeField] private float slideDeceleration;

    [SerializeField] private float verticalMomentumBoostAmount = 10f;
    [SerializeField] private AnimationCurve verticalBoostCurve;
    private float acceleration;
    private Vector3 slideDirection;
    private bool SlopeLastFrame;
    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        stateMachine.canSlide = false;
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        rb.drag = 0;
        StartSlide();
    }

    public override void DoExitLogic()
    {
        stateMachine.canSlide = false;
        stateMachine.timeOfLastSlide = Time.time;
        base.DoExitLogic();
    }

    public override void DoUpdateState()
    {
        GetInput();
        MovementSpeedHandler();

        //Disables gravity while on slopes
        rb.useGravity = !stateMachine.SlopeCheck();

        base.DoUpdateState();
    }

    public override void DoFixedUpdateState()
    {
        SpeedControl();
        SlidingMovement();
        if (stateMachine.CollisionCheck())
        {
            StopSlide();
        }


        base.DoFixedUpdateState();
    }

    private void GetInput()
    {
        inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();

        if (!stateMachine.crouching)
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

        // if transitioning from airborne state and landing on a slope add a force proportional to height
        if (stateMachine.previousState == stateMachine.AirborneState && stateMachine.SlopeCheck() && rb.velocity.y < 0.1f)
        {
            float magnitude = verticalBoostCurve.Evaluate(rb.velocity.y);

            //Debug.Log(magnitude);

            stateMachine.desiredMoveSpeed = stateMachine.maxSpeed;
            stateMachine.moveSpeed += magnitude;

            //Get direction DOWN the slope
            slideDirection = Vector3.Cross(stateMachine.slopeHit.normal, Vector3.up) ;
            Vector3 test = Vector3.Cross(stateMachine.slopeHit.normal, slideDirection);
            slideDirection = test;
            Debug.DrawRay(gameObject.transform.position, slideDirection * 100f, Color.blue);

            rb.AddForce(stateMachine.GetSlopeMoveDirection(slideDirection) * magnitude, ForceMode.Impulse);

            if (stateMachine.moveSpeed < stateMachine.desiredMoveSpeed)
            {
                acceleration = slideAccelerationSlope;
                
                stateMachine.StopCoroutine(stateMachine.SmoothlyLerpMoveSpeed(acceleration));
                stateMachine.StartCoroutine(stateMachine.SmoothlyLerpMoveSpeed(acceleration));
            }



            stateMachine.lastDesiredMoveSpeed = stateMachine.maxSpeed;
            // Animation curve
        }
    }

    private void MovementSpeedHandler()
    {

        if (stateMachine.Boosting) return;

        if (rb.velocity.magnitude >= maxSlideSpeed)
            reachedMaxSpeed = true;


        //Debug.Log("Sliding Velocity: " + rb.velocity.magnitude);
        // sliding down a slope
        if (stateMachine.SlopeCheck() && rb.velocity.y < 0.1f)
        {
            slideDirection = Vector3.Cross(stateMachine.slopeHit.normal, Vector3.up);
            Vector3 test = Vector3.Cross(stateMachine.slopeHit.normal, slideDirection);
            slideDirection = test;
            Debug.DrawRay(gameObject.transform.position, slideDirection * 100f, Color.blue);

            SlopeLastFrame = true;
            stateMachine.desiredMoveSpeed = stateMachine.maxSpeed;
            acceleration = slideAccelerationSlope;
        }
        // max speed hasn't been reached yet
        else if (!reachedMaxSpeed)
        {
            if (SlopeLastFrame)
                stateMachine.moveSpeed = rb.velocity.magnitude;

            SlopeLastFrame = false;
            stateMachine.desiredMoveSpeed = maxSlideSpeed;
            acceleration = slideAcceleration;
        }
        else
        {
            if (SlopeLastFrame)
                stateMachine.moveSpeed = rb.velocity.magnitude;

            SlopeLastFrame = false;
            stateMachine.desiredMoveSpeed = minimumSlideSpeed;
            acceleration = slideDeceleration;
        }

        if (Mathf.Abs(stateMachine.desiredMoveSpeed - stateMachine.lastDesiredMoveSpeed) > 1f && stateMachine.moveSpeed != 0)
        {
            //Debug.Log("Sliding Coroutine: " + acceleration);
            stateMachine.StopCoroutine(stateMachine.SmoothlyLerpMoveSpeed(acceleration));
            stateMachine.StartCoroutine(stateMachine.SmoothlyLerpMoveSpeed(acceleration));
        }

        stateMachine.lastDesiredMoveSpeed = stateMachine.desiredMoveSpeed;
    }
    
    private void SlidingMovement()
    {
 
        slideDirection = orientation.forward * inputVector.y + orientation.right * inputVector.x;

        //Sliding on flat ground or up slope
        if (!stateMachine.SlopeCheck() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(slideDirection.normalized * 100f, ForceMode.Force);
        }
        
        //Sliding down a slope
        else
        {
            rb.AddForce(stateMachine.GetSlopeMoveDirection(slideDirection) * 100f, ForceMode.Force);
        }


        if (rb.velocity.magnitude <= minimumSlideSpeed + 1f)
        {
            StopSlide();
        }

    }

    private void SpeedControl()
    {
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
        reachedMaxSpeed = false;
        stateMachine.ChangeState(stateMachine.MovingState);
    }
    

    private void Jump()
    {
        stateMachine.ChangeState(stateMachine.MovingState);
    }

    public override void CheckTransitions()
    {
        if(!stateMachine.GroundedCheck() && !stateMachine.SlopeCheck())
        {
            reachedMaxSpeed = false;
            stateMachine.ChangeState(stateMachine.AirborneState);
        }
    }
}
