using System;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class PopupFlameTrap : FlameTrap
{
    private Animator animator;
    private int openID, closeID;

    private Transform head;
    private Quaternion initialHeadRotation;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        openID = Animator.StringToHash("Open");
        closeID = Animator.StringToHash("Close");
        head = transform.GetChild(0);
        initialHeadRotation = head.localRotation;
    }

    protected override void On()
    {
        animator.SetTrigger(openID);
        base.On();
    }

    protected override void Off()
    {
        animator.SetTrigger(closeID);
        base.Off();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        base.Off();
        head.localRotation = initialHeadRotation;
    }
}
