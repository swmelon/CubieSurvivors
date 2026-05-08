using System;
using UnityEngine;


public abstract class CoveredItem<T> : Item where T : MonoBehaviour, IItemizable
{
    protected enum AnimationMode
    {
        None,
        Popout,
        Tracking
    }

    [SerializeField]
    protected T content;

    [SerializeField]
    private AnimationCurve magnetForceCurve;

    [SerializeField]
    private TransformChannelSO playerTransformChannel;

    private Transform contentParent;
    private Animator animator;
    protected Collider collider;
    private int popUpHash, parachuteHash;

    private Vector3 endPos, startPos;
    protected bool isMoving;
    protected float timeCount;
    protected float moveTime = 1f;
    protected AnimationMode animationMode = AnimationMode.None;
    private float magnetForce = 10f;
    private Transform playerTransform;

    protected virtual void Awake()
    {
        contentParent = transform.GetChild(0);
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider>();
        popUpHash = Animator.StringToHash("PopUp");
        parachuteHash = Animator.StringToHash("Parachute");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        collider.enabled = false;
    }

    private void PopUp() => animator.SetTrigger(popUpHash);
    private void Parachute() => animator.SetTrigger(parachuteHash);
    
    public virtual void SetContent(T content, bool parachute)
    {
        SetContent(content);
        
        if (parachute)
        {
            Parachute();
        }
        else
        {
            PopUp();
        }

        content.BeItem();
    }

    public virtual void SetContent(T content)
    {
        if (!ReferenceEquals(this.content, null))
        {
        }

        this.content = content;

        Transform contentTransform = content.transform;
        contentTransform.parent = contentParent;
        contentTransform.localPosition = Vector3.zero;
        contentTransform.rotation = Quaternion.identity;
        content.BeItem();
    }
    private void EndParachute()
    {
        Release();
    }

    public void SetPopOutPosition(Vector3 startPos, Vector3 endPos)
    {
        this.startPos = startPos;
        this.endPos = endPos;
        isMoving = true;
        timeCount = 0f;
        animationMode = AnimationMode.Popout;
    }

    protected override void Update()
    {
        base.Update();

        switch (animationMode)
        {
            case AnimationMode.Popout:
                timeCount += Time.deltaTime;
                float t = timeCount / moveTime;
                transform.position = Vector3.Lerp(startPos, endPos, t);

                if (t >= moveTime)
                {
                    animationMode = AnimationMode.None;
                }
                break;
            case AnimationMode.Tracking:
                Vector3 direction = playerTransform.position - transform.position;
                float distance = direction.magnitude;
                direction.Normalize();
                transform.position += Time.deltaTime * magnetForce * magnetForceCurve.Evaluate(Mathf.Max(0, 10 - distance)) * direction;
                break;
        }
    }

    public void TrackPlayer()
    {
        if(ReferenceEquals(playerTransformChannel, null) || !playerTransformChannel.TryGetVariable(out playerTransform))
        {
            return;
        }

        animationMode = AnimationMode.Tracking;
    }

    public override void Activate(Player player)
    {
        base.Activate(player);
        animationMode = AnimationMode.None;
    }
}
