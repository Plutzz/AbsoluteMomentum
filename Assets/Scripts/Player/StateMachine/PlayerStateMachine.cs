using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Unity.Netcode.Components;
using System;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using System.Text;
using UnityEngine.UIElements;
using UnityEditor.Animations;

public class PlayerStateMachine : NetworkBehaviour
{
    #region States Variables

    public PlayerState currentState { get; private set; } // State that player is currently in
    private PlayerState initialState; // State that player starts as
    public PlayerState previousState { get; private set; } // State that player starts as

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
    public JumpHandler jumpHandler { get; private set; }

    public NetworkAnimator animator { get; private set; }

    [SerializeField] private Vector3 startPos;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float playerHeight;
    [SerializeField] private GameObject playerCameraPrefab;

    public float moveSpeed;
    public float desiredMoveSpeed;
    public float lastDesiredMoveSpeed;

    public KinematicsVariables kinematicsVariables;

    [SerializeField] private float slopeIncreaseMultiplier;
    [SerializeField] public float maxSpeed = 100f;


    [HideInInspector] public float timeOfLastJump;


    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Transform playerHitbox;

    [Header("Crouching Variables")]
    [SerializeField] private float crouchYScale = 0.5f;
    private float startYScale;
    public bool crouching { get; private set; }

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
    public RaycastHit slopeHit;
    public bool Boosting;

    [Header("Wallrun Handling")]
    [SerializeField] private float wallrunCooldown = 0.25f;
    [HideInInspector] public float timeOfLastWallrun;
    public bool canWallrun;
    public RaycastHit wallLeft;
    public RaycastHit wallRight;
    public bool isWallLeft;
    public bool isWallRight;

    [Header("Collision Handling")]
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private float maxHardCollisionAngle = 45f;

    [Header("Sliding Handling")]
    [SerializeField] private float slideCooldown = 0.5f;
    [HideInInspector] public float timeOfLastSlide;
    public bool canSlide;

    [Header("Switch Characters")]
    [SerializeField] private Character[] characters;
    [SerializeField] private CharacterNames currentCharacter;

    [Serializable]
    public struct Character
    {
        [SerializeField] public CharacterNames name;
        [SerializeField] public Mesh characterMesh;
        [SerializeField] public AnimatorOverrideController animatorController;
        [SerializeField] public Material characterMaterial;
        [SerializeField] public Avatar characterAvatar;

    }
    [Serializable]
    public enum CharacterNames
    {
        Baku,
        Sangeo
    };

    #endregion

    #region Debug Variables
    private List<TextMeshProUGUI> debugMenuList = new List<TextMeshProUGUI>();
    public GameObject debugMenuParent;
    public TextMeshProUGUI debugMenuItemprefab;


/*    public TextMeshProUGUI CurrentStateText;
    public TextMeshProUGUI GroundedText;
    public TextMeshProUGUI WallrunText;
    public TextMeshProUGUI VelocityText;
    public TextMeshProUGUI SpeedText;*/
    public Vector3 RespawnPos;
    public float teleportAmount;
    #endregion

    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();

        //TEMP CODE TO TEST MOVEMENT
        //PlayerIdleBaseInstance = playerIdleBase;
        //PlayerMovingBaseInstance = playerMovingBase;
        //PlayerAirborneBaseInstance = playerAirborneBase;
        //PlayerSlidingBaseInstance = playerSlidingBase;
        //PlayerWallrunBaseInstance = playerWallrunBase;
        //COMMENT CODE TO TEST MOVEMENT VALUES WITHOUT HAVING TO RESTART PLAY MODE
        PlayerIdleBaseInstance = Instantiate(playerIdleBase);
        PlayerMovingBaseInstance = Instantiate(playerMovingBase);
        PlayerAirborneBaseInstance = Instantiate(playerAirborneBase);
        PlayerSlidingBaseInstance = Instantiate(playerSlidingBase);
        PlayerWallrunBaseInstance = Instantiate(playerWallrunBase);

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Crouch.performed += StartCrouch;
        playerInputActions.Player.Crouch.canceled += StopCrouch;
        playerInputActions.Player.Sprint.performed += StartSprint;
        playerInputActions.Player.Sprint.canceled += StopSprint;


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
        InitDebugMenu();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        InitDebugMenu();

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        if (RaceManager.Instance != null)
        {
            RaceManager.Instance.playerList.Add(gameObject);
            Debug.Log(gameObject + "Added to player list");
        }

        if(scene.name == "Lobby")
        {
            transform.position = startPos;
        }
    }

    public override void OnNetworkSpawn()
    {
        // Set up client side objects (Camera, debug menu)
        if (IsOwner)
        {
            transform.position = startPos;

            jumpHandler = GetComponent<JumpHandler>();
            animator = GetComponentInChildren<NetworkAnimator>();

            CinemachineFreeLook playerCamera = Instantiate(playerCameraPrefab.GetComponent<CinemachineFreeLook>(), transform);
            playerCamera.m_LookAt = transform;
            playerCamera.m_Follow = transform;

            ThirdPersonCam camSetup = playerCamera.GetComponent<ThirdPersonCam>();
            camSetup.orientation = orientation;
            camSetup.playerObj = playerObj;
            camSetup.player = player;
            camSetup.playerInputActions = playerInputActions;


            Boosting = false;

            //Debug Stuff
            /*CurrentStateText = DebugMenu.Instance.PlayerStateText;
            GroundedText = DebugMenu.Instance.GroundedCheckText;
            WallrunText = DebugMenu.Instance.WallrunCheckText;
            VelocityText = DebugMenu.Instance.VelocityText;
            SpeedText = DebugMenu.Instance.SpeedText;*/

            currentState = initialState;
            currentState.EnterLogic();

            if (RaceManager.Instance != null)
            {
                RaceManager.Instance.playerList.Add(gameObject);
                Debug.Log(gameObject + "Added to player list");
            }


            SwitchCharacters();
        }
    }

    public void Start()
    {
        TeleportPlayer(startPos);
    }
    public void TeleportPlayer(Vector3 position)
    {
        transform.position = startPos;
    }

    public override void OnNetworkDespawn()
    {
        playerInputActions.Player.Crouch.performed -= StartCrouch;
        playerInputActions.Player.Crouch.canceled -= StopCrouch;
        playerInputActions.Player.Sprint.performed -= StartSprint;
        playerInputActions.Player.Sprint.canceled -= StopSprint;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.P))
            player.position = new Vector3(0, 10, 0);

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPlayer();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            TeleportPlayer();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            SwitchCharacters();

            currentCharacter++;

            if((int)currentCharacter >= characters.Length)
            {
                currentCharacter = 0;
            }

        }



        //rb.velocity = new Vector3(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -10f, 100f), rb.velocity.z);

        //Crouching logic
        crouching = playerInputActions.Player.Crouch.ReadValue<float>() == 1f;

        if (crouching)
        {
            playerHitbox.localScale = new Vector3(playerHitbox.localScale.x, crouchYScale, gameObject.transform.localScale.z);
        }
        else
        {
            playerHitbox.localScale = new Vector3(playerHitbox.localScale.x, startYScale, gameObject.transform.localScale.z);
        }

        currentState.UpdateState();

        UpdateDebugMenu();
        //CurrentStateText.text = "Current State: " + currentState.ToString();
        //GroundedText.text = "Grounded: " + GroundedCheck();
        //WallrunText.text = "Wallrun: " + WallCheck();
        //VelocityText.text = "Input: " + playerInputActions.Player.Movement.ReadValue<Vector2>().x + "," + playerInputActions.Player.Movement.ReadValue<Vector2>().y;
        //VelocityText.text = "Vertical Speed: " + rb.velocity.y;
        //WallCheck();
        //Debug.Log(wallRight);
        //Debug.Log(wallLeft);

        if (Time.time > timeOfLastWallrun + wallrunCooldown)
        {
            canWallrun = true;
        }

        if(Time.time > timeOfLastSlide + slideCooldown)
        {
            canSlide = true;
        }


    }

    

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        currentState.FixedUpdateState();

        //SpeedText.text = "Speed: " + rb.velocity.magnitude.ToString("F1");
    }

    public void ChangeState(PlayerState newState)
    {
        Debug.Log("Changing to: " + newState);
        currentState.ExitLogic();
        previousState = currentState;
        currentState = newState;
        HandleAnimations();
        currentState.EnterLogic();
    }

    #region Logic Checks

    //Consider adding core functionalities here
    // Ex: GroundedCheck
    public bool GroundedCheck()
    {
        Debug.DrawRay(transform.position, Vector3.down * playerHeight * 0.5f + Vector3.down * 0.2f);
        return Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);
    }

    public bool WallCheck()
    {
        // Debugging rays
        float raycastDistance = 1.5f; // Adjust this distance based on your needs
        float raycastAngle = 10f;   // Adjust the angle of rotation based on your needs

        // Rotate right ray direction
        Vector3 rightRayDirection = Quaternion.Euler(0, -raycastAngle, 0) * playerObj.right;
        isWallRight = Physics.Raycast(player.position, rightRayDirection, out wallRight, raycastDistance, wallLayer);

        // Rotate left ray direction
        Vector3 leftRayDirection = Quaternion.Euler(0, raycastAngle, 0) * -playerObj.right;
        isWallLeft = Physics.Raycast(player.position, leftRayDirection, out wallLeft, raycastDistance, wallLayer);

        Debug.DrawRay(player.position, leftRayDirection * raycastDistance, Color.blue);
        Debug.DrawRay(player.position, rightRayDirection * raycastDistance, Color.blue);

        if(isWallLeft)
        {
            float _angle = Vector3.Angle(wallLeft.normal, rb.velocity);
        }
        else if(isWallRight)
        {
            float _angle = Vector3.Angle(wallRight.normal, rb.velocity);
        }

        return (isWallRight || isWallLeft);
    }

    public bool CollisionCheck()
    {
        // Debugging rays
        float raycastDistance = 1.5f; // Adjust this distance based on your needs
        float raycastAngle = 10f;   // Adjust the angle of rotation based on your needs

        // Rotate right ray direction
        Vector3 rightRayDirection = Quaternion.Euler(0, -raycastAngle, 0) * playerObj.right;
        isWallRight = Physics.Raycast(player.position, rightRayDirection, out wallRight, raycastDistance);

        // Rotate left ray direction
        Vector3 leftRayDirection = Quaternion.Euler(0, raycastAngle, 0) * -playerObj.right;
        isWallLeft = Physics.Raycast(player.position, leftRayDirection, out wallLeft, raycastDistance);

        float _angle = 90f;

        if (isWallLeft)
        {
            _angle = Vector3.Angle(rb.velocity, -wallLeft.normal);
        }
        else if (isWallRight)
        {
            _angle = Vector3.Angle(rb.velocity, -wallRight.normal);
        }

        //Debug.Log(_angle < maxHardCollisionAngle);

        return _angle < maxHardCollisionAngle;
    }

    public bool SlopeCheck()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f * gameObject.transform.localScale.y + 0.5f, groundLayer))
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

        if (currentState == IdleState)
        {
            animator.SetTrigger("Crouch Idle");
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
    }

    #endregion

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

            if (SlopeCheck())
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

    public void addHorizontalVelocity() { 
    }

    public void increaseAcceleration() { 
    }

    #endregion

    #region Debug Functions

    private void TeleportPlayer()
    {
        StopAllCoroutines();
        rb.velocity = Vector3.zero;
        moveSpeed = 0;
        desiredMoveSpeed = 0;
        lastDesiredMoveSpeed = 0;
        transform.position = new Vector3(transform.position.x, transform.position.y + teleportAmount, transform.position.z);
    }

    private void ResetPlayer()
    {
        StopAllCoroutines();
        rb.velocity = Vector3.zero;
        moveSpeed = 0;
        desiredMoveSpeed = 0;
        lastDesiredMoveSpeed = 0;
        transform.position = RespawnPos;
    }

    private void InitDebugMenu()
    {
        debugMenuList.Clear();
        if (DebugMenu.Instance == null) return;

        int debugMenuSize = 6;
        for (int i = 0; i < debugMenuSize; i++)
        {
            TextMeshProUGUI newText = Instantiate(debugMenuItemprefab);
            newText.transform.SetParent(DebugMenu.Instance.transform);
            debugMenuList.Add(newText);
        }
    }

    //MAKE SURE TO HARD CODE IN THE VARIABLE FOR DEBUG MENU SIZE ABOVE
    private void UpdateDebugMenu() {
        if (DebugMenu.Instance == null) return;

        debugMenuList[0].text = "Current State: " + currentState.ToString();
        debugMenuList[1].text = "Grounded: " + GroundedCheck();
        debugMenuList[2].text = "Wallrun: " + WallCheck();
        //VelocityText.text = "Input: " + playerInputActions.Player.Movement.ReadValue<Vector2>().x + "," + playerInputActions.Player.Movement.ReadValue<Vector2>().y;
        debugMenuList[3].text = "Vertical Speed: " + rb.velocity.y.ToString("F2");
        debugMenuList[4].text = "Horizontal Speed: " + new Vector2(rb.velocity.x, rb.velocity.z).magnitude.ToString("F2");
        debugMenuList[5].text = "Speed: " + rb.velocity.magnitude.ToString("F2");
        //debugMenuList[6].text = "Current Speed: " + kinematicsVariables.currentSpeed.ToString("F2");
        //debugMenuList[7].text = "Current Acceleration: " + kinematicsVariables.currentAcceleration.ToString("F2");
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(gameObject.transform.position, sphereCastRadius);
    }

    #endregion

    #region Animations

    private void StartSprint(InputAction.CallbackContext context)
    {
        if (currentState == MovingState && !crouching)
        {
            animator.SetTrigger("Running");
        }
    }

    private void StopSprint(InputAction.CallbackContext context)
    {
        if (currentState == MovingState)
        {
            animator.SetTrigger("Jogging");
        }
    }
    private void StopCrouch(InputAction.CallbackContext context)
    {
        if (currentState == MovingState)
        {
            animator.SetTrigger("Jogging");
        }
        else if (currentState == IdleState)
        {
            animator.SetTrigger("Idle");
        }
    }

    private void HandleAnimations()
    {
        switch (currentState)
        {
            case PlayerIdleState _:

                if (crouching)
                {
                    animator.SetTrigger("Crouch Idle");
                }
                else
                {
                    animator.SetTrigger("Idle");
                }
                break;

            case PlayerAirborneState _:
                animator.SetTrigger("Airborne");
                break;

            case PlayerSlidingState _:

                animator.SetTrigger("Sliding");
                break;

            case PlayerWallrunState _:
                if(isWallLeft)
                {
                    animator.SetTrigger("WallrunLeft");
                }
                else
                {
                    animator.SetTrigger("WallrunRight");
                }
                break;

            case PlayerMovingState _:

                if (crouching)
                {
                    animator.SetTrigger("Crouch Walk");
                }
                else if (playerInputActions.Player.Sprint.ReadValue<float>() == 1)
                {
                    animator.SetTrigger("Running");
                }
                else
                {
                    animator.SetTrigger("Jogging");
                }

                break;
        }
    }

    private void SwitchCharacters()
    {
        //Switch character objects
        var _character = characters[(int)currentCharacter];

        //Switch animation controller
        GetComponentInChildren<Animator>().runtimeAnimatorController = _character.animatorController;

        //Switch Avatars
        GetComponentInChildren<Animator>().avatar = _character.characterAvatar;

        //Switch Mesh
        GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = _character.characterMesh;

        //Switch Material
        GetComponentInChildren<SkinnedMeshRenderer>().material = _character.characterMaterial;
    }
    #endregion
}

[System.Serializable]
public class KinematicsVariables {
    //Horizontal only atm
    public float currentSpeed;
    public float currentAcceleration;
}
