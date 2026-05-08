using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiscoveredEventStageUIContent : MonoBehaviour
{
    [SerializeField]
    private Image icon;

    [SerializeField]
    private TextMeshProUGUI title;

    [SerializeField]
    private Button startButton;

    private DiscoveredEventStageUIManager manager;
    private int index;

    private void Awake()
    {
        manager = GetComponentInParent<DiscoveredEventStageUIManager>();

        startButton.onClick.AddListener(OnButtonClicked);
    }

    public void SetContent(int index, Sprite icon)
    {
        this.icon.sprite = icon;
        this.index = index;
    }

    private void OnButtonClicked()
    {
        manager.LoadStage(index);
    }
}