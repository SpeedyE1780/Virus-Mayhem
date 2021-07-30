using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Grenades/Grenade")]
public class Grenade : Weapon
{
    public int Duration;

    public override void Shoot(Transform position , float shotPower)
    {
        GrenadeController grenade = (GrenadeController)Instantiate(WeaponProjectile, position.transform.position, position.rotation);
        grenade.SetProjectile(Damage, Raduis, position.forward, Velocity * shotPower, Force, WeaponExplosion, Duration);
        CameraManager.Instance.SetTarget(grenade.transform , TargetType.Bullet);
    }
}