using System.Collections.Generic;
public enum Team { None, Team1, Team2, Team3, Team4 }
public enum GameStates { PLAYER1TURN = 1, PLAYER2TURN = 2, PLAYER3TURN = 3, PLAYER4TURN = 4, WIN }
public class Constants
{
    public const float SPEEDOFGAME = 0.35f;

    public const string REGULAR = "REGULAR GRID";
    public const string CUSTOM = "CUSTOM GRID";
    public const string PLAYER = "PLAYER";

    public static readonly Dictionary<Team, GameStates> TeamsDictionary = new()
    {
        { Team.Team1, GameStates.PLAYER1TURN },
        { Team.Team2, GameStates.PLAYER2TURN },
        { Team.Team3, GameStates.PLAYER3TURN },
        { Team.Team4, GameStates.PLAYER4TURN }
    };
}
