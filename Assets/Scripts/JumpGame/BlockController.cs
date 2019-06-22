namespace JumpGame
{
    using UnityEngine;

    public class BlockController : MonoBehaviour
    {
        [SerializeField]
        private Transform _spawnPoint;

        public Transform SpawnPoint => _spawnPoint;

        private Rigidbody _selfRigidbody;

        [HideInInspector]
        public float Angle { get { return _angle; } }

        //[HideInInspector]
        public BlockController nextBlock;
        public Transform prefectLandingPoint;

        #region Variables for determine angle
        private Vector3 _initVector = Vector3.zero;
        private Vector3 _currentVector = Vector3.zero;
        private float _angle = 0f;
        private bool _jumped = false;
        #endregion

        private void Start()
        {
            _selfRigidbody = GetComponent<Rigidbody>();
            _initVector = transform.TransformDirection(Vector3.up);
            _currentVector = _initVector;
        }

        void Update()
        {            
            _currentVector = transform.TransformDirection(Vector3.up);
            _angle = 90f - Vector3.Angle(_initVector, _currentVector);                        
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                var playerController = other.GetComponent<PlayerController>();
                if(playerController)
                {
                    GameManager.Instance.playerController.Rigidbody.drag = 2f;                    
                }
                GameManager.Instance.currentBlockController = this;
                GameManager.Instance.playerController.state = PlayerController.PlayerState.Landed;
                AddTorque();
            }
        }

        [ContextMenu("Test Torque")]
        private void AddTorque()
        {
            if (_selfRigidbody)
            {
                _selfRigidbody.AddTorque(Vector3.right * 3f, ForceMode.VelocityChange);
            }
        }
    }
}
