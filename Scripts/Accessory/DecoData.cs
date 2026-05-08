using UnityEngine;

public class DecoData
{
    public Material RollerMaterial => rollerMaterial;
    public AccData Hat => hat;
    public AccData Glasses => glasses;
    public AccData Facial => facial;
    public AccData Particles => particles;

    private Material rollerMaterial;
    private AccData hat;
    private AccData glasses;
    private AccData facial;
    private AccData particles;

    public DecoData(AccData hat = null, AccData glasses = null, 
        AccData facial = null, AccData particles = null, Material rollerMaterial = null)
    {
        this.hat = hat;
        this.glasses = glasses;
        this.facial = facial;
        this.particles = particles;
        this.rollerMaterial = rollerMaterial;
    }

    public void AddAccessory(AccData data)
    {
        switch (data.accessory.AccessoryType)
        {
            case AccessoryType.Hat:
                hat = data;
                break;
            case AccessoryType.Glasses:
                glasses = data;
                break;
            case AccessoryType.Facial:
                facial = data;
                break;
            case AccessoryType.Particles:
                particles = data;
                break;
        }
    }
}
