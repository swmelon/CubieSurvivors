using Local.Scripts.Extensions;
using System;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class EnemyAnimationController : MonoBehaviour
{
    public event Action OnLanded;
    public event Action OnTossUp;

    [SerializeField]
    protected FloorGeoDataChannel floorGeoDataChannel;

    private static readonly string walkingName = "Walking", spinName = "Spin", spinLeftName = "SpinLeft", swingName = "Swing", jumpName = "Jump", jumpInPlaceName = "JumpInPlace",
        jumpForwardName = "JumpForward", hammerDownName = "HammerDown", dashName = "Dash", lookAroundName = "LookAround",
        getAngryName = "GetAngry", spinEyeBallsName = "SpinEyeBalls", sadName = "Sad", winkName = "Wink", blinkName = "Blink",
        sleepMName = "SleepM", sleepEName = "SleepE", wakeUpMName = "WakeUpM", wakeUpEName = "WakeUpE", shakeName = "Shake",
        stabRightName = "StabRight", deadName = "Dead", dancingName = "Dancing", walkSpeedName = "walkSpeed", motionSpeedName = "motionSpeed", emotionSpeedName = "emotionSpeed",
        isBackWeaponSlotFrontName = "IsBackWeaponSlotFront", tossupName = "Tossup", reconstructName = "Reconstruct";
    private static int walkSpeedHash, motionSpeedHash, emotionSpeedHash;
    private static int walkingHash, spinHash, spinLeftHash, swingHash, jumpHash, jumpInPlaceHash, jumpForwardHash,
        hammerDownHash, dashHash, shakeHash, stabRightHash, stabLeftHash, deadHash, dancingHash;
    private static int isBackWeaponSlotFrontHash, tossupHash, reconstructHash;

    private static int lookAroundHash, getAngryHash, spinEyeBallsHash, sadHash, winkHash, blinkHash, sleepMHash, sleepEHash,
        wakeUpMHash, wakeUpEHash;

    private static bool hashInitialized = false;


    public AnimationCurve scaleWhileJumping, heightWhileJumping, jumpTimeByDistance;
    private const float jumpHeightDistanceFactor = 0.1f;
    private const float landingCheckHeight = 5f;
    private const float landingBoxCastRange = 10f;
    private const float enemyHeightHalfScale = 0.5f;
    protected static float jumpDistanceMult = 0.85f;
    private static float MaxJumpTimeDistance = 15f;

    private Animator animator;
    protected Enemy enemy;
    private const float walkSpeedConst = 0.8f;
    private bool awaked = false;
    private bool isJumping = false;
    private Vector3 initialScale, scaleOnStartJump;
    private float jumpTimeCount = 0;
    protected float jumpTime = 1f;
    private BoxCollider boxCollider;
    private float motionSpeed = 1f;
    private float jumpSpeed = 1f;
    private float jumpHeight = 1f;
    private Vector3 landingPos, posOnStartJump;
    private float jumpDistance = 0f;
    private Rigidbody rb;
    private Transform enemyBody;

    private bool spinning = false;
    public bool IsSpinning => spinning;
    public bool IsJumping => isJumping;

    protected virtual void Awake()
    {
        if (!hashInitialized)
        {
            InitializeHash();
            hashInitialized = true;
        }

        if (awaked)
        {
            return;
        }

        initialScale = transform.localScale;
        awaked = true;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        enemy = GetComponent<Enemy>();
        enemyBody = transform.GetChild(0);
    }

    private static void InitializeHash()
    {
        walkingHash = Animator.StringToHash(walkingName);
        spinHash = Animator.StringToHash(spinName);
        spinLeftHash = Animator.StringToHash(spinLeftName);
        swingHash = Animator.StringToHash(swingName);
        jumpHash = Animator.StringToHash(jumpName);
        jumpInPlaceHash = Animator.StringToHash(jumpInPlaceName);
        jumpForwardHash = Animator.StringToHash(jumpForwardName);
        hammerDownHash = Animator.StringToHash(hammerDownName);
        dashHash = Animator.StringToHash(dashName);
        lookAroundHash = Animator.StringToHash(lookAroundName);
        getAngryHash = Animator.StringToHash(getAngryName);
        spinEyeBallsHash = Animator.StringToHash(spinEyeBallsName);
        sadHash = Animator.StringToHash(sadName);
        walkSpeedHash = Animator.StringToHash(walkSpeedName);
        motionSpeedHash = Animator.StringToHash(motionSpeedName);
        emotionSpeedHash = Animator.StringToHash(emotionSpeedName);
        winkHash = Animator.StringToHash(winkName);
        blinkHash = Animator.StringToHash(blinkName);
        sleepMHash = Animator.StringToHash(sleepMName);
        sleepEHash = Animator.StringToHash(sleepEName);
        wakeUpMHash = Animator.StringToHash(wakeUpMName);
        wakeUpEHash = Animator.StringToHash(wakeUpEName);
        isBackWeaponSlotFrontHash = Animator.StringToHash(isBackWeaponSlotFrontName);
        shakeHash = Animator.StringToHash(shakeName);
        stabRightHash = Animator.StringToHash(stabRightName);
        deadHash = Animator.StringToHash(deadName);
        dancingHash = Animator.StringToHash(dancingName);
        tossupHash = Animator.StringToHash(tossupName);
        reconstructHash = Animator.StringToHash(reconstructName);
    }


    public void SetMotionSpeed(float speed)
    { 
        Debug.Assert(speed >= 0f, "Speed must be positive.");
        motionSpeed = speed;
        animator.SetFloat(motionSpeedHash, speed); 
    }

    public void ResetMotionSpeed() => animator.SetFloat(motionSpeedHash, 1f);
    public void SetEmotionSpeed(float speed = 1f) => animator.SetFloat(emotionSpeedHash, speed);

    public void SetWalkSpeed(float speed)
    {
        Debug.Assert(speed >= 0f, "Speed must be positive.");

        if (!awaked)
        {
            Awake();
        }

        animator.SetFloat(walkSpeedHash, walkSpeedConst * speed);
    }

    public void Walk(bool val)
    {
        animator.SetBool(walkingHash, val);
        animator.SetBool(dancingHash, false);   
    }

    public void StopWalking()
    {
        animator.SetBool(walkingHash, false);
    }

    public void Spin() => animator.SetTrigger(spinHash);
    public void SpinLeft() => animator.SetTrigger(spinLeftHash);

    public void SwingLeftAndRight() => animator.SetTrigger(swingHash);

    public void Jump()
    {
        CheckLandingSpace(0f);
        animator.SetTrigger(jumpHash);
        isJumping = true;

        scaleOnStartJump = transform.localScale;
        posOnStartJump = transform.position;
    }

    public void JumpAndDive()
    {
        CheckLandingSpace(0f);
        animator.SetTrigger(jumpHash);
        isJumping = true;

        scaleOnStartJump = transform.localScale;
        posOnStartJump = transform.position;
    }

    public void JumpAndDiveFixedSpeed(float animSpeed)
    {
        JumpAndDive();
        SetMotionSpeed(animSpeed);
        jumpSpeed = animSpeed;
    }

    public void RotateTowardTarget()
    {
        enemy.RotateTowardTarget();
    }

    public void JumpForwardAndDive()
    {
        if (isJumping || !enemy.TryGetTarget(out Transform target)) return;

        landingPos = GetLandingPos(target);

        if (!CheckLandingSpace(landingPos))
        {
            return;
        }

        jumpSpeed = 1 / jumpTime;
        animator.SetTrigger(jumpHash);
        isJumping = true;

        scaleOnStartJump = transform.localScale;
        posOnStartJump = transform.position;
    }

    protected virtual Vector3 GetLandingPos(Transform target)
    {
        float jumpDistance = Mathf.Min((target.transform.position - transform.position).magnitude);
        jumpTime = CalculateJumpTime(jumpDistance);
        return GetLandingPosFromJumpDistance(jumpDistance);
    }

    protected float CalculateJumpTime(float distance)
    {
        return jumpTimeByDistance.Evaluate(Mathf.Min(distance, MaxJumpTimeDistance)/ MaxJumpTimeDistance);
    }

    public void HammerDown() => animator.SetTrigger(hammerDownHash);
    
    public void Dash() => animator.SetTrigger(dashHash);
    
    public void GetAngry() => animator.SetTrigger(getAngryHash);
    
    public void LookAround() => animator.SetTrigger(lookAroundHash);
    
    public void SpinEyeBalls() => animator.SetTrigger(spinEyeBallsHash);
    public void BeSad() => animator.SetTrigger(sadHash);
    public void Wink() => animator.SetTrigger(winkHash);
    public void Blink() => animator.SetTrigger(blinkHash);
    public void Shake() => animator.SetTrigger(shakeHash);
    public void StabRight() => animator.SetTrigger(stabRightHash);
    
    public void BackWeaponSlotToFront()
    {
        Jump();
        animator.SetBool(isBackWeaponSlotFrontHash, true);
    }

    public void BackWeaponSlotToBack()
    {
        Jump();
        animator.SetBool(isBackWeaponSlotFrontHash, false);
    }

    public void Die()
    {
        animator.SetBool(deadHash, true);
    }


    public void Sleep()
    {
        animator.ResetTrigger(wakeUpMHash);
        animator.ResetTrigger(wakeUpEHash);
        animator.SetTrigger(sleepMHash);
        animator.SetTrigger(sleepEHash);
    }
    
    public void WakeUp()
    {
        animator.ResetTrigger(sleepMHash);
        animator.ResetTrigger(sleepEHash);
        animator.SetTrigger(wakeUpMHash);
        animator.SetTrigger(wakeUpEHash);
    }

    public void Dance()
    {
        animator.SetBool(walkingHash, false);
        animator.SetBool(dancingHash, true);
    }

    public void TossUp()
    {
        animator.SetTrigger(tossupHash);
    }

    public void Reconstruct()
    {
        animator.ResetTrigger(tossupHash);
        animator.ResetTrigger(reconstructHash);
        animator.ResetTrigger(jumpHash);
        animator.ResetTrigger(jumpForwardHash);
        animator.ResetTrigger(jumpInPlaceHash);
        animator.ResetTrigger(spinHash);
        animator.ResetTrigger(spinLeftHash);
        animator.SetTrigger(reconstructHash);
    }

    public void InvokeOnTossUp()
    {
        OnTossUp?.Invoke();
        OnTossUp = null;
    }

    protected virtual void Update()
     {
        if (!isJumping)
        {
            return;
        }

        jumpTimeCount += Time.deltaTime;

        float animTime = jumpTimeCount * jumpSpeed;

        float yScaleFactor = scaleWhileJumping.Evaluate(animTime);
        Vector3 scale = scaleOnStartJump;
        scale.y *= yScaleFactor;

        transform.localScale = scale;
        transform.position = Vector3.Lerp(posOnStartJump, landingPos, animTime)
            + new Vector3(0f, heightWhileJumping.Evaluate(animTime) * (jumpHeight + jumpHeightDistanceFactor * jumpDistance), 0f);

        if (animTime > 1f)
        {
            OnLanding();
        }
    }

    protected virtual void OnLanding()
    {
        isJumping = false;
        jumpTimeCount = 0;
        transform.localScale = initialScale;
        transform.position = landingPos;
        jumpTime = 0f;
        OnLanded?.Invoke();
        OnLanded = null;
    }


    private void OnDisable()
    {
        // 재사용시 스케일이 바뀌는 것을 방지
        transform.localScale = initialScale;
        animator.SetBool(dancingHash, false);
        enemyBody.localRotation = Quaternion.identity;
    }


    private bool CheckLandingSpace(float jumpDistance)
    {
        landingPos = GetLandingPosFromJumpDistance(jumpDistance);
        return CheckLandingSpace(landingPos);
    }

    private Vector3 GetLandingPosFromJumpDistance(float jumpDistance)
    {
        Vector3 forwardNormalized = transform.forward;
        forwardNormalized.y = 0;
        forwardNormalized.Normalize();
        Vector3 pos = transform.position + jumpDistanceMult * jumpDistance * forwardNormalized;

        if (!floorGeoDataChannel.OnStage(pos))
        {
            return Vector3.zero;
        }

        return pos;
    }

    private bool CheckLandingSpace(Vector3 landingPosition)
    {
        // check ground layer using BoxCast from height 5 to -5 from the landing position which is height 0
        landingPosition.y = landingCheckHeight;

        float enemyHeight = transform.localScale.x * enemyHeightHalfScale;
        bool noGround = !floorGeoDataChannel.TryGetHeightOf(landingPosition, out float geoDataHeight);

        if (noGround)
        {
            return false;
        }

        if (Physics.BoxCast(landingPosition, (boxCollider.size * transform.localScale.x) / 2, Vector3.down, out RaycastHit hit, Quaternion.identity, landingBoxCastRange, LayerMaskCash.GroundAndWater))
        {
            // calculate height of ground
            float height = hit.point.y;

            height = Mathf.Max(height, geoDataHeight);

            print("Height: " + height);

            landingPos.y = height + enemyHeight;
        }
        else
        {
            landingPos.y = geoDataHeight + enemyHeight;
        }

        return true;
    }
} 
