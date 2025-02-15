using UnityEngine;

public class Constants
{
    // ******* AssetReference *******   

    public static string AssetReferenceParameter = "Parameter";
    public static string AssetReferencePlayer = "Player";
    public static string AssetReferenceEnemy = "Enemy";

    // ******* Player *******

    public static float PlayerMoveSpeed = 1.5f;

    // ******* Enemy *******

    // ******* Field *******

    public static Vector3 FieldCornerDownLeft = new Vector3(-8, -8, 0);
    public static Vector3 FieldCornerUpRight = new Vector3(8, 8, 0);
    public static Vector3 PlayerSpownPosition = new Vector3(-8, -8, 0);
}
