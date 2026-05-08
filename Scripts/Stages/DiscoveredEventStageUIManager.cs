using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DiscoveredEventStageUIManager : MonoBehaviour
{
    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private BooleanEventChannelSO inputOnOffChannel, ingameUIOnOffChannel;

    [SerializeField]
    private Transform contentParent;

    [SerializeField]
    private GameObject contentPrefab;

    [SerializeField]
    private EventStageManager eventStageManager;

    [SerializeField]
    private IntEventChannelSO loadEventChannel;

    private SaveFile saveFile;
    private ObjectActiveController activeController;

    private void Awake()
    {
        saveFile = saveLoadManager.SaveFile;
        activeController = GetComponent<ObjectActiveController>();

        activeController.Activate += OnActivated;
        activeController.Deactivate += OnDeactivated;
    }

    private void OnActivated()
    {
        inputOnOffChannel.Raise(false);
        ingameUIOnOffChannel.Raise(false);

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var info in saveFile.eventStageUnlocked)
        {
            var index = info.Key;
            var unlocked = info.Value; 

            if (unlocked)
            {
                var content = Instantiate(contentPrefab, contentParent).GetComponent<DiscoveredEventStageUIContent>();

                EventStageData eventStageData = eventStageManager.eventStageDatas.Find(x => x.index == index);

                if (eventStageData == null)
                {
                    Debug.LogError("Event stage data of index" + index + "not found.");
                    continue;
                }
            }
        }
    }

    public void LoadStage(int stageIndex)
    {
        loadEventChannel.Raise(stageIndex);
    }

    private void OnDeactivated()
    {
        inputOnOffChannel.Raise(true);
        ingameUIOnOffChannel.Raise(true);
    }
}