using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : ScriptableObject
{
    [Header("Weapon Objects")]
    public Projectile WeaponProjectile;
    public ParticleSystem WeaponExplosion;
    public Sprite WeaponSprite;

    [Header("Weapon Stats")]
    public string Name;
    public int Damage;
    public float Raduis;
    public float Force;
    public float Velocity;

    public abstract void Shoot(Transform position , float shotPower);
}