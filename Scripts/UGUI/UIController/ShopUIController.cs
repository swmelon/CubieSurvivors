using UnityEngine;
using UnityEngine.UI;

public class ShopUIController : ObjectActiveController
{
    [SerializeField]
    private Button backButton;

    [SerializeField]
    private EventChannelSO exitShopEC;

    private void Start()
    { 
        backButton.onClick.AddListener(RaiseExitShopEC);
    }

    private void RaiseExitShopEC()
    {
        if (gameObject.activeSelf == false)
        {
            Debug.LogWarning("Shop UI is not active");
            return;
        }

        exitShopEC.Raise();
    }
}