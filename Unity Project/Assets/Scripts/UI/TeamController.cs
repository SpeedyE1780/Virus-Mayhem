using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamController : MonoBehaviour
{
    public Slider TeamHealth;
    public Text TeamName;
    public Image Fill;
    public Image BackgroundFill;
    

    Team team;
    public Team GetTeam => team;

    //Update the team in the overlay
    public void SetTeam(Team t)
    {
        team = t;
        TeamName.text = team.Name;
        TeamHealth.maxValue = team.MaxHealth;
        TeamHealth.value = team.CurrentHealth;
        Fill.color = team.TeamColor;
        Color color = team.TeamColor;
        color.a = 0.5f;
        BackgroundFill.color = color;
    }
}