using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class ButtonClickedEvent : MonoBehaviour
{
    private Button button;
    public Button.ButtonClickedEvent Event
    {
        get
        {
            return button.onClick;
        }
    }

    private void Awake()
    {
        button = transform.GetComponentInChildren<Button>();
    }
}
