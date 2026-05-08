using UnityEngine;

public class Coin : Poolable<Coin>
{
    [SerializeField]
    private SFXTags SFXTags;

    [SerializeField]
    private GemManagerSO gemManager;

    private AnimationScript animationScript;
    private int coinAmount = 1;
    private static string playerTag = "Player";

    protected override void OnEnable()
    {
        base.OnEnable();

        TryGetComponent(out animationScript);
        animationScript.isAnimated = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Released && other.CompareTag(playerTag))
        {
            gemManager.GetCoin(coinAmount);
            FMODAudioManager.instance.PlayOneShot(SFXTags);
            Release();
        }
    }
}