using Local.Scripts.Extensions;
using UnityEngine;


[RequireComponent(typeof(DamagableNoText))]
public class ExplosiveProp : MonoBehaviour, IWeapon
{

    [SerializeField]
    private LayerMask targetLayer;

    [SerializeField]
    private ExplosiveSpawner explosiveSpawner;

    [SerializeField]
    private int damage = 30;

    private Damagable damagable;

    private Renderer crystalRenderer;
    private MaterialPropertyBlock propBlock;


    private float pulseOffset;
    private float pulseSpeed = 2f;
    private float minEmission = 0.5f;
    private float maxEmission = 1.5f;
    private Color emissionColor = Color.red;
    private int emissionColorID;

    public bool UsedByPlayer() => false;

    private void Awake()
    {
        damagable = GetComponent<Damagable>();
        damagable.OnDead.AddListener(Damage);

        crystalRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
        pulseOffset = Random.Range(0f, Mathf.PI * 2);
        emissionColorID = Shader.PropertyToID("_EmissionColor");
    }

    public void Damage()
    {
        Explosive explosive = explosiveSpawner.Spawn();
        explosive.transform.position = transform.position;
        explosive.SetDamage(damage);
        explosive.SetTargetLayer(targetLayer);
        explosive.SetWeapon(this);
        explosive.Explode();
        Destroy(gameObject);
    }


    void Update()
    {
        float emissionIntensity = Mathf.Lerp(minEmission, maxEmission, (Mathf.Sin(Time.time * pulseSpeed) + 1.0f) / 2.0f);

        // Update the MaterialPropertyBlock with the new emission value
        Color finalEmissionColor = emissionColor * emissionIntensity;
        propBlock.SetColor(emissionColorID, finalEmissionColor);

        // Apply the MaterialPropertyBlock to the renderer
        crystalRenderer.SetPropertyBlock(propBlock);
    }
}