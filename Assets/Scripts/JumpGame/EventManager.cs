namespace JumpGame
{
    public class EventManager : mardevmil.Core.EventManager
    {
        public delegate void BlockDelegate(BlockController blockController);

        public static VoidDelegate gameStarted = VoidIdle;
        public static VoidDelegate playerPassEndOfSegment = VoidIdle;
        public static BlockDelegate playerLandedOnBlock = BlockIdle;
        public static BlockDelegate blockFellOnGround = BlockIdle;        

        //idle methods
        static void VoidIdle() { }
        static void BlockIdle(BlockController blockController) { }
    }
}
