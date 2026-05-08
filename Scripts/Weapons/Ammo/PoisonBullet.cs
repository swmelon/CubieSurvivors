using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine;


public class PoisonBullet : Bullet<PoisonBullet>, IColorable
{
    [SerializeField] 
    private ParticleSystem mainPS, sparkPS, glowPS;

    [SerializeField] 
    private float heightLimit = 0.25f;
    
    private ParticleSystem.ColorOverLifetimeModule mainColorOverLifetimeModule, sparkColorOverLifetimeModule, 
        glowColorOverLifetimeModule;
    private Color color;
    
    private List<Quaternion> rotations = new List<Quaternion>();
    
    public void SetMaxBounces(float value) => maxBounces = value;

    private float maxBounces, currentBounces;

    protected override void Awake()
    {
        base.Awake();
        mainColorOverLifetimeModule = mainPS.colorOverLifetime;
        sparkColorOverLifetimeModule = sparkPS.colorOverLifetime;
        glowColorOverLifetimeModule = glowPS.colorOverLifetime;
        rotations.Add(Quaternion.Euler(0, 45, 0));
        rotations.Add(Quaternion.Euler(0, -45, 0));
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        currentBounces = 0;
    }

    private void Update()
    {
        ((Component)this).transform.position += speed * Time.deltaTime * direction;

        if (((Component)this).transform.position.y > heightLimit)
        {
            ((Component)this).transform.position -= Vector3.up * Time.deltaTime;
        }
    }
    
    protected override void FixedUpdate()
    {
        RaycastHit hit;
        
        Debug.DrawRay(((Component)this).transform.position, direction, Color.red, 0.5f);
        
        if (Physics.Raycast(((Component)this).transform.position, direction, out hit, maxDistance:0.5f, 
                targetLayer | LayerMaskCash.Obstacle,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore))
        {
            if(hit.collider.TryGetComponent(out Damagable damagable))
            {
                OnHitDamagable(hit, damagable);
                direction = rotations.PickRandom() * direction;
            }
            else
            {
                Vector3 reflection = Vector3.Reflect(direction, hit.normal);
                reflection.y = 0;
                direction = reflection.normalized;
            }
                
            currentBounces++;

            if (currentBounces >= maxBounces)
            {
                Release();
            }
        }
    }
    public void ReflectNow(Vector3 targetPosition, float bulletSpeed, int bulletDamage)
    {
        damage = bulletDamage;
        speed = bulletSpeed;
        currentBounces = maxBounces;

        targetPosition.y = heightLimit;
        
        //Vector3 incident = direction.normalized;
        Vector3 targetDirection = (targetPosition - ((Component)this).transform.position).normalized;

        // Directly set the bullet's direction towards the target
        direction = targetDirection;

        // Optionally, you can re-normalize the direction vector
        direction.Normalize();
    }

    public void SetColor(Color color)
    {
        if (this.color != color)
        {
            this.color = color;
            Gradient mainGrad = new Gradient();
            mainGrad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.white, 0.0f), 
                    new GradientColorKey(this.color, 1.0f)
                }, 
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(0.0f, 1.0f)
                });
            Gradient glowGrad = new Gradient();
            glowGrad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(this.color, 0.0f),
                    new GradientColorKey(Color.white, 1.0f) 
                }, 
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1.0f, 0.0f), 
                    new GradientAlphaKey(0.0f, 1.0f)
                });
            Gradient sparkGrad = new Gradient();
            sparkGrad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.white, 0.0f),
                    new GradientColorKey(this.color, 1.0f) 
                }, 
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0.0f, 0.0f), 
                    new GradientAlphaKey(1f, 0.224f),
                    new GradientAlphaKey(1f, 0.835f),
                    new GradientAlphaKey(0.0f, 1.0f)
                });
            
            mainColorOverLifetimeModule.color = mainGrad;
            glowColorOverLifetimeModule.color = glowGrad;
            sparkColorOverLifetimeModule.color = sparkGrad;
        }
    }
}
