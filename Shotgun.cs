using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used for the shotgun weapon. The methods in the class instantiates a number of pellets at the end of the shotgun and applies a
/// force to them so that they shoot forward. The pellets instantiate at random positions in a radius around the shootpoint.
/// </summary>
public class Shotgun : MonoBehaviour
{

    [Header("ShotgunProperties")]
    public float spreadAngle;
    public GameObject pellet;
    List<Quaternion> pellets;
    public float pelletFireVel;
    public int pelletCount;

    private Weapon wp;

    /// <summary>
    /// Creates a list for the pellets to go in.
    /// </summary>
    private void Awake()
    {
        pellets = new List<Quaternion>(pelletCount);
        for (int i = 0; i < pelletCount; i++)
        {
            pellets.Add(Quaternion.Euler(Vector3.zero));
        }

    }

    /// <summary>
    /// Instantiates a number of pellets at the shoot point in a spread.
    /// Force is applied to pellets to simulate gun fire.
    /// </summary>
    public void ShotGunFire()
    {
        int i = 0;
        foreach (Quaternion quat in pellets)
        {
            pellets[i] = Random.rotation;
            GameObject p = Instantiate(pellet, wp.GetShootPoint.position, wp.GetShootPoint.rotation);
            p.transform.rotation = Quaternion.RotateTowards(p.transform.rotation, pellets[i], spreadAngle);
            p.GetComponent<Rigidbody>().AddForce(p.transform.forward * pelletFireVel);
            i++;
        }
    }



    // Use this for initialization
    void Start()
    {
        wp = GameObject.FindObjectOfType<Weapon>();
    }


}
