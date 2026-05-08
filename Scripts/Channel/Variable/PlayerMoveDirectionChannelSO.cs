using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMoveInfoChannel", menuName = "ScriptableObjects/Channels/PlayerMoveInfoChannel", order = SOAssetMenuIndex.Channel)]
public class PlayerMoveDirectionChannelSO : ScriptableObject
{
    [SerializeField] 
    private int fixedFrameDelay = 10;
    
    private Queue<KeyValuePair<Vector3, Vector3>> playerMoveInfoQueue = new Queue<KeyValuePair<Vector3, Vector3>>(); 
    
    /// <summary>
    /// Must be called in FixedUpdate.
    /// </summary>
    /// <param name="playerMoveInfo"></param>
    public void UpdateMoveInfo(Vector3 pos, Vector3 inputDirection)
    {
        playerMoveInfoQueue.Enqueue(new KeyValuePair<Vector3, Vector3>(pos, inputDirection));
        
        if (playerMoveInfoQueue.Count > fixedFrameDelay)
        {
            playerMoveInfoQueue.Dequeue();
        }
    }
    
    public void Clear()
    {
        playerMoveInfoQueue.Clear();
    }
     
    public KeyValuePair<Vector3, Vector3> GetDelayedMoveInfo()
    {
        return playerMoveInfoQueue.First();
    }

    public KeyValuePair<Vector3, Vector3> GetLatestMoveInfo()
    {
        return playerMoveInfoQueue.Last();
    }

    public void GetDelayedMoveInfo(out Vector3 pos, out Vector3 inputDirection)
    {
        var delayedMoveInfo = GetDelayedMoveInfo();
        pos = delayedMoveInfo.Key;
        inputDirection = delayedMoveInfo.Value;
    }

    public void GetLatestMoveInfo(out Vector3 pos, out Vector3 inputDirection)
    {
        var latestMoveInfo = GetLatestMoveInfo();
        pos = latestMoveInfo.Key;
        inputDirection = latestMoveInfo.Value;
    }
}
