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

        private BlockController[] _blockControllers;
        private int _blockControllerIndex = 0;

        [HideInInspector]
        public BlockController currentBlockController;
        [HideInInspector]
        public PlayerController playerController;

        private ObjectPooler _blocksPool;
        private ObjectPooler _groundPool;
        Vector3 segmentSpawnPos = Vector3.zero;
        private int _currentSegmentIndex = 0;
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
            _groundPool = new ObjectPooler(_levelData.groundPrefab, _levelData.segments.Length);
            _blocksPool = new ObjectPooler(_levelData.blockPrefab, _levelData.segments.Length * _levelData.segments[0].blocks.Length);
            EventManager.playerLandedOnBlock += OnPlayerLandedOnBlock;
            EventManager.playerPassEndOfSegment += OnPlayerPassEndOfSegment;
            Initialization();
        }

        private void OnDestroy()
        {
            EventManager.playerLandedOnBlock -= OnPlayerLandedOnBlock;
            EventManager.playerPassEndOfSegment -= OnPlayerPassEndOfSegment;
        }
        #endregion

        #region Listeners
        private void OnPlayerLandedOnBlock(BlockController blockController)
        {
            currentBlockController = blockController;
        }

        private void OnPlayerPassEndOfSegment()
        {
            Debug.LogError("+++ OnPlayerPassEndOfSegment");
            if(_currentSegmentIndex < _levelData.segments.Length - 1)
                SpawnSegment(_levelData.segments[_currentSegmentIndex++]);
        }
        #endregion

        #region Misc

        public void Initialization()
        {
            if (_levelData.segments.Length == 0) return;

            int blockCount = 0;
            for (int i = 0; i < _levelData.segments.Length; i++)
                for (int j = 0; j < _levelData.segments[i].blocks.Length; j++)
                    blockCount++;

            _blockControllers = new BlockController[blockCount];            

            for (int i = 0; i < 2; i++)
            {
                SpawnSegment(_levelData.segments[i]);
                _currentSegmentIndex++;
            }

            var playerPos = _blockControllers[0].SpawnPoint.position;
            var player = Instantiate(_levelData.playerPrefab, playerPos, Quaternion.identity, _startPoint);
            player.name = "Player";

            playerController = player.GetComponent<PlayerController>();

            if (_autoCam != null)
                _autoCam.SetTarget(player.transform);
        }

        private void SpawnSegment(SegmentData segmentData)
        {
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
            ground.transform.localScale = groundScale;

            var groundPos = ground.transform.position;
            groundPos.z += ground.transform.localScale.z / 2f - halfOfFirst;
            ground.transform.position = groundPos;

            for (int j = 0; j < segmentData.blocks.Length; j++)
            {
                var block = _blocksPool.Get();
                block.transform.position = segmentSpawnPos;
                block.transform.SetParent(_startPoint);
                block.name = "Block_[" + segmentData.id + "]_[" + segmentData.blocks[j].id + "]";

                var scale = block.transform.localScale;
                scale.x = segmentData.blocks[j].width;
                scale.z = segmentData.blocks[j].width;
                scale.y = segmentData.blocks[j].height;
                block.transform.localScale = scale;

                var meshRenderer = block.GetComponentInChildren<MeshRenderer>();
                if (meshRenderer != null)
                    meshRenderer.material.color = segmentData.blocks[j].color;

                var tmpPos = block.transform.position;
                tmpPos.z += j * _levelData.offsetZ;
                block.transform.position = tmpPos;

                var blockController = block.GetComponent<BlockController>();
                if (blockController)
                {
                    blockController.data = segmentData.blocks[j];
                    blockController.isLastInSegment = j == segmentData.blocks.Length - 1;
                    _blockControllers[_blockControllerIndex] = blockController;
                }

                if (!(segmentData.id == 0 && segmentData.blocks[j].id == 0))
                    _blockControllers[_blockControllerIndex - 1].nextBlock = _blockControllers[_blockControllerIndex];

                _blockControllerIndex++;

                if (j == segmentData.blocks.Length - 1)
                {
                    segmentSpawnPos = block.transform.position;
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

