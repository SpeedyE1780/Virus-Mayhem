using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour
{
    public Transform AimPosition;
    public Text Health;
    [HideInInspector]
    public int currentHealth;
    [HideInInspector]
    public bool healthUpdated;
    
    int movementSpeed;
    Rigidbody rigidBody;
    readonly float rotationSpeed = 5f;
    Weapon currentWeapon;
    TeamManager team;
    Quaternion healthRotation;
    bool aiming;
    float shotPower;

    private void OnEnable()
    {
        EventManager.gameEnded += DestroyPlayer;
    }

    private void OnDisable()
    {
        EventManager.gameEnded -= DestroyPlayer;
    }

    //Keep the health text looking at the camera
    private void Update()
    {
        Health.transform.rotation = healthRotation * Camera.main.transform.rotation;
    }

    //Initialize the player
    public void InitializePlayer(TeamManager tm ,int health)
    {
        team = tm;
        gameObject.name = $"{team.team.Name} {transform.parent.childCount}";
        currentHealth = health;
        Health.text = currentHealth.ToString();
        movementSpeed = 5;
        rigidBody = GetComponent<Rigidbody>();

        healthRotation = Health.transform.rotation;
    }

    //Destroy the player once the game ends
    void DestroyPlayer()
    {
        Destroy(gameObject);
    }

    //Enable the player's movement and set it as the camera's target
    public void StartMoving()
    {
        StartCoroutine(nameof(GameUpdate));
        CameraManager.Instance.SetTarget(this.transform , TargetType.Player);
        EventManager.changeWeapon += UpdateWeapon;
    }

    //Stop the player's movement
    public void StopMoving()
    {
        StopCoroutine(nameof(GameUpdate));
        rigidBody.velocity = Vector3.zero;
        AimPosition.localRotation = Quaternion.identity;
        currentWeapon = null;
        UIManager.Instance.SetCurrentWeapon(null);
        EventManager.changeWeapon -= UpdateWeapon;
    }

    //Update the current weapon to the selected
    void UpdateWeapon(Weapon w)
    {
        currentWeapon = w;
    }

    //Replace update function
    public  IEnumerator GameUpdate()
    {
        float rotX = 0;
        
        while(true)
        {
            //Stop player from moving while side menu is open
            //yield return new WaitUntil(() => !UIManager.Instance.PopUpVisible);
            if(!UIManager.Instance.PopUpVisible)
            {
                Vector3 direction = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
                rigidBody.velocity = direction.normalized * movementSpeed;

                transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * rotationSpeed);

                //Aiming
                if (Input.GetMouseButtonDown(1))
                {
                    aiming = true;
                    CameraManager.Instance.StartAim(AimPosition);
                }

                if (Input.GetMouseButton(1))
                {
                    //Rotate the camera on the X
                    rotX += -1 * Input.GetAxis("Mouse Y") * rotationSpeed;
                    rotX = Mathf.Clamp(rotX, -90, 90);
                    AimPosition.localRotation = Quaternion.Euler(rotX, 0, 0);

                    //Shooting
                    if(currentWeapon != null && aiming)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            shotPower = 0;
                        }

                        if(Input.GetMouseButton(0))
                        {
                            shotPower += Time.deltaTime * 3;
                        }

                        if(Input.GetMouseButtonUp(0))
                        {
                            currentWeapon.Shoot(AimPosition , Mathf.Abs(Mathf.Sin(shotPower)));
                            StopMoving(); //Stop the players update
                            EventManager.endTurn.Invoke();
                        }
                    }
                }

                //Stop aiming
                if (Input.GetMouseButtonUp(1))
                {
                    aiming = false;
                    shotPower = 0;
                    AimPosition.localRotation = Quaternion.identity;
                    CameraManager.Instance.SetTarget(transform, TargetType.Player);
                }
            }
            else if(aiming)
            {
                aiming = false;
                shotPower = 0;
                AimPosition.localRotation = Quaternion.identity;
                CameraManager.Instance.SetTarget(transform, TargetType.Player);
            }

            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Pick up medkit
        if(collision.gameObject.CompareTag("Medkit"))
        {
            MedkitDrop medkitDrop = collision.gameObject.GetComponent<MedkitDrop>();
            StartCoroutine(nameof(UpdateHealth), medkitDrop.Health);
            medkitDrop.DestroyDrop();
        }

        //Pick up weapon drop
        if (collision.gameObject.CompareTag("WeaponDrop"))
        {
            WeaponDrop weaponDrop = collision.gameObject.GetComponent<WeaponDrop>();
            team.AddWeapon(weaponDrop.DropWeapon); ;
            weaponDrop.DestroyDrop();
        }
    }

    public void TakeDamage(int damage)
    {
        StartCoroutine(nameof(UpdateHealth), -1 * damage);
        healthUpdated = false;
    }

    //Add/Reduce the player's heatlh
    IEnumerator UpdateHealth(int amount)
    {
        bool heal = amount > 0;
        amount = Mathf.Abs(amount);

        for (int i = 0; i < amount && currentHealth > 0 ; i++) 
        {
            currentHealth += heal ? 1 : -1;
            Health.text = currentHealth.ToString();

            yield return new WaitForSeconds(0.05f);
        }

        healthUpdated = true;
    }

    public void KillPlayer()
    {
        team.KillPlayer(this);
        Destroy(gameObject);

        SoundManager.Instance.Explosion.Play();
    }
}