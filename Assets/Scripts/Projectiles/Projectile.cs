using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Projectile : MonoBehaviour
{
    protected LayerMask playerLayer;
    protected Rigidbody rb;
    protected int damage;
    protected float raduis;
    protected float force;
    protected ParticleSystem explosion;
    protected HashSet<PlayerController> damagedPlayers;

    Renderer objectRenderer;

    private void OnEnable()
    {
        EventManager.gameEnded += KillProjectile;
    }

    private void OnDisable()
    {
        EventManager.gameEnded -= KillProjectile;
    }

    private void FixedUpdate()
    {
        transform.forward = rb.velocity;
    }

    //Initialize the Projectile
    virtual public void SetProjectile(int d, float r, Vector3 dir,float velocity, float f , ParticleSystem exp)
    {
        playerLayer = GameManager.Instance.Players;
        rb = GetComponent<Rigidbody>();
        damage = d;
        raduis = r;
        force = f;
        rb.velocity = dir.normalized * velocity;
        explosion = exp;
        damagedPlayers = new HashSet<PlayerController>();
        objectRenderer = GetComponent<Renderer>();
    }

    //Trigger the explopsion
    virtual protected void TriggerExplosion()
    {
        //Get all colliders in the raduis
        foreach (Collider collider in Physics.OverlapSphere(transform.position, raduis, playerLayer))
        {
            //Add an explosion force
            collider.attachedRigidbody.AddExplosionForce(force * 50, transform.position, raduis);

            //Get the distance from the explosion
            float distance = 1 - Vector3.Distance(transform.position, collider.ClosestPoint(transform.position)) / raduis;

            //Apply damage and add it to the damaged players list
            PlayerController pc = collider.gameObject.GetComponent<PlayerController>();
            pc.TakeDamage((int)(damage * distance));
            damagedPlayers.Add(pc);
        }

        //Zoom out on and trigger the explosion
        CameraManager.Instance.ZoomOut();

        //Instantiate as a child so that it gets destroy with the parent
        explosion = Instantiate(explosion, transform.position, transform.rotation , transform);

        //Play explosion sound
        SoundManager.Instance.Explosion.Play();

        //Turn off the projectile and freeze it in place
        objectRenderer.enabled = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        //Destroy gameobject after explosion finished
        Invoke(nameof(DestroyProjectile), explosion.main.duration);
    }

    //Destroy after the explosion ended
    protected virtual void DestroyProjectile()
    {
        EventManager.damagedPlayers.Invoke(damagedPlayers);
        KillProjectile();
    }

    //Destroy when the game ends
    void KillProjectile()
    {
        Destroy(this.gameObject);
    }
}