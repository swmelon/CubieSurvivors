
using UnityEngine;
/// <summary>
/// Prefab 교체의 편의를 위해 만든 wrapper class
/// </summary>
public abstract class PrefabDataSO<T> : ScriptableObject where T : MonoBehaviour
{
    [SerializeField] 
    protected T prefab;
    public T Prefab => prefab;
}
