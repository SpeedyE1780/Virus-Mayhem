using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamMenuController : MonoBehaviour
{
    public InputField TeamName;
    public Image FactionLogo;
    public Transform ButtonParents;
    public List<Faction> Factions;
    public Button FactionButton;

    public static List<Faction> SelectedFactions = new List<Faction>();
    public static List<string> SelectedNames = new List<string>();

    Faction currentFaction;
    string currentName;

    private void OnEnable()
    {
        //Add a button each faction available
        foreach(Faction faction in Factions)
        {
            GameObject g = Instantiate(FactionButton.gameObject, ButtonParents);
            g.GetComponentInChildren<Text>().text = faction.name;
            g.GetComponent<Button>().onClick.AddListener(() => SetFaction(faction));
        }

        currentName = string.Empty;

        //Get a faction that isn't selected
        foreach (Faction faction in Factions)
        {
            if (!SelectedFactions.Contains(faction))
            {
                //currentName = faction.FactionName;
                SetFaction(faction);
                return;
            }
        }
    }

    public static void EmptySelectedTeams()
    {
        SelectedFactions = new List<Faction>();
        SelectedNames = new List<string>();
    }

    public void UpdateName()
    {
        //Check if name is already taken
        if (!SelectedNames.Contains(TeamName.text))
        {
            //Remove the old name and add the new one
            SelectedNames.Remove(currentName);
            gameObject.name = TeamName.text;
            currentName = TeamName.text;

            if(currentName != string.Empty)
            {
                SelectedNames.Add(currentName);
            }
        }

        else
        {
            //Set team name to the current name
            gameObject.name = currentName;
            TeamName.text = currentName;
        }
    }

    void SetFaction(Faction faction)
    {
        //Check if faction is already selected
        if(!SelectedFactions.Contains(faction))
        {
            //Remove old faction and replace it with the new faction
            if (currentFaction != null)
            {
                SelectedFactions.Remove(currentFaction);
            }

            //Update current faction
            currentFaction = faction;
            FactionLogo.sprite = currentFaction.FactionLogo;

            SelectedFactions.Add(currentFaction);
        }

        SoundManager.Instance.ButtonClick.Play();
    }

    public void RemoveTeam()
    {
        //Change the parent to null before checking the child count
        transform.SetParent(null);
        
        if (currentFaction != null)
            SelectedFactions.Remove(currentFaction);

        SelectedNames.Remove(currentName);

        UIManager.Instance.CheckTeamCount();
        SoundManager.Instance.ButtonClick.Play();
        Destroy(gameObject);
    }

    public Team GetTeam()
    {
        //Checks if a team chose a different faction name and set it to the current faction name
        foreach(Faction faction in SelectedFactions)
        {
            if(faction != currentFaction && currentName == faction.FactionName)
            {
                currentName = currentFaction.FactionName;
                break;
            }
        }

        Team team = new Team()
        {
            Name = currentName != string.Empty ? currentName : currentFaction.FactionName,
            TeamFaction = currentFaction
        };

        return team;
    }
}