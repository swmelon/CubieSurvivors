

using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Auranium : Poolable<Auranium>
{
    private const float collectAnimationDuration = 2f;
    [SerializeField]
    private SFXTags SFXTags;

    [SerializeField]
    private GemManagerSO gemManager;
    
    [SerializeField]
    private TransformChannelSO playerTransformChannel;

    [SerializeField] 
    private Vector3EventChannelSO playerPositionResetEventChannel;
    
    [SerializeField]
    private float initialMagnetForce = 1f;

    [SerializeField]
    private AnimationCurve magnetForceCurve;

    [SerializeField]
    private MeshRenderer meshRenderer;

    private AnimationScript animationScript;
    private Transform playerTransform;
    private float magnetForce;
    
    private bool animating = false;
    private float time, animTime, period;
    private int coinAmount = 1;
    private static string playerTag = "Player";
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        TryGetComponent(out animationScript);
        animationScript.isAnimated = true;
        animating = false;
        animTime = 0f;
        time = 0f;
        meshRenderer.enabled = true;
        playerPositionResetEventChannel.Subscribe(MoveAlongWithPlayer);
        
        if (!playerTransformChannel.TryGetVariable(out playerTransform))
        {
            Debug.LogError(name + ": Player transform not found!");
            Release();
        }

        Vector3 direction = playerTransform.position - transform.position;
        float distance = direction.magnitude;

        period = distance;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!animating && !Released && other.CompareTag(playerTag))
        {
            gemManager.GetAuranium(coinAmount);
            animating = true;
            meshRenderer.enabled = false;
            if (!ReferenceEquals(animationScript, null))
            {
                animationScript.isAnimated = false;
            }

            FMODAudioManager.instance.PlayOneShot(SFXTags);
        }
    }

    private void Update()
    {
        time += Time.deltaTime;

        if (animating)
        {
            animTime += Time.deltaTime;

            if (animTime >= collectAnimationDuration)
            {
                Release();
            }
        }

        transform.position = Vector3.Lerp(transform.position, playerTransform.position, magnetForceCurve.Evaluate(time / period));
    }

    private void MoveAlongWithPlayer(Vector3 movement)
    {
        transform.position += movement;
    }

    private void OnDisable()
    {
        playerPositionResetEventChannel.Unsubscribe(MoveAlongWithPlayer);
    }
}
