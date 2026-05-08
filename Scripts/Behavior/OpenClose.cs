using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class OpenClose : MonoBehaviour
{
    private Animator animator;
    private int count;
    private bool opened;

    private const string OpenTrigger = "TrOpen";
    private const string CloseTrigger = "TrClose";
    private const string EnemyTag = "Enemy";
    private const string PlayerTag = "Player";

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        count = 0;
        opened = false;
    }

    public void Open()
    {
        if (opened)
        {
            return;
        }

        animator.ResetTrigger(CloseTrigger);
        animator.SetTrigger(OpenTrigger);
        opened = true;
    }

    public void Close()
    {
        if (!opened)
        {
            return;
        }

        animator.ResetTrigger(OpenTrigger);
        animator.SetTrigger(CloseTrigger);
        opened = false;
    }
    
    public void Close(float delay)
    {
        Invoke(nameof(Close), delay);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(EnemyTag) || other.CompareTag(PlayerTag))
        {
            count++;
            Open();
        }
    }
    
    private const float closeDelay = 1f;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(EnemyTag) || other.CompareTag(PlayerTag))
        {
            count--;
            if (count == 0)
            {
                Close(closeDelay);
            }
        }
    }
}
