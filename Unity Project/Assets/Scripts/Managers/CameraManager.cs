using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject MapCamera;

    //Offsets for different target types
    public Vector3 PlayerOffset;
    public Vector3 BulletOffset;
    public Vector3 SupplyDropOffset;

    Transform target; //Follow target
    Action updateCamera; //Function that is called in lateupdate
    Vector3 currentOffset;

    //Singleton
    private static CameraManager _instance;
    public static CameraManager Instance { get => _instance; }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this.gameObject);

        MapCamera.SetActive(false);
    }

    public void ToggleMapCamera(bool turnoff = false)
    {
        if(turnoff)
        {
            MapCamera.SetActive(false);
        }
        else
        {
            MapCamera.SetActive(!MapCamera.activeSelf);
        }
    }

    public void SetTarget(Transform t , TargetType targetType)
    {
        target = t;

        //Set the offset based on the target type
        if(targetType == TargetType.Player)
        {
            currentOffset = PlayerOffset;
        }
        else if(targetType == TargetType.Bullet)
        {
            currentOffset = BulletOffset;
        }
        else if(targetType == TargetType.SupplyDrop)
        {
            currentOffset = SupplyDropOffset;
        }

        updateCamera = Follow;
    }

    //Set the camera position to the target position
    public void StartAim(Transform t)
    {
        target = t;
        transform.SetPositionAndRotation(target.position, target.rotation);
        updateCamera = Aim;
    }

    //Zoom out after projectile explodes
    public void ZoomOut()
    {
        updateCamera = null;

        transform.position = target.TransformPoint(currentOffset * 3);

        //Make sure camera is above the target
        if (transform.position.y < target.position.y)
        {
            float diff = Mathf.Abs(transform.position.y - target.position.y);
            transform.position = new Vector3(transform.position.x, target.transform.position.y + diff, transform.position.z);
        }

        transform.LookAt(target);
    }

    //Look at player before killing him
    public void LookAtTarget(Transform target)
    {
        transform.position = target.TransformPoint(currentOffset);
        transform.LookAt(target);

         Vector3 direction = target.position - transform.position;

        //Make sure nothing is blocking the player from the camera
        for (int i = 0; i < 6; i++)
        {
            //Shoot a raycast towards the player
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit))
            {
                //If the raycast didn't hit the player rotate the camera
                if (hit.transform != target)
                {
                    transform.RotateAround(target.position, Vector3.up, 60);
                }
                else
                {
                    break;
                }
            }
        }

        updateCamera = null;
    }

    public void Freeze()
    {
        updateCamera = null;
    }

    private void LateUpdate()
    {
        updateCamera?.Invoke();
    }

    //Follow target
    void Follow()
    {
        transform.position = target.TransformPoint(currentOffset);

        //Make sure camera is above the target
        if(transform.position.y < target.position.y)
        {
            float diff = Mathf.Abs(transform.position.y - target.position.y);
            transform.position = new Vector3(transform.position.x, target.transform.position.y + diff, transform.position.z);
        }

        transform.LookAt(target);
    }

    //Set the camera position to the target position
    void Aim()
    {
        transform.SetPositionAndRotation(target.position, target.rotation);
    }
}

public enum TargetType
{
    Player,
    Bullet,
    SupplyDrop
}