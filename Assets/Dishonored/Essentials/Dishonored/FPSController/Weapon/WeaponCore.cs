using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCore : MonoBehaviour {

    public enum WeaponType
    {
        ranged, melee
    }

    public WeaponType weaponType;

    public Sprite cursor;

    public GameObject shootPoint, explosion;
    public Bullet bullet;
    public int bullets;

    public float damage;

    public Vector3 localPosition, localRotation;
}
