using StarterAssets;
using UnityEngine;


public class OnAcidBehaviour : OnLiquidBehaviour
{
    [SerializeField]
    private float acidDamageRate = 0.1f;

    [SerializeField]
    private float acidDamagePeriod = 2f;

    [SerializeField]
    [Range(0f, 1f)]
    private float acidSpeedRate = 0.7f;

    private float time;
    private DamagablePlayer damagablePlayer;

    public override void OnLiquid(CustomThirdPersonController controller, CharacterVFXController vfxController)
    {
        if (isInLiquid)
        {
            time += Time.deltaTime;

            if (time > acidDamagePeriod)
            {
                damagablePlayer?.HitRate(acidDamageRate);
                time = 0f;
            }

            return;
        }

        isInLiquid = true;
        vfxController.Poisoning();
        controller.TryGetComponent(out damagablePlayer);
    }

    public override void OutLiquid(CustomThirdPersonController controller, CharacterVFXController vfxController)
    {
        if (!isInLiquid)
        {
            return;
        }

        isInLiquid = false;
        vfxController.FinishPoisoning();
    }

    public override bool TryGetSpeedRate(out float speed)
    {
        speed = acidSpeedRate;
        return isInLiquid;
    }
}