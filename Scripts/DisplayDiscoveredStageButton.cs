using UnityEngine;

public class DisplayDiscoveredStageButton : MonoBehaviour
{
    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private GameObject ledButton;

    [SerializeField]
    private GameObject exclamationMark;

    private void OnEnable()
    {
        SaveFile saveFile = saveLoadManager.SaveFile;

        if (!saveFile.ShowDiscoveredStageButton)
        {
            ledButton.SetActive(false);
            return;
        }
        
        ledButton.SetActive(true);

        if (!saveFile.DiscoverExclamationMark)
        {
            exclamationMark.SetActive(false);
        }
      
        exclamationMark.SetActive(true);
    }

    public void OnButtonPushed()
    {
        exclamationMark.SetActive(false);
        saveLoadManager.SaveFile.DiscoverExclamationMark = false;
        saveLoadManager.Save();
    }
}