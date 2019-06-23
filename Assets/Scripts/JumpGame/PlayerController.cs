namespace JumpGame
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

        // Start is called before the first frame update
        void Start()
        {
            EventManager.playerLandedOnBlock += OnPlayerLandedOnBlock;

            Rigidbody = GetComponent<Rigidbody>();
            if(Rigidbody)
            {
                //_selfRigidbody.isKinematic = false;
                Rigidbody.useGravity = true;
            }
        }

        private void OnDestroy()
        {
            EventManager.playerLandedOnBlock -= OnPlayerLandedOnBlock;
        }

        private void OnPlayerLandedOnBlock(BlockController blockController)
        {            
            Rigidbody.drag = 2f;            
            state = PlayerState.Landed;
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKey(KeyCode.Mouse0))
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
}
