using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public Team team;

    List<PlayerController> teamMembers;
    List<Weapon> teamWeapons;
    int currentPlayer;
    ParticleSystem playerDiedExplosion;

    private void OnEnable()
    {
        EventManager.gameEnded += DestroyTeam;
    }

    private void OnDisable()
    {
        EventManager.gameEnded -= DestroyTeam;
    }

    public void Initialize(Team t, int squadCount, Transform spawnPoint , List<Weapon> weapons , int health)
    {
        team = t;
        gameObject.name = team.Name;
        teamMembers = new List<PlayerController>();
        currentPlayer = 0;

        //Copy the inital weapons into the teams weapons
        teamWeapons = new List<Weapon>();
        foreach(Weapon weapon in weapons)
        {
            teamWeapons.Add(weapon);
        }

        //Instantiate players based on squadcount
        for (int i = 0; i < squadCount; i++)
        {
            PlayerController pc = Instantiate(team.TeamFaction.Character, spawnPoint.GetChild(i).position, Quaternion.identity, this.transform);
            pc.InitializePlayer(this ,health);
            teamMembers.Add(pc);
        }

        //Instantiate particle system that will play once a player dies
        playerDiedExplosion = Instantiate(team.TeamFaction.PlayerDeadExplosion, transform);
        playerDiedExplosion.name = $"{team.Name}Explosion";
    }

    //Destroys once the game ends
    void DestroyTeam()
    {
        Destroy(gameObject);
    }

    //Update the inventory and starts the player & team coroutine
    public void StartTurn()
    {
        UIManager.Instance.UpdateWeaponsButton(teamWeapons);
        currentPlayer = Random.Range(0, teamMembers.Count); //Get a random player
        teamMembers[currentPlayer].StartMoving();
        StartCoroutine(nameof(GameUpdate));
        EventManager.endTurn += StopTurn;
    }

    //Stops team coroutine
    void StopTurn()
    {
        EventManager.endTurn -= StopTurn;
        StopCoroutine(nameof(GameUpdate));
    }

    //Stop the team's turn and player's movement
    public void StopTeamTurn()
    {
        StopTurn();
        StopPlayer();
    }

    //Stop the player from moving after the timer ends
    void StopPlayer()
    {
        teamMembers[currentPlayer].StopMoving();
    }

    void ChangeCurrentPlayer(int index)
    {
        //Make sure not to change to the current player
        if (currentPlayer == index)
            return;

        //Stop moving the current player then start moving the next player
        teamMembers[currentPlayer].StopMoving();
        currentPlayer = index;
        teamMembers[currentPlayer].StartMoving();
    }

    //Enables switching between team members
    IEnumerator GameUpdate()
    {
        while(true)
        {
            yield return new WaitUntil(() => Input.anyKey);
            int index = -1;

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                index = 0;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                index = 1;
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                index = 2;
            }

            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                index = 3;
            }

            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            {
                index = 4;
            }

            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
            {
                index = 5;
            }

            if (index < teamMembers.Count && index != -1)
            {
                ChangeCurrentPlayer(index);
            }
        }
    }

    //Get the sum of all player's health
    public void UpdateHealth()
    {
        team.CurrentHealth = 0;
        foreach (PlayerController pc in teamMembers)
            team.CurrentHealth += pc.currentHealth;
    }

    public void AddWeapon(Weapon weapon)
    {
        //Add weapon to list if it doesn't exit already
        if(!teamWeapons.Contains(weapon))
        {
            teamWeapons.Add(weapon);
            UIManager.Instance.UpdateWeaponsButton(teamWeapons);
        }
    }

    public void KillPlayer(PlayerController pc)
    {
        //Remove killed player from members list
        teamMembers.Remove(pc);
        playerDiedExplosion.transform.SetPositionAndRotation(pc.transform.position, pc.transform.rotation);
        playerDiedExplosion.Play();

        //Destroy the team once all players are dead
        if (teamMembers.Count == 0)
        {
            Invoke(nameof(KillTeam), playerDiedExplosion.main.duration);
        }
    }

    void KillTeam()
    {
        GameManager.Instance.KillTeam(this);
        Destroy(gameObject);
    }
}