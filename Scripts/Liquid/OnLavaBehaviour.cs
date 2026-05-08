using StarterAssets;
using UnityEngine;


public class OnLavaBehaviour : OnLiquidBehaviour
{
    [SerializeField]
    private float lavaDamageRate = 0.2f;

    public override void OnLiquid(CustomThirdPersonController controller, CharacterVFXController vfxController)
    {
        if (isInLiquid)
        {
            return;
        }

        isInLiquid = true;
        controller.Jump(highJump: true);
        controller.ClampRotation();
        vfxController.LavaSplash();

        if (controller.TryGetComponent(out DamagablePlayer damagablePlayer))
        {
            damagablePlayer.HitRate(lavaDamageRate);
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