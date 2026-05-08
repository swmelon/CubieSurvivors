using DG.Tweening.Core.Easing;
using Local.Scripts.Extensions;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour, IAnimationController
{
    [SerializeField]
    private Renderer rollerRenderer;

    private Color baseRollerColor;
    private Animator animator;
    private int walkSpeedHash, motionSpeedHash, emotionSpeedHash;
    private int walkingHash, spinHash, spinLeftHash, swingHash, jumpHash, jumpInPlaceHash, jumpForwardHash,
        hammerDownHash, dashHash, shakeHash, stabRightHash, stabLeftHash, hitHash;
    private int isBackWeaponSlotFront;

    private int lookAroundHash, getAngryHash, spinEyeBallsHash, sadHash, winkHash, blinkHash, sleepMHash, sleepEHash,
        wakeUpMHash, wakeUpHash, deadHash;

    private int freeFallHash;
    private int groundedHash;

    private const float walkSpeedConst = 0.8f;
    private bool awaked = false;
    private bool isJumping = false;
    private Vector3 initialScale, scaleOnStartJump;
    private float jumpTimeCount = 0;
    private BoxCollider boxCollider;
    private float motionSpeed = 1f;
    private float jumpHeight = 1f;
    private Vector3 landingPos, posOnStartJump;

    private Rigidbody rb;
    private DamagableAlly damagable;
    private int currentEmotionHash;

    private bool isDead;
    private float blinkInterval;
    private float blinkTimeCount = 0f;
    
    private bool hit = false;
    private float hitFlashTime = 0.2f;
    private float hitFlashTimeCount = 0f;

    private bool spinning = false;

    public bool IsSpinning => spinning;
    public bool IsInAir => animator.GetBool(freeFallHash);
    private void Awake()
    {
        if (awaked)
        {
            return;
        }

        initialScale = transform.localScale;

        awaked = true;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        damagable = GetComponent<DamagableAlly>();
        walkingHash = Animator.StringToHash("Walking");
        spinHash = Animator.StringToHash("Spin");
        spinLeftHash = Animator.StringToHash("SpinLeft");
        swingHash = Animator.StringToHash("Swing");
        jumpHash = Animator.StringToHash("Jump");
        jumpInPlaceHash = Animator.StringToHash("JumpInPlace");
        jumpForwardHash = Animator.StringToHash("JumpForward");
        hammerDownHash = Animator.StringToHash("HammerDown");
        dashHash = Animator.StringToHash("Dash");
        lookAroundHash = Animator.StringToHash("LookAround");
        getAngryHash = Animator.StringToHash("GetAngry");
        spinEyeBallsHash = Animator.StringToHash("SpinEyeBalls");
        sadHash = Animator.StringToHash("Sad");
        walkSpeedHash = Animator.StringToHash("walkSpeed");
        motionSpeedHash = Animator.StringToHash("motionSpeed");
        emotionSpeedHash = Animator.StringToHash("emotionSpeed");
        winkHash = Animator.StringToHash("Wink");
        blinkHash = Animator.StringToHash("Blink");
        sleepMHash = Animator.StringToHash("SleepM");
        sleepEHash = Animator.StringToHash("SleepE");
        wakeUpHash = Animator.StringToHash("WakeUp");
        isBackWeaponSlotFront = Animator.StringToHash("IsBackWeaponSlotFront");
        shakeHash = Animator.StringToHash("Shake");
        stabRightHash = Animator.StringToHash("StabRight");
        deadHash = Animator.StringToHash("Dead");
        hitHash = Animator.StringToHash("Hit");
        damagable.OnDead.AddListener(Die);
        damagable.OnRevive.AddListener(OnPlayerRevive);
        damagable.OnHit.AddListener(OnHit);

        freeFallHash = Animator.StringToHash("FreeFall");
        groundedHash = Animator.StringToHash("Grounded");

        // 인스턴스가 생겨도 상관없다.
        baseRollerColor = rollerRenderer.material.color;
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
        if (isDead)
        {
            return;
        }

        animator.SetBool(walkingHash, val);
    }

    public void StopWalking()
    {
        if (isDead)
        {
            return;
        }

        animator.SetBool(walkingHash, false);
    }

    public void Spin()
    {
        if (isDead || spinning)
        {
            return;
        }

        if (animator.GetBool(freeFallHash))
        {
            animator.SetBool(freeFallHash, false);
        }

        if (animator.GetBool(groundedHash))
        {
            animator.SetBool(groundedHash, false);
        }

        spinning = true;
        animator.SetTrigger(spinHash);
    }

    public void SpinLeft()
    {
        if (isDead || spinning)
        {
            return;
        }

        if (animator.GetBool(freeFallHash))
        {
            animator.SetBool(freeFallHash, false);
        }

        if (animator.GetBool(groundedHash))
        {
            animator.SetBool(groundedHash, true);
        }

        spinning = true;
        animator.SetTrigger(spinLeftHash);
    }

    public void FinishSpinning()
    {
        spinning = false;
    }

    public void SwingLeftAndRight()
    {
        if (isDead)
        {
            return;
        }
        animator.SetTrigger(swingHash);
    }

    public void HammerDown()
    {
        if (isDead)
        {
            return;
        }
        animator.SetTrigger(hammerDownHash);
    }

    public void Dash()
    {
        if (isDead)
        {
            return;
        }
        animator.SetTrigger(dashHash);
    }

    public void GetAngry() 
    {
        if (isDead)
        {
            return;
        }
        SetCurrentEmotionHash(getAngryHash);
        animator.SetTrigger(getAngryHash);
    }

    public void LookAround()
    {
        if (isDead)
        {
            return;
        }

        SetCurrentEmotionHash(lookAroundHash);
        animator.SetTrigger(lookAroundHash);
    }

    public void SpinEyeBalls()
    {
        if (isDead)
        {
            return;
        }

        SetCurrentEmotionHash(spinEyeBallsHash);
        animator.SetTrigger(spinEyeBallsHash);
    }
    public void BeSad() 
    {
        if (isDead)
        {
            return;
        }
        SetCurrentEmotionHash(sadHash);
        animator.SetTrigger(sadHash);
    }
    public void Wink()
    {
        if (isDead)
        {
            return;
        }
        SetCurrentEmotionHash(winkHash);
        animator.SetTrigger(winkHash);
    }
    public void Blink()
    {
        if (isDead)
        {
            return;
        }
        SetCurrentEmotionHash(blinkHash);
        animator.SetTrigger(blinkHash);
    }
    public void Shake() 
    {
        if (isDead)
        {
            return;
        }
        SetCurrentEmotionHash(shakeHash);
        animator.SetTrigger(shakeHash);
    }

    public void StabRight() 
    {
        if (isDead)
        {
            return;
        }
        animator.SetTrigger(stabRightHash);
    }

    public void Die() 
    {

        SetCurrentEmotionHash(deadHash);
        animator.SetBool(deadHash, true);
        isDead = true;
        spinning = false;
    }

    public void OnHit(Vector3 val)
    {
        if (isDead)
        {
            return;
        }

        if (hit)
        {
            return;
        }

        SetCurrentEmotionHash(hitHash);
        animator.SetTrigger(hitHash);
        hit = true;
        hitFlashTimeCount = 0;
    }

    public void Sleep()
    {
        if (isDead)
        {
            return;
        }
        animator.ResetTrigger(wakeUpMHash);
        animator.ResetTrigger(wakeUpHash);
        animator.SetTrigger(sleepMHash);
        animator.SetTrigger(sleepEHash);
    }
    
    public void WakeUp()
    {
        if (isDead)
        {
            return;
        }
        animator.ResetTrigger(sleepMHash);
        animator.ResetTrigger(sleepEHash);
        animator.SetTrigger(wakeUpMHash);
        animator.SetTrigger(wakeUpHash);
    }

    private void SetCurrentEmotionHash(int hash)
    {
        if (hash != currentEmotionHash)
        {
            animator.ResetTrigger(currentEmotionHash);
        }

        currentEmotionHash = hash;
    }

    private void OnPlayerRevive()
    {
        isDead = false;
        animator.SetBool(deadHash, false);
        animator.SetTrigger(wakeUpHash);
    }

    private void Update()
    {
        if (hit)
        {
            hitFlashTimeCount += Time.deltaTime;
            rollerRenderer.material.color = Color.Lerp(baseRollerColor, Color.white, Mathf.PingPong(hitFlashTimeCount/ hitFlashTime, 1));

            if (hitFlashTime < hitFlashTimeCount)
            {
                hit = false;
                hitFlashTimeCount = 0;
                rollerRenderer.material.color = baseRollerColor;
            }

            return;
        }

        
        blinkTimeCount += Time.deltaTime;

        if (blinkTimeCount > blinkInterval)
        {
            Blink();
            blinkTimeCount = 0;
            blinkInterval = RandomExtenstion.GetFloatInRange(2f, 4f);
        }
    }

    private void OnDisable()
    {
        transform.localScale = initialScale;
        animator.ResetTrigger(spinLeftHash);
        animator.ResetTrigger(spinHash);
        spinning = false;
    }    


    private IEnumerator SpinCoroutine()
    {
        while (true)
        {
            Spin();
            yield return new WaitForSeconds(1f);
        }
    }
} 
