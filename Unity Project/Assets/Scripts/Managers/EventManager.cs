using System.Collections.Generic;
public class EventManager
{
    public delegate void ChangeWeapon(Weapon w);
    public static ChangeWeapon changeWeapon;

    public delegate void GameEnded();
    public static GameEnded gameEnded;

    public delegate void StartTurn();
    public static StartTurn startTurn;

    public delegate void EndTurn();
    public static EndTurn endTurn;

    public delegate void SendDamagedPlayers(HashSet<PlayerController> damagedPlayers);
    public static SendDamagedPlayers damagedPlayers;
}