﻿namespace JumpGame
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "LevelData", menuName = "Config/LevelData", order = 1)]
    public class LevelData : ScriptableObject
    {
        public GameObject groundPrefab;
        public GameObject blockPrefab;
        public GameObject playerPrefab;

        [Space, Range(0.5f, 100f)]
        public float offsetX = 0f;
        [Range(0.5f, 100f)]
        public float offsetZ = 0f;

        [Space]
        public SegmentData[] segments;


        private Color[] _colors = new Color[3]
        {
            Color.blue,
            Color.yellow,
            Color.red
        };

        [ContextMenu("Generate Random Blocks")]
        public void GenerateRandom()
        {
            int segmentLength = 5;
            int length = 5;

            segments = new SegmentData[segmentLength];
            for (int i = 0; i < segmentLength; i++)
            {
                var blocks = new BlockData[length];
                for (int j = 0; j < length; j++)
                {
                    var blockData = new BlockData()
                    {
                        id = j,
                        height = Random.Range(10f, 12f),
                        width = Random.Range(2f, 4f),
                        color = _colors[Random.Range(0,3)]                        
                    };

                    blocks[j] = blockData;
                }

                var segmentData = new SegmentData()
                {
                    id = i,
                    difficulty = Random.Range(0, 3),
                    blocks = blocks
                };

                segments[i] = segmentData;
            }

        }
    }
}
