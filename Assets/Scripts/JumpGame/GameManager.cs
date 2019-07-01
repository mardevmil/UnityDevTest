namespace JumpGame
{
    using mardevmil.Core;
    using System;
    using UnityEngine;
    using UnityStandardAssets.Cameras;

    public class GameManager : Singleton<GameManager>
    {
        #region vars
        [SerializeField]
        private Transform _startPoint;
        [SerializeField]
        private LevelData _levelData;
        [SerializeField]
        private GameManagerData _gameManagerData;

        [SerializeField]
        private AutoCam _autoCam;
        
        [HideInInspector]
        public BlockController currentBlockController;
        [HideInInspector]
        public PlayerController playerController;

        private ObjectPooler _blocksPool;
        private ObjectPooler _groundPool;
        private ObjectPooler _bottomPool;

        Vector3 segmentSpawnPos = Vector3.zero;
        private int _currentSegmentIndex = 0;
        private BlockController _previousBlock;
        private BlockController _firstSpawedBlock;
        
        #endregion

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

        #region Mono
        private void Start()
        {
            _groundPool = new ObjectPooler(_levelData.groundPrefab, 10);
            _blocksPool = new ObjectPooler(_levelData.blockPrefab, 20);
            _bottomPool = new ObjectPooler(_levelData.bottomPrefab, 20);

            EventManager.playerLandedOnBlock += OnPlayerLandedOnBlock;
            EventManager.playerPassEndOfSegment += OnPlayerPassEndOfSegment;
            EventManager.blockFellOnGround += OnBlockFellOnGround;
            Initialization();
        }

        private void OnDestroy()
        {
            EventManager.playerLandedOnBlock -= OnPlayerLandedOnBlock;
            EventManager.playerPassEndOfSegment -= OnPlayerPassEndOfSegment;
            EventManager.blockFellOnGround -= OnBlockFellOnGround;
        }
        #endregion

        #region Listeners
        private void OnPlayerLandedOnBlock(BlockController blockController)
        {
            currentBlockController = blockController;
        }

        private void OnPlayerPassEndOfSegment()
        {            
            if (_currentSegmentIndex < _levelData.segments.Length)
            {
                SpawnSegment(_levelData.segments[_currentSegmentIndex]);
                _currentSegmentIndex++;
            }
        }
        private void OnBlockFellOnGround(BlockController blockController)
        {
            _bottomPool.Release(blockController.bottom);
            if (blockController.isLastInSegment)            
                _groundPool.Release(blockController.ground);

            blockController.ResetValues();
            _blocksPool.Release(blockController.gameObject);
        }
        #endregion

        #region Misc

        public void Initialization()
        {
            if (_levelData.segments.Length == 0) return;
            
            for (int i = 0; i < 2; i++)
            {
                SpawnSegment(_levelData.segments[i]);
                _currentSegmentIndex++;
            }

            var playerPos = Vector3.zero;
            if (_firstSpawedBlock != null)
                playerPos = _firstSpawedBlock.SpawnPoint.position;
            
            var player = Instantiate(_levelData.playerPrefab, playerPos, Quaternion.identity, _startPoint);
            player.name = "Player";

            playerController = player.GetComponent<PlayerController>();

            if (_autoCam != null)
                _autoCam.SetTarget(player.transform);
        }

        private void SpawnSegment(SegmentData segmentData)
        {
            //Debug.LogError("*** segment " + segmentData.id);
            var ground = _groundPool.Get();
            ground.transform.position = segmentSpawnPos;
            ground.transform.SetParent(_startPoint);
            ground.name = "Ground[" + segmentData.id + "]";

            var groundScale = ground.transform.localScale;
            groundScale.z = segmentData.blocks.Length * _levelData.offsetZ;
            var halfOfFirst = segmentData.blocks[0].width / 2f;
            var halfOfLast = segmentData.blocks[segmentData.blocks.Length - 1].width / 2f;
            groundScale.z += halfOfFirst;
            groundScale.z += halfOfLast;
            groundScale.x = 120f;
            ground.transform.localScale = groundScale;

            var groundPos = ground.transform.position;
            groundPos.z += ground.transform.localScale.z / 2f - halfOfFirst;
            groundPos.y -= 15f;
            ground.transform.position = groundPos;
            
            for (int j = 0; j < segmentData.blocks.Length; j++)
            {
                var bottom = _bottomPool.Get();
                bottom.transform.position = segmentSpawnPos;
                bottom.transform.SetParent(_startPoint);
                var tmpScale = bottom.transform.localScale;
                tmpScale.x = 4f;
                tmpScale.z = 4f;
                bottom.transform.localScale = tmpScale;
                bottom.name = "Bottom_[" + segmentData.id + "]_[" + segmentData.blocks[j].id + "]";

                var bottomPos = bottom.transform.position;
                bottomPos.z += j * _levelData.offsetZ;
                bottom.transform.position = bottomPos;

                var block = _blocksPool.Get();
                var scale = block.transform.localScale;
                scale.x = segmentData.blocks[j].width;
                scale.z = segmentData.blocks[j].width;
                scale.y = segmentData.blocks[j].height;
                block.transform.localScale = scale;
                
                var tmpPos = new Vector3(segmentSpawnPos.x, segmentSpawnPos.y, segmentSpawnPos.z);
                tmpPos.z += j * _levelData.offsetZ;
                //tmpPos.y += 0.12f;
                block.transform.position = tmpPos;
                block.transform.SetParent(_startPoint);
                block.name = "Block_[" + segmentData.id + "]_[" + segmentData.blocks[j].id + "]";


                var meshRenderer = block.GetComponentInChildren<MeshRenderer>();
                if (meshRenderer != null)
                    meshRenderer.material.color = segmentData.blocks[j].color;

                var blockController = block.GetComponent<BlockController>();
                if (blockController)
                {
                    if (segmentData.id == 0 && segmentData.blocks[j].id == 0)
                        _firstSpawedBlock = blockController;

                    blockController.ground = ground;
                    blockController.bottom = bottom;
                    blockController.data = segmentData.blocks[j];
                    blockController.isLastInSegment = j == segmentData.blocks.Length - 1;                    
                }

                if (_previousBlock != null)
                    _previousBlock.nextBlock = blockController;

                _previousBlock = blockController;
                
                if (j == segmentData.blocks.Length - 1)
                {
                    segmentSpawnPos = bottom.transform.position;
                    segmentSpawnPos.z += _levelData.offsetZ;
                }
            }                      
        }
        
        /// <summary>
        /// Calculation velocity with given target and angle
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

        #endregion


    }
}

