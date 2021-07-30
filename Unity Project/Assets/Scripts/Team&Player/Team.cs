using UnityEngine;

public class Team
{
    public string Name;
    public Faction TeamFaction;
    public float MaxHealth;
    public float CurrentHealth;
    public Color TeamColor;
    
    public void SetHealth(int value)
    {
        MaxHealth = value;
        CurrentHealth = value;
    }

    public void SetTeamColor(Color color)
    {
        TeamColor = color;
    }
}