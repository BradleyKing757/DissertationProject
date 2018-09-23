using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Responsible for the players guns in terms of firing, reloading, ammo count and gun sounds. The player uses the left mouse click to fire the weapon.  
/// </summary>
public class Weapon : MonoBehaviour
{
    [Header("Weapon Properties")]
    public float range = 100f; //max range of the weapon
    public int bulletsPerMag = 30; //bullets per each magazine
    public int totalBullets = 200; //bullets left
    public float fireRate = 2.0f;  //delay between each shot
    float fireTimer; //time counter for the delay
    public float spreadFactor = 0.1f; //amount of bullet spread
    public int currentBullets; //current bullets in magazine
    public Transform shootPoint; //the point from which the bullet leaves the muzzle

    [Header("UI")]
    public Text ammoText;

    public ParticleSystem muzzleFlash; //Muzzle flash

    [Header("Weapon Effects")]
    public AudioClip shootSound;
    public GameObject hitParticles;
    public GameObject bulletImpact;

    [Header("Private Variables")]
    private bool isReloading;
    private Animator anim;
    private AudioSource _AudioSource;
    private FPSController fps1;
    private Shotgun shotgun;

    private WeaponManager wm;

    /// <summary>
    /// When a gun is enabled, the guns individual ammo count will update.
    /// </summary>
    private void OnEnable()
    {
        UpdateAmmoText();
    }

    /// <summary>
    /// Getter for the Shootpoints transform.
    /// </summary>
    public Transform GetShootPoint
    {
        get
        {
            return shootPoint;
        }
    }



    /// <summary>
    /// Use this for initialisation.
    /// </summary>
    void Start()
    {

        anim = GetComponent<Animator>();
        _AudioSource = GetComponent<AudioSource>();
        fps1 = GameObject.FindObjectOfType<FPSController>();
        shotgun = GameObject.FindObjectOfType<Shotgun>();
        currentBullets = bulletsPerMag;
        wm = gameObject.GetComponentInParent(typeof(WeaponManager)) as WeaponManager;
    }

    /// <summary>
    /// Update is called once per frame.
    /// Attaches methods to key bindings.
    /// </summary>
    void Update()
    {
        if (wm.IsShotgunActive() == false)
        {
            if (Input.GetButton("Fire1"))
            {
                if (currentBullets > 0)
                {
                    Fire(); //execute Fire function if press and hold left mouse button
                }
                else
                {
                    DoReload();
                }

            }
        }
        else if (wm.IsShotgunActive() == true)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (currentBullets > 0)
                {
                    Fire();
                }
                else
                {
                    DoReload();
                }

            }
        }


        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentBullets < bulletsPerMag && totalBullets > 0)
            {
                DoReload();
            }

        }

        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime; //add into timer counter
        }
    }

    /// <summary>
    /// Plays after every fixed step
    /// </summary>
    private void FixedUpdate()
    {
        //gets base layer
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        isReloading = info.IsName("Reload");

    }

    /// <summary>
    /// Uses Raycast to simulate firing at an object.
    /// Raycasts forward direction adjusted slightly and randomly to represent bullet spread.
    /// Bullet holes and hit particles are instantiated at the Raycasts impact of a gameObject before being destroyed later. Bullet sounds and muzzle flash effect
    /// played on and ammo text is updated after every call of this method. If shotgun is active then the method in the shotgun class is called.
    /// </summary>
    void Fire()
    {
        //if fireTimer has not reached the fireRate (delay between each shot(0.1fsecs)) then we cannot fire.
        if (fireTimer < fireRate || currentBullets <= 0 || isReloading) return;

        if (wm.IsShotgunActive() == false)
        {
            RaycastHit hit;

            Vector3 shootDirection = shootPoint.transform.forward;
            shootDirection.x += Random.Range(-spreadFactor, spreadFactor);  //add -0.1 and +0.1
            shootDirection.y += Random.Range(-spreadFactor, spreadFactor);  //add -0.1 and +0.1

            if (Physics.Raycast(shootPoint.position, shootDirection, out hit, range))
            {
                Debug.Log(hit.transform.name + "found!");

                GameObject hitParticleEffect = Instantiate(hitParticles, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                GameObject bulletHole = Instantiate(bulletImpact, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));

                Destroy(hitParticleEffect, 1f);
                Destroy(bulletHole, 5f);
            }

            anim.CrossFadeInFixedTime("Fire", 0.1f);    //directly jump to this state - play the fire animatiom
            muzzleFlash.Play(); //show muzzle flash
            PlayShootSound();   //play shooting sound effect


            currentBullets--;   //deduct one bullet
            UpdateAmmoText();
            fireTimer = 0.0f; //reset fire
        }
        else
        {
            shotgun.ShotGunFire();
            currentBullets--;
            UpdateAmmoText();
            anim.CrossFadeInFixedTime("Fire", 0.1f);    //directly jump to this state - play the fire animatiom
            muzzleFlash.Play(); //show muzzle flash
            PlayShootSound();   //play shooting sound effect
        }


    }

    /// <summary>
    /// Controls the guns bullet counts and updates the UI text.
    /// </summary>
    public void Reload()
    {
        if (totalBullets <= 0) return;    //if we have no bullets, dont play this function

        int bulletsToLoad = bulletsPerMag - currentBullets; //how many bullets we need to load into gun
        int bulletsToDeduct = (totalBullets >= bulletsToLoad) ? bulletsToLoad : totalBullets;

        totalBullets -= bulletsToDeduct;
        currentBullets += bulletsToDeduct;

        UpdateAmmoText();
    }

    /// <summary>
    /// Controls the reload animation.
    /// </summary>
    private void DoReload()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        if (isReloading) return;

        anim.CrossFadeInFixedTime("Reload", 0.01f);

    }

    /// <summary>
    /// Plays the audio sound when gun is fired.
    /// </summary>
    private void PlayShootSound()
    {
        _AudioSource.PlayOneShot(shootSound);
        //_AudioSource.clip = shootSound;
        // _AudioSource.Play();
    }

    /// <summary>
    /// Updates the ammo text in the UI.
    /// </summary>
    private void UpdateAmmoText()
    {
        ammoText.text = currentBullets + " / " + totalBullets;
    }
}