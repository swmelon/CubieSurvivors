using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CircleRay : MonoBehaviour
{
    public void SetTargetLayer (LayerMask value) => targetLayer = value;
    public void SetDamage (int value) => damage = value;
    public void SetWeapon (Weapon value) => weapon = value;

    public void SetExpand(bool value) => expand = value;
    public void SetExpandHeight(float value) => expandHeight = value;

    [SerializeField]
    private bool semiCircle = false;
    
    [SerializeField]
    private float radius = 5f;
    
    [SerializeField]
    private int numRays = 8;
    
    [SerializeField]
    private LayerMask targetLayer;

    [SerializeField][Range(10, 200)] 
    private int damage = 10;

    [SerializeField][Range(0.5f, 1f)]
    private float damageInterval = 0.5f;

    [SerializeField][Range(0f, 1f)]
    private float hitForceMultiplier = 0.5f;

    [SerializeField]
    private float maxLifeTime = 1f;

    [SerializeField]
    private bool expand = false;

    [SerializeField]
    private float expandHeight = 0.3f;

    [SerializeField]
    private bool useHitEffect = false;

    [SerializeField]
    private OnePureEffectSpawner hitEffectSpawner;

    private Dictionary<Damagable, float> hitEnemies = new ();
    private Dictionary<Damagable, float> spareHitEnemies = new ();
    private List<Damagable> keysToRemove = new ();

    private float angleIncrement;
    private float rayLength;

    private Vector3[] vertices;
    Vector3[] updatedVertices;
    private RaycastHit[] hits;
    private Vector3[] rayDirections;
    private Vector3[] updatedRayDirections;
    private Weapon weapon;
    private bool needUpdateVertices = true;
    private float time;


    public int NumRays
    {
        set
        {
            if (value < 3)
            {
                Debug.LogError("Number of rays must be greater than or equal to 3");
                return;
            }

            numRays = value;
            vertices = new Vector3[numRays + 1];
            updatedVertices = new Vector3[numRays + 1];
            rayDirections = new Vector3[numRays];
            updatedRayDirections = new Vector3[numRays];
            angleIncrement = 360f / numRays;

            if (semiCircle)
            {
                angleIncrement *= 0.5f;
            }


            rayLength = 2 * Mathf.Cos(Mathf.Deg2Rad * (90f - angleIncrement * 0.5f));
            Vector3 vertexPos = new Vector3(-1, 0, 0);
            vertices[0] = vertexPos;

            
            // Cashing the start position and direction of each ray
            for (int i = 0; i < numRays; i++)
            {
                float angle = (i + 0.5f) * angleIncrement;
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                vertexPos += direction * rayLength;
                vertices[i + 1] = vertexPos;
                rayDirections[i] = direction;
                updatedRayDirections[i] = direction;
            }
        }
    }

    public float Radius
    {
        set
        {
            radius = value;
            needUpdateVertices = true;
            time = maxLifeTime;
        }
    }

    private void Awake()
    {
        vertices = new Vector3[numRays + 1];
        updatedVertices = new Vector3[numRays + 1];
        rayDirections = new Vector3[numRays];
        Radius = radius;
        NumRays = numRays;
        hits = new RaycastHit[16];
    }

    private void FixedUpdate()
    {
        time -= Time.fixedDeltaTime;

        if (time <= 0)
        {
            return;
        }

        UpdateVertices(ref updatedVertices);

        Vector3 vertexPos;
        Vector3 updatedVertex; 
        Vector3 updatedRayDirection;

        for (int i = 0; i < numRays; i++)
        {
            // no need to rotate the vertices
            updatedVertex = updatedVertices[i];
            updatedRayDirection = updatedRayDirections[i];
            
            vertexPos = updatedVertex + transform.position;
            CheckHit(vertexPos, updatedRayDirection);
            
            if (expand)
            {
                Vector3 upperVertexPos = vertexPos + Vector3.up * expandHeight;
                Vector3 lowerVertexPos = vertexPos - Vector3.up * expandHeight;
                CheckHit(upperVertexPos, updatedRayDirection);
                CheckHit(lowerVertexPos, updatedRayDirection);
            }
        }

        CountTime();
    }

    private void CheckHit(Vector3 vertexPos, Vector3 rayDirection)
    {
        Debug.DrawRay(vertexPos, radius * rayLength * rayDirection, Color.red);

        // Cast a ray in the current direction and get all hits
        int numHits = Physics.RaycastNonAlloc(vertexPos, rayDirection, hits, radius * rayLength, targetLayer,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < numHits; i++)
        {
            if (hits[i].transform.TryGetComponent(out Damagable damagable) && !hitEnemies.ContainsKey(damagable))
            {
                if (ReferenceEquals(weapon, null))
                {
                    damagable.Hit(damage);
                }
                else
                {
                    damagable.Hit(weapon.ComputeFinalDamage(damage, out bool isCritical),
                        ComputeHitForce(hits[i].point), isCritical: isCritical);

                    if (useHitEffect)
                    {
                        hitEffectSpawner.Spawn().transform.position = hits[i].point;
                    }
                }

                hitEnemies.Add(damagable, 0f);
                spareHitEnemies.Add(damagable, 0f);
            }
        }
    }

    protected virtual void UpdateVertices(ref Vector3[] updatedVertices)
    {
        Debug.Assert(vertices.Length == numRays + 1, "The number of vertices must be equal to the number of rays + 1");
        
        if (!needUpdateVertices)
        {
            return;
        }

        for(int i = 0; i < numRays + 1; i++)
        {
            updatedVertices[i] = vertices[i] * radius;
        }

        needUpdateVertices = false;
    }


    /// <summary>
    /// use when SetYRotation is called
    /// </summary>
    /// <param name="updatedVertices"></param>
    /// <param name="updatedRayDirections"></param>
    /// <param name="rotation"></param>
    protected virtual void UpdateVerticesAndRayDirections(ref Vector3[] updatedVertices, ref Vector3[] updatedRayDirections, Quaternion rotation)
    {
        Debug.Assert(vertices.Length == numRays + 1, "The number of vertices must be equal to the number of rays + 1");

        for (int i = 0; i < numRays + 1; i++)
        {
            updatedVertices[i] = rotation * vertices[i] * radius;
        }

        for (int i = 0; i < numRays; i++)
        {
            updatedRayDirections[i] = rotation * rayDirections[i];

        }
    }

    private void CountTime()
    {
        keysToRemove.Clear();

        foreach (var item in hitEnemies)
        {
            spareHitEnemies[item.Key] = hitEnemies[item.Key] + Time.fixedDeltaTime;

            if (spareHitEnemies[item.Key] > damageInterval || item.Key == null)
            {
                keysToRemove.Add(item.Key);
            }
        }

        for (int i = 0; i < keysToRemove.Count; i++)
        {
            hitEnemies.Remove(keysToRemove[i]);
            spareHitEnemies.Remove(keysToRemove[i]);
        }

        foreach (var key in spareHitEnemies.Keys)
        {
            hitEnemies[key] = spareHitEnemies[key];
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the circular enemy detector in the Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    
    private Vector3 ComputeHitForce(Vector3 hitPosition)
    {
        Vector3 hitDirection = (hitPosition - transform.position).normalized;
        return hitDirection * hitForceMultiplier;
    }

    public void SetYRotationAndRadius(Quaternion rotation, float radius)
    {
        Radius = radius;
        UpdateVertices(ref updatedVertices);
        UpdateVerticesAndRayDirections(ref updatedVertices, ref updatedRayDirections, rotation);
    }
}
