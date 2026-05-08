using Local.Scripts.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Serialization;
using UnityEngine.SocialPlatforms.Impl;
using VolumetricLines;

public class BeamSaber : MonoBehaviour
{
    public int SetDamage(int value) => damage = value;
    public float SetMaxBeamLength(float value) => BeamLength = value;
    public void SetWeaponUser(Transform user, WeaponUser userType)
    {
        weaponUser = userType;
        userTransform = user;

        SetMeshColliders(userType == WeaponUser.Enemy);
    }
    public LayerMask SetTargetLayer(LayerMask value) => targetLayer = value;
    public Weapon SetParentWeapon(Weapon value) => parentWeapon = value;
    public void SetBeamColor(Color color) => line.LineColor = color;

    [SerializeField]
    private OnePureEffectSpawner vfxSpawner;

    [SerializeField]
    private RayPoint rayPoint;

    [SerializeField]
    private Transform gunHeadTransform;

    private const float throwSpeed = 20f;
    private const float throwDistance = 30f;
    private const float throwReturnThreshold = 1.5f;
    private const float throwRotationSpeed = 500f;
    private const float playerCastHalfWidth = 0.3f;
    private const float enemyCastHalfWidth = 0.2f;
    private const float returnHeightOffset = 1f;
    private float beamLengthOffset = 0.5f;
    private Vector3[] rayStartPos = new Vector3[4];

    private float BeamLength
    {
        set
        {
            maxLength = value;
            line.StartPos = new Vector3(0, 0, maxLength);
        }
    }

    [SerializeField]
    private LayerMask targetLayer;

    [SerializeField]
    private CustomVolumetricLineBehavior line;

    private int damage = 0;
    private const float maxLength = 3f;
    private float currentLength;
    private HashSet<Damagable> hitMemory = new HashSet<Damagable>();

    private bool isOn = false;
    private int onHash, offHash, throwHash;
    private Animator animator;
    private RaycastHit[] hits = new RaycastHit[32];
    private Weapon parentWeapon;
    private bool isCritical;
    private int finalDamage;
    private Transform userTransform;
    private WeaponUser weaponUser;
    private Vector3 initialLocalPosition;
    private Vector3 initialLocalScale;
    private Vector3 locationInFrontOfPlayer;
    private Quaternion initialLocalRotation;
    private bool thrown = false;
    private bool go = true;
    private Coroutine coroutine;
    private int rotationWay = 1;
    private Collider[] meshColliders;
    private Vector3 origin;

    private void Awake()
    {
        onHash = Animator.StringToHash("On");
        offHash = Animator.StringToHash("Off");
        throwHash = Animator.StringToHash("Throw");
        animator = GetComponent<Animator>();

        initialLocalPosition = gunHeadTransform.localPosition;
        initialLocalRotation = gunHeadTransform.localRotation;
        initialLocalScale = gunHeadTransform.localScale;

        meshColliders = GetComponentsInChildren<Collider>();
    }

    private void FixedUpdate()
    {
        if (!isOn)
        {
            return;
        }

        currentLength = (maxLength + beamLengthOffset) * rayPoint.transform.localScale.z * userTransform.transform.localScale.x;


        switch (weaponUser)
        {
            case WeaponUser.Player:
                CastPlayer();
                break;
            case WeaponUser.Enemy:
                CastEnemy();
                break;
        }
    }

    private void Update()
    {
        if (!thrown)
        {
            return;
        }

        gunHeadTransform.Rotate(0, rotationWay * Time.deltaTime * throwRotationSpeed, 0);

        if (go)
        {
            gunHeadTransform.position = Vector3.MoveTowards(gunHeadTransform.position, locationInFrontOfPlayer, Time.deltaTime * throwSpeed); //Change The Position To The Location In Front Of The Player
        }

        if (!go)
        {
            gunHeadTransform.position = Vector3.MoveTowards(gunHeadTransform.position,
                new Vector3(userTransform.position.x, userTransform.position.y + returnHeightOffset, userTransform.position.z), Time.deltaTime * throwSpeed); //Return To Player
        }

        if (!go && Vector3.Distance(userTransform.position, gunHeadTransform.position) < throwReturnThreshold)
        {
            ThrownBack();
        }
    }

    IEnumerator Boom(float interval)
    {

        go = true;
        yield return new WaitForSeconds(interval);//Any Amount Of Time You Want
        go = false;
    }

    private void CastPlayer()
    {
        float halfWidth = playerCastHalfWidth * userTransform.transform.localScale.x;
        // four corners of the box
        origin = rayPoint.Origin;

        rayStartPos[0] = origin + rayPoint.transform.right * halfWidth;
        rayStartPos[1] = origin - rayPoint.transform.right * halfWidth;
        rayStartPos[2] = origin + rayPoint.transform.up * halfWidth;
        rayStartPos[3] = origin - rayPoint.transform.up * halfWidth;

        for (int i = 0; i < 4; i++)
        {
            Raycast(rayStartPos[i]);
        }

    }

    private void CastEnemy()
    {
        Vector3[] rayStartPos = new Vector3[4];
        float halfWidth = enemyCastHalfWidth * userTransform.transform.localScale.x;
        // four corners of the box
        origin = rayPoint.Origin;

        rayStartPos[0] = origin + rayPoint.transform.right * halfWidth;
        rayStartPos[1] = rayPoint.transform.position - rayPoint.transform.right * halfWidth;

        for (int i = 0; i < 2; i++)
        {
            Raycast(rayStartPos[i]);
        }
    }

    public void TurnOnBeam()
    {
        animator.ResetTrigger(offHash);
        animator.SetTrigger(onHash);
        isOn = true;
    }
    public void TurnOffBeam()
    {
        animator.ResetTrigger(onHash);
        animator.SetTrigger(offHash);
        isOn = false;
    }

    public void ClearHitMemory()
    {
        hitMemory.Clear();
    }

    private void Raycast(Vector3 startPos)
    {
        int count = Physics.RaycastNonAlloc(startPos, rayPoint.transform.forward, hits, currentLength, targetLayer | LayerMaskCash.Prop, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Transform hitTransform = hits[i].transform;
            Vector3 hitPoint = hits[i].point;

            if (hitTransform.TryGetComponent(out Prop prop))
            {
                if (hitTransform.TryGetComponent(out RemovableProp removableProp) && removableProp.Remove())
                {
                    vfxSpawner.Spawn(hitPoint);
                }
            }
            else if (hits[i].transform.TryGetComponent(out Damagable damagable)
                               && !hitMemory.Contains(damagable))
            {
                if (ReferenceEquals(hits[i].transform, userTransform))
                {
                    continue;
                }

                finalDamage = parentWeapon.ComputeFinalDamage(damage, out isCritical);
                damagable.Hit(finalDamage, isCritical: isCritical);
                hitMemory.Add(damagable);
                vfxSpawner.Spawn(hitPoint);
            }
        }
    }

    public void Throw(float interval)
    {
        if (thrown)
        {
            return;
        }

        thrown = true;
        locationInFrontOfPlayer = userTransform.position + userTransform.forward * throwDistance; //Location In Front Of The Player
        gunHeadTransform.parent = null;
        animator.enabled = false;
        coroutine = StartCoroutine(Boom(interval));
    }

    public void SetThrowRotation(bool clockwise)
    {
        if (clockwise)
        {
            rotationWay = 1;
        }
    }

    public void ThrownBack()
    {
        gunHeadTransform.parent = transform.GetChild(0);
        thrown = false;
        go = true;

        gunHeadTransform.localScale = initialLocalScale;
        gunHeadTransform.SetLocalPositionAndRotation(initialLocalPosition, initialLocalRotation);
        animator.enabled = true;

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    private void SetMeshColliders(bool enable)
    {

        foreach (Collider collider in meshColliders)
        {
            collider.enabled = enable;
        }
    }
}