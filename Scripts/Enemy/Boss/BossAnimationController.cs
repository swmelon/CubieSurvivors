using UnityEngine;
using System.Collections.Generic;
using Local.Scripts.Extensions;
using System;

public class BossAnimationController : EnemyAnimationController
{
    [SerializeField]
    private PlayerMoveDirectionChannelSO playerMoveDirectionChannel;

    [SerializeField]
    private PlayerChannelSO playerChannel;

    [SerializeField]
    private Renderer rollerRenderer;

    [SerializeField]
    private OnePureEffectSpawner landingPositionMarkerSpawner;

    [Range(0.1f, 1f)]
    [SerializeField]
    private float reversePredictionStrength = 0.5f;

    private const float maxPredictionStrengthCap = 0.9f;
    private const float reverseProbFactor = 0.5f;
    private const float reversePredictSign = -0.5f;

    private bool hit = false;
    private const float hitFlashTime = 0.2f;
    private float hitFlashTimeCount = 0f;

    private Color baseRollerColor;
    private Player player;
    private EnemyStateManager stateManager;
    private List<Vector3> landingPosAvailable = new List<Vector3>();

    private const int maxPredictionStrengthDashCount = 5;
    private const int truncateStage = 3;
    private const float pointStep = 0.5f;
    private PureEffect marker;


    protected override void Awake()
    {
        base.Awake();

        if (!TryGetComponent(out Damagable damagable))
        {
            return;
        }

        damagable.OnHit.AddListener(OnHit);
        stateManager = GetComponent<EnemyStateManager>();
        playerChannel.Subscribe(SetPlayer);
        landingPositionMarkerSpawner.Initialize();
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            playerChannel.Unsubscribe(SetPlayer);
        }
    }

    protected override Vector3 GetLandingPos(Transform target)
    {
        if (!ReferenceEquals(player.transform, target))
        {
            return Vector3.zero;
        }

        Vector3 landingPos;
        Vector3 currentPosition = transform.position;


        // ù ������ �׳� ����
        if (stateManager.DashStateCount <= 1)
        {
            landingPos = base.GetLandingPos(target);
        }
        else
        {
            landingPos = GetBossLandingPos(target);
        }

        Vector3 directionToLandingPos = landingPos - currentPosition;
        directionToLandingPos.y = 0;
        directionToLandingPos.Normalize();

        enemy.Rotate(directionToLandingPos);
        SetMarker(landingPos, directionToLandingPos);

        return landingPos;
    }
    private Vector3 GetPredictedPoint(Vector3 targetPos, Vector3 targetMoveDirection,
        float targetSpeed, float jumpTime, bool reverse = false)
    {
        float sign = reverse ? reversePredictSign : 1f;
        Vector3 predictedPoint = targetPos + sign * targetSpeed * jumpTime * targetMoveDirection;
        return predictedPoint;
    }

    private Vector3 GetBossLandingPos(Transform target)
    {
        // �ʿ��� ���� ��� �� �ҷ�����
        float moveSpeed = player.MoveSpeed;
        Vector3 currentPosition = transform.position;
        Vector3 position = target.position;
        Vector3 targetDirection = position - currentPosition;
        targetDirection.y = 0;
        float distance = targetDirection.magnitude;
        targetDirection.Normalize();
        jumpTime = CalculateJumpTime(distance);
        Debug.Log("Jump Time: " + jumpTime);
        playerMoveDirectionChannel.GetLatestMoveInfo(out Vector3 pos, out Vector3 moveDirection);

        // ���� ���� �����ϱ�
        float predictionStrength = Math.Clamp((float)stateManager.DashStateCount / maxPredictionStrengthDashCount,
            0f, maxPredictionStrengthCap);
        Debug.Log("Prediction Strength: " + predictionStrength);

        // ���� ���� �����ϱ�
        float reverseProb = predictionStrength * reversePredictionStrength * reverseProbFactor;
        bool reversePredict = RandomExtenstion.IsHappen(reverseProb);

        // ���� ����Ʈ �����ϱ�
        Vector3 predictedPoint = GetPredictedPoint(pos, moveDirection, moveSpeed, jumpTime, reversePredict);

        Vector3 defaultPoint = transform.position + jumpDistanceMult * distance * targetDirection;
        Vector3 directionPoint2Point = defaultPoint - predictedPoint;
        directionPoint2Point.y = 0;
        float distancePoint2Point = directionPoint2Point.magnitude;
        directionPoint2Point.Normalize();

        landingPosAvailable.Clear();
        Vector3 pointToCheck = predictedPoint;

        Debug.DrawRay(pointToCheck, Vector3.up, Color.red, 5f);
        Debug.DrawRay(predictedPoint, Vector3.up * 5, Color.blue, 5f);

        if (floorGeoDataChannel.OnStage(pointToCheck))
        {
            landingPosAvailable.Add(pointToCheck);
        }


        pointToCheck += directionPoint2Point * pointStep;

        float minX = Mathf.Min(predictedPoint.x, defaultPoint.x);
        float maxX = Mathf.Max(predictedPoint.x, defaultPoint.x);

        int availableCount = Mathf.RoundToInt((1 - predictionStrength) * (distancePoint2Point / pointStep));

        while (pointToCheck.x < maxX && pointToCheck.x > minX
            && landingPosAvailable.Count < availableCount)
        {
            if (floorGeoDataChannel.OnStage(pointToCheck, truncate: truncateStage))
            {
                landingPosAvailable.Add(pointToCheck);
                Debug.DrawRay(pointToCheck, Vector3.up, Color.yellow, 5f);
            }

            pointToCheck += directionPoint2Point * pointStep;
        }

        if (landingPosAvailable.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 positionPicked = landingPosAvailable.PickRandom();
        Debug.DrawRay(positionPicked, Vector3.up * 5, Color.green, 5f);
        return positionPicked;
    }

    private void SetMarker(Vector3 landingPos, Vector3 directionToRandingPos)
    {
        if(!floorGeoDataChannel.TryGetHeightOf(landingPos, out landingPos.y))
        {
            // ���� �˻縦 �ϱ� ������ �Ͼ �� ���� ��Ȳ
            return;
        }

        Quaternion rotation = Quaternion.LookRotation(directionToRandingPos);
        marker = landingPositionMarkerSpawner.Spawn();
        marker.transform.SetPositionAndRotation(landingPos, rotation);
    }

    protected override void OnLanding()
    {
        base.OnLanding();

        if (ReferenceEquals(marker, null))
        {
            return;
        }

        marker.Release();
        marker = null;
    }


    private void Start()
    {
        if (rollerRenderer != null)
        {
            baseRollerColor = rollerRenderer.material.color;
        }
    }

    public void OnHit(Vector3 val)
    { 
        if (hit)
        {
            return;
        }

        hit = true;
        hitFlashTimeCount = 0;
    }

    protected override void Update()
    {
        base.Update();

        if (hit)
        {
            hitFlashTimeCount += Time.deltaTime;
            rollerRenderer.material.color = Color.Lerp(baseRollerColor, Color.white, Mathf.PingPong(hitFlashTimeCount / hitFlashTime, 1));

            if (hitFlashTime < hitFlashTimeCount)
            {
                hit = false;
                hitFlashTimeCount = 0;
                rollerRenderer.material.color = baseRollerColor;
            }
        }
    }

    private void SetPlayer(Player player)
    {
        this.player = player;
    }
}