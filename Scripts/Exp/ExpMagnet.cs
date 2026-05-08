using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class ExpMagnet : MonoBehaviour
{
    [SerializeField]
    private EventChannelSO finishLevelUpEventChannel, finishStageMoveEventChannel;
    
    [SerializeField] 
    private float magnetForce = 1f;
    
    private HashSet<Transform> exps = new HashSet<Transform>();
    private List<Transform> expsToRemove = new List<Transform>();

    private SphereCollider collider;
    private string expTag = "Exp";
    private string magneticItemTag = "MagneticItem";

    private void Start()
    {
        collider = GetComponent<SphereCollider>();
       
    }

    private void OnEnable()
    {
        CharacterUpgradableStat.MagnetRangeChanged += ChangeMagnetRange;
        finishStageMoveEventChannel.Subscribe(TurnOnMagnet);
        finishLevelUpEventChannel.Subscribe(TurnOffMagnet);
    }

    private void OnDisable()
    {
        CharacterUpgradableStat.MagnetRangeChanged -= ChangeMagnetRange;
    }

    private void OnDestroy()
    {
        finishStageMoveEventChannel.Unsubscribe(TurnOnMagnet);
        finishLevelUpEventChannel.Unsubscribe(TurnOffMagnet);
    }

    private void TurnOnMagnet()
    {
        enabled = true;
    }

    private void TurnOffMagnet()
    {
        enabled = false;
    }

    private void ChangeMagnetRange(float magnetRange)
    {
        collider.radius = magnetRange;
    }
    
    // 플레이어와 부딛힐 경우 삭제해야함
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(expTag) || other.CompareTag(magneticItemTag))
        {
            if (exps.Contains(other.transform))
            {
                Debug.Log("Exp already in list.");
                return;
            }
            
            exps.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(expTag) || other.CompareTag(magneticItemTag))
        {
            if (!exps.Contains(other.transform))
            {
                Debug.LogError("Exp not in list.");
            }
            
            exps.Remove(other.transform);
        }
    }

    private void Update()
    {
        foreach (Transform exp in exps)
        {
            if (exp == null || exp.gameObject.activeSelf == false)
            {
                expsToRemove.Add(exp);
                continue;
            }
            
            Vector3 direction = (transform.position - exp.transform.position).normalized;
            exp.transform.position += direction * (magnetForce * Time.deltaTime);
        }

        foreach (Transform exp in expsToRemove)
        {
            exps.Remove(exp);
        }
        
        expsToRemove.Clear();
    }
}
