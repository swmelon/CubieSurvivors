
using System.Collections.Generic;
using System.Linq;
using AYellowpaper;
using UnityEngine;

public class Initializer : MonoBehaviour
{
    [RequireInterface(typeof(IDependentInitialization))]
    public List<UnityEngine.Object> dependentInitializations;
    
    private void Awake()
    {
        foreach (var dependentInitialization in dependentInitializations)
        {
            var dependentInitializationComponent = dependentInitialization as IDependentInitialization;
            dependentInitializationComponent?.Initialize();
        }
    }
}
