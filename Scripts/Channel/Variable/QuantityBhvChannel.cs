using Minimalist.Quantity;
using UnityEngine;
using System;


[CreateAssetMenu(menuName = "Channel/Variable/QuantityBhv")]
public class QuantityBhvChannel : VariableChannelSO<QuantityBhv>
{
    public Action HasListener;
    public Action NoListener;

    public override void Register(QuantityBhv variable)
    {
        base.Register(variable);

        if (listeners != null)
        {
            HasListener?.Invoke();
        }
        else
        {
            NoListener?.Invoke();
        }
    }

    public override void Subscribe(Action<QuantityBhv> listener)
    {
        base.Subscribe(listener);

        if (listeners != null)
        {
            HasListener?.Invoke();
        }
        else
        {
            NoListener?.Invoke();
        }
    }

    public override void Unsubscribe(Action<QuantityBhv> listener)
    {
        base.Unsubscribe(listener);

        if (listeners != null)
        {
            HasListener?.Invoke();
        }
        else
        {
            NoListener?.Invoke();
        }
    }
}
