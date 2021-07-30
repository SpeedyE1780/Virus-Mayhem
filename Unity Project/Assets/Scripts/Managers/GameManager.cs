using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region PUBLIC VARIABLES

    public Transform TeamsParent;
    public TeamManager TeamPrefab;
    public Transform SpawnPoints;
    public Transform SupplyPoints;
    public List<Color> TeamColors;
    public List<Weapon> InitialWeapons;
    public List<GameObject> SupplyDrops;
    public LayerMask Players;

    #endregion

    #region PRIVATE VARIABLES

    readonly int minTurnDuration = 30;
    readonly int minGameDuration = 15;
    readonly int minSquadSize = 2;
    readonly int minPlayerHealth = 100;
    int turnDuration = 60;
    int gameDuration = 30 * 60; // *60 to convert minutes to seconds
    int squadSize = 4;
    int playerHealth = 100;
    List<TeamManager> teamManagers;
    int currentTeam;
    List<Transform> spawnPoints;
    List<Transform> supplyPoints;

    private static GameManager _instance;
    public static GameManager Instance { get => _instance; }

    #endregion

    #region AWAKE ENABLE DISABLE

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this.gameObject);

        //Initialize the current settings value
        UIManager.Instance.InitializeGameSettings(turnDuration, turnDuration / minTurnDuration,
            gameDuration / 60, gameDuration / minGameDuration / 60,
            squadSize, squadSize / minSquadSize ,
            playerHealth, playerHealth / minPlayerHealth);
    }

    private void OnEnable()
    {
        EventManager.startTurn += StartTurn;
        EventManager.endTurn += EndTurn;
        EventManager.damagedPlayers += StartWaitDamage;
    }

    private void OnDisable()
    {
        EventManager.startTurn -= StartTurn;
        EventManager.endTurn -= EndTurn;
        EventManager.damagedPlayers -= StartWaitDamage;
    }

    #endregion

    #region SET GAME SETTINGS

    public void SetTurnDuration(float value)
    {
        turnDuration = (int)(value * minTurnDuration);
        UIManager.Instance.SetTurnDurationText(turnDuration);
    }

    public void SetGameDuration(float value)
    {
        gameDuration = value != 3 ? (int)(value * minGameDuration * 60) : int.MaxValue;
        UIManager.Instance.SetGameDurationText(gameDuration != int.MaxValue ? gameDuration / 60 : -1);
    }

    public void SetSquadTeam(float value)
    {
        squadSize = (int)(value * minSquadSize);
        UIManager.Instance.SetSquadSizeText(squadSize);
    }

    public void SetPlayerHealth(float value)
    {
        playerHealth = (int)(value * minPlayerHealth);
        UIManager.Instance.SetPlayerHealthText(playerHealth);
    }

    #endregion

    #region GAME FUNCTIONS

    public void StartGame()
    {
        //Reset the teams
        teamManagers = new List<TeamManager>();
        UIManager.Instance.ResetTeamOverlay();
        currentTeam = -1;

        //Get the position of each transform
        spawnPoints = GetSpawnPoints(SpawnPoints);
        supplyPoints = GetSpawnPoints(SupplyPoints);

        //Create Teams
        foreach (Transform child in UIManager.Instance.TeamSelectionParents)
        {
            TeamMenuController tmc = child.GetComponent<TeamMenuController>();

            if (tmc != null)
            {
                //Get a random spawnpoint and remove it from the list
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                spawnPoints.Remove(spawnPoint);

                //Get the current team
                Team t = tmc.GetTeam();
                t.SetHealth(playerHealth * squadSize);
                t.SetTeamColor(TeamColors[teamManagers.Count]);

                //Create a team manager
                TeamManager tm = Instantiate(TeamPrefab, TeamsParent);
                tm.Initialize(t, squadSize, spawnPoint, InitialWeapons, playerHealth);
                teamManagers.Add(tm);
                
                //Add a team overlay
                UIManager.Instance.AddTeamOverlay(t);
            }
        }

        //Turn off the map camera
        CameraManager.Instance.ToggleMapCamera(true);

        //Start the game clock if game time isn't unlimited
        if (gameDuration != int.MaxValue)
        {
            StartCoroutine(nameof(StartGameClock));
            UIManager.Instance.GameClock.gameObject.SetActive(true);
        }
        else
        {
            UIManager.Instance.GameClock.gameObject.SetActive(false);
        }

        //Set the turn clock
        UIManager.Instance.TurnClock.text = turnDuration.ToString();

        //Set the current team to the first team
        ChangeCurrentTeam();
    }

    //Get all the child in the parent
    List<Transform> GetSpawnPoints(Transform parent)
    {
        List<Transform> Points = new List<Transform>();

        foreach (Transform child in parent)
        {
            Points.Add(child);
        }

        return Points;
    }

    //Returns the teams sorted
    List<Team> GetTeams()
    {
        List<Team> teams = new List<Team>();
        foreach (TeamManager tm in teamManagers)
        {
            tm.UpdateHealth();
            teams.Add(tm.team);
        }

        teams.Sort((x, y) => x.CurrentHealth < y.CurrentHealth ? 1 : -1);
        return teams;
    }

    public void KillTeam(TeamManager tm)
    {
        teamManagers.Remove(tm);
        UIManager.Instance.UpdateTeamOverlay(GetTeams());

        //End game when one team remains
        if (teamManagers.Count == 1)
            Invoke(nameof(EndGame), 1);
    }

    void EndGame()
    {
        //End the current turn and stop all coroutines
        EndTurn();
        StopAllCoroutines();

        UIManager.Instance.InitializeResultUI(GetTeams()[0]); //Send the winning team to the result UI
        CameraManager.Instance.Freeze(); //Stop updating the camera's position
        CameraManager.Instance.ToggleMapCamera(true); //Turn off the map camera
        EventManager.gameEnded.Invoke(); //Destroys all available player's/teams/drops/bullets
    }

    #endregion

    #region TURN FUNCTIONS

    //Called when supply drop hits the ground
    void StartTurn()
    {
        UIManager.Instance.ToggleTurnStarted(true);
        teamManagers[currentTeam].StartTurn();
        StartCoroutine(nameof(StartTurnTimer));
    }

    //Called after turn timer ends or player takes a shot
    public void EndTurn()
    {
        UIManager.Instance.ToggleTurnStarted(false);
        StopCoroutine(nameof(StartTurnTimer));
    }

    //Called after a projectile has exploded
    public void StartWaitDamage(HashSet<PlayerController> damagedPlayers)
    {
        StartCoroutine(nameof(WaitForDamage), damagedPlayers);
    }

    //Waits until all enemies afected by explosion updates their health
    IEnumerator WaitForDamage(HashSet<PlayerController> damagedPlayers)
    {
        if (damagedPlayers.Count != 0)
        {
            bool changeTeam = false;

            //Wait a maximum of 10 seconds
            for (int i = 0; i < 50 && !changeTeam; i++)
            {
                changeTeam = true;
                foreach (PlayerController player in damagedPlayers)
                {
                    changeTeam = changeTeam && (player.healthUpdated || player.currentHealth <= 0);
                }

                yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(2f);

            //Kill all players that died
            foreach(PlayerController player in damagedPlayers)
            {
                if (player.currentHealth <= 0)
                {
                    CameraManager.Instance.LookAtTarget(player.transform);
                    player.KillPlayer();
                    yield return new WaitForSeconds(1f);
                }
            }
        }

        //Makes sure that the game hasn't ended
        if (teamManagers.Count > 1)
        {
            ChangeCurrentTeam();
        }
    }

    //Called after turn timer ends or after all damaged players finishes updating their health
    void ChangeCurrentTeam()
    {
        currentTeam = (currentTeam + 1) % teamManagers.Count;
        UIManager.Instance.UpdateTeamOverlay(GetTeams());

        //Get a random supply drop and spawn position
        int supplyIndex = Random.Range(0, SupplyDrops.Count);
        int pointIndex = Random.Range(0, supplyPoints.Count);
        Instantiate(SupplyDrops[supplyIndex], supplyPoints[pointIndex].position, Quaternion.identity);
    }

    #endregion

    #region UPDATE GAME TIMER/CLOCK

    //Called when the turn starts
    IEnumerator StartTurnTimer()
    {
        int currentTime = turnDuration;
        UIManager.Instance.TurnClock.text = currentTime.ToString();

        //Update the timer every second
        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1);
            currentTime -= 1;
            UIManager.Instance.TurnClock.text = currentTime.ToString();
        }

        //Stop the team's turn and player's movement
        teamManagers[currentTeam].StopTeamTurn();
        EndTurn();
        ChangeCurrentTeam();
    }

    //Called at the beginning of the game
    IEnumerator StartGameClock()
    {
        //Convert time from seconds to minutes
        int currentTime = gameDuration;
        string minutes = currentTime / 60 < 10 ? $"0{currentTime / 60}" : $"{currentTime / 60}";
        string seconds = currentTime % 60 < 10 ? $"0{currentTime % 60}" : $"{currentTime % 60}";
        string time = $"{minutes}:{seconds}";
        UIManager.Instance.GameClock.text = time;

        //Update the clock every second
        while (currentTime > 0)
        {
            yield return new WaitForSeconds(1);
            currentTime -= 1;

            minutes = currentTime / 60 < 10 ? $"0{currentTime / 60}" : $"{currentTime / 60}";
            seconds = currentTime % 60 < 10 ? $"0{currentTime % 60}" : $"{currentTime % 60}";
            time = $"{minutes}:{seconds}";
            UIManager.Instance.GameClock.text = time;
        }

        //End the game when the clock runs out
        EndGame();
    }

    #endregion
}