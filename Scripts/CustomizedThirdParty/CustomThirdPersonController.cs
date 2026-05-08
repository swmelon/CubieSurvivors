using System;
using Local.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;





#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the characterAbillity and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class CustomThirdPersonController : GravityDelegator, ICharacterController

    {
        [Header("Player")]
        [Tooltip("Move speed of the characterAbillity in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the characterAbillity in m/s")]
        public float SprintSpeed = 5.335f;

        [Range(360f , 3600f)]
        public float RotationLimitPerSecond = 360f;

        [Tooltip("How fast the characterAbillity turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;


        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("This determine dash distance of the character")]
        [Range(0.05f, 0.2f)]
        public float DashForce = 0.2f;

        [Space(10)]
        [Tooltip("The Height the player can jump")]
        public float JumpHeight = 1.2f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Tooltip("Time required to pass before being able to use ability again. Set to 0f to instantly use again")]
        public float AbilityTimeout = 1f;

        [Header("Player Grounded")]
        [Tooltip("If the characterAbillity is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")] public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the characterAbillity uses as ground")]
        public LayerMask GroundLayers;

        [FormerlySerializedAs("playerMoveInfoChannel")]
        public PlayerMoveDirectionChannelSO playerMoveDirectionChannel;

        [SerializeField]
        private CharacterControllerChannelSO characterControllerChannel;

        [SerializeField]
        private OnLiquidBehaviourChannel onLiquidBehaviourChannel;

        private Vector3 inputDirection;
        private Vector3 extraMoveForce;
        private bool inputEnabled = true;
        private bool ignoreInputUntillHitGround = false;
        private bool clampRotation = false;
        private Collider[] liquidColliders = new Collider[4];
        private CharacterVFXController characterVFXController;
        private IAnimationController animationController;
        private float baseRotationSmoothTime;
        private float agilityMult = 0.15f;

        [SerializeField]
        private EventChannelSO enterBossStageEventChannel;

        [SerializeField]
        private Vector3EventChannelSO enterEventStageEventChannel;

        [SerializeField]
        private EventChannelSO restartGameEventChannel;

        [SerializeField]
        private EventChannelSO abilityButtonPressedEventChannel;

        [SerializeField]
        private MainCameraChannelSO mainCameraChannel;

        [SerializeField]
        private Vector3EventChannelSO playerPositionMovedChannel;

        private CameraController mainCameraController;
        private CharacterAbillity characterAbillity;
        private bool isSpeedBoosted = false;
        private float prevSpeed;
        private bool liquidEffectActivated = false;
        private float bonusMoveSpeed = 0f;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;

        private float _rotationVelocity;

        private const float groundedVelocityClamp = -2f;
        private const float speedBoostIncrement = 0.5f;
        private const float highJumpMultiplier = 1.5f;
        private const float hyperJumpMultiplier = 2f;
        private const float teleportDistance = 3f;
        private const float speedRoundFactor = 1000f;
        private const float liquidHeightCheckRange = 5f;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        private float _abilityTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;        

        public Component component => this;

        //mobile joystick and jump button

        [FormerlySerializedAs("joystickChannelSo")]
        [SerializeField]
        private JoystickChannelSO joystickChannel;

        private Joystick joystick;

        [SerializeField]
        private BooleanEventChannelSO userInputControlChannel;

        [SerializeField]
        private GamePauser gamePauser;

        private bool _isJumpPressed;
        private Vector3 _targetDirection;

        private Collider[] colliders = new Collider[1];
        private float _rotationSmoothTime;
        private float _speedChangeRate;

        private OnLiquidBehaviour liquidBehaviour;
        private bool hasLiquidBehaviour = false;



#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;



        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

            characterVFXController = GetComponentInChildren<CharacterVFXController>();
            _rotationSmoothTime = RotationSmoothTime;
            _speedChangeRate = SpeedChangeRate;
            
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif


            characterAbillity = GetComponent<CharacterAbillity>();
            animationController = GetComponent<IAnimationController>();
            _playerInput.enabled = false;
            hasLiquidBehaviour = false;
            baseRotationSmoothTime = RotationSmoothTime;

        }

        private void Start()
        {
            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

        }

        private void OnEnable()
        {
            CharacterUpgradableStat.SpeedChanged += ChangeSpeed;
            joystickChannel.Subscribe(SetJoystick);
            enterBossStageEventChannel.Subscribe(ResetCharacterPosition);
            enterEventStageEventChannel.Subscribe(MoveCharacterTo);
            restartGameEventChannel.Subscribe(ResetCharacterXZPos);
            userInputControlChannel.Subscribe(OnOffControl);
            abilityButtonPressedEventChannel.Subscribe(OnJumpPressed);
            mainCameraChannel.Subscribe(SetMainCameraController);
            characterControllerChannel.Register(_controller);
            onLiquidBehaviourChannel.Subscribe(SetLiquidBehaviour);
        }

        private void OnDisable()
        {
            CharacterUpgradableStat.SpeedChanged -= ChangeSpeed;
            joystickChannel.Unsubscribe(SetJoystick);
            enterBossStageEventChannel.Unsubscribe(ResetCharacterPosition);
            enterEventStageEventChannel.Unsubscribe(MoveCharacterTo);
            restartGameEventChannel.Unsubscribe(ResetCharacterXZPos);
            userInputControlChannel.Unsubscribe(OnOffControl);
            abilityButtonPressedEventChannel.Unsubscribe(OnJumpPressed);
            mainCameraChannel.Unsubscribe(SetMainCameraController);
            characterControllerChannel.Unregister(_controller);
            onLiquidBehaviourChannel.Unsubscribe(SetLiquidBehaviour);
        }

        private void SetMainCameraController(CameraController cameraController)
        {
            mainCameraController = cameraController;
        }

        private void SetJoystick(Joystick joystick)
        {
            this.joystick = joystick;
        }

        private void ChangeSpeed(float speed)
        {
            if (isSpeedBoosted)
            {
                CancelInvoke(nameof(ResetSpeed));
                ResetSpeed();
            }

            MoveSpeed = speed;
        }

        public void BoostMoveSpeed(float time)
        {
            if (isSpeedBoosted)
            {
                CancelInvoke(nameof(ResetSpeed));
            }
            else
            {
                prevSpeed = MoveSpeed;
            }

            MoveSpeed += speedBoostIncrement;
            Invoke(nameof(ResetSpeed), time);
            isSpeedBoosted = true;
        }

        private void FixedUpdate()
        {
            playerMoveDirectionChannel.UpdateMoveInfo(transform.position, GetDirection());
        }

        private void Update()
        {
            if (gamePauser.Pause)
            {
                return;
            }

            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void OnOffControl(bool val)
        {
            _controller.Move(Vector3.zero);
            _playerInput.enabled = val;
            _input.move = Vector2.zero;
            inputEnabled = val;
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        public void OnJumpPressed()
        {
            _isJumpPressed = true;
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);

            int colliderCount = Physics.OverlapSphereNonAlloc(spherePosition, GroundedRadius, liquidColliders, LayerMaskCash.Water, QueryTriggerInteraction.Collide);

            bool OnLiquid = false;

            if (colliderCount > 0)
            {
                GameObject liquid = liquidColliders[0].gameObject;

                if (Mathf.Abs(liquid.transform.position.y - transform.position.y) < liquidHeightCheckRange) // 그냥 최소 기준?
                {
                    OnLiquid = true;
                }
            }

           if (OnLiquid)
            {
                Grounded = true;
                OnHitGround();

                if (hasLiquidBehaviour)
                {
                    liquidBehaviour.OnLiquid(this, characterVFXController);
                }
            }
            else
            {
                Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                    QueryTriggerInteraction.Ignore);

                if (Grounded)
                {
                    OnHitGround();
                }

                if (hasLiquidBehaviour)
                {
                    liquidBehaviour.OutLiquid(this, characterVFXController);
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
            float targetSpeed = GetTargetSpeed();

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if ((_input.move == Vector2.zero && joystick.Direction == Vector2.zero) || !inputEnabled) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x - extraMoveForce.x, 0.0f, _controller.velocity.z - extraMoveForce.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * _speedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * speedRoundFactor) / speedRoundFactor;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (joystick.Direction != Vector2.zero && inputEnabled)
            {
                inputDirection = new Vector3(joystick.Horizontal, 0.0f, joystick.Vertical).normalized;
            }

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if ((_input.move != Vector2.zero || joystick.Direction != Vector2.zero) 
                && inputEnabled)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;

                if (clampRotation)
                {
                    float clampAmount = RotationLimitPerSecond * Time.deltaTime;
                    float currentYRotation = transform.eulerAngles.y;

                    if (_targetRotation < 0) 
                    {
                        _targetRotation += 360;
                    } 
                    // _targetRotation과 transform.eulerAngles.y의 사이클을 맞추어줘야함
                    _targetRotation = ClampAngle(_targetRotation, currentYRotation - clampAmount, currentYRotation + clampAmount);
                }


                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    _rotationSmoothTime);

                if (rotation == float.NaN)
                {
                    Debug.LogError("rotation is NaN" + transform.eulerAngles.y + " " + _targetRotation + " " + _rotationVelocity);
                }

                // rotate to face input direction relative to camera position
                if (!animationController.IsSpinning)
                {
                    // 회전하지 않을 때만 로테이션 업데이트
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }

            }

            _targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            // move the player
            _controller.Move(_targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, VerticalVelocity, 0.0f) * Time.deltaTime + 
                             extraMoveForce * Time.deltaTime);

            extraMoveForce = Vector3.zero;

            // update animator if using characterAbillity
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (_abilityTimeoutDelta >= 0.0f)
            {
                _abilityTimeoutDelta -= Time.deltaTime;
            }

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
                    VerticalVelocity = groundedVelocityClamp;
                }

                // Jump
                if ((_input.jump || _isJumpPressed) && _abilityTimeoutDelta <= 0.0f && inputEnabled)
                {
                    characterAbillity.PerformIfNotLocked();
                    _isJumpPressed = false;
                    _input.jump = false;
                    _abilityTimeoutDelta = AbilityTimeout;
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
            // Determine if the range crosses the 0/360 boundary

            if ( lfMin < lfAngle && lfAngle < lfMax)
            {
                return lfAngle;
            }
            else
            {
                
                float minDelta = Mathf.DeltaAngle(lfAngle, lfMin);
                float maxDelta = Mathf.DeltaAngle(lfAngle, lfMax);

                return Mathf.Abs(minDelta) < Mathf.Abs(maxDelta) ? lfMin : lfMax;                
            }
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle < -720f) angle += 360f;
            while (angle >= 720f) angle -= 360f;
            return angle;
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
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center),
                        FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center),
                    FootstepAudioVolume);
            }
        }

        #region Custom Public Methods

        public Vector3 GetDirection()
        {
            return _targetDirection;
        }

        public float GetSpeed()
        {
            return _speed;
        }

        public float GetMaxSpeed()
        {
            return MoveSpeed;
        }

        public void MoveCharacterTo(Vector3 position)
        {
            _controller.enabled = false;
            Vector3 movement = position - transform.position;
            playerPositionMovedChannel.Raise(movement);
            transform.position = position;
            _controller.enabled = true;
        }

        public void MoveOnlyCharacterTo(Vector3 position)
        {
            _controller.enabled = false;
            transform.position = position;
            _controller.enabled = true;
        }

        public void PutAwayCharacter()
        {
            MoveOnlyCharacterTo(TemporalPlatform.Position);
        }

        private void ResetCharacterPosition()
        {
            MoveCharacterTo(Vector3.zero);
        }

        private void ResetCharacterXZPos()
        {
            Vector3 resetPos = new Vector3(0, transform.position.y, 0);
            MoveCharacterTo(resetPos);
        }

        public void Jump(bool highJump = false)
        {
            // the square root of H * -2 * G = how much velocity needed to reach desired Height
            VerticalVelocity = Mathf.Sqrt(-2f * JumpHeight * Gravity);

            if (highJump)
            {
                VerticalVelocity *= highJumpMultiplier;
            }

            // update animator if using characterAbillity
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, true);
            }
        }

        public void JumpIfOnGround()
        {
            if (Grounded)
            {
                Jump();
            }
        }

        public void HyperJump()
        {
            VerticalVelocity = hyperJumpMultiplier * Mathf.Sqrt(-2f * JumpHeight * Gravity) * highJumpMultiplier;
            
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, true);
            }
        }

        public void Teleport()
        {
            Vector3 movement = teleportDistance * _targetDirection;

            // check if player collide when teleport

            Vector3 teleportPosition = transform.position + movement;

            if (IsLocationOccupied(teleportPosition))
            {
                if (!IsLocationOccupied(teleportPosition + Vector3.up))
                {
                    teleportPosition += Vector3.up;
                }
                else if (!IsLocationOccupied(teleportPosition - _targetDirection))
                {
                    teleportPosition -= _targetDirection;
                }
                else if (!IsLocationOccupied(teleportPosition + _targetDirection))
                {
                    teleportPosition += _targetDirection;
                }
                else
                {
                    return;
                }
            }
            _controller.enabled = false;
            transform.position = teleportPosition;
            _controller.enabled = true;
            mainCameraController.TargetMove(movement);
        }

        public void Dash()
        {
            _controller.Move(_targetDirection.normalized * DashForce);
        }

        public void AddExtraForce(Vector3 direction, float force)
        {
            if (gamePauser.Pause)
            {
                return;
            }

            if (Grounded)
            {
                force -= 0.8f; // static friction

                if (force > 0)
                {
                    extraMoveForce += direction.normalized * force;
                }   
            }
            else
            {
                extraMoveForce += direction.normalized * force;
            }
        }

        private bool IsLocationOccupied(Vector3 position)
        {
            return Physics.OverlapBoxNonAlloc(position, Vector3.one * 0.47f, colliders, Quaternion.identity,
                LayerMaskCash.Enemy + LayerMaskCash.Ground, QueryTriggerInteraction.Ignore) > 0;
        }


        public void MoveManually(Vector3 movement)
        {
            _controller.Move(movement * Time.deltaTime);
        }

        private void ResetSpeed()
        {
            MoveSpeed = prevSpeed;
            isSpeedBoosted = false;
        }

        public void IgnoreInput()
        {
            userInputControlChannel.Raise(false);
            _input.move = Vector2.zero;
        }

        public void IgnoreInputUntillHitGround()
        {
            userInputControlChannel.Raise(false);
            ignoreInputUntillHitGround = true;
        }

        private void OnHitGround()
        {
            if (ignoreInputUntillHitGround)
            {
                ListenInput();
                ignoreInputUntillHitGround = false;
            }

            if (clampRotation)
            {
                clampRotation = false;
            }
        }

        private float GetTargetSpeed()
        {
            float targetSpeed = MoveSpeed;

            if (hasLiquidBehaviour && liquidBehaviour.TryGetSpeedRate(out float speedRate))
            {
                targetSpeed *= speedRate;
            }

            targetSpeed += bonusMoveSpeed;
            return targetSpeed;
        }

        public void ClampRotation()
        {
            clampRotation = true;
        }


        public void ListenInput()
        {
            userInputControlChannel.Raise(true);
        }

        public void SetAgilityStat(int agility)
        {
            bonusMoveSpeed = agility * agilityMult;
        }

        public void SetFastRotation(bool value)
        {
            if (value)
            {
                SetBaseRotationSmoothTime(0.02f);
            }
            else
            {
                SetBaseRotationSmoothTime(baseRotationSmoothTime);
            }
        }

        public void SetSpeedChangeRate(float val)
        {
            _speedChangeRate = val;
        }

        public void ResetSpeedChangeRate()
        {
            _speedChangeRate = SpeedChangeRate;
        }

        public void SetBaseRotationSmoothTime(float val)
        {
            _rotationSmoothTime = val;
            RotationSmoothTime = val;
        }
        
        public void SetRotationSmoothTime(float val)
        {
            _rotationSmoothTime = val;
        }

        public void ResetRotationSmoothTime()
        {
            _rotationSmoothTime = RotationSmoothTime;
        }

        private void SetLiquidBehaviour(OnLiquidBehaviour liquidBehaviour)
        {
            if (!ReferenceEquals(this.liquidBehaviour, null))
            {
                this.liquidBehaviour.OutLiquid(this, characterVFXController);
            }

            this.liquidBehaviour = liquidBehaviour;
            liquidBehaviour.ResetState();
            hasLiquidBehaviour = liquidBehaviour != null;
        }
        #endregion
    }
    }