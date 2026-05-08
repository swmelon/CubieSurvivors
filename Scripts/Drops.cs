
using System.Drawing;
using UnityEngine;

public abstract class Drops : Poolable<Drops>
{
    public float Size = 2f;

    public abstract void Drop(Vector3 position);
}