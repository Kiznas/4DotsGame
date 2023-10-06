using ConstantValues;
using UnityEngine;

namespace Game_Managing
{
    public struct Player
    {
        public readonly string Name;
        public readonly Enums.Team Team;
        public readonly Enums.GameStates GameState;
        public Color TeamColor;

        public Player(string name, Enums.Team team, Enums.GameStates gameState, Color color) : this()
        {
            Name = name;
            Team = team;
            GameState = gameState;
            TeamColor = color;
        }
    }
}