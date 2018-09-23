using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

/// <summary>
/// This class is used to allow the player to swap between the guns they are using. The weapons gameObjects are placed in an array and are all
/// set to inactive. The methods in the class set each weapon to their individual key bindings. When the player presses a number key (1,2,3), the
/// gun associated with the key is set to active while the other are set to inactive, so only one guy is active at a time. There is also a time limit
/// when switching guns so that the player cannot switch between guns really fast.
/// </summary>
public class WeaponManager : MonoBehaviour
{

    [SerializeField] private GameObject[] weapons;
    [SerializeField] float switchDelay = 1f;

    public bool isShotgunActive = false;

    private int index;
    private bool isSwitching;
    private FPSController fps;

    /// <summary>
    /// Use this for initialization.
    /// </summary>
    void Start()
    {
        InitialiseWeapons();
        fps = GameObject.FindObjectOfType<FPSController>();
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    void Update()
    {
        NewSwitchWeapons();

    }

    /// <summary>
    /// Sets the weapon index at 0.
    /// </summary>
    private void InitialiseWeapons()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(false);
        }
        weapons[0].SetActive(true);
    }


    /// <summary>
    /// Returns the boolean value for isShotgunActive.
    /// </summary>
    /// <returns>isShotgunActive</returns>
    public bool IsShotgunActive()
    {
        if (isShotgunActive == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Sets a small delay between switching weapons.
    /// </summary>
    /// <param name="newIndex"></param>
    /// <returns></returns>
    private IEnumerator SwitchAfterDelay(int newIndex)
    {
        isSwitching = true;
        yield return new WaitForSeconds(switchDelay);

        isSwitching = false;

    }

    /// <summary>
    /// Assigns key bindings to weapons.
    /// Sets the weapons and sprites to active/inactive
    /// </summary>
    private void NewSwitchWeapons()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && (!isSwitching))
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].SetActive(false);
            }
            weapons[0].SetActive(true);
            isShotgunActive = false;
            StartCoroutine(SwitchAfterDelay(0));
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && (!isSwitching) && fps.M4Enabled == true)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].SetActive(false);
            }
            weapons[1].SetActive(true);
            isShotgunActive = false;
            StartCoroutine(SwitchAfterDelay(1));
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && (!isSwitching) && fps.ShotgunEnabled == true)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].SetActive(false);
            }
            weapons[2].SetActive(true);
            isShotgunActive = true;
            StartCoroutine(SwitchAfterDelay(2));
        }



    }

}