using UnityEngine;

public class PureEffect : Poolable<PureEffect>
{
    [SerializeField]
    private bool playSFX = true;

    [SerializeField]
    private SFXTags SFXTags;

    private bool needPlay = true;

    protected override void OnEnable()
    {
        base.OnEnable();
        needPlay = playSFX;
    }

    protected virtual void Update()
    {
        if (needPlay)
        {
            FMODAudioManager.instance.PlayOneShot(SFXTags, transform.position);
            needPlay = false;
        }
    }
}
