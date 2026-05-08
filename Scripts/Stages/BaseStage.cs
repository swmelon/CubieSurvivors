using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class BaseStage<T> : Poolable<T> where T : BaseStage<T>
{
    public StageType StageType
    {
        get => stageType;
        set => stageType = value;
    }
   
    public StageType stageType;
}
