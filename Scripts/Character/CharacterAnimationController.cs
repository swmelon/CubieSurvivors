using System;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class CharacterAnimationController : MonoBehaviour
{
    private Animator animator;
    private int walkSpeedHash,motionSpeedHash, emotionSpeedHash;
    private int walkingHash, spinHash, swingHash, jumpHash, jumpInPlaceHash, jumpForwardHash, 
        hammerDownHash, dashHash, shakeHash, stabRightHash, stabLeftHash;
    private int isBackWeaponSlotFront;
    
    private int lookAroundHash, getAngryHash, spinEyeBallsHash, sadHash, winkHash, blinkHash, sleepMHash, sleepEHash, 
        wakeUpMHash, wakeUpEHash;

    private const float walkSpeedConst = 0.8f;
    private bool awaked = false;
    private void Awake()
    {
        if (awaked)
        {
            return;
        }
        
        awaked = true;
        animator = GetComponent<Animator>();
        walkingHash = Animator.StringToHash("Walking");
        spinHash = Animator.StringToHash("Spin");
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
        wakeUpMHash = Animator.StringToHash("WakeUpM");
        wakeUpEHash = Animator.StringToHash("WakeUpE");
        isBackWeaponSlotFront = Animator.StringToHash("IsBackWeaponSlotFront");
        shakeHash = Animator.StringToHash("Shake");
        stabRightHash = Animator.StringToHash("StabRight");
    }
    
    
    public void SetMotionSpeed(float speed) => animator.SetFloat(motionSpeedHash, speed);
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
    }
    
    public void StopWalking()
    {
        animator.SetBool(walkingHash, false);
    }
    
    public void Spin() => animator.SetTrigger(spinHash);
    
    public void SwingLeftAndRight() => animator.SetTrigger(swingHash);
    
    public void Jump() => animator.SetTrigger(jumpHash);
    
    public void JumpAndDive() => animator.SetTrigger(jumpInPlaceHash);
    
    public void JumpForwardAndDive() => animator.SetTrigger(jumpForwardHash);
    
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
        animator.SetBool(isBackWeaponSlotFront, true);
    }

    public void BackWeaponSlotToBack()
    {
        Jump();
        animator.SetBool(isBackWeaponSlotFront, false);
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
}
