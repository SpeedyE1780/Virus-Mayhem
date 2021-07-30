using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Launchers/Semi Automatic Launcher")]
public class Launchers : Weapon
{
    public override void Shoot(Transform position , float shotPower)
    {
        Bullet bullet = (Bullet)Instantiate(WeaponProjectile, position.transform.position, position.rotation);
        bullet.SetProjectile(Damage, Raduis, position.forward, Velocity * shotPower, Force, WeaponExplosion);
        CameraManager.Instance.SetTarget(bullet.transform , TargetType.Bullet);
    }
}