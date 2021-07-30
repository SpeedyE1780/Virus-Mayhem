using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragGrenadeController : GrenadeController
{
    Rigidbody fragments;
    int fragNumber;
    int fragRaduis;
    int fragForce;
    int fragDamage;
    int fragVelocity;

    public void SetFragments(Rigidbody frag, int fragNum, int fragRad, int fragF, int fragD , int fragV)
    {
        fragments = frag;
        fragNumber = fragNum;
        fragRaduis = fragRad;
        fragForce = fragF;
        fragDamage = fragD;
        fragVelocity = fragV;
    }

    protected override void TriggerExplosion()
    {
        base.TriggerExplosion();
        
        CancelInvoke(); //Cancel the invoke to end the turn

        //Spawn fragments and give them a random velocity
        for(int i = 0; i< fragNumber; i++)
        {
            Rigidbody fragment = Instantiate(fragments, transform.position, transform.rotation, transform);
            fragment.velocity = new Vector3(Random.Range(0, 5f), 10f, Random.Range(0, 5f)).normalized * fragVelocity;
            StartCoroutine(nameof(FragExplosion), fragment.gameObject);
        }
        
        //Destroy after all fragments are destroyed
        Invoke(nameof(DestroyProjectile), duration + explosion.main.duration);
    }

    IEnumerator FragExplosion(GameObject fragment)
    {
        //Add an offset between the explosions
        float offset = Random.Range(0.1f, 0.3f);
        yield return new WaitForSeconds(duration + offset);

        //Get all colliders in the raduis
        foreach (Collider collider in Physics.OverlapSphere(fragment.transform.position, fragRaduis, playerLayer))
        {
            //Add an explosion force
            collider.attachedRigidbody.AddExplosionForce(fragForce * 50 , fragment.transform.position, fragRaduis);

            //Get the distance from the explosion
            float distance = 1 - Vector3.Distance(transform.position, collider.ClosestPoint(transform.position)) / fragRaduis;

            //Apply damage and add it to the damaged players list
            PlayerController pc = collider.gameObject.GetComponent<PlayerController>();
            pc.TakeDamage((int)(fragDamage * distance));

            damagedPlayers.Add(pc);
        }

        Destroy(fragment);
        explosion = Instantiate(explosion, fragment.transform.position, fragment.transform.rotation, transform);
        SoundManager.Instance.Explosion.Play();
    }

    protected override void DestroyProjectile()
    {
        base.DestroyProjectile();
        StopAllCoroutines();
    }
}
