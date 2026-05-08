using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountWeapon : MonoBehaviour
{
   private void OnTriggerEnter(Collider other)
   {
      if (other.transform.root.TryGetComponent(out WeaponManager weaponManager) && other.TryGetComponent(out Player player))
      {
         weaponManager.Mount(GetComponent<Weapon>());
      }
   }
}
