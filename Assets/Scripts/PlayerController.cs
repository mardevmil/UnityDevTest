namespace JumpGame
{
    using mardevmil.Core;
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
                    if (Animator != null)
                        Animator.SetTrigger("Jump");
                    else
                        Jump();
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
            if(Animator != null) Animator.SetTrigger("Run");
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
                    var blockAngle = currentBlock.Angle;
                    var jumpAngle = Random.Range(GameManager.Instance.Data.JumpMinAngle, GameManager.Instance.Data.JumpMaxAngle);
                    var vel = PhysicMath.CalculateVelocity(transform, target, jumpAngle);

                    // correct velocity for over jump
                    if (blockAngle < GameManager.Instance.Data.OverjumpAnglesTopLimit && blockAngle > GameManager.Instance.Data.OverjumpAnglesBottomLimit)
                    {
                        Debug.LogError("+++ over jump");
                        vel = vel * 1.05f;
                    }


                    // correct velocity for under jump
                    if (blockAngle < GameManager.Instance.Data.UnderjumpAnglesTopLimit && blockAngle > GameManager.Instance.Data.UnderjumpAnglesBottomLimit)
                    {
                        Debug.LogError("+++ under jump");
                        vel = vel * 0.9f;
                    }

                    // limit velocity if angle too high
                    if (blockAngle > GameManager.Instance.Data.OptimalAnglesTopLimit)
                    {
                        Debug.LogError("+++ too high jump");
                        vel = vel * 0.9f;
                    }

                    Rigidbody.velocity = vel;

                    state = PlayerState.Jumped;                    
                }
            }
        }        
    }
}
