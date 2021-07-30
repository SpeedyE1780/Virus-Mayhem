using UnityEngine;
public class MedkitDrop : SupplyDrop
{
    [Range(1,4)]
    public int CrateSize;
    public int Health { get => CrateSize * 25; }
    
    override protected void OnEnable()
    {
        base.OnEnable();
        transform.localScale *= CrateSize;
    }
}