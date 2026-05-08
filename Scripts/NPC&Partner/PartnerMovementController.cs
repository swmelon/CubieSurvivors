using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
#endif

/* Note: animations are called via the controller for both the characterAbillity and capsule using animator null checks
 */


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PartnerInput))]
public class PartnerMovementController : GravityDelegator, ICharacterController
{
    [Header("Player")]
    [Tooltip("Move speed of the characterAbillity in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the characterAbillity in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the characterAbillity turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The Height the player can jump")]
    public float JumpHeight = 1.2f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the characterAbillity is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the characterAbillity uses as ground")]
    public LayerMask GroundLayers;

    public bool OnWater = false;
    public float WaterOffset = -0.5f;
    public float OnWaterSpeed;
    public LayerMask WaterLayers;
    public GameObject WaterInSplashPrefab;
    public GameObject WaterOutSplashPrefab;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    public Component component => this;

    private Animator _animator;
    private CharacterController _controller;
    private PartnerInput _input;

    private GameObject waterOutSplash, waterInSplash;

    private bool _hasAnimator;

    private void Awake()
    {
        // get a reference to our main camera

        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PartnerInput>();
        _hasAnimator = TryGetComponent(out _animator);

    }

    private void Start()
    {
        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        waterOutSplash = Instantiate(WaterOutSplashPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        waterInSplash = Instantiate(WaterInSplashPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        waterOutSplash.SetActive(false);
        waterInSplash.SetActive(false);
    }

    private void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);

        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);

        bool prevOnWater = OnWater;

        OnWater = Physics.CheckSphere(spherePosition, GroundedRadius, WaterLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using characterAbillity

        if (OnWater)
        {
            Grounded = true;

            if (prevOnWater == false)
            {
                waterInSplash.SetActive(false);
                waterInSplash.SetActive(true);
                waterInSplash.transform.position = new Vector3(transform.position.x, -WaterOffset, transform.position.z);
            }
        }
        else
        {
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            if (prevOnWater == true)
            {
                waterOutSplash.SetActive(false);
                waterOutSplash.SetActive(true);
                waterOutSplash.transform.position = new Vector3(transform.position.x, -WaterOffset, transform.position.z);
            }

        }

        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = OnWater ? OnWaterSpeed : MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }


        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, VerticalVelocity, 0.0f) * Time.deltaTime);

        // update animator if using characterAbillity
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using characterAbillity
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (VerticalVelocity < 0.0f)
            {
                VerticalVelocity = -2f;
            }

            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired Height
                VerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // update animator if using characterAbillity
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using characterAbillity
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }

            // if we are not grounded, do not jump
            _input.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        VerticalVelocity += Gravity * Time.deltaTime;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }

    public void SetPosition(Vector3 position)
    {
        _controller.enabled = false;
        transform.position = position;
        _controller.enabled = true;
    }

    public Vector3 GetDirection()
    {
        return new Vector3(_input.move.x, 0.0f, _input.move.y);
    }

    public float GetSpeed()
    {
        return _speed;
    }

    public float GetMaxSpeed()
    {
        return MoveSpeed;
    }
}