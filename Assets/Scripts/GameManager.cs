namespace JumpGame
{
    using mardevmil.Core;
    using System;
    using UnityEngine;
    using UnityStandardAssets.Cameras;

    public class GameManager : Singleton<GameManager>
    {
        #region vars

        public enum GameStatus
        {
            OnStart,
            Play,
            Paused,
        }

        [SerializeField]
        private Transform _startPoint;
        [SerializeField]
        private LevelData _levelData;
        [SerializeField]
        private GameManagerData _gameManagerData;
        
        public GameManagerData Data { get => _gameManagerData; }

        [HideInInspector]
        public GameStatus gameStatus = GameStatus.OnStart;

        [HideInInspector]
        public BlockController currentBlockController;
        [HideInInspector]
        public PlayerController playerController;
        [HideInInspector]
        public CameraController cameraController;

        private ObjectPooler _blocksPool;
        private ObjectPooler _groundPool;
        private ObjectPooler _bottomPool;

        Vector3 segmentSpawnPos = Vector3.zero;
        private int _currentSegmentIndex = 0;
        private int _blockCounter = 0;
        private BlockController _previousBlock;
        private BlockController _firstSpawedBlock;
        private BlockController _nextBlockController;
        
        public int Counter { get => _blockCounter; }

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
            if (GuiManager.Instance == null)
            {
                var guiManager = Instantiate(_gameManagerData.GuiManagerPrefab);
                guiManager.name = "GuiManager[InGame Init]";
            }

            if (gameStatus == GameStatus.OnStart)
                GuiManager.Instance.ShowWindow("UI_IN_GAME");

            _groundPool = new ObjectPooler(_levelData.groundPrefab, 10);
            _blocksPool = new ObjectPooler(_levelData.blockPrefab, 20);
            _bottomPool = new ObjectPooler(_levelData.bottomPrefab, 20);

            EventManager.gameStarted += OnGameStarted;
            EventManager.playerLandedOnBlock += OnPlayerLandedOnBlock;
            EventManager.playerPassEndOfSegment += OnPlayerPassEndOfSegment;
            EventManager.blockFellOnGround += OnBlockFellOnGround;
            EventManager.playerDeath += OnPlayerDeath;
            Initialization();
        }

        private void OnDestroy()
        {
            EventManager.gameStarted -= OnGameStarted;
            EventManager.playerLandedOnBlock -= OnPlayerLandedOnBlock;
            EventManager.playerPassEndOfSegment -= OnPlayerPassEndOfSegment;
            EventManager.blockFellOnGround -= OnBlockFellOnGround;
            EventManager.playerDeath -= OnPlayerDeath;
        }
        #endregion

        #region Listeners
        private void OnGameStarted()
        {
            if (playerController != null)
                playerController.Release();
        }

        private void OnPlayerLandedOnBlock(BlockController blockController)
        {
            currentBlockController = blockController;
            _nextBlockController = currentBlockController.nextBlock;
            _blockCounter++;
            Time.timeScale += _blockCounter * 0.01f;            
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

        private void OnPlayerDeath()
        {
            Time.timeScale = 0f;
            GuiManager.Instance.ShowWindow("UI_DEATH");

        }
        #endregion

        #region Misc

        public void Revive()
        {
            Time.timeScale = 1f;
            _blockCounter = 0;
            var spawnPoint = _nextBlockController.SpawnPoint;
            currentBlockController = _nextBlockController;
            playerController.ResetPhysic();
            playerController.transform.position = spawnPoint.position;
            GuiManager.Instance.ShowWindow("UI_IN_GAME");
        }

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

            cameraController = Camera.main.GetComponent<CameraController>();
            if(cameraController != null)            
                cameraController.Init();                        
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
            groundScale.x = 200f;
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
        
        #endregion


    }
}

