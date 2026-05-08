using StarterAssets;
using UnityEngine;


public class OnBolingSugarBehaviour : OnLiquidBehaviour
{
    [SerializeField]
    private float burningDamageRate = 0.2f;

    public override void OnLiquid(CustomThirdPersonController controller, CharacterVFXController vfxController)
    {
        if (isInLiquid)
        {
            return;
        }

        isInLiquid = true;
        controller.Jump(highJump: true);
        controller.ClampRotation();
        vfxController.PlaySteam();

        if (controller.TryGetComponent(out DamagablePlayer damagablePlayer))
        {
            damagablePlayer.HitRate(burningDamageRate);
        }
    }

    public override void OutLiquid(CustomThirdPersonController controller, CharacterVFXController vfxController)
    {
        if (!isInLiquid)
        {
            return;
        }

        isInLiquid = false;
    }

    public override bool TryGetSpeedRate(out float speed)
    {
        speed = 0.0f;
        return false;
    }
}