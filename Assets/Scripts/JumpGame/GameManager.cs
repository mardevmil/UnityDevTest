namespace JumpGame
{
    using System;
    using UnityEngine;
    using UnityStandardAssets.Cameras;

    public class GameManager : Singleton<GameManager>
    {
        [SerializeField]
        private Transform _startPoint;
        [SerializeField]
        private LevelData _levelData;
        [SerializeField]
        private GameManagerData _gameManagerData;

        [SerializeField]
        private AutoCam _autoCam;

        private BlockController[] _blockControllers;

        [HideInInspector]
        public BlockController currentBlockController;
        [HideInInspector]
        public PlayerController playerController;

        private void Start()
        {
            if (_levelData.segments.Length == 0) return;

            int blockCount = 0; int blockControllerIndex = 0;
            for (int i = 0; i < _levelData.segments.Length; i++)            
                for (int j = 0; j < _levelData.segments[i].blocks.Length; j++)                
                    blockCount++;
                            
            _blockControllers = new BlockController[blockCount];
            var segmentSpawnPos = Vector3.zero;

            for (int i = 0; i < _levelData.segments.Length; i++)
            {
                var ground = Instantiate(_levelData.groundPrefab, segmentSpawnPos, Quaternion.identity, _startPoint);
                ground.name = "Ground";

                var groundScale = ground.transform.localScale;
                groundScale.z = _levelData.segments[i].blocks.Length * _levelData.offsetZ;
                var halfOfFirst = _levelData.segments[i].blocks[0].width / 2f;
                var halfOfLast = _levelData.segments[i].blocks[_levelData.segments[i].blocks.Length - 1].width / 2f;
                groundScale.z += halfOfFirst;
                groundScale.z += halfOfLast;
                ground.transform.localScale = groundScale;

                var groundPos = ground.transform.position;
                groundPos.z += ground.transform.localScale.z / 2f - halfOfFirst;
                ground.transform.position = groundPos;

                for (int j = 0; j < _levelData.segments[i].blocks.Length; j++)
                {
                    var block = Instantiate(_levelData.blockPrefab, segmentSpawnPos, Quaternion.identity, _startPoint);
                    block.name = "Block_[" + _levelData.segments[i].id + "]_[" + _levelData.segments[i].blocks[j].id + "]";

                    var scale = block.transform.localScale;
                    scale.x = _levelData.segments[i].blocks[j].width;
                    scale.z = _levelData.segments[i].blocks[j].width;
                    scale.y = _levelData.segments[i].blocks[j].height;
                    block.transform.localScale = scale;

                    var meshRenderer = block.GetComponentInChildren<MeshRenderer>();
                    if(meshRenderer != null)                
                        meshRenderer.material.color = _levelData.segments[i].blocks[j].color;                

                    var tmpPos = block.transform.position;
                    tmpPos.z += j * _levelData.offsetZ;
                    block.transform.position = tmpPos;

                    _blockControllers[blockControllerIndex] = block.GetComponent<BlockController>();

                    if (!(_levelData.segments[i].id == 0 && _levelData.segments[i].blocks[j].id == 0))                    
                        _blockControllers[blockControllerIndex - 1].nextBlock = _blockControllers[blockControllerIndex];
                    
                    blockControllerIndex++;

                    if(j == _levelData.segments[i].blocks.Length - 1)
                    {
                        segmentSpawnPos = block.transform.position;
                        segmentSpawnPos.z += _levelData.offsetZ;
                    }
                }
            }

            
            var playerPos = _blockControllers[0].SpawnPoint.position;
            var player = Instantiate(_levelData.playerPrefab, playerPos, Quaternion.identity, _startPoint);
            player.name = "Player";

            playerController = player.GetComponent<PlayerController>();

            if (_autoCam != null)
                _autoCam.SetTarget(player.transform);
        }

        #region Variables for calculating jump velocity
        private Vector3 _direction = Vector3.zero;
        private float _heightDiff = 0f;
        private float _distance = 0f;
        private float _correctedAngle = 0f;
        private float _angleRadians = 0f;
        private float _velocity = 0f;
        private float _correctedVelocity = 0f;
        private float _velocityCorrectionStep = 0f;
        private string _jumpDebugStr = "";        
        #endregion

        /// <summary>
        /// Calculation  velocity with given target and angle
        /// </summary>
        /// <param name="target"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public Vector3 CalculateVelocity(Transform target, float angle)
        {
            _direction = target.position - playerController.transform.position;  // get target direction
            _heightDiff = _direction.y;  // get height difference
            _direction.y = 0;  // retain only the horizontal direction
            _distance = _direction.magnitude;  // get horizontal distance

            // clamp angle if higher than max or lower than min
            _correctedAngle = angle;
            if (_correctedAngle >= _gameManagerData.JumpMaxAngle)
                _correctedAngle = _gameManagerData.JumpMaxAngle;

            if (_correctedAngle <= _gameManagerData.JumpMinAngle)
                _correctedAngle = _gameManagerData.JumpMinAngle;


            _angleRadians = _correctedAngle * Mathf.Deg2Rad;  // convert angle to radians
            _direction.y = _distance * Mathf.Tan(_angleRadians);  // set dir to the elevation angle
            _distance += _heightDiff / Mathf.Tan(_angleRadians);  // correct for small height differences

            // calculate the velocity magnitude
            _velocity = Mathf.Sqrt(_distance * Physics.gravity.magnitude / Mathf.Sin(2 * _angleRadians));

            _correctedVelocity = _velocity;
            _velocityCorrectionStep = 0.03f + (0.11f * Math.Abs(_heightDiff));

            if (angle < _gameManagerData.OptimalAnglesTopLimit && angle > _gameManagerData.OptimalAnglesBottomLimit)
                _jumpDebugStr = "Optimal Angle";

            // correct velocity for overjump
            if (angle < _gameManagerData.OverjumpAnglesTopLimit && angle > _gameManagerData.OverjumpAnglesBottomLimit)
            {
                _correctedVelocity = _correctedVelocity + (angle - _gameManagerData.OverjumpAnglesBottomLimit) * _velocityCorrectionStep;
                _jumpDebugStr = "Over Angle";
            }

            // correct velocity for underjump
            if (angle < _gameManagerData.UnderjumpAnglesTopLimit && angle > _gameManagerData.UnderjumpAnglesBottomLimit)
            {
                _correctedVelocity = _correctedVelocity - (angle - _gameManagerData.UnderjumpAnglesBottomLimit) * _velocityCorrectionStep / 2f;
                _jumpDebugStr = "Under Angle";
            }

            // limit velocity if angle too high
            if (angle > _gameManagerData.OptimalAnglesTopLimit)
            {
                _correctedVelocity = _gameManagerData.VelocityNormalLimit;
                _jumpDebugStr = "Too High Angle";
            }

            _jumpDebugStr += ": " + (int)angle + " degrees";                    

            if (_correctedVelocity > 20f) // just in case of buug with wrong block reference, it's fixed but...
                _correctedVelocity = 3f;

            return _correctedVelocity * _direction.normalized;
        }
    }
}

