using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Chase state sets the destination of the NPC to the Zombie's location. There is a distance check between the NPC and Zombie which makes sure
/// the NPC stops moving when it gets to close to it to avoid one gameObject moving through the other.
/// </summary>
public class ChaseNew : NPCStateMachine
{

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {



        agent.SetDestination(opponent1.transform.position);



        if (Vector3.Distance(opponent1.transform.position, NPC.transform.position) < 3.0f)
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
