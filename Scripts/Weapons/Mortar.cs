using System.Collections;
using System.Collections.Generic;
using Local.Scripts.Extensions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityStandardAssets.Utility;
public class Mortar : QuickFirableWeapon<UWaitForSeconds>
{
    [SerializeField] 
    private Bomb bombPrefab;
    
    [SerializeField]
    private Explosive explosivePrefab;

    [SerializeField]
    private Transform rotatingPart;

    [SerializeField] 
    private Transform barrel;

    [SerializeField] 
    private GameObject fireFX;

    [SerializeField]
    private GameObject fallPointMarkerPrefabForPlayer;

    [SerializeField]
    private SFXTags launchSFX;

    [SerializeField]
    private CharacterControllerChannelSO characterControllerChannel;

    [SerializeField]
    private PlayerMoveDirectionChannelSO playerMoveInfoChannel;

    private TrajectoryRenderer trajectoryRenderer;
    private GameObject fallPointMarker;
    private CharacterController characterController;
    private bool useTrajectoryRenderer, onShooting;
    private const float predictPositionFactor = 0.2f;
    private const float angleWrapThreshold = 180f;
    private const float angleWrapFull = 360f;
    private const float velocityConvergenceStep = 0.1f;
    private const float velocityConvergenceTolerance = 0.5f;
    private const int maxConvergenceIterations = 1000;
    private float forceConst = 5f;
    private Transform mainCameraTransform;
    private FollowTarget followingRotatingPart;
    private float predictShootProbability = 0.5f;
    private float reversePredictShootProbability = 0.1f;
    private PredictMode predictMode;
    private Vector3 predictionOffset;
    private int clockwise = 1;

    private Vector3 initialRotatingPartLocalPos;
    private Quaternion initialRotatingPartLocalRot;

    private struct UpgradableStat
    {
        public List<float> ExplosiveDamage;
        public List<float> ExplosiveRange, OverHeatDelay, NumProjectiles;
    }

    private UInt UExplosiveDamage;
    private UInt UNumProjectilePerOnce;
    private UFloat UExplosiveRange;
    
    IObjectPool<Bomb> bombPool;
    ObjectPool<Explosive> explosivePool;

    private enum RotationMode
    {
        Align,  // 타겟을 따라가는 모드
        RotateOnce, // 회전하면서 발사하는 모드
        Reset, // 회전 발사 전 회전값을 초기화 하는 모드
        None, 
    }

    private enum PredictMode
    {
        Predict,
        ReversePredict,
        None,
    }

    private RotationMode rotationMode;
    float timeCount = 0f;

    public float SlerpTime => slerpTime;

    float slerpTime = 0.5f;
    Quaternion targetRotation;
    Quaternion startRotation;

    float rotationTime = 3f;
    float shootTime;
    float shootTimeCount = 0f;

    protected override void Awake()
    {
        base.Awake();
        
        trajectoryRenderer = GetComponent<TrajectoryRenderer>();
        bombPool = new ObjectPool<Bomb>(CreateBomb, OnGetBomb, OnReleaseBomb, OnDestroyBomb, maxSize:10);
        explosivePool = new ObjectPool<Explosive>(CreateExplosive, OnGetExplosive, OnReleaseExplosive, OnDestroyExplosive, maxSize:10);
        fireFX.SetActive(false);
        fireFX.transform.parent = null;
        onMountedOnPlayer = OnMountedOnPlayer;
        onMountedOnEnemy = OnMountedOnEnemy;
        fallPointMarker = Instantiate(fallPointMarkerPrefabForPlayer);
        fallPointMarker.SetActive(false);
        followingRotatingPart =rotatingPart.GetComponent<FollowTarget>();
        followingRotatingPart.enabled = false;

        initialRotatingPartLocalPos = rotatingPart.localPosition;
        initialRotatingPartLocalRot = rotatingPart.localRotation;
    }

    
    private void OnMountedOnPlayer()
    {
        useTrajectoryRenderer = true;
        trajectoryRenderer.enabled = true;
        trajectoryRenderer.RayHit += DrawPredictedFallPoint;
        fallPointMarker.SetActive(true);
        bombPool.Clear();
        explosivePool.Clear();
        StartCoroutine(Fire());

        if(!characterControllerChannel.TryGetVariable(out characterController))
        {
            Debug.LogError("Fail to get CharacterController.");
        }
    }
    
    private void OnMountedOnEnemy()
    {
        useTrajectoryRenderer = false;
        rotatingPart.transform.parent = transform;
        followingRotatingPart.enabled = false;
        bombPool.Clear();
        explosivePool.Clear();
        rotationMode = RotationMode.None;
        StopAllCoroutines();
    }

    protected override void SetupUpgradables()
    {
        UpgradableStat upgradableStat = UpgradeExtension.ReadUpgradableStat<UpgradableStat>(this);
        InitializeUpgradables(upgradableStat);
    }

    private void Update()
    {
        if (!useTrajectoryRenderer)
        {
            return; // 이러면 Enemy
        }

        if (!onShooting && user.TryGetTarget(out target))
        {
            StartCoroutine(Fire());
        }

        trajectoryRenderer.UpdateTrajectory(barrel.forward * forceConst + 0.5f *characterController .velocity);
    }

    private void LateUpdate()
    {
       // slerp or rotate
       switch (rotationMode)
        {
            case RotationMode.Align:
                AlignBarrelTowardTarget();
                break;
            case RotationMode.RotateOnce:
                ShootingWhileRotating();
                break;
            case RotationMode.Reset:
                ResetBarrelRotation();
                break;
            case RotationMode.None:
                break;
        }
    }

    private void AlignBarrelTowardTarget()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = target.position;
        Vector3 targetForward = targetPosition - transform.position;
        float distance = targetForward.magnitude;

        if (mountedOnEnemy)
        {
            switch (predictMode) 
            {
                case PredictMode.Predict:
                    targetPosition += predictPositionFactor * playerMoveInfoChannel.GetLatestMoveInfo().Value * distance;
                    targetForward = targetPosition - transform.position;
                    break;
                case PredictMode.ReversePredict:
                    targetPosition -= predictPositionFactor * playerMoveInfoChannel.GetLatestMoveInfo().Value * distance;
                    targetForward = targetPosition - transform.position;
                    break;
            }
        }

        targetForward.y = 0f;
        targetRotation = Quaternion.LookRotation(targetForward, Vector3.up);

        if (timeCount < slerpTime)
        {
            timeCount += Time.deltaTime;
            rotatingPart.rotation = Quaternion.Slerp(startRotation, targetRotation, timeCount / slerpTime);
        }
        else
        {
            rotatingPart.rotation = targetRotation;
        }

    }

    private void ResetBarrelRotation()
    {
        if (timeCount < slerpTime)
        {
            timeCount += Time.deltaTime;
            targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            rotatingPart.rotation = Quaternion.Slerp(startRotation, targetRotation, timeCount / slerpTime);

            if (timeCount > slerpTime)
            {
                rotatingPart.transform.parent = transform;
                followingRotatingPart.enabled = false;
                rotationMode = RotationMode.None;
            }
        }
    }

    private void ShootingWhileRotating()
    {
        if (timeCount < rotationTime)
        {
            timeCount += Time.deltaTime;
            shootTimeCount += Time.deltaTime;
            rotatingPart.Rotate(Vector3.up, 360f * Time.deltaTime / rotationTime);

            if (shootTimeCount > shootTime)
            {
                shootTimeCount = 0f;
                ShootBombs();
            }
        }
    }

    public override void OnUnmounted()
    {
        useTrajectoryRenderer = false;
        trajectoryRenderer.enabled = false;
        trajectoryRenderer.RayHit -= DrawPredictedFallPoint;
        fallPointMarker.SetActive(false);
        rotatingPart.transform.parent = transform;
        followingRotatingPart.enabled = false;

        rotatingPart.localPosition = initialRotatingPartLocalPos;
        rotatingPart.localRotation = initialRotatingPartLocalRot;

        StopAllCoroutines();
    }

    private void DrawPredictedFallPoint((Vector3, Vector3) pointAndNormal)
    {
        Vector3 point = pointAndNormal.Item1;
        Vector3 normal = pointAndNormal.Item2;

        fallPointMarker.transform.position = point;

        // y axis of object is forward
        // forward value must be some straight line from the surface of the plane which normal is normal.
        fallPointMarker.transform.forward = normal;
    }

    public override void Damage()
    {
        ShootBombs();
    }

    public void ShootTarget()
    {
        if (ReferenceEquals(target, null))
        {
            return;
        }

        Vector3 moveDirectionPlayer =playerMoveInfoChannel.GetLatestMoveInfo().Value;
        Vector3 targetPos = target.position - Vector3.up * 0.5f;
        Vector3 barrelPos = barrel.position;

        float heightDiff = barrelPos.y - targetPos.y;
        targetPos.y = barrelPos.y = 0f;

        float xzDistance = Vector3.Distance(targetPos, barrelPos);

        switch (predictMode)
        {
            case PredictMode.Predict:
                targetPos += predictPositionFactor * moveDirectionPlayer * xzDistance;
                break;
            case PredictMode.ReversePredict:
                targetPos -= predictPositionFactor * moveDirectionPlayer * xzDistance;
                break;
        }

        xzDistance = Vector3.Distance(targetPos, barrelPos);    // recalculate xzDistance


        float angle = barrel.rotation.eulerAngles.x;

        if (angle > angleWrapThreshold)
        {
            angle = Mathf.Abs(angleWrapFull - angle);
        }



        float velocity = CalculateRequiredInitialVelocity(angle, xzDistance, heightDiff);
        
        ShootBombs(velocity);
    }

    public void SetAlignMode() 
    {
        float prob = RandomExtenstion.GetFloatInRange(0f, 1f);

        if (prob < predictShootProbability)
        {
            predictMode = PredictMode.Predict;
        }
        else if (prob < reversePredictShootProbability)
        {
            predictMode = PredictMode.ReversePredict;
        }
        else
        {
            predictMode = PredictMode.None;
        }

        timeCount = 0f;
        startRotation = rotatingPart.rotation;

        rotatingPart.transform.parent = null;
        followingRotatingPart.enabled = true;
    
        rotationMode = RotationMode.Align;
    }

    public void ResetRotationMode()
    {
        timeCount = 0f;
        startRotation = rotatingPart.rotation;

        rotatingPart.transform.parent = null;
        followingRotatingPart.enabled = true;
        rotationMode = RotationMode.Reset;
    }


    private void InitializeUpgradables(UpgradableStat upgradableStat)
    {
        UExplosiveDamage = new UInt(upgradableStat.ExplosiveDamage, symbol: symbolContainer.ExplosionDamage,
            optionText: CardText.EXPLOSION_DAMAGE);
        UExplosiveRange = new UFloat(upgradableStat.ExplosiveRange, symbol: symbolContainer.Scale,
            optionText: CardText.EXPLOSION_RADIUS);
        UExplosiveRange.Upgraded += OnRangeUpgraded;
        UFireWaitTime = new UWaitForSeconds(upgradableStat.OverHeatDelay, symbol: symbolContainer.Refresh,
            optionText: CardText.RELOAD_SPEED);
        UNumProjectilePerOnce = new UInt(upgradableStat.NumProjectiles, symbol: symbolContainer.RateOfFire,
            optionText: CardText.RATE_OF_FIRE, noBonus: true);
    }
    
    private IEnumerator Fire()
    {
        onShooting = true;

        if (!user.TryGetTarget(out Transform target))
        {
            onShooting = false;
            rotationMode = RotationMode.None;
            yield break;
        }

        // 1. 정렬
        // 시작 y 회전 값에서 카메라 y 회전값으로 Slerp

        rotationMode = RotationMode.Align;
        timeCount = 0f;
        startRotation = rotatingPart.rotation;
        rotatingPart.transform.parent = null;
        followingRotatingPart.enabled = true;

        yield return new WaitForSeconds(slerpTime);

        // 2. 회전하며 발싸

        rotationMode = RotationMode.RotateOnce;
        shootTime = rotationTime / UNumProjectilePerOnce.Value;
        shootTimeCount = 0f;
        timeCount = 0f;
        ShootBombs();
        
        yield return new WaitForSeconds(rotationTime);

        // 3. 대기

        rotatingPart.transform.parent = transform;
        followingRotatingPart.enabled = false;
        yield return UFireWaitTime.Value;

        StartCoroutine(Fire());
    }

    private void ShootBombs()
    {
        fireFX.SetActive(false);
        Bomb bomb = bombPool.Get();
        Vector3 force = barrel.forward * forceConst;
        FMODAudioManager.instance.PlayOneShot(launchSFX, barrel.position);

        // Assume that the player is moving and enemy is not.
        if (!mountedOnEnemy)
        {
            force += 0.5f * characterController.velocity;
            bomb.Shoot(force);
        }
        else
        {
            if (trajectoryRenderer.TryCalcFallPoint(force, out Vector3 fallPoint, out Vector3 normal))
            {
                bomb.Shoot(force, fallPoint, normal);
            }
            else
            {
                bomb.Shoot(force);
            }
        }

        fireFX.transform.SetPositionAndRotation(barrel.position, barrel.rotation);
        fireFX.SetActive(true);
    }

    private void ShootBombs(float forceMagnitude)
    {
        fireFX.SetActive(false);
        Bomb bomb = bombPool.Get();
        Vector3 force = barrel.forward * forceMagnitude;
        FMODAudioManager.instance.PlayOneShot(launchSFX, barrel.position);

        if (!mountedOnEnemy)
        {
            force += 0.5f * characterController.velocity;
        }

        if (mountedOnEnemy && trajectoryRenderer.TryCalcFallPoint(force, out Vector3 fallPoint, out Vector3 normal))
        {
            bomb.Shoot(force, fallPoint, normal);
        }
        else
        {
            bomb.Shoot(force);
        }

        fireFX.transform.SetPositionAndRotation(barrel.position, barrel.rotation);
        fireFX.SetActive(true);
    }

    public void Explode(Vector3 position)
    {
        Explosive explosive = explosivePool.Get();
        explosive.transform.position = position;
        explosive.Explode();
    }

    private void OnRangeUpgraded()
    {
        // update explosive's scale in pool.
    }
    
    private Bomb CreateBomb()
    {
        Bomb bomb = Instantiate(bombPrefab.gameObject, barrel.position, barrel.rotation).GetComponent<Bomb>();
        bomb.SetManagedPool(bombPool);
        bomb.SetMother(this);
        return bomb;
    }

    private void OnGetBomb(Bomb bomb)
    {
        bomb.transform.position = barrel.position;
        bomb.gameObject.SetActive(true);
        bomb.SetUsedByPlayer(!mountedOnEnemy);
    }

    private void OnReleaseBomb(Bomb bomb)
    {
        bomb.gameObject.SetActive(false);
    }

    private void OnDestroyBomb(Bomb bomb)
    {
        Destroy(bomb.gameObject);
    }
    
    private Explosive CreateExplosive()
    {
        Explosive explosive = Instantiate(explosivePrefab).GetComponent<Explosive>();
        explosive.SetManagedPool(explosivePool);
        explosive.SetWeapon(this);
        return explosive;
    }
    
    private void OnGetExplosive(Explosive explosive)
    {
        explosive.gameObject.SetActive(true);
        explosive.SetDamage(UExplosiveDamage.Value);
        explosive.SetRange(UExplosiveRange.Value);
        explosive.SetTargetLayer(mountedOnEnemy ? LayerMaskCash.PlayerAndEnemy : LayerMaskCash.Enemy);
    }
    
    private void OnReleaseExplosive(Explosive explosive)
    {
        explosive.gameObject.SetActive(false);
    }
    
    private void OnDestroyExplosive(Explosive explosive)
    {
        Destroy(explosive.gameObject);
    }
    
    public override void BeItem()
    {
        transform.localPosition -= 0.2f * Vector3.up;
    }

    private float GetForceToReachTarget(Transform target)
    {
        Vector3 targetPos = target.position;
        Vector3 barrelPos = barrel.position;

        float g = Physics.gravity.y;
        float verticalDistance = targetPos.y - barrelPos.y;
        float launchAngleRad = Mathf.Abs(barrel.rotation.eulerAngles.x) * Mathf.Deg2Rad;

        targetPos.y = barrelPos.y = 0f;

        float horizontalDistance = Vector3.Distance(targetPos, barrelPos);
        
        float v0 = Mathf.Sqrt(g * horizontalDistance / Mathf.Sin(2 * launchAngleRad));

        return v0;

        


    }

    private float CalculateRequiredInitialVelocity(float angleDegrees, float distance, float heightDiff)
    {

        float gravity = -Physics.gravity.y;
        float tolerance = velocityConvergenceTolerance;
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float initialVelocityGuess = Mathf.Sqrt(distance * gravity / Mathf.Sin(2 * angleRadians)); // Initial guess
        float calculatedRange = 0f;

        int count = 0;

        while (true)
        {
            calculatedRange = (initialVelocityGuess * Mathf.Cos(angleRadians)) / gravity *
                (initialVelocityGuess * Mathf.Sin(angleRadians) +
                Mathf.Sqrt(Mathf.Pow(initialVelocityGuess * Mathf.Sin(angleRadians), 2) + 2 * gravity * heightDiff));

            // Check if the calculated range is within the tolerance of the target distance
            if (Mathf.Abs(calculatedRange - distance) <= tolerance || count > maxConvergenceIterations)
            {
                break;
            }

            if (calculatedRange < distance)
            {
                initialVelocityGuess += velocityConvergenceStep; // Adjust guess as needed
            }
            else
            {
                initialVelocityGuess -= velocityConvergenceStep; // Adjust guess as needed
            }

            count++;
        }

        return initialVelocityGuess;
    }

    private void OnEnable()
    {
        rotatingPart.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (gameObject.activeSelf)
        {
            return;
        }

        if (rotationMode != RotationMode.None)
        {
            rotatingPart.gameObject.SetActive(false);
        }

        rotationMode = RotationMode.None;
    }
}
