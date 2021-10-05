using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region PUBLIC VARIABLES

    [Header("Main Menu UI")]
    public GameObject MainMenuUI;

    [Header("Settings UI")]
    public GameObject SettingsUI;
    public Button GameSettingsButton;
    public Button AudioSettingsButton;
    public RectTransform GameSettings;
    public RectTransform AudioSettings;

    [Header("Settings Labels")]
    public Text TurnDurationText;
    public Slider TurnDurationSlider;
    public Text GameDurationText;
    public Slider GameDurationSlider;
    public Text SquadSizeText;
    public Slider SquadSizeSlider;
    public Text PlayerHealthText;
    public Slider PlayerHealthSlider;

    [Header("Team SelectionUI")]
    public GameObject TeamSelectionUI;
    public Transform TeamSelectionParents;
    public Transform AddTeamButton;
    public GameObject TeamMenu;
    public Button StartGameButton;

    [Header("Game UI")]
    public GameObject GameUI;
    public GameObject ShotPowerPanel;
    public Image CurrentWeapon;
    public Slider ShotPower;
    public GameObject CrossHair;

    [Header("Inventory")]
    public RectTransform InventoryMenu;
    public GameObject InventoryTitle;
    public Transform WeaponsParent;
    public WeaponController WeaponButton;
    public Text WeaponName;

    [Header("Team Overlay")]
    public Image TeamOverlay;
    public Transform TeamInfoBox;
    public TeamController TeamInfo;

    [Header("Map Overlay")]
    public RawImage MapOverlay;

    [Header("Game Clocks")]
    public Text TurnClock;
    public Text GameClock;

    [Header("ResultUI")]
    public GameObject ResultUI;
    public Text WinningTeamName;
    public Image WinningTeamLogo;

    public bool PopUpVisible { get => inventoryVisible || overlayVisible || mapVisible; } //Checks if one of the pop ups is open

    #endregion

    #region PRIVATE VARIABLES

    List<TeamController> overlayTeams;
    bool overlayVisible;
    bool inventoryVisible;
    bool mapVisible;
    float shotPower;
    bool turnStarted;

    //Singleton
    private static UIManager _instance;
    public static UIManager Instance { get => _instance; }

    #endregion

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this.gameObject);

        //Set the Main Menu as the active screen
        SettingsUI.SetActive(false);
        MainMenuUI.SetActive(true);
        TeamSelectionUI.SetActive(false);
        GameUI.SetActive(false);
        ResultUI.SetActive(false);
    }

    #region SETTINGS FUNCTIONS

    public void InitializeSettings()
    {
        SettingsUI.SetActive(true);

        GameSettingsButton.interactable = false;
        AudioSettingsButton.interactable = true;

        GameSettings.localScale = Vector3.one;
        GameSettings.gameObject.SetActive(true);
        AudioSettings.localScale = Vector3.zero;
        AudioSettings.gameObject.SetActive(false);
    }
    public void InitializeGameSettings(int tDuration, int tValue, int gDuration, int gValue, int sSize, int sValue, int pHealth, int pValue)
    {
        //Set turn label and slider
        TurnDurationText.text = $"Turn Duration: {tDuration} seconds";
        TurnDurationSlider.value = tValue;

        //Set game label and slider
        GameDurationText.text = $"Game Duration: {gDuration} minutes";
        GameDurationSlider.value = gValue;

        //Set squad label and slider
        SquadSizeText.text = $"Squad Team: {sSize} members";
        SquadSizeSlider.value = sValue;

        //Set player label and slider
        PlayerHealthText.text = $"Player Health: {pHealth}HP";
        PlayerHealthSlider.value = pValue;
    }
    public void SetTurnDurationText(int duration)
    {
        TurnDurationText.text = $"Turn Duration: {duration} seconds";
    }

    public void SetGameDurationText(int duration)
    {
        GameDurationText.text = duration == -1 ? "Game Duration: ∞" : $"Game Duration: {duration} minutes";
    }

    public void SetSquadSizeText(int amount)
    {
        SquadSizeText.text = $"Squad Team: {amount} members";
    }

    public void SetPlayerHealthText(int amount)
    {
        PlayerHealthText.text = $"Player Health: {amount}HP";
    }

    #endregion

    #region LEVELSELECTION FUNCTIONS

    public void InitializeTeamSelectionUI()
    {
        TeamSelectionUI.SetActive(true);
        TeamMenuController.EmptySelectedTeams(); //Empty the static list of selected factions
        CheckTeamCount();
    }

    public void CheckTeamCount()
    {
        StartGameButton.interactable = TeamSelectionParents.childCount > 2; //Enable the start game button after 2 teams are selected
        AddTeamButton.gameObject.SetActive(TeamSelectionParents.childCount < 5); //Disable the add team button once maximum number of team has been reached
    }

    //Add a team menu to change the teams info
    public void AddTeamSelection()
    {
        Instantiate(TeamMenu, TeamSelectionParents);
        AddTeamButton.SetAsLastSibling();
        CheckTeamCount();
    }

    //Empty the team selection after leaving the team selection UI
    public void EmptyTeamSelection()
    {
        foreach(Transform child in TeamSelectionParents)
        {
            if (child != AddTeamButton)
                Destroy(child.gameObject);
        }
    }

    #endregion

    #region GAME FUNCTIONS

    public void StartGame()
    {
        //Switch between team selection and game UI
        TeamSelectionUI.SetActive(false);
        EmptyTeamSelection();
        GameUI.SetActive(true);

        //Hide inventory if visible
        InventoryMenu.sizeDelta = Vector2.up * InventoryMenu.sizeDelta.y;
        WeaponName.gameObject.SetActive(false);
        InventoryTitle.SetActive(false);
        inventoryVisible = false;

        //Hide overlay if visible
        TeamOverlay.gameObject.SetActive(false);
        Color color = TeamOverlay.color;
        color.a = 0;
        TeamOverlay.color = color;
        TeamInfoBox.gameObject.SetActive(false);
        overlayVisible = false;

        //Hide map if visible
        MapOverlay.gameObject.SetActive(false);
        color = MapOverlay.color;
        color.a = 0;
        MapOverlay.color = color;
        mapVisible = false;

        //Hide current weapon && Crosshair
        ShotPowerPanel.SetActive(false);
        CrossHair.SetActive(false);


        turnStarted = false;
        StartCoroutine(nameof(GameUpdate));
    }

    public void ToggleTurnStarted(bool started)
    {
        turnStarted = started;
        
        //Hide the crosshair between turns
        CrossHair.SetActive(false);
        
        //Hide any visible pop up
        if (PopUpVisible)
        {
            if (mapVisible)
            {
                mapVisible = false;
                CameraManager.Instance.ToggleMapCamera(true);
                ToggleMap();
            }

            if (overlayVisible)
            {
                overlayVisible = false;
                ToggleOverlay();
            }

            if(inventoryVisible)
            {
                inventoryVisible = false;
                ToggleInventory();
            }
        }
    }

    //Reset the list at the start of the game
    public void ResetTeamOverlay()
    {
        overlayTeams = new List<TeamController>();
    }

    //Update the inventory to show the current team's weapons
    public void UpdateWeaponsButton(List<Weapon> weapons)
    {
        RemoveWeaponsButton();

        foreach(Weapon w in weapons)
        {
            WeaponController wc = Instantiate(WeaponButton, WeaponsParent);
            wc.SetInfo(w);
        }
    }

    //Empty inventory
    void RemoveWeaponsButton()
    {
        foreach (Transform child in WeaponsParent)
        {
            Destroy(child.gameObject);
        }
    }

    //Add team to the overlay
    public void AddTeamOverlay(Team t)
    {
        TeamController tc = Instantiate(TeamInfo, TeamInfoBox);
        tc.SetTeam(t);
        overlayTeams.Add(tc);
    }

    //Update the overlay to be ranked correctly
    public void UpdateTeamOverlay(List<Team> teams)
    {
        //Remove teams that died
        while (teams.Count != overlayTeams.Count)
        {
            Destroy(overlayTeams[0].gameObject);
            overlayTeams.RemoveAt(0);
        }

        //Change the team info
        for (int i = 0; i < overlayTeams.Count; i++)
        {
            overlayTeams[i].SetTeam(teams[i]);
        }
    }

    //Empty the team overlay
    void RemoveTeamOverlay()
    {
        foreach(TeamController team in overlayTeams)
        {
            Destroy(team.gameObject);
        }
    }

    public void SetCurrentWeapon(Sprite weapon)
    {
        //Player didn't choose a weapon yet
        if (weapon == null)
        {
            ShotPowerPanel.SetActive(false);
        }
        else
        {
            ShotPowerPanel.SetActive(true);
            CurrentWeapon.sprite = weapon;
            ShotPower.value = 0;
        }
    }

    //Detect any Inputs while ingame
    IEnumerator GameUpdate()
    {
        while (true)
        {
            yield return new WaitUntil(() => Input.anyKey);

            //Prevents opening mutliple pop up or opening pop ups after turn ended
            if(!PopUpVisible && turnStarted)
            {
                //Show Inventory & Hide Crosshair
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    //Reset aiming
                    ResetAiming();

                    inventoryVisible = true;
                    ToggleInventory();
                }

                //Show Team Overlay & Hide Crosshair
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    ResetAiming();

                    overlayVisible = true;
                    ToggleOverlay();
                }

                //Show Map & Hide Crosshair
                if (Input.GetKeyDown(KeyCode.M))
                {
                    ResetAiming();

                    mapVisible = true;
                    CameraManager.Instance.ToggleMapCamera(); //Turn on the map camera
                    ToggleMap();
                }

                //Show Crosshair
                if(Input.GetMouseButtonDown(1))
                {
                    CrossHair.SetActive(true);
                }

                //Charge shot while aiming shotpower makes the player selected a weapon & cross hair makes sure the player is aiming
                if(Input.GetMouseButton(1) && ShotPowerPanel.activeSelf && CrossHair.activeSelf)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        ShotPower.value = 0;
                        shotPower = 0;

                    }

                    if (Input.GetMouseButton(0))
                    {
                        shotPower += Time.deltaTime * 3;
                        ShotPower.value = Mathf.Abs(Mathf.Sin(shotPower));
                    }

                    if (Input.GetMouseButtonUp(0))
                    {
                        ResetAiming();
                    }
                }
            }

            //Hide Crosshair
            if(Input.GetMouseButtonUp(1))
            {
                CrossHair.SetActive(false);
                ResetAiming();
            }
            
            //Hide Inventory
            if (Input.GetKeyUp(KeyCode.Q))
            {
                inventoryVisible = false;
                ToggleInventory();
            }

            //Hide Team Overlay
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                overlayVisible = false;
                ToggleOverlay();
            }

            //Hide Map
            if (Input.GetKeyUp(KeyCode.M))
            {
                mapVisible = false;
                CameraManager.Instance.ToggleMapCamera(); //Turn off the map camera
                ToggleMap();
            }
        }
    }

    void ResetAiming()
    {
        CrossHair.SetActive(false);
        ShotPower.value = 0;
        shotPower = 0;
    }

    void ToggleOverlay()
    {
        StopCoroutine(nameof(IToggleOverlay));
        StartCoroutine(nameof(IToggleOverlay));
    }

    void ToggleMap()
    {
        StopCoroutine(nameof(IToggleMap));
        StartCoroutine(nameof(IToggleMap));
    }

    void ToggleInventory()
    {
        StopCoroutine(nameof(IToggleInventory));
        StartCoroutine(nameof(IToggleInventory));
    }

    IEnumerator IToggleOverlay()
    {
        float maxAlpha = 0.6f;
        Color start = TeamOverlay.color;
        Color end = overlayVisible ? new Color(start.r, start.g, start.b, maxAlpha) : new Color(start.r, start.g, start.b, 0);

        if (overlayVisible)
        {
            TeamOverlay.gameObject.SetActive(true);
        }
        else
        {
            TeamInfoBox.gameObject.SetActive(false);
        }

        float time = 0.1f;
        float iterations = 10;
        float step = 1 / iterations;
        float wait = time / iterations;
        float i = 0;

        while (i <= 1.0f)
        {
            i += step;

            TeamOverlay.color = Color.Lerp(start, end, i);

            yield return new WaitForSeconds(wait);
        }

        if (overlayVisible)
        {
            TeamInfoBox.gameObject.SetActive(true);
        }
        else
        {
            TeamOverlay.gameObject.SetActive(false);
        }

        yield return null;
    }

    IEnumerator IToggleMap()
    {
        float maxAlpha = 1f;
        Color start = MapOverlay.color;
        Color end = mapVisible ? new Color(start.r, start.g, start.b, maxAlpha) : new Color(start.r, start.g, start.b, 0);

        if (mapVisible)
        {
            MapOverlay.gameObject.SetActive(true);
        }

        float time = 0.1f;
        float iterations = 10;
        float step = 1 / iterations;
        float wait = time / iterations;
        float i = 0;

        while (i <= 1.0f)
        {
            i += step;

            MapOverlay.color = Color.Lerp(start, end, i);

            yield return new WaitForSeconds(wait);
        }

        if (!mapVisible)
        {
            MapOverlay.gameObject.SetActive(false);
        }

        yield return null;
    }

    public IEnumerator IToggleInventory()
    {
        float maxWidth = 400;
        Vector2 start = InventoryMenu.sizeDelta;
        Vector2 end = inventoryVisible ? Vector2.right * maxWidth + Vector2.up * InventoryMenu.sizeDelta.y : Vector2.up * InventoryMenu.sizeDelta.y;

        //Hide the title before closing
        if (!inventoryVisible)
            InventoryTitle.SetActive(false);

        float time = 0.5f;
        float iterations = 30;
        float step = 1 / iterations;
        float wait = time / iterations;
        float i = 0;

        while (i < 1.0f)
        {
            i += step;
            InventoryMenu.sizeDelta = Vector2.Lerp(start, end, i);
            yield return new WaitForSeconds(wait);
        }

        //Show the title after opening
        if (inventoryVisible)
            InventoryTitle.SetActive(true);

        yield return null;
    }

    #endregion

    #region RESULT FUNCTIONS

    public void InitializeResultUI(Team team)
    {
        //Disable the game UI and empty the inventory and team overlay
        StopAllCoroutines();
        RemoveWeaponsButton();
        RemoveTeamOverlay();
        GameUI.SetActive(false);

        //Enable the result screen and update the team info to the winning team
        ResultUI.SetActive(true);
        WinningTeamName.text = $"{team.Name} Won!";
        WinningTeamLogo.sprite = team.TeamFaction.FactionLogo;

        SoundManager.Instance.Victory.Play();
    }

    #endregion
}