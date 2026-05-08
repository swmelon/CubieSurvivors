using UnityEngine;

public class ChasePlayer : MonoBehaviour
{

    [SerializeField]
    float moveSpeed = 5f;
    Transform target;
    private Rigidbody rb;
    private Vector3 moveDriection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, angle, 0);
        moveDriection = direction;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveDriection * moveSpeed;
        // ADD FORCE VS VELOCITY
    }
}
