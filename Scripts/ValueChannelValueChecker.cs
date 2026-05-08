using UnityEngine;

public class ValueChannelValueChecker : MonoBehaviour
{
    public FloatChannelSO valueChannel;
    private void Update()
    {
        Debug.Log("[ValueChecker] " + valueChannel.Value);
    }
}