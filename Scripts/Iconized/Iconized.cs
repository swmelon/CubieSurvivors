using UnityEngine;

public class Iconized<T> : IIconized
{
    private T content;
    private Sprite icon;
    
    public Iconized(T content, Sprite icon = null)
    {
        this.content = content;
        this.icon = icon;
    }
    
    public T GetContent() => content;
    
    public Sprite GetIcon()
    {
        return icon;
    }
}
