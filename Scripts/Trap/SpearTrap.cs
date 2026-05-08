using UnityEngine;

public class SpearTrap : Trap
{
    private Animator animator;
    private int popupID;
    
    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        popupID = Animator.StringToHash("Popup");
    }
    
    protected override void On()
    {
        animator.SetTrigger(popupID);
    }
    
    protected override void Off()
    {
        // do nothing
    }
}
