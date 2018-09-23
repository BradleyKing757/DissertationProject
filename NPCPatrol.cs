using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class creates an array of waypoints which have already been set in the inspector. The NPC will travel between the waypoints in the array
/// within an accuracy of 3 metres until the state of the NPC changes again.
/// </summary>
public class NPCPatrol : NPCStateMachine
{
    GameObject[] waypoints;
    int currentWP;

    private void Awake()
    {
        waypoints = GameObject.FindGameObjectsWithTag("waypointsnew");
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        currentWP = 0;

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(waypoints.Length == 0) return;
        if(Vector3.Distance(waypoints[currentWP].transform.position, //where NPC is in relation to waypoint within accuracy of 3.0f
                            NPC.transform.position) < waypointAccuracy)
        {
            currentWP++;
            if(currentWP >= waypoints.Length)
            {
                currentWP = 0;
            }
        }

        agent.SetDestination(waypoints[currentWP].transform.position);
        //rotate towards target
        /* var direction = waypoints[currentWP].transform.position - NPC.transform.position;
         NPC.transform.rotation = Quaternion.Slerp(NPC.transform.rotation, 
                                     Quaternion.LookRotation(direction), 
                                     rotSpeed * Time.deltaTime);
         NPC.transform.Translate(0, 0, Time.deltaTime * speed);*/
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }


}
