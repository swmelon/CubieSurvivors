using StarterAssets;
using UnityEngine;


public class OnIceBehaviour : OnLiquidBehaviour
{
    [SerializeField]
    private float speedChangeRate = 0.5f;

    [SerializeField]
    private float rotationSmoothTime = 0.25f;

    public override void OnLiquid(CustomThirdPersonController controller, CharacterVFXController vfxController)
    {
        if (isInLiquid)
        {
            return;
        }

        isInLiquid = true;
        controller.SetSpeedChangeRate(speedChangeRate);
        controller.SetRotationSmoothTime(rotationSmoothTime);
    }

    public override void OutLiquid(CustomThirdPersonController controller, CharacterVFXController vfxController)
    {
        if (!isInLiquid)
        {
            return;
        }

        isInLiquid = false;
        controller.ResetSpeedChangeRate();
        controller.ResetRotationSmoothTime();
    }

    public override bool TryGetSpeedRate(out float speed)
    {
        speed = 1f;
        return false;
    }
}