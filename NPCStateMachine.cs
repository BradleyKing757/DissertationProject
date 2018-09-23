using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class creates an array of waypoints which have already been set in the inspector. The NPC will travel between the waypoints in the array
/// within an accuracy of 3 metres until the state of the NPC changes again.
/// </summary>
public class NPCStateMachine : StateMachineBehaviour
{

    public UnityEngine.AI.NavMeshAgent agent;
    public GameObject NPC; //friendly cop
    public GameObject opponent; //zombie object
    public Transform opponent1; //zombie position
    public float speed = 2.0f;
    public float rotSpeed = 20.0f;
    public float waypointAccuracy = 3.0f;
    public float nPCMoveAccuracy = 10.0f;
    public Animator anima;
    public CapsuleCollider cc;


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        NPC = animator.gameObject;
        opponent = NPC.GetComponent<FriendlyAI>().GetZombie();
        opponent1 = NPC.GetComponent<FriendlyAI>().GetZombieTransform();
        agent = NPC.GetComponent<UnityEngine.AI.NavMeshAgent>();
        anima = NPC.GetComponent<Animator>();
        cc = NPC.GetComponent<FriendlyAI>().GetZombie().GetComponent<CapsuleCollider>();
    }
}
