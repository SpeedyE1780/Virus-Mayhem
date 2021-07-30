using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : Projectile
{
    //Trigger on collision
    private void OnCollisionEnter(Collision collision)
    {
        base.TriggerExplosion();
    }
}
