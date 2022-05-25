using System;
using System.Collections.Generic;
using System.Text;

namespace Poong.Engine
{
    /// <summary>
    /// Zones of the board that the ball can intersect, not necessarily collide with.
    /// </summary>
    [Flags]
    public enum Boundaries
    {
        None = 0,
        LeftGoal       /**/ = 0b100000,
        LeftPaddle     /**/ = 0b010000,
        TopBoundary    /**/ = 0b001000,
        BottomBoundary /**/ = 0b000100,
        RightPaddle    /**/ = 0b000010,
        RightGoal      /**/ = 0b000001
    }
    /// <summary>
    /// Collisions the ball can make. A collision takes place when the ball first intersects a zone.
    /// </summary>
    public enum Collisions
    {
        None = 0,
        LeftGoal           /**/ = 0b10000000,
        LeftPaddleSide     /**/ = 0b01000000,
        LeftPaddleFace     /**/ = 0b00100000,
        TopBoundary        /**/ = 0b00010000,
        BottomBoundary     /**/ = 0b00001000,
        RightPaddleFace    /**/ = 0b00000100,
        RightPaddleSide    /**/ = 0b00000010,
        RightGoal          /**/ = 0b00000001
    }
    public enum Side
    {
        None = 0,
        Left = 1,
        Right = 2
    }
    public enum GamePhase
    {
        PreGame = 0,
        Playing = 1,
        Ready = 2,
        Endgame = 3
    }
    public static class Cooldowns
    {
        public static float Goal = 4.0f;
        public static float Pregame = 8.0f;
        public static float Endgame = 8.0f;
    }
}
