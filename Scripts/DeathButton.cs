using UnityEngine;
using UnityEngine.UI;

public class DeathButton : MonoBehaviour
{
    [SerializeField]
    private PlayerChannelSO currentPlayerChannel;

    private Button button;
    private Player player;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        currentPlayerChannel.Subscribe(SetPlayer);
        button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        currentPlayerChannel.Unsubscribe(SetPlayer);
    }

    private void SetPlayer(Player player)
    {
        this.player = player;
    }

    public void OnClick()
    {
        player!.Damagable.ForceToDieForDebug();
    }
}