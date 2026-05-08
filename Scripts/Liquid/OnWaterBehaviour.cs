using StarterAssets;
using UnityEngine;


public class OnWaterBehaviour : OnLiquidBehaviour
{
    [SerializeField]
    [Range(0f, 1f)]
    private float waterSpeedRate = 0.66f;

    public override void OnLiquid(CustomThirdPersonController controller, CharacterVFXController vfxController)
    {
        if (isInLiquid)
        {
            return;
        }

        isInLiquid = true;
        vfxController.WaterInSplash();
    }

    public override void OutLiquid(CustomThirdPersonController controller, CharacterVFXController vfxController)
    {
        if (!isInLiquid)
        {
            return;
        }

        isInLiquid = false;
        vfxController.WaterOutSplash();
    }

    public override bool TryGetSpeedRate(out float speed)
    {
        speed = waterSpeedRate;
        return isInLiquid;
    }
}