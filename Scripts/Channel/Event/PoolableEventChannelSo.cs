using System;
using UnityEngine;

public class PoolableEventChannelSo<T> : TypeEventChannelSO<T> where T : IPoolable {}
