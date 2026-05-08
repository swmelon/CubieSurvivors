using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FollowTargetFromChannel : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private TransformChannelSO transformChannel;
    
    private Transform targetTransform;
    
    private void OnEnable()
    {
        transformChannel.Subscribe(SetTarget);
    }
    
    private void OnDisable()
    {
        transformChannel.Unsubscribe(SetTarget);
    }
    
    private void SetTarget(Transform target)
    {
        targetTransform = target;
    }

    private void LateUpdate()
    {
        if(ReferenceEquals(targetTransform, null))
        {
            return;
        }
        
        transform.position = targetTransform.position + offset;
    }
}
