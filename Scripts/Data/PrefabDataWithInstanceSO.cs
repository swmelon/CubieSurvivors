
using UnityEngine;

public class PrefabDataWithInstanceSO<T> : PrefabDataSO<T> where T : MonoBehaviour
{
    public T Instance
    {
        set => instance = value; 
    }

    protected T instance;
}
