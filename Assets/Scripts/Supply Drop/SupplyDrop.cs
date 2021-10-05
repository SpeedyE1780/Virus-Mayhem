using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SupplyDrop : MonoBehaviour
{
    bool landed;
    
    virtual protected void OnEnable()
    {
        CameraManager.Instance.SetTarget(transform, TargetType.SupplyDrop);
        landed = false;
        EventManager.gameEnded += DestroyDrop;
    }

    private void OnDisable()
    {
        EventManager.gameEnded -= DestroyDrop;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Start the player's turn after hitting the ground or another supply drop
        if(!landed && !collision.gameObject.CompareTag("Player"))
        {
            Invoke(nameof(StartTurn), 1f);
            landed = true;
        }
    }

    void StartTurn()
    {
        EventManager.startTurn.Invoke();
    }

    //Destroy the drop after picking it up
    public void DestroyDrop()
    {
        //If drop landed on player start the turn
        if (!landed)
            EventManager.startTurn.Invoke();

        Destroy(gameObject);
    }
}