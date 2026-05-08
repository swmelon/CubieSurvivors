using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;


public abstract class EnemySpawnDevice<T> : Poolable<T>, IEnemySpawnDevice, IEasyListener where T : EnemySpawnDevice<T>
{
    public EnemyData EnemyData
    {
        get => enemyData;
        set => enemyData = value;
    }
    public float SpawnPeriod
    {
        set
        {
            spawnPeriod = value;
        }
    }
    
    [SerializeField]
    protected EnemySpawner enemySpawner;
    
    [SerializeField]
    protected EnemyData enemyData;

    [SerializeField] 
    private UnityEvent afterSpawning;

    [SerializeField] 
    private bool spawnPeriodically = true;
    
    [SerializeField]
    private float spawnPeriod = 4.0f;

    [SerializeField] 
    private bool ignoreOverflow = false;

    private float startDelay = 3.0f;
    private float timeCounter = 0.0f;
    private bool activated = false;
    private Transform invisablePusher;

    private Vector3 startPusherPosition;
    private Vector3 endPusherPosition;
    private static Quaternion identity = Quaternion.identity;

    private void Awake()
    {
        if (spawnPeriodically)
        {
            // ������ ��Ģ�̾� �� ������!

            invisablePusher = transform.GetChild(transform.childCount - 1);
            startPusherPosition = invisablePusher.localPosition;
        }
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        activated = false;

        if (!spawnPeriodically)
        {
            return;
        }

        invisablePusher.localPosition = startPusherPosition;
        endPusherPosition = new Vector3(startPusherPosition.x, startPusherPosition.y, -startPusherPosition.z);

        // spawn immediately finish delay
        timeCounter = spawnPeriod;
    }

    public void Activate()
    {
        if (spawnPeriodically)
        {
            StartCoroutine(ActivateAfterDelay());
        }   
    }
    
    
    private IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        activated = true;
    }

    private void Update()
    {
        if (!spawnPeriodically || !activated)
        {
            return;
        }

        timeCounter += Time.deltaTime;

        if (timeCounter < 1f)
        {
            invisablePusher.localPosition = Vector3.Lerp(startPusherPosition, endPusherPosition, timeCounter);
        }
    }

    public void OnBeat(EasyEvent easyEvent)
    {
        if (!spawnPeriodically || !activated)
        {
            return;
        }

        if (timeCounter >= spawnPeriod)
        {
            timeCounter = 0.0f;
            SpawnEnemy();
        }
    }

    public virtual Enemy SpawnEnemy()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = enemyData.Scale / 2;

        if (enemySpawner.TrySpawn(enemyData, spawnPosition, identity, out Enemy enemy))
        {
            afterSpawning.Invoke();
            return enemy;
        }

        return null;
    }

    public void AddActionAfterSpawning(UnityAction action)
    {
        afterSpawning.AddListener(action);
    }
}
