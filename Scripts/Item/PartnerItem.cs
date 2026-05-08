using System;
using UnityEngine;


public class PartnerItem : CoveredItem<Partner>
{
    public override void Activate(Player player)
    {
        player.BeMaster(content);
        Component[] components = content.gameObject.GetComponentsInChildren<Component>();
        content.transform.parent = null;
        content.transform.position = transform.position;
        content.transform.localScale = Vector3.one;

        // Loop through the components and enable them
        foreach (Component component in components)
        {
            if (component is Renderer)
            {
                // Enable the Renderer component
                ((Renderer)component).enabled = true;
            }
            else if (component is Behaviour)
            {
                // Enable the OnAttack component
                ((Behaviour)component).enabled = true;
            }
            else if (component is Collider)
            {
                // Enable the Collider component
                ((Collider)component).enabled = true;
            }
        }
        
        base.Activate(player);
    }
    
    public override void SetContent(Partner content, bool parachute)
    {
        base.SetContent(content, parachute);
        content.transform.localScale = 2 * Vector3.one;
    }
}
