﻿namespace JumpGame
{
    using UnityEngine;

    public class PlayerController : MonoBehaviour
    {
        public enum PlayerState
        {
            OnSpawnPoint,
            Jumped,
            Landed
        }
        public PlayerState state = PlayerState.OnSpawnPoint;
        public Rigidbody Rigidbody { get; private set; }
        public Animator Animator { get; private set; }

        void Start()
        {
            EventManager.playerLandedOnBlock += OnPlayerLandedOnBlock;

            Rigidbody = GetComponent<Rigidbody>();
            Animator = GetComponent<Animator>();

            if (Rigidbody)
            {
                //_selfRigidbody.isKinematic = false;
                Rigidbody.useGravity = true;
            }
        }

        // Update is called once per frame // temp code until implement Input Controller
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (Rigidbody && state == PlayerState.Landed)
                {
                    //Debug.LogError("***** Animator.SetTrigger Jump");
                    Animator.SetTrigger("Jump");
                }
            }

            if (state == PlayerState.Landed && GameManager.Instance.currentBlockController != null)
                transform.rotation = Quaternion.Lerp(transform.rotation, GameManager.Instance.currentBlockController.transform.rotation, Time.deltaTime);
        }

        private void OnDestroy()
        {
            EventManager.playerLandedOnBlock -= OnPlayerLandedOnBlock;
        }

        private void OnPlayerLandedOnBlock(BlockController blockController)
        {            
            Rigidbody.drag = 2f;            
            state = PlayerState.Landed;
            Animator.SetTrigger("Run");
        }

        public void Release()
        {
            Rigidbody.useGravity = true;
            Rigidbody.isKinematic = false;
        }

        public void ResetPhysic()
        {
            Rigidbody.useGravity = false;
            Rigidbody.isKinematic = true;
        }

        public void Jump()
        {
            if (Rigidbody && state == PlayerState.Landed)
            {
                var currentBlock = GameManager.Instance.currentBlockController;
                var nextBlock = GameManager.Instance.currentBlockController.nextBlock;
                if (currentBlock != null && nextBlock != null)
                {
                    Rigidbody.drag = 0f;
                    var target = nextBlock.prefectLandingPoint;
                    var angle = currentBlock.Angle;
                    Rigidbody.velocity = GameManager.Instance.CalculateVelocity(target, 65f);
                    state = PlayerState.Jumped;                    
                }
            }
        }        
    }
}
