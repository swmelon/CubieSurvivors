using UnityEngine;
using System.Collections.Generic;


namespace Local.Scripts.Extensions
{
    public static class LayerMaskCash
    {
        public static LayerMask Enemy = LayerMask.GetMask("Enemy", "DamagableStructure");
        public static LayerMask Ground = LayerMask.GetMask("Ground");
        public static LayerMask OnlyEnemy = LayerMask.GetMask("Enemy");
        public static LayerMask Player = LayerMask.GetMask("Player", "DamagableStructure");
        public static LayerMask OnlyPlayer = LayerMask.GetMask("Player");
        public static LayerMask PlayerAndEnemy = LayerMask.GetMask("Player", "Enemy", "DamagableStructure");
        public static LayerMask OnlyPlayerAndEnemy = LayerMask.GetMask("Player", "Enemy");
        public static LayerMask Obstacle = LayerMask.GetMask("Ground", "DamagableStructure", "Water");
        public static LayerMask GroundAndWater = LayerMask.GetMask("Ground", "Water");
        public static LayerMask Item = LayerMask.GetMask("Item");
        public static LayerMask AccessorySlot = LayerMask.GetMask("AccessorySlot");
        public static LayerMask Liquid = Water;
        public static LayerMask Lava = LayerMask.GetMask("Lava");
        public static LayerMask Water = LayerMask.GetMask("Water");
        public static LayerMask Acid = LayerMask.GetMask("Acid");
        public static LayerMask Prop = LayerMask.GetMask("Prop");
        public static LayerMask PropAndDamagableStructrue = LayerMask.GetMask("Prop", "DamagableStructure");
        public static int WaterLayer = LayerMask.NameToLayer("Water");
        public static int AcidLayer = LayerMask.NameToLayer("Acid");
        public static int LavaLayer = LayerMask.NameToLayer("Lava");
        public static int IceLayer = LayerMask.NameToLayer("Ice");
    }
}