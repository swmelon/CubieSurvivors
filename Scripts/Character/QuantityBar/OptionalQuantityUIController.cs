using Minimalist.Bar;
using Minimalist.Quantity;
using System.Collections;
using UnityEngine;

public class OptionalQuantityUIController : MonoBehaviour
{
    [SerializeField]
    private QuantityBhvChannel quantityBhvChannel;

    protected QuantityBhv quantityBhv;

    private void OnEnable()
    {
        quantityBhvChannel.Subscribe(SetQuantityBhv);
    }

    private void OnDisable()
    {
        quantityBhvChannel.Unsubscribe(SetQuantityBhv);
    }

    private void SetQuantityBhv(QuantityBhv quantityBhv)
    {
        this.quantityBhv = quantityBhv;
    }
}
