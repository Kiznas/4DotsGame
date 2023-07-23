using EventHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum GameStates { START, PLAYER1TURN = 1, PLAYER2TURN = 2, PLAYER3TURN = 3, PLAYER4TURN = 4, WIN }

public class PlayersTurnSystem : MonoBehaviour
{
    [SerializeField] private Button _randomPlayerButton;
    [SerializeField] private GameManagerScript _gameManager;

    [SerializeField] private Toggle _player1BotToggle; 
    [SerializeField] private Toggle _player2BotToggle; 
    [SerializeField] private Toggle _player3BotToggle; 
    [SerializeField] private Toggle _player4BotToggle; 

    private List<string> _players;
    private Dictionary<string, GameStates> states;
    private Dictionary<string, Toggle> toggles;
    private Dictionary<GameStates, Team> teamsDictionary;

    public GameStates GameState;

    private void Awake()
    {
        states = new Dictionary<string, GameStates>()
        {
            { "PLAYER1", GameStates.PLAYER1TURN },
            { "PLAYER2", GameStates.PLAYER2TURN },
            { "PLAYER3", GameStates.PLAYER3TURN },
            { "PLAYER4", GameStates.PLAYER4TURN },
        };

        teamsDictionary = new Dictionary<GameStates, Team>()
        {
            { GameStates.PLAYER1TURN, Team.Team1},
            { GameStates.PLAYER2TURN, Team.Team2},
            { GameStates.PLAYER3TURN, Team.Team3},
            { GameStates.PLAYER4TURN, Team.Team4}
        };

        toggles = new Dictionary<string, Toggle>()
        {
            { "PLAYER1", _player1BotToggle},
            { "PLAYER2", _player2BotToggle},
            { "PLAYER3", _player3BotToggle},
            { "PLAYER4", _player4BotToggle},
        };
    }

    private void Start()
    {
        GameState = GameStates.START;
        _randomPlayerButton.onClick.AddListener(RandomStartingPlayer);
        _players = new List<string>();
        EventAggregator.Subscribe<Initialization>(InitializePlayers);
    }

    private void InitializePlayers(object arg1, Initialization arg2)
    {
        for (int i = 0; i < _gameManager.numberOfPlayers; i++)
        {
            _players.Add($"PLAYER{i + 1}");
            toggles[_players[i]].gameObject.SetActive(true);
        }
        _randomPlayerButton.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        _randomPlayerButton.onClick.RemoveAllListeners();
        EventAggregator.Unsubscribe<NextTurn>(ChangeTurn);
        EventAggregator.Unsubscribe<PlayerLost>(PLost);
        EventAggregator.Unsubscribe<Initialization>(InitializePlayers);
    }

    private void RandomStartingPlayer()
    {
        GameState = (GameStates)UnityEngine.Random.Range(1, _gameManager.numberOfPlayers + 1);
        Debug.Log(GameState.ToString());
        EventAggregator.Subscribe<NextTurn>(ChangeTurn);
        EventAggregator.Subscribe<PlayerLost>(PLost);
        List<Team> teams = new List<Team>();
        for (int i = 0; i < _gameManager.numberOfPlayers; i++)
        {
            if (toggles[_players[i]].isOn)
            {
                teams.Add(teamsDictionary[states[_players[i]]]);
            }
            toggles[_players[i]].gameObject.SetActive(false);
        }
        EventAggregator.Post(this, new AddBots { teams = teams });
        _randomPlayerButton.gameObject.SetActive(false);
        EventAggregator.Post(this, new GetTurn { gameState = GameState });
    }

    private void PLost(object arg1, PlayerLost data)
    {
        _players?.Remove(data.PlayerName);

        if (_players.Count <= 1)
        {
            GameState = GameStates.WIN;
            Debug.Log("Game Over - Winner: " + _players[0]);
            Time.timeScale = 0;
        }
        else
        {
            ChangeTurnToNextPlayer();
        }
    }

    private void ChangeTurn(object arg1, NextTurn turnData)
    {
        if (teamsDictionary[GameState] == turnData.cellTeam)
        {
            ChangeTurnToNextPlayer();
        }
    }

    private void ChangeTurnToNextPlayer()
    {
        int currentPlayerIndex = _players.IndexOf(states.FirstOrDefault(x => x.Value == GameState).Key);
        int nextPlayerIndex = (currentPlayerIndex + 1) % _players.Count;

        GameState = states[_players[nextPlayerIndex]];
        EventAggregator.Post(this, new GetTurn { gameState = GameState });
        Debug.Log(GameState.ToString());
    }
}
