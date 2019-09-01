namespace JumpGame
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayerStateMachineBehaviour : StateMachineBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {            
            if(stateInfo.IsName("Jumpping"))
            {                
                var controller = animator.GetComponent<PlayerController>();
                if(controller != null)
                {
                    //Debug.LogError("+++ controller.Jump");
                    controller.Jump();
                }
            }
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }

        override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }

        override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }
    }
}
