using StarterAssets;
using UnityEngine;

public abstract class OnLiquidBehaviour : MonoBehaviour
{
    protected bool isInLiquid = false;
    public abstract void OnLiquid(CustomThirdPersonController controller, CharacterVFXController vfxController);
    public abstract void OutLiquid(CustomThirdPersonController controller, CharacterVFXController vfxController);
    public abstract bool TryGetSpeedRate(out float speed);
    public void ResetState()
    {
        isInLiquid = false;
    }

    public void OnGround()
    {

    }
}