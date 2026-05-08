using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BlinkWhenHit : MonoBehaviour
{
    private PlayerAnimationController playerAnimationController;
    private Damagable damagable;
    private UnityAction<Vector3> action;
    void Awake()
    {
        playerAnimationController = GetComponent<PlayerAnimationController>();
        action = (direction) => Blink();
        damagable = GetComponent<Damagable>();
        damagable.OnHit.AddListener(action);
    }

    private void Blink()
    {
        playerAnimationController.Blink();
    }
}
