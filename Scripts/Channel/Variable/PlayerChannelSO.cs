
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerChannel", menuName = "ScriptableObjects/Channels/PlayerChannel", order = SOAssetMenuIndex.Channel)]
public class PlayerChannelSO : VariableChannelSO<Player>
{
    public void GivePlayerEternalLife()
    {
        if(TryGetVariable(out Player player))
        {
            player.EnableEternalLifeForDebug();
        } 
    }

    public void Revive()
    {
        if (TryGetVariable(out Player player))
        {
            player.Revive();
        }
        else
        {
            oneTimeActionOnRegister = (p) => p.Revive();
        }
    } 
}
