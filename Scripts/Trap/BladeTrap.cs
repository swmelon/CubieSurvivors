
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class BladeTrap : Trap
{
    private Animator animator;
    private int popupID, hideID; 
    
    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        popupID = Animator.StringToHash("Popup");
        hideID = Animator.StringToHash("Hide");
    }
    
    protected override void On()
    {
        animator.SetTrigger(popupID);
    }

    protected override void Off()
    {
        animator.SetTrigger(hideID);
    }
}
