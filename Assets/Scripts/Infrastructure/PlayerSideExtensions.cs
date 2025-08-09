using Enums;

namespace Infrastructure
{
    public static class PlayerSideExtensions
    {
        public static PlayerSide GetOpposingPlayer(this PlayerSide player)
        {
            var opposingPlayer = player == PlayerSide.Orange
                    ? PlayerSide.Blue
                    : PlayerSide.Orange;

            return opposingPlayer;
        }
    }
}