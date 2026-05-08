using UnityEngine;
using System.Collections.Generic;
using Local.Scripts.Extensions;

public abstract class UserWeapon : Weapon
{
    [SerializeField]
    protected SymbolIconContainer symbolContainer;

    [SerializeField]
    private FXCameraChannelSO fxCameraChannel;

    private FXCameraController particleIconCamera;
    private UFloat UFinalDamageRate;

    private float addFinalDamageRate = 0.5f;
    protected override void Awake()
    {
        base.Awake();
    
        if (!ReferenceEquals(fxCameraChannel, null))
        {
            fxCameraChannel.Subscribe(SetParticleIconCam);
        }

        UFinalDamageRate = new UFloat(new List<float>() { 1, 0.1f, 10 },
            exposed: false, symbol: symbolContainer.TotalDamage, name: Name, icon: weaponIcon, optionText: CardText.TOTAL_DAMAGE);
    }
    protected override void NoUpgradables(List<IUpgradable> upgradables)
    {
        if (!UFinalDamageRate.IsUpgradable())
        {
            return;
        }

        if (!RandomExtenstion.IsHappen(addFinalDamageRate))
        {
            return;
        }

        // 모든 업그레이드 완료시 확률적으로 파이널 데미지 업그레이드 옵션 추가
        upgradables.Add(UFinalDamageRate);
    }

    public override int ComputeFinalDamage(int damage, out bool isCritical)
    {
        return (int)(UFinalDamageRate.Value * base.ComputeFinalDamage(damage, out isCritical));
    }

    public bool TryGetParticleIconCam(out FXCameraController particleIconCamera)
    {
        if (ReferenceEquals(this.particleIconCamera, null))
        {
            particleIconCamera = null;
            return false;
        }

        particleIconCamera = this.particleIconCamera;
        return true;
    }

    private void SetParticleIconCam(FXCameraController particleIconCamera)
    {
        this.particleIconCamera = particleIconCamera;
    }

    private void OnDestroy()
    {
        if (!ReferenceEquals(fxCameraChannel, null))
        {
            fxCameraChannel.Unsubscribe(SetParticleIconCam);
        }
    }

   
}