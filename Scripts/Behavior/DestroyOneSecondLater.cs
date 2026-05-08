using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOneSecondLater : MonoBehaviour
{
    private const float destroyDelay = 1f;

    void Awake()
    {
        Destroy(gameObject, destroyDelay);
    }
}
