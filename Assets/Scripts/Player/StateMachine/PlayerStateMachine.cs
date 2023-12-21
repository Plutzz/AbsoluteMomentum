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
    public PlayerWallrunState WallrunState;

    #endregion

    #region ScriptableObject Variables

    [SerializeField] private PlayerIdleSOBase playerIdleBase;
    [SerializeField] private PlayerMovingSOBase playerMovingBase;
    [SerializeField] private PlayerAirborneSOBase playerAirborneBase;
    [SerializeField] private PlayerSlidingSOBase playerSlidingBase;
    [SerializeField] private PlayerWallrunSOBase playerWallrunBase;

    public PlayerIdleSOBase PlayerIdleBaseInstance { get; private set; }
    public PlayerMovingSOBase PlayerMovingBaseInstance { get; private set; }
    public PlayerAirborneSOBase PlayerAirborneBaseInstance { get; private set; }
    public PlayerSlidingSOBase PlayerSlidingBaseInstance { get; private set; }
    public PlayerWallrunSOBase PlayerWallrunBaseInstance { get; private set; }

    #endregion

    #region Player Variables
    public PlayerInputActions playerInputActions { get; private set; }
    public Rigidbody rb { get; private set; }

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float playerHeight;
    [SerializeField] private GameObject playerCameraPrefab;

    public float moveSpeed;
    public float desiredMoveSpeed;
    public float lastDesiredMoveSpeed;



    [HideInInspector] public float timeOfLastJump;
    public bool exitingGround;

    [SerializeField] private float slopeIncreaseMultiplier;
    [SerializeField] public float maxSpeed = 100f;

    


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

    #endregion

    #region Debug Variables
    public TextMeshProUGUI CurrentStateText;
    public TextMeshProUGUI GroundedText;
    public TextMeshProUGUI WallrunText;
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
        PlayerWallrunBaseInstance = playerWallrunBase;
        //COMMENT CODE TO TEST MOVEMENT VALUES WITHOUT HAVING TO RESTART PLAY MODE
        //PlayerIdleBaseInstance = Instantiate(playerIdleBase);
        //PlayerMovingBaseInstance = Instantiate(playerMovingBase);
        //PlayerAirborneBaseInstance = Instantiate(playerAirborneBase);
        //PlayerSlidingBaseInstance = Instantiate(playerSlidingBase);
        //PlayerWallrunBaseInstance = Instantiate(playerWallrunBase);

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Crouch.performed += StartCrouch;

        IdleState = new PlayerIdleState(this);
        MovingState = new PlayerMovingState(this);
        AirborneState = new PlayerAirborneState(this);
        SlidingState = new PlayerSlidingState(this);
        WallrunState = new PlayerWallrunState(this);
        

        PlayerIdleBaseInstance.Initialize(gameObject, this, playerInputActions);
        PlayerMovingBaseInstance.Initialize(gameObject, this, playerInputActions);
        PlayerAirborneBaseInstance.Initialize(gameObject, this, playerInputActions);
        PlayerSlidingBaseInstance.Initialize(gameObject, this, playerInputActions, orientation);
        PlayerWallrunBaseInstance.Initialize(gameObject, this, playerInputActions);

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
            WallrunText = DebugMenu.Instance.WallrunCheckText;
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
        WallrunText.text = "Wallrun: " + WallCheck();
        VelocityText.text = "Input: " + playerInputActions.Player.Movement.ReadValue<Vector2>().x + "," + playerInputActions.Player.Movement.ReadValue<Vector2>().y;

    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        currentState.FixedUpdateState();

        SpeedText.text = "Speed: " + rb.velocity.magnitude.ToString("F1");
    }

    public void ChangeState(PlayerState newState)
    {
        Debug.Log("Changing to: " + newState);
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

    public bool WallCheck()
    {
        // Debugging rays
        Debug.DrawRay(player.position, -playerObj.right * 2f, Color.red);
        Debug.DrawRay(player.position, playerObj.right * 2f, Color.red);
        return Physics.Raycast(player.position, -orientation.right, 2f, LayerMask.GetMask("Wall")) ||
                Physics.Raycast(player.position, orientation.right, 2f, LayerMask.GetMask("Wall"));
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

    #region Speed Control

    public IEnumerator SmoothlyLerpMoveSpeed(float speedIncreaseMultiplier)
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

    public IEnumerator CoyoteFrames(float _coyoteTime)
    {
        rb.useGravity = true;
        yield return new WaitForSeconds(_coyoteTime);
        ChangeState(AirborneState);
    }

    #endregion





}
