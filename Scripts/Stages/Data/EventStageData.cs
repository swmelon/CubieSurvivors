
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EventStageData", menuName = "ScriptableObjects/Stage/EventStageData", order = SOAssetMenuIndex.Stage)]
public class EventStageData : PrefabDataWithInstanceSO<EventStage>
{
    public int index;
    public Texture2D icon;
}
