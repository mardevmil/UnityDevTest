namespace JumpGame
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "GameManagerData", menuName = "Config/GameManagerData", order = 1)]
    public class GameManagerData : ScriptableObject
    {
        [Header("Jump Max Angle")]
        [Range(40f, 80f)]
        public float JumpMaxAngle = 43f;

        [Header("Jump Min Angle")]
        [Range(40f, 80f)]
        public float JumpMinAngle = 42f;

        [Header("Optimal angles top limit")]
        [Range(40f, 80f)]
        public float OptimalAnglesTopLimit = 70f;

        [Header("Optimal angles bottom limit")]
        [Range(40f, 80f)]
        public float OptimalAnglesBottomLimit = 60f;

        [Header("Overjump angles top limit")]
        [Range(30f, 80f)]
        public float OverjumpAnglesTopLimit = 60f;

        [Header("Overjump angles bottom limit")]
        [Range(30f, 80f)]
        public float OverjumpAnglesBottomLimit = 50f;

        [Header("Underjump angles top limit")]
        [Range(10f, 40f)]
        public float UnderjumpAnglesTopLimit = 40f;

        [Header("Underjump angles bottom limit")]
        [Range(10f, 40f)]
        public float UnderjumpAnglesBottomLimit = 15f;

        [Header("Velocity normal limit")]
        [Range(2f, 8f)]
        public float VelocityNormalLimit = 6f;
    }
}

