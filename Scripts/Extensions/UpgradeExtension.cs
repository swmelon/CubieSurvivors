using UnityEngine;

public static class UpgradeExtension
{
    public static T ReadUpgradableStat<T>(Object unityObject) where T : struct
    {
        string name = unityObject.GetType().Name; 
        TextAsset json = Resources.Load<TextAsset>("UpgradableStats/" + name);
        return JsonUtility.FromJson<T>(json.ToString());
    }
    
    public static T ReadUpgradableStat<T>(string name) where T : struct
    {
        TextAsset json = Resources.Load<TextAsset>("UpgradableStats/" + name);
        return JsonUtility.FromJson<T>(json.ToString());
    }
}
