namespace JumpGame
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class GroundController : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {            
            var blockController = other.GetComponent<BlockController>();
            if(blockController)            
                EventManager.blockFellOnGround(blockController);
            
            var playerController = other.GetComponent<PlayerController>();
            if (playerController)
                EventManager.playerDeath();
        }
    }
}

