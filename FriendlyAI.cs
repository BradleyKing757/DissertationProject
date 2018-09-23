using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The FriendlyAI class is attached to the gameObject Douglas who is going to be controlled by this class. This class uses the FireGun()
///  method to simulate the NPC shooting at a target using a Raycast. The NPCSight method controls the NPC's with the conditions of 
///  line of sight, field of vision and distance between the NPC and the Zombie. The method will tell the Zombie to either, patrol, 
///  attack or chase depending on the conditions. 
/// </summary>
public class FriendlyAI : MonoBehaviour
{
    private AudioSource _AudioSource;

    [SerializeField] private Animator anim;
    [SerializeField] private GameObject zombieObject;
    [SerializeField] private float range = 100f; //max range of the weapon
    [SerializeField] private Transform shootPoint; //the point from which the bullet leaves the muzzle
    [SerializeField] private GameObject hitParticles;
    [SerializeField] private GameObject bulletImpact;
    [SerializeField] private Transform zombieTransform;
    [SerializeField] private Transform head;
    [SerializeField] private float FieldOfView;
    [SerializeField] private ParticleSystem muzzleFlash; //Muzzle flash
    [SerializeField] private AudioClip shootSound;
    public float fireRate = 0.1f;  //delay between each shot
    public float fireTimer; //time counter for the delay

    
    /// <summary>
    /// This method creates a Raycast starting at the NPC's pistol location (shootpoint) and ending after 100 meters. When the Raycast hits
    /// an object, a bullet hole gameObject will instantiate at the gameObjects position and a hit particle effects will also run. After 1 
    /// second the bullet hole and hitparticle effect will be destroyed to free up memory.
    /// When this method is called, the Attack animation, audio sound of a weapon firing and a muzzle flash effect will occur.
    /// </summary>
    public void FireGun()
    {
        //if fireTimer has not reached the fireRate (delay between each shot(0.1fsecs)) then we cannot fire.
        if (fireTimer < fireRate) return;

        RaycastHit hit;

        if (Physics.Raycast(shootPoint.position, shootPoint.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name + "found!");

            GameObject hitParticleEffect = Instantiate(hitParticles, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            GameObject bulletHole = Instantiate(bulletImpact, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));

            Destroy(hitParticleEffect, 1f);
            Destroy(bulletHole, 1f);
        }

        anim.CrossFadeInFixedTime("Attack", 0.01f);    //directly jump to this state - play the fire animatiom
        //muzzleFlash.Play(); //show muzzle flash
        PlayShootSound();   //play shooting sound effect

        fireTimer = 0.0f; //reset fire
    }


    /// <summary>
    /// Plays an AK-47 firing sound when gun shoots.
    /// </summary>
    private void PlayShootSound()
    {
        _AudioSource.PlayOneShot(shootSound);
    }

    /// <summary>
    /// Instantiation.
    /// </summary>
    void Start()
    {
        anim = GetComponent<Animator>();
        _AudioSource = GetComponent<AudioSource>();
        anim.SetBool("Chase", false);
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    void Update()
    {
        anim.SetFloat("distance", Vector3.Distance(transform.position, zombieObject.transform.position));
        NPCSight();
        
    }

    /// <summary>
    /// This method monitors the distance between the NPC and the Zombie gameObject. The NPC is also given an angle which represents 
    /// its field of vision. A Raycast is drawn from the NPC's shootpoint, this is used to detect the Zombie's collider.
    /// The chase behaviour of the NPC will occur when the Zombie is in the NPC's field of view and when the distance is less than 16 meteres.
    /// The attack behaviour will occur when the Zombie is in the NPC's field of vision and the distance between them is less than 10 meteres
    /// and the Zombie's collider has been detected by the Raycast. So the attack behaviour only occurs once the Zombie is infront of the player 
    /// and not behind another object.
    /// </summary>
    void NPCSight()
    {
        Vector3 direction = zombieTransform.position - this.transform.position; //NPC will rotate toward FPS position
        direction.y = 0;    //Stop NPC tipping over when FPS moves close
        float angle = Vector3.Angle(direction, head.forward); //the NPC field of vision

        RaycastHit hit;
        Physics.Raycast(shootPoint.position, shootPoint.transform.forward, out hit, 16);
        Debug.DrawRay(shootPoint.position, shootPoint.transform.forward * 10, Color.green);

        //Deals with Chase animation
        if (angle < 100 && anim.GetFloat("distance") < 16)
        {
            anim.SetBool("Chase", true);
            //Move back so not too close to zombie
            if (anim.GetFloat("distance") < 3.0f)
            {
                var direction1 = zombieObject.transform.position - this.transform.position;
                this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                                            Quaternion.LookRotation(direction),
                                            3.0f * Time.deltaTime);
            }
        }
        else
        {
            anim.SetBool("Chase", false);
        }


        //Deals with Attack animation
        if (hit.collider.gameObject.tag == "Zombie" && angle < 100 && anim.GetFloat("distance") < 10)
        {
            anim.SetBool("Attack", true);
            anim.SetBool("Chase", false);
        }
        else if (hit.collider.gameObject.tag != "Zombie" && angle < 100 && anim.GetFloat("distance") < 10)
        {
            this.transform.LookAt(zombieObject.transform.position);
            anim.SetBool("Attack", false);
            anim.SetBool("Chase", true);
        }

        else
        {
            anim.SetBool("Attack", false);

        }
    }

    /// <summary>
    /// Getter for the Zombie Game Object
    /// </summary>
    /// <returns>zombieObject</returns>
    public GameObject GetZombie()
    {
        return zombieObject;
    }

    /// <summary>
    /// Getter for the Zombie's Transform
    /// </summary>
    /// <returns>zombieTransform</returns>
    public Transform GetZombieTransform()
    {
        return zombieTransform;
    }

    /// <summary>
    /// If firetimer is less than the fireRate then the fireTimer
    /// </summary>
    public void Firing()
    {
        if(fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }
    }
}
