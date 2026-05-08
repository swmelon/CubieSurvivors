
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCAnimationContoller : MonoBehaviour
{
    private Animator animator;
    private int animIDSpeed, animIDMotionSpeed;
    
    private void Awake()
    {
        animator = GetComponent<Animator>(); 
        animIDSpeed = Animator.StringToHash("Speed");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void OnEnable()
    {
        animator.SetFloat(animIDMotionSpeed, 0.5f);
        animator.SetFloat(animIDSpeed, 0);
    }
}
