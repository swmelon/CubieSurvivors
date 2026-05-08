using UnityEngine;
using UnityEngine.Events;

public class ObjectClicker : MonoBehaviour
{
    public UnityEvent OnClick;
    
    [SerializeField]
    private string targetTag;
    
    void Update()
    {
        // Check if the left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // The ray hit a collider - do something with the hit object
                GameObject hitObject = hit.collider.gameObject;
                
                if (hitObject.CompareTag(targetTag))
                {
                    Debug.Log("Clicked on " + hitObject.name);
                    LEDNode node = hitObject.GetComponent<LEDNode>();
                    
                    if (node.IsOn)
                    {
                        node.TurnOff();
                    }
                    else
                    {
                        node.TurnOn();
                    }
                    
                    OnClick.Invoke();
                }

                // You can then do something with hitObject, like calling a method on it
                // For example: hitObject.GetComponent<YourComponent>().YourMethod();
            }
        }
    }
}