using System;
using UnityEngine.UIElements;
using UnityEngine;

public class ShopItemData
{
    protected static readonly string NameLabel = "label-name";
    protected static readonly string PriceLabel = "label-price";
    protected static readonly string IconImage = "image";
    protected static readonly string ItemButton = "button-item";
    protected static readonly string ParticleImage = "image-particle";


    public ShopItemData()
    {

    }

    public void SetItemVisualAndCallback(VisualElement item, Sprite icon, Accessory acc,int price, Action<Accessory> callback, RenderTexture renderTexture = null)
    {
        item.Q<Label>(PriceLabel).text = price.ToString();
        item.Q<VisualElement>(IconImage).style.backgroundImage = new StyleBackground(icon);
        item.Q<Button>(ItemButton).clicked += () => callback(acc);

        if (renderTexture == null) return;

        item.Q<VisualElement>(ParticleImage).style.backgroundImage = new StyleBackground(Background.FromRenderTexture(renderTexture));
    }
}