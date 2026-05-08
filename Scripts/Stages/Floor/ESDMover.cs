using UnityEngine;
using System.Collections.Generic;
using System;
using Local.Scripts.Extensions;
using System.Linq;

public class ESDMover : MonoBehaviour, IEasyListener, IEasyPitchListener
{
    private enum Mode
    {
        cycle,
        shake
    }

    [SerializeField]
    private WorldDirectionChannelSO worldDirectionChannel;

    [SerializeField]
    private AnimationCurve moveSpeedCurve;

    private List<LocatableESD>[] esdLists;
    private List<LocatableESD>[] esdWaitLists;
    private Func<Vector3, bool>[] counterClockwiseConditionCheck, clockwiseConditionCheck;
    private Action<Transform>[] action;
    private FloorLEDBuilder floorLEDBuilder;
    private List<Vector3> fixedPoint1;
    private List<Vector3> fixedPoint2;
    private bool moveToPoint1 = true;

    private bool clockwise = false;
    private float vertexCutLength = 2f;
    private float moveSpeedMultiplier = 3f;
    private float period = 1f;
    private float time = 0f;
    private float lastBeatTime;
    private bool needUpdatePeriod = false;
    private Mode mode = Mode.cycle;
    private float shakeLength = 1f;
    private float weakBeatProb = 0.1f;

    private Dictionary<int, Vector3> moveDirections = new Dictionary<int, Vector3>
    {
        {0, Vector3.right},
        {1, new Vector3 (1, 0, 1)},
        {2, Vector3.forward},
        {3, new Vector3 (-1, 0, 1) },
        {4, Vector3.left},
        {5, new Vector3 (-1, 0, -1)},
        {6, Vector3.back},
        {7, new Vector3 (1, 0, -1)}
    };

    private Dictionary<int, Quaternion> rotations = new Dictionary<int, Quaternion>
    {
        //counter clockwise

        {4, Quaternion.Euler(0, 0, 0)},
        {3, Quaternion.Euler(0, 45, 0)},
        {2, Quaternion.Euler(0, 90, 0)},
        {1, Quaternion.Euler(0, 135, 0)},
        {0, Quaternion.Euler(0, 180, 0)},
        {7, Quaternion.Euler(0, 225, 0)},
        {6, Quaternion.Euler(0, 270, 0)},
        {5, Quaternion.Euler(0, 315, 0)}
    };

    private bool updateESD = false;

    private void Awake()
    {
        esdLists = new List<LocatableESD>[8];
        esdWaitLists = new List<LocatableESD>[8];
        counterClockwiseConditionCheck = new Func<Vector3, bool>[8];
        clockwiseConditionCheck = new Func<Vector3, bool>[8];
        action = new Action<Transform>[8];
        fixedPoint1 = new List<Vector3>();
        fixedPoint2 = new List<Vector3>();

        counterClockwiseConditionCheck[0] = (Vector3 position) => position.x > floorLEDBuilder.Size - vertexCutLength;
        counterClockwiseConditionCheck[1] = (Vector3 position) => position.x > floorLEDBuilder.Size;
        counterClockwiseConditionCheck[2] = (Vector3 position) => position.z > floorLEDBuilder.Size - vertexCutLength;
        counterClockwiseConditionCheck[3] = (Vector3 position) => position.z > floorLEDBuilder.Size;
        counterClockwiseConditionCheck[4] = (Vector3 position) => position.x < -floorLEDBuilder.Size + vertexCutLength;
        counterClockwiseConditionCheck[5] = (Vector3 position) => position.x < -floorLEDBuilder.Size;
        counterClockwiseConditionCheck[6] = (Vector3 position) => position.z < -floorLEDBuilder.Size + vertexCutLength;
        counterClockwiseConditionCheck[7] = (Vector3 position) => position.z < -floorLEDBuilder.Size;

        clockwiseConditionCheck[0] = (Vector3 position) => position.x < -floorLEDBuilder.Size + vertexCutLength;
        clockwiseConditionCheck[1] = (Vector3 position) => position.z < -floorLEDBuilder.Size;
        clockwiseConditionCheck[2] = (Vector3 position) => position.z < -floorLEDBuilder.Size + vertexCutLength;
        clockwiseConditionCheck[3] = (Vector3 position) => position.x > floorLEDBuilder.Size;
        clockwiseConditionCheck[4] = (Vector3 position) => position.x > floorLEDBuilder.Size - vertexCutLength;
        clockwiseConditionCheck[5] = (Vector3 position) => position.z > floorLEDBuilder.Size;
        clockwiseConditionCheck[6] = (Vector3 position) => position.z > floorLEDBuilder.Size - vertexCutLength;
        clockwiseConditionCheck[7] = (Vector3 position) => position.x < -floorLEDBuilder.Size;

        action[0] = (Transform transform) => transform.position = new Vector3(floorLEDBuilder.Size - vertexCutLength, transform.position.y, transform.position.z);
        action[1] = (Transform transform) => transform.position = new Vector3(floorLEDBuilder.Size, transform.position.y, -floorLEDBuilder.Size + vertexCutLength);
        action[2] = (Transform transform) => transform.position = new Vector3(transform.position.x, transform.position.y, floorLEDBuilder.Size - vertexCutLength);
        action[3] = (Transform transform) => transform.position = new Vector3(floorLEDBuilder.Size - vertexCutLength, transform.position.y, floorLEDBuilder.Size);
        action[4] = (Transform transform) => transform.position = new Vector3(-floorLEDBuilder.Size + vertexCutLength, transform.position.y, transform.position.z);
        action[5] = (Transform transform) => transform.position = new Vector3(-floorLEDBuilder.Size, transform.position.y, floorLEDBuilder.Size - vertexCutLength);
        action[6] = (Transform transform) => transform.position = new Vector3(transform.position.x, transform.position.y, -floorLEDBuilder.Size + vertexCutLength);
        action[7] = (Transform transform) => transform.position = new Vector3(-floorLEDBuilder.Size + vertexCutLength, transform.position.y, -floorLEDBuilder.Size);

        for (int i = 0; i < esdLists.Length; i++)
        {
            esdLists[i] = new List<LocatableESD>();
            esdWaitLists[i] = new List<LocatableESD>();
        }

        floorLEDBuilder = transform.root.GetComponentInChildren<FloorLEDBuilder>();
        lastBeatTime = Time.time;

    }

    public void OnBeat(EasyEvent audioEvent)
    {
        if (needUpdatePeriod)
        {
            period = FMODAudioManager.instance.BeatLength * 2;
            needUpdatePeriod = false;
        }

        if (!audioEvent.StrongBeat() && !RandomExtenstion.IsHappen(weakBeatProb))
        {
            return;
        }

        time = 0f;
        moveToPoint1 = !moveToPoint1;
    }

    public void OnPitchChanged()
    {
        needUpdatePeriod = true;
    }

    public void AddESD(LocatableESD locatableESD)
    {
        WorldDirection direction = worldDirectionChannel.RotationToDirection(locatableESD.transform.rotation);

        if (direction == WorldDirection.West)
        {
            direction = WorldDirection.East;
        }
        else if (direction == WorldDirection.East)
        {
            direction = WorldDirection.West;
        }

        int index = (2 * ((int)direction + 2)) % 8;
        esdWaitLists[index].Add(locatableESD);
    }

    public void SortESD()
    {
        int sign = false ? 1 : -1;

        esdWaitLists[0].Sort((a, b) => sign * a.transform.position.x.CompareTo(b.transform.position.x));
        esdWaitLists[2].Sort((a, b) => sign * a.transform.position.z.CompareTo(b.transform.position.z));
        esdWaitLists[4].Sort((a, b) => -sign * a.transform.position.x.CompareTo(b.transform.position.x));
        esdWaitLists[6].Sort((a, b) => -sign * a.transform.position.z.CompareTo(b.transform.position.z));

        for (int i = 0; i < 8; i++)
        {
            esdLists[i].Clear();

            if (i % 2 == 0)
            {
                for (int j = 0; j < esdWaitLists[i].Count; j++)
                {
                    esdLists[i].Add(esdWaitLists[i][j]);
                }
            }
        }
    }

    private void Update()
    {
        if (!updateESD)
        {
            return;
        }

        switch (mode)
        {
            case Mode.cycle:
                UpdateCycleMode();
                break;
            case Mode.shake:
                UpdateShakeMode();
                break;
        }
    }

    private void UpdateCycleMode()
    {
        time += Time.deltaTime;

        if (time > period)
        {
            time = 0f;
        }

        float moveSpeed = moveSpeedCurve.Evaluate(time / period) * moveSpeedMultiplier;
        float absMoveSpeed = Mathf.Abs(moveSpeed);

        if (clockwise ^ moveSpeed < 0)
        {
            MoveClockwise(absMoveSpeed);
        }
        else
        {
            MoveCounterClockwise(absMoveSpeed);
        }
    }

    private void UpdateShakeMode()
    {
        time += Time.deltaTime;
        int index = 0;

        for (int i = 0; i < 8; i++)
        {
            List<LocatableESD> esdList = esdLists[i];

            if (esdList.Count == 0)
            {
                continue;
            }

            for (int j = 0; j < esdList.Count; j++)
            {
                Vector3 position = esdList[j].transform.localPosition;

                if (time < period)
                {
                    Vector3 destination = moveToPoint1 ? fixedPoint1[index] : fixedPoint2[index];
                    Vector3 newPosition = Vector3.Slerp(position, destination, time / (period / 2));
                    esdList[j].transform.localPosition = newPosition;
                }

                index += 1;
            }
        }
    }

    private void MoveCounterClockwise(float moveSpeed)
    {
        for (int i = 0; i < 8; i++)
        {
            List<LocatableESD> esdList = esdLists[i];

            if (esdList.Count == 0)
            {
                continue;
            }

            foreach (LocatableESD esd in esdList)
            {
                Vector3 moveDirection = moveDirections[i];
                Vector3 newPosition = esd.transform.position + Time.deltaTime * moveSpeed * moveDirection;
                esd.transform.position = newPosition;
            }

            LocatableESD headESD = esdList[0];

            if (counterClockwiseConditionCheck[i](headESD.transform.position))
            {
                esdList.RemoveAt(0);
                action[i](headESD.transform);

                int nextIndex = (i + 1) % 8;
                esdLists[nextIndex].Add(headESD);
                headESD.transform.rotation = rotations[nextIndex];
            }
        }
    }

    private void MoveClockwise(float moveSpeed)
    {
        for (int i = 0; i < 8; i++)
        {
            List<LocatableESD> esdList = esdLists[i];

            if (esdList.Count == 0)
            {
                continue;
            }

            foreach (LocatableESD esd in esdList)
            {
                Vector3 moveDirection = moveDirections[i];
                Vector3 newPosition = esd.transform.position - Time.deltaTime * moveSpeed * moveDirection;
                esd.transform.position = newPosition;
            }

            int lastIndex = esdList.Count - 1;
            LocatableESD headESD = esdList[lastIndex];

            if (clockwiseConditionCheck[i](headESD.transform.position))
            {
                int nextIndex;
                nextIndex = i - 1;

                if (nextIndex < 0)
                {
                    nextIndex = 7;
                }

                esdList.RemoveAt(lastIndex);
                action[nextIndex](headESD.transform);

                // add front of the list
                List<LocatableESD> nextESDList = esdLists[nextIndex];

                nextESDList.Add(headESD);

                if (nextESDList.Count > 1)
                {
                    for (int j = nextESDList.Count - 2; j >= 0; j--)
                    {
                        nextESDList[j + 1] = nextESDList[j];
                    }

                    nextESDList[0] = headESD;
                }

                headESD.transform.rotation = rotations[nextIndex];
            }
        }
    }

    public void StartMoveESD(bool cycle)
    {
        mode = cycle ? Mode.cycle : Mode.shake;

        switch (mode)
        {
            case Mode.cycle:
                clockwise = RandomExtenstion.FiftyFifty();
                SortESD();
                break;
            case Mode.shake:
                CacheFixedPoint();
                break;
        }

        updateESD = true;
        period = 1;

        ClearWaitList();
    }

    private void CacheFixedPoint()
    {
        fixedPoint1.Clear();
        fixedPoint2.Clear();

        int sign = 1;
        float length = 0.5f * shakeLength;

        for (int i = 0; i < 8; i++)
        {
            esdLists[i].Clear();

            if (i % 2 != 0)
            {
                continue;
            }

            Vector3 delta1 = length * sign * moveDirections[i];
            Vector3 delta2 = -length * sign * moveDirections[i];

            for (int j = 0; j < esdWaitLists[i].Count; j++)
            {
                Vector3 position = esdWaitLists[i][j].transform.localPosition;
                fixedPoint1.Add(position + delta1);
                fixedPoint2.Add(position + delta2);
                esdLists[i].Add(esdWaitLists[i][j]);
            }

            sign *= -1;
        }
    }

    public void StopMoveESD()
    {
        updateESD = false;
    }

    public void ClearWaitList()
    {
        for (int i = 0; i < 8; i++)
        {
            esdWaitLists[i].Clear();
        }
    }
}