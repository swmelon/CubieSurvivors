using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Bomb : Poolable<Bomb>
{
    [SerializeField]
    private OnePureEffectSpawner fallPointMarkerSpawner;

    [SerializeField]
    private FloorGeoDataChannel floorHeightChannel;

    [SerializeField]
    private BooleanEventChannelSO freefallEC;

    [SerializeField]
    private BooleanChannelSO freefallChannel;

    private Mortar mortar;
    private Rigidbody rb;
    private PureEffect marker;
    private const float rotationSpeed = 100f;
    private const float maxVelocity = 30f;
    private readonly float lifeTime = 10f;
    private float time;
    private bool usedbyPlayer = false;

    private static string playerTag = "Player";

    //�ϴ� �Ϲ� ���� �� ���⸦ ����� ���� ���ٰ� �����Ѵ�.
    private static string bossTag = "Boss";
    private float cumulatedGravity = 0;

    public void SetUsedByPlayer(bool used)
    {
        usedbyPlayer = used;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        time = 0;
        cumulatedGravity = 0;
        freefallEC.Subscribe(OnGravityUserChanged);
        OnGravityUserChanged(freefallChannel.Value);
    }

    public void SetMother(Mortar mortar)
    {
        this.mortar = mortar;
    }
    
    private void Update()
    {
        transform.Rotate(0, Time.deltaTime * rotationSpeed, 0);
        time += Time.deltaTime;

        if (time > lifeTime)
        {
            ResetAndRelease();
        }

        if (freefallChannel.Value)
        {
            cumulatedGravity += Physics.gravity.y * Time.deltaTime;
        }
    }

    private void OnDisable()
    {
        freefallEC.Unsubscribe(OnGravityUserChanged);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Released)
        {
            return;
        }

        if (usedbyPlayer && collision.gameObject.CompareTag(playerTag))
        {
            return;
        }

        if (!usedbyPlayer && collision.gameObject.CompareTag(bossTag))
        {
            return;
        }

        mortar.Explode(transform.position);
        ResetAndRelease();
    }

    public void Shoot(Vector3 force, bool markingFallPoint = false)
    {
        rb.AddForce(force, ForceMode.Impulse);
        Vector3 fallPoint = CalculateFallPoint(force);

        if (markingFallPoint)
        {
            marker = fallPointMarkerSpawner.Spawn();
            marker.transform.position = fallPoint;
        }
    }

    public void Shoot(Vector3 force, Vector3 fallPoint, Vector3 normal)
    {
        rb.AddForce(force, ForceMode.Impulse);
        marker = fallPointMarkerSpawner.Spawn();
        marker.transform.position = fallPoint;
        marker.transform.up = normal;
    }

    private void ResetAndRelease()
    {
        rb.linearVelocity = Vector3.zero;

        if(!ReferenceEquals(marker, null))
        {
            marker.Release();
            marker = null;
        }

        Release();
    }

    private Vector3 CalculateFallPoint(Vector3 initialVelocity)
    {
        float g = Physics.gravity.magnitude; // Acceleration due to gravity
        float initialHeight = transform.position.y; // Initial height

        // Calculate time to hit ground using quadratic formula
        const float halfGravityFactor = 0.5f;
        float a = -halfGravityFactor * g;
        float b = initialVelocity.y;
        float c = initialHeight;

        // Calculate the discriminant
        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            // No real roots, indicating the object will not hit the ground based on the current trajectory
            return Vector3.zero;
        }

        // Calculate time to hit ground (only considering the positive root)
        float timeToHitGround = (-b - Mathf.Sqrt(discriminant)) / (2 * a);

        // Calculate the fall point
        Vector3 horizontalVelocity = new Vector3(initialVelocity.x, 0, initialVelocity.z);
        Vector3 fallPoint = transform.position + horizontalVelocity * timeToHitGround;
        fallPoint.y = floorHeightChannel.GetHeightOf(fallPoint);

        return fallPoint;
    }

    private void OnGravityUserChanged(bool isFreefall)
    {
        if (isFreefall)
        {
            cumulatedGravity = 0;
        }
        else
        {
            if (cumulatedGravity != 0)
            {
                Vector3 velocity = rb.linearVelocity;
                velocity += cumulatedGravity * Vector3.up;

                if (velocity.y < -maxVelocity)
                {
                    velocity.y = -maxVelocity;
                }

                rb.linearVelocity = velocity;
            }
        }

        rb.useGravity = !isFreefall;


    }

}
