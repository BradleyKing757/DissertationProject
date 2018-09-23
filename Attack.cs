using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class inherits from the NPCStateMachine. When the NPC enters Attack state, it will turn to face the Zombie with the LookAt method. 
/// This is just a check to make sure the NPC will fire the gun directly at the Zombie. The class calls the Firing and FireGun method from
/// the FriendlyAI script to begin attacking the Zombie. There is a distance check between the NPC and Zombie to make sure that there is always
/// a gap between them to avoid one gameObject moving thorugh the other. 
/// </summary>
public class Attack : NPCStateMachine
{

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (anima.GetBool("Attack"))
        {
            NPC.transform.LookAt(opponent1.transform.position);

            NPC.GetComponent<FriendlyAI>().Firing();
            NPC.GetComponent<FriendlyAI>().FireGun();


        }

        if (Vector3.Distance(opponent1.transform.position, NPC.transform.position) < 2.0f)
        {
            NPC.transform.Translate(Vector3.back * Time.deltaTime);
        }

        if (Vector3.Distance(opponent1.transform.position, NPC.transform.position) < nPCMoveAccuracy)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }



    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }


}
