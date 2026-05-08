using UnityEngine;

public class PartnerInput : MonoBehaviour
{
    public bool jump;
    public bool sprint;
    public bool analogMovement;
    public Vector2 move;
    
    private Transform master;

    [SerializeField]
    private float jumpThreshold;

    [SerializeField] 
    private float sprintThreshold;

    [SerializeField]
    private float distanceThresholdToMaster;
   
    private void Awake()
    {
        jump = false;
        sprint = false;
        analogMovement = false;
        move = Vector2.zero;
    }

    public void SetMaster(Transform master)
    {
        this.master = master;
    }

    private void FixedUpdate()
    {
        if (ReferenceEquals(master, null))
        {
            return;
        }

        jump = false;
        


        Vector3 direction =(master.position - transform.position) ;
        float distance = direction.magnitude;
        direction = direction.normalized;
        

        if (direction.y > 0.5f)
        {
            jump = true;
        }
        
        direction = direction.normalized;
        move.x = direction.x;
        move.y = direction.z;

        if (distance < distanceThresholdToMaster)
        {
            move = Vector2.zero;
        }
        else if (distance < sprintThreshold)
        {
            sprint = false;
        }
        else
        {
            sprint = true;
        }

        if (jumpThreshold < master.position.y - transform.position.y) 
        {
            jump = true;
        }
    }
    
}
