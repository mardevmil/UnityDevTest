namespace JumpGame
{
    using UnityEngine;

    public class GameDefinitions
    {
        
    }

    [System.Serializable]
    public struct SegmentData
    {
        public int id;        
        public int difficulty;
        public BlockData[] blocks;
    }

    [System.Serializable]
    public struct BlockData
    {
        public int id;
        public float height;
        public float width;
        public Color color;        
    }
}

