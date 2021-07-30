using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeController : Projectile
{
    protected int duration;

    //Trigger after duration ends
    public void SetProjectile(int d, float r, Vector3 dir,float velocity, float force, ParticleSystem exp, int dur)
    {
        base.SetProjectile(d, r, dir,velocity, force , exp);
        duration = dur;
        Invoke(nameof(TriggerExplosion), duration);
    }
}