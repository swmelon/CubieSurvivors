

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EventStageManager : SingleInstancePoolingWithDataMB<EventStage, EventStageData>
{
    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private EventChannelSO completeScenarioFirstTimeEC;

    [SerializeField]
    private IntChannelSO eventStageIndexChannel;

    private SaveFile saveFile;
    private Dictionary<int, bool> eventStageUnlocked;
    private int numUnlockedEventStages = 0;

    private void Start()
    {
        completeScenarioFirstTimeEC.Subscribe(UnlockEventStage);

        saveFile = saveLoadManager.SaveFile;
        eventStageUnlocked = saveFile.eventStageUnlocked;

        foreach (KeyValuePair<int, bool> stage in eventStageUnlocked)
        {
            if (stage.Key != numUnlockedEventStages)
            {
                // index와 현재 해금된 스테이지의 개수가 일치하지 않는 경우
                saveLoadManager.DataCorrupted();
                break;
            }

            if (stage.Value)
            {
                numUnlockedEventStages += 1;
            }
        }
    }

    private void OnDestroy()
    {
        completeScenarioFirstTimeEC.Unsubscribe(UnlockEventStage);
    }

    public bool AtLeastOneStageIsUndiscovered()
    {
        return numUnlockedEventStages != 0;
    }

    public List<EventStageData> eventStageDatas => prefabDataList;


    public EventStage Get(int index)
    {
        if (index >= numUnlockedEventStages)
        {
            Debug.LogError("Stage index is not discoverable yet.");
        }

        foreach (var data in prefabDataList)
        {
            if (data.index == index)
            {
                eventStageIndexChannel.Register(index);
                return Get(data);
            }
        }

        Debug.LogError("Stage index not found.");
        return null;
    }

    public EventStageData GetData(int index)
    {
        if (index >= numUnlockedEventStages)
        {
            Debug.LogError("Stage index is not discoverable yet.");
        }

        foreach (var data in prefabDataList)
        {
            if (data.index == index)
            {
                return data;
            }
        }

        Debug.LogError("Stage index not found.");
        return null;
    }

    public bool TryGetNextData(int currentIndex, out EventStageData data)
    {
        if (currentIndex >= numUnlockedEventStages - 1)
        {
            data = null;
            return false;
        }

        foreach (var d in prefabDataList)
        {
            if (d.index == currentIndex + 1)
            {
                data = d;
                return true;
            }
        }

        data = null;
        return false;
    }

    public bool TryGetPrevData(int currentIndex, out EventStageData data)
    {
        if (currentIndex <= 0)
        {
            data = null;
            return false;
        }

        if (currentIndex >= numUnlockedEventStages)
        {
            Debug.LogError("Stage index is not discoverable yet.");
            data = null;
            return false;
        }

        foreach (var d in prefabDataList)
        {
            if (d.index == currentIndex - 1)
            {
                data = d;
                return true;
            }
        }

        data = null;
        return false;
    }
    
    // 언제 새로운 이벤트 스테이지를 해금할 것인지?
    // 스테이지를 클리어 할 때마다 
    public void UnlockEventStage()
    {
        foreach (var data in prefabDataList)
        {
            if (data.index == numUnlockedEventStages)
            {
                saveFile.eventStageUnlocked[numUnlockedEventStages] = true;
                saveFile.ShowDiscoveredStageButton = true;
                saveFile.DiscoverExclamationMark = true;
                saveLoadManager.Save();
                numUnlockedEventStages += 1;
                return;
            }
        }

    }
    public EventStage GetRecentlyUnlocked()
    {
        foreach (var data in prefabDataList)
        {
            if (data.index == numUnlockedEventStages - 1)
            {
                return Get(data);
            }
        }

        Debug.LogError("Stage index not found.");
        return null;
    }

    public EventStage GetRandomUndiscovered()
    {
        return null;
    }

}
