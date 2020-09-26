using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmsScript : MonoBehaviour
{
    public void RightTog()
    {
        SendMessageUpwards("RightTogFunc", SendMessageOptions.DontRequireReceiver);
    }

    public void LeftTog()
    {
        SendMessageUpwards("LeftTogFunc", SendMessageOptions.DontRequireReceiver);
    }

    public void ResetSwordAttack()
    {
        SendMessageUpwards("ResetSwordAttackFunc", SendMessageOptions.DontRequireReceiver);
    }

    public void BlockReset()
    {
        SendMessageUpwards("BlockResetFunc", SendMessageOptions.DontRequireReceiver);
    }

    public void Shoot()
    {
        SendMessageUpwards("ShootFunc", SendMessageOptions.DontRequireReceiver);
    }

    public void ResetGunAttack()
    {
        SendMessageUpwards("ResetGunAttackFunc", SendMessageOptions.DontRequireReceiver);
    }

    public void Attack(float dmg)
    {
        SendMessageUpwards("AttackFunc", dmg, SendMessageOptions.DontRequireReceiver);
    }

    public void StartAssasinate()
    {
        SendMessageUpwards("StartAssasinateFunc", SendMessageOptions.DontRequireReceiver);
    }

    public void DetachBody()
    {
        SendMessageUpwards("DetachBodyFunc", SendMessageOptions.DontRequireReceiver);
    }

    public void ResetAssasinate()
    {
        SendMessageUpwards("ResetAssasianteFunc", SendMessageOptions.DontRequireReceiver);
    }

}