using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerStateMachine : NetworkBehaviour
{
    #region States Variables

    private PlayerState currentState; // State that player is currently in
    private PlayerState initialState; // State that player starts as

    // References to all player states
    public PlayerIdleState IdleState;
    public PlayerMovingState MovingState;
    public PlayerAirborneState AirborneState;
    public PlayerSlidingState SlidingState;

    #endregion

    #region ScriptableObject Variables

    [SerializeField] private PlayerIdleSOBase playerIdleBase;
    [SerializeField] private PlayerMovingSOBase playerMovingBase;
    [SerializeField] private PlayerAirborneSOBase playerAirborneBase;
    [SerializeField] private PlayerSlidingSOBase playerSlidingBase;

    public PlayerIdleSOBase PlayerIdleBaseInstance { get; private set; }
    public PlayerMovingSOBase PlayerMovingBaseInstance { get; private set; }
    public PlayerAirborneSOBase PlayerAirborneBaseInstance { get; private set; }
    public PlayerSlidingSOBase PlayerSlidingBaseInstance { get; private set; }

    #endregion

    #region Player Variables
    public PlayerInputActions playerInputActions { get; private set; }
    public Rigidbody rb { get; private set; }

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float playerHeight;
    [SerializeField] private GameObject playerCameraPrefab;

    private float moveSpeed;
    [HideInInspector] public float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    [SerializeField] private float speedIncreaseMultiplier;
    [SerializeField] private float slopeIncreaseMultiplier;
    


    public Transform orientation;
    public Transform player;
    public Transform playerObj;

    [Header("Crouching Variables")]
    [SerializeField] private float crouchYScale = 0.5f;
    private float startYScale;
    public bool crouching { get; private set; }

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
    public RaycastHit slopeHit;
    public bool exitingSlope;

    #endregion

    #region Debug Variables
    public TextMeshProUGUI CurrentStateText;
    public TextMeshProUGUI GroundedText;
    public TextMeshProUGUI VelocityText;
    public TextMeshProUGUI SpeedText;
    #endregion

    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();

        //TEMP CODE TO TEST MOVEMENT
        PlayerIdleBaseInstance = playerIdleBase;
        PlayerMovingBaseInstance = playerMovingBase;
        PlayerAirborneBaseInstance = playerAirborneBase;
        PlayerSlidingBaseInstance = playerSlidingBase;
        //COMMENT CODE TO TEST MOVEMENT VALUES WITHOUT HAVING TO RESTART PLAY MODE
        ///PlayerIdleBaseInstance = Instantiate(playerIdleBase);
        ///PlayerMovingBaseInstance = Instantiate(playerMovingBase);
        ///PlayerAirborneBaseInstance = Instantiate(playerAirborneBase);
        ///PlayerSlidingBaseInstance = Instantiate(playerSlidingBase);

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Crouch.performed += StartCrouch;

        IdleState = new PlayerIdleState(this);
        MovingState = new PlayerMovingState(this);
        AirborneState = new PlayerAirborneState(this);
        SlidingState = new PlayerSlidingState(this);
        

        PlayerIdleBaseInstance.Initialize(gameObject, this, playerInputActions);
        PlayerMovingBaseInstance.Initialize(gameObject, this, playerInputActions);
        PlayerAirborneBaseInstance.Initialize(gameObject, this, playerInputActions);
        PlayerSlidingBaseInstance.Initialize(gameObject, this, playerInputActions, orientation);

        initialState = IdleState;

        startYScale = gameObject.transform.localScale.y;
    }
    private void Start()
    {
        // Set up client side objects (Camera, debug menu)
        if (IsOwner)
        {
            CinemachineFreeLook playerCamera = Instantiate(playerCameraPrefab).GetComponent<CinemachineFreeLook>();
            playerCamera.m_LookAt = transform;
            playerCamera.m_Follow = transform;

            ThirdPersonCam camSetup = playerCamera.GetComponent<ThirdPersonCam>();
            camSetup.orientation = orientation;
            camSetup.playerObj = playerObj;
            camSetup.player = player;
            camSetup.playerInputActions = playerInputActions;

            CurrentStateText = DebugMenu.Instance.PlayerStateText;
            GroundedText = DebugMenu.Instance.GroundedCheckText;
            VelocityText = DebugMenu.Instance.VelocityText;
            SpeedText = DebugMenu.Instance.SpeedText;
        }

        currentState = initialState;
        currentState.EnterLogic();
    }

    private void Update()
    {
        if (!IsOwner) return;

        //Crouching logic
        crouching = playerInputActions.Player.Crouch.ReadValue<float>() == 1f;
        if (crouching)
        {
            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, crouchYScale, gameObject.transform.localScale.z);
        }
        else
        {
            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, startYScale, gameObject.transform.localScale.z);
        }
        //Disables gravity while on slopes
        rb.useGravity = !SlopeCheck();


        currentState.UpdateState();

        CurrentStateText.text = "Current State: " + currentState.ToString();
        GroundedText.text = "Grounded: " + GroundedCheck();
        VelocityText.text = "Input: " + playerInputActions.Player.Movement.ReadValue<Vector2>().x + "," + playerInputActions.Player.Movement.ReadValue<Vector2>().y;




    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        SpeedControl();

        currentState.FixedUpdateState();

        SpeedText.text = "Speed: " + rb.velocity.magnitude.ToString("F1");
    }

    public void ChangeState(PlayerState newState)
    {
        //Debug.Log("Changing to: " + newState);
        currentState.ExitLogic();
        currentState = newState;
        currentState.EnterLogic();
    }

    //Consider adding core functionalities here
    // Ex: GroundedCheck
    public bool GroundedCheck()
    {
        return Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f * gameObject.transform.localScale.y + 0.2f , groundLayer);
        //return Physics.OverlapBox(GroundCheck.position, GroundCheckSize * 0.5f, Quaternion.identity, groundLayer).Length > 0;
    }
    public bool SlopeCheck()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f * gameObject.transform.localScale.y + 0.3f))
        {
            float _angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            //Debug.Log("OnSlope: " + (_angle < maxSlopeAngle && _angle != 0));
            return _angle < maxSlopeAngle && _angle != 0;
        }

        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 _direction)
    {
        return Vector3.ProjectOnPlane(_direction, slopeHit.normal).normalized;
    }
 

    private void StartCrouch(InputAction.CallbackContext context)
    {
        if(currentState == IdleState || currentState == MovingState)
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    #region SpeedControl

    // Limits the speed of the player to speed
    private void SpeedControl()
    {

        // limit velocity on slope if player is not leaving the slope
        if (SlopeCheck() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        // limit velocity on ground
        else
        {
            Vector3 _flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (_flatVel.magnitude > moveSpeed)
            {
                Vector3 _limitedVelTarget = _flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(_limitedVelTarget.x, rb.velocity.y, _limitedVelTarget.z);
            }
        }


        // check if desiredMoveSpeed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            Debug.Log("START COROUTINE");
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;

    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        //smoothly lerp movementSpeed to desired value
        float _time = 0;
        float _difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float _startValue = moveSpeed;

        while (_time < _difference)
        {
            moveSpeed = Mathf.Lerp(_startValue, desiredMoveSpeed, _time / _difference);

            if(SlopeCheck())
            {
                float _slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float _slopeAngleIncrease = 1 + (_slopeAngle / 90f);

                _time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * _slopeAngleIncrease;
            }
            else
            {
                _time += Time.deltaTime * speedIncreaseMultiplier;
            }
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    #endregion





}
