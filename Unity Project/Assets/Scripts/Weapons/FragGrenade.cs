using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Grenades/Frag Grenade")]
public class FragGrenade : Grenade
{
    [Header("Fragments")]
    public Rigidbody Fragment;
    public int FragNumber;
    public int FragRaduis;
    public int FragDamage;
    public int FragForce;
    public int FragVelocity;
    
    public override void Shoot(Transform position , float shotPower)
    {
        FragGrenadeController fragGrenade = (FragGrenadeController)Instantiate(WeaponProjectile, position.transform.position, position.rotation);
        fragGrenade.SetProjectile(Damage, Raduis, position.forward, Velocity * shotPower, Force, WeaponExplosion, Duration);
        fragGrenade.SetFragments(Fragment, FragNumber, FragRaduis, FragForce, FragDamage , FragVelocity);
        CameraManager.Instance.SetTarget(fragGrenade.transform, TargetType.Bullet);
    }
}