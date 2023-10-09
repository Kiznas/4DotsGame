using System.Collections.Generic;

namespace ConstantValues
{
    public abstract class Constants
    {
        public const float SpeedOfGame = 0.35f;

        public const string Regular = "REGULAR GRID";
        public const string Custom = "CUSTOM GRID";
        public const string Player = "PLAYER";

        public static readonly Dictionary<Enums.Team, Enums.GameStates> TeamsDictionary = new()
        {
            { Enums.Team.Team1, Enums.GameStates.Player1Turn },
            { Enums.Team.Team2, Enums.GameStates.Player2Turn },
            { Enums.Team.Team3, Enums.GameStates.Player3Turn },
            { Enums.Team.Team4, Enums.GameStates.Player4Turn }
        };
    }
}
