namespace JumpGame
{
    using UnityEngine;

    public class BlockController : MonoBehaviour
    {
        [SerializeField]
        private Transform _spawnPoint;

        public Transform SpawnPoint => _spawnPoint;

        private Rigidbody _selfRigidbody;
        
        public float Angle { get { return _angle; } }
        public Transform prefectLandingPoint;        

        public BlockData data;
        public BlockController nextBlock;
        public GameObject bottom;
        public GameObject ground;
        public bool isLastInSegment;

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
                //Debug.LogError("****************** OnTriggerEnter " + other.gameObject);
                EventManager.PlayerLandedOnBlock(this);
                if (isLastInSegment)
                {
                    //Debug.LogError("+++++++++++++++ PlayerPassEndOfSegment " + other.gameObject);
                    EventManager.PlayerPassEndOfSegment();                    
                }

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

        public void ResetValues()
        {
            _selfRigidbody.velocity = Vector3.zero;
            _selfRigidbody.angularVelocity = Vector3.zero;            
            transform.rotation = Quaternion.Euler(Vector3.zero);            
            data = default;
            nextBlock = null;
            bottom = null;
            ground = null;
            isLastInSegment = false;
        }       
    }
}
