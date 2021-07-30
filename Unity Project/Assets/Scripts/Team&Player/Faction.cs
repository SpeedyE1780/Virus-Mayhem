using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Faction")]
public class Faction : ScriptableObject
{
    public string FactionName;
    public PlayerController Character;
    public Sprite FactionLogo;
    public ParticleSystem PlayerDeadExplosion;
}