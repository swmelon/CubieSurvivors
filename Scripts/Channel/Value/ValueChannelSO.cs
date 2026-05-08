using UnityEngine;

public class ValueChannelSO<T> : ScriptableObject
{
    public T Value => value;
    private T value;

    private void OnEnable()
    {
        value = default;
    }


    public void Register(T val)
    {
        this.value = val;
    }
}