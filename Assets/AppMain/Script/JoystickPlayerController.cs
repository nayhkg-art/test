using UnityEngine;

public class JoystickPlayerController : MonoBehaviour
{
    [Header("Component References")]
    public VariableJoystick variableJoystick;

    [Header("Movement Settings")]
    public float minMoveSpeed = 2.0f;
    public float maxMoveSpeed = 8.0f;
    public float moveAcceleration = 5.0f;
    [Tooltip("この値より速度が小さければ停止とみなす")]
    public float stopThreshold = 0.05f;

    [Header("Audio Settings")]
    public AudioClip footSound;
    public float walkSoundInterval = 0.6f;
    public float runSoundInterval = 0.3f;
    private float footSoundTimer = 0f;

    public bool isWalking { get; private set; }
    public float currentMoveSpeed { get; private set; }

    private CharacterController characterController;
    private Transform cameraTransform;
    private Vector3 playerVelocity;
    private readonly float gravityValue = -9.81f;

    // ★★★ 変更点①：この2行を追加 ★★★
    private Vector2 _input;
    public bool IsMoving => _input.magnitude > 0.1f;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        currentMoveSpeed = 0f;
    }

    void Update()
    {
        if (characterController.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -1.0f;
        }

        Vector2 joystickInput = variableJoystick.Direction;
        Vector2 keyboardInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 combinedInput = joystickInput + keyboardInput;
        
        // ★★★ 変更点②：「inputDir」をクラス変数の「_input」に変更 ★★★
        _input = Vector2.ClampMagnitude(combinedInput, 1.0f);
        
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();
        
        // ★★★ 変更点③：「inputDir」を「_input」に書き換え ★★★
        Vector3 moveDirection = (camForward * _input.y + camRight * _input.x);

        bool hasInput = _input.magnitude > 0.1f;

        if (hasInput)
        {
            float targetSpeed = maxMoveSpeed;
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetSpeed, moveAcceleration * Time.deltaTime);
            
            characterController.Move(moveDirection.normalized * currentMoveSpeed * Time.deltaTime);
            
            isWalking = characterController.velocity.magnitude > stopThreshold;

            float speedRatio = Mathf.InverseLerp(minMoveSpeed, maxMoveSpeed, currentMoveSpeed);
            float soundInterval = Mathf.Lerp(walkSoundInterval, runSoundInterval, speedRatio);
            PlayFootSound(soundInterval);
        }
        else
        {
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, 0f, moveAcceleration * Time.deltaTime);
            isWalking = false; 
        }
        
        playerVelocity.y += gravityValue * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);
    }
    
    private void PlayFootSound(float interval)
    {
        if (!isWalking) return;

        footSoundTimer += Time.deltaTime;
        if (footSoundTimer >= interval)
        {
            if (AudioManager.Instance != null && footSound != null)
            {
                AudioManager.Instance.PlaySFXAtPoint(footSound, transform.position);
            }
            footSoundTimer = 0f;
        }
    }
}