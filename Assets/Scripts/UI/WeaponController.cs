using UnityEngine;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    public Image WeaponImage;
    Weapon weapon;

    public void SetInfo(Weapon w)
    {
        weapon = w;
        WeaponImage.sprite = weapon.WeaponSprite;
    }

    //Show info when mouse hovers over button
    public void ShowInfo()
    {
        UIManager.Instance.WeaponName.gameObject.SetActive(true);
        UIManager.Instance.WeaponName.text = weapon.Name;
    }

    //Hide info when mouse leaves button
    public void HideInfo()
    {
        UIManager.Instance.WeaponName.gameObject.SetActive(false);
    }

    //Send the selected weapon to the player
    public void WeaponSelected()
    {
        UIManager.Instance.SetCurrentWeapon(weapon.WeaponSprite);
        EventManager.changeWeapon.Invoke(weapon);
    }
}