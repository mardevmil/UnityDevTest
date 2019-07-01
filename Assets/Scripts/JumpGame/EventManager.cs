namespace JumpGame
{
    public class EventManager : mardevmil.Core.EventManager
    {
        public delegate void BlockDelegate(BlockController blockController);

        public static VoidDelegate playerPassEndOfSegment;
        public static BlockDelegate playerLandedOnBlock;
        public static BlockDelegate blockFellOnGround;
                
        public static void PlayerPassEndOfSegment() { playerPassEndOfSegment?.Invoke(); }
        public static void PlayerLandedOnBlock(BlockController blockController) { playerLandedOnBlock?.Invoke(blockController); }
        public static void BlockFellOnGround(BlockController blockController) { blockFellOnGround?.Invoke(blockController); }

    }
}
