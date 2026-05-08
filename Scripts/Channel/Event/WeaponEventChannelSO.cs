using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponEventChannel", menuName = "ScriptableObjects/Channels/WeaponEventChannel", order = SOAssetMenuIndex.Channel)]
public class WeaponEventChannelSO : TypeEventChannelSO<Weapon> {}
