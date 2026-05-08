
public static class CardText
{
    //Character Upgradable Options
    public static string UPGRADE = "Upgrade";
    public static string SPEED = "Move Speed";
    public static string MAX_HEALTH = "Max Health";
    public static string HEAL_PERIOD = "Healing Speed";
    public static string MAGNET_RANGE = "Magnet Radius";

    public static string NOTHING = "";
    public static string OPTION = "Option";
    public static string SPECIAL = "Special";
    public static string NEW_WEAPON = "Get New Weapon";
    public static string OTHER_WEAPONS = "Get Other Weapons";

    public static string COIN = "Coin";

    public static string DAMAGE = "Damage";
    public static string TOTAL_DAMAGE = "Total Damage";
    public static string RANGE = "Range";
    public static string ATTACK_RANGE = "Attack Range";
    public static string RATE_OF_FIRE = "Rate of Fire";
    public static string EXPLOSION_DAMAGE = "Explosion Dmg";
    public static string EXPLOSION_RADIUS = "Explosion Radius";
    public static string NUM_OF_PROJECTILES = "Number of Projectiles";
    public static string NUM_OF_LIGHTNING = "Number of Lightning";
    public static string SHIELD_LEVEL_UP = "Shield Level Up";
    public static string RELOAD_SPEED = "Reload Speed";
    public static string FLAME_SCALE = "Flame Scale";
    public static string BEAM_LENGTH = "Beam Length";
    public static string CHANGE_MODE = "Change Mode";

    public static string ULTIMATE_UPGRADE = "Ultimate Upgrade";
    public static string ULTIMATE_UPGRADE_LANCER = "Ultimate Upgrade Lancer";
    public static string ULTIMATE_UPGRADE_NAVBOMBER = "Ultimate Upgrade NavBomber";
    public static string ULTIMATE_UPGRADE_NOVAWAVE = "Ultimate Upgrade NovaWave";
    public static string ULTIMATE_UPGRADE_LIGHTOFZEUS = "Ultimate Upgrade LightofZeus";
    public static string ULTIMATE_UPGRADE_QUADLAUNCHER = "Ultimate Upgrade QuadLauncher";
    public static string ULTIMATE_UPGRADE_DUALSABER = "Ultimate Upgrade DualSaber";


    public static string THREE_X = "3X";

    public static string KILL_ALL_ENEMIES = "Kill All Enemies";
    public static string EMERGENCY_HEALTH_PACK = "Emergency Health Pack";
    public static string ADD_CARD = "Add Card";

    public static string GetUpgradeNTime(int time)
    {
        return $" X{time}";
    }

    public static string GetNCoin(int coin)
    {
        return $"Get {coin} Coins";
    }
}
