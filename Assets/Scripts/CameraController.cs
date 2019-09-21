namespace JumpGame
{
    using UnityEngine;

    public class CameraController : MonoBehaviour
    {
        private Camera _camera;
        private PlayerController _playerController;

        private Transform _cameraTransform;
        private Transform _playerTransform;

        [SerializeField, Range(1f, 100f)]
        private float _distance = 5f;
        [SerializeField, Range(1f, 100f)]
        private float _height = 5f;
        [SerializeField, Range(1f, 100f)]
        private float _minHeight = 5f;
        
        private Vector3 _movePos;
        private bool _isInitialized = false;

        // Update is called once per frame
        void Update()
        {
            if(_isInitialized && _playerTransform.position.y > _minHeight)
            {                
                _movePos = _cameraTransform.position;
                _movePos.y = _playerTransform.position.y + _height;
                _movePos.z = _playerTransform.position.z - _distance;
                _cameraTransform.position = _movePos;
                _cameraTransform.rotation = Quaternion.LookRotation(_playerTransform.position - _cameraTransform.position, Vector3.up);
            }
        }

        public void Init()
        {
            _camera = GetComponent<Camera>();

            if (_camera != null)
                _cameraTransform = _camera.transform;

            _playerController = FindObjectOfType<PlayerController>();
            if (_playerController != null)
                _playerTransform = _playerController.transform;

            if(_cameraTransform != null && _playerTransform != null)
            {
                _cameraTransform.position = new Vector3(_playerTransform.position.x, _playerTransform.position.y + _height, _playerTransform.position.z - _distance);
                _cameraTransform.rotation = Quaternion.LookRotation(_playerTransform.position - _cameraTransform.position, Vector3.up);
                _isInitialized = true;
            }
        }
    }

}