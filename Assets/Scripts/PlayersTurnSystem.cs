using EventHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum GameStates { START, PLAYER1TURN = 1, PLAYER2TURN= 2, PLAYER3TURN = 3, PLAYER4TURN = 4, WIN }
public class PlayersTurnSystem : MonoBehaviour
{
    [SerializeField] private Button _randomPlayerButton;
    [SerializeField] private GameManagerScript _gameManager;

    List<string> _players;

    public GameStates GameState;
    private int _currentIndex = 1;

    private static TeamClass _team1 = new TeamClass(Team.Team1);
    private static TeamClass _team2 = new TeamClass(Team.Team2);
    private static TeamClass _team3 = new TeamClass(Team.Team3);
    private static TeamClass _team4 = new TeamClass(Team.Team4);

    private Dictionary<string, GameStates> states = new Dictionary<string, GameStates>()
    {
        { "PLAYER1", GameStates.PLAYER1TURN },
        { "PLAYER2", GameStates.PLAYER2TURN },
        { "PLAYER3", GameStates.PLAYER3TURN },
        { "PLAYER4", GameStates.PLAYER4TURN },
    };

    private Dictionary<GameStates, TeamClass> teamsDictionary = new Dictionary<GameStates, TeamClass>()
    {
        { GameStates.PLAYER1TURN, _team1},
        { GameStates.PLAYER2TURN, _team2},
        { GameStates.PLAYER3TURN, _team3},
        { GameStates.PLAYER4TURN, _team4}
    };


    void Start()
    {
        GameState = GameStates.START;
        _randomPlayerButton.onClick.AddListener(RandomStartingPlayer);
        _players = new List<string>();
}
    private void RandomStartingPlayer()
    {
        for (int i = 0; i < _gameManager.NumberOfPlayers; i++)
        {
            _players.Add($"PLAYER{i + 1}");
        }
        GameState = (GameStates)UnityEngine.Random.Range(1, _gameManager.NumberOfPlayers);
        _currentIndex = (int)GameState;
        Debug.Log(GameState.ToString());
        EventAggregator.Post(this, new GetTurn { gameState = GameState });
        EventAggregator.Subscribe<NextTurn>(ChangeTurn);
        EventAggregator.Subscribe<PlayerLost>(PLost);
    }

    private void PLost(object arg1, PlayerLost data)
    {
        _players?.Remove(data.PlayerName);
    }

    private void ChangeTurn(object arg1, NextTurn turnData)
    {
        if (teamsDictionary[GameState].Team == turnData.cellTeam)
        {
            if (GameState != states[_players.Last()])
            {
                GameState = states[_players.ElementAt(_currentIndex++)];
                EventAggregator.Post(this, new GetTurn { gameState = GameState });
            }
            else if (GameState == states[_players.Last()])
            {
                _currentIndex = 1;
                GameState = states[_players.First()];
                EventAggregator.Post(this, new GetTurn { gameState = GameState });
            }
            Debug.Log(GameState.ToString());
        }
    }
}

public class TeamClass
{
    private Team _team;
    private bool _alive = true;
    public bool Alive { get { return _alive; } set { _alive = value; } }
    public Team Team { get { return _team; } set { _team = value; } }

    public TeamClass(Team team)
    {
        _team = team;
    }
}
