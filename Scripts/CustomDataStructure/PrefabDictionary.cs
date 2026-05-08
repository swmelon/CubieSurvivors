using System.Collections.Generic;
using UnityEngine;


namespace Local.Scripts
{
    public class PrefabDictionary
    {
        private Dictionary<string, GameObject> prefabs;

        public PrefabDictionary(string gameObjectName, List<string> options)
        {
            prefabs = new Dictionary<string, GameObject>();
            
            foreach (var option in options)
            {
                prefabs.Add(option, LoadPrefabInResource(gameObjectName, option));
            }
        }
        
        private GameObject LoadPrefabInResource(string gameObjectName, string option)
        {
            return Resources.Load<GameObject>("Prefabs/" + gameObjectName + "/" + gameObjectName + "." + option);
        }

        public GameObject GetPrefab(string option)
        {
            return prefabs[option];
        }
    }
}