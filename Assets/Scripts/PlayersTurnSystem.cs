using TMPro;
using UnityEngine;
using EventHandler;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayersTurnSystem : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button _randomPlayerButton;
    [SerializeField] private Button _restartButton;

    [SerializeField] private Toggle[] _playerBotToggles;

    [SerializeField] private TMP_Text _winText;
    
    [Header("Essentials")]
    [SerializeField] private GameManagerScript _gameManager;
    [SerializeField] private GameObject _winPanel;

    private const string PLAYER = "PLAYER";

    private List<Player> _players;
    private int _currentPlayerIndex;

    public GameStates GameState;

    private void Start()
    {
        _randomPlayerButton.onClick.AddListener(RandomStartingPlayer);
        _players = new List<Player>();
        EventAggregator.Subscribe<Initialization>(InitializePlayers);
    }

    private void OnDestroy()
    {
        _randomPlayerButton.onClick.RemoveAllListeners();
        EventAggregator.Unsubscribe<NextTurn>(ChangeTurn);
        EventAggregator.Unsubscribe<PlayerLost>(PLost);
        EventAggregator.Unsubscribe<Initialization>(InitializePlayers);
    }

    private void InitializePlayers(object arg1, Initialization data)
    {
        int numberOfPlayers = _gameManager.numberOfPlayers;
        _players.Clear();

        for (int i = 1; i < numberOfPlayers + 1; i++)
        {
            _players.Add(new Player(PLAYER + i, (Team)i, (GameStates)i, data.teamsColorList[i - 1]));
            _playerBotToggles[i - 1].gameObject.SetActive(true);
        }

        _randomPlayerButton.gameObject.SetActive(true);
    }

    private void RandomStartingPlayer()
    {
        int numberOfPlayers = _gameManager.numberOfPlayers;
        GameState = (GameStates)Random.Range(1, numberOfPlayers + 1);

        EventAggregator.Subscribe<NextTurn>(ChangeTurn);
        EventAggregator.Subscribe<PlayerLost>(PLost);

        List<Team> teams = new();

        for (int i = 0; i < numberOfPlayers; i++)
        {
            if (_playerBotToggles[i].isOn)
            {
                teams.Add(_players[i].Team);
            }

            _playerBotToggles[i].gameObject.SetActive(false);
        }

        EventAggregator.Post(this, new AddBots { teams = teams });
        EventAggregator.Post(this, new GetTurn { gameState = GameState });

        _currentPlayerIndex = _players.FindIndex(player => player.GameState == GameState);

        _randomPlayerButton.gameObject.SetActive(false);
        _restartButton.gameObject.SetActive(true);
    }

    private void ChangeTurn(object arg1, NextTurn turnData)
    {
        if (GameState != GameStates.WIN)
        {
            ChangeTurnToNextPlayer();
        }
    }

    private void PLost(object arg1, PlayerLost data)
    {
        int currentPlayerIndex = _players.FindIndex(player => player.Name == data.PlayerName);
        if (currentPlayerIndex != -1)
        {
            _players.RemoveAt(currentPlayerIndex);
        }

        if (_players.Count <= 1)
        {
            GameState = GameStates.WIN;
            _winPanel.SetActive(true);
            Color teamColor =  _players[0].TeamColor;
            _winText.text = " WINNER: " + _players[0].Name;
            _winText.color = teamColor;
            Time.timeScale = 0;
        }
    }

    private void ChangeTurnToNextPlayer()
    {
        int nextPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
        while (_players[nextPlayerIndex].GameState == GameStates.WIN)
        {
            nextPlayerIndex = (nextPlayerIndex + 1) % _players.Count;
        }

        _currentPlayerIndex = nextPlayerIndex;
        GameState = _players[_currentPlayerIndex].GameState;

        EventAggregator.Post(this, new GetTurn { gameState = GameState });
    }
}

public struct Player
{
    public string Name;
    public Team Team;
    public GameStates GameState;
    public Color TeamColor;

    public Player(string name, Team team, GameStates gameState, Color color) : this()
    {
        Name = name;
        Team = team;
        GameState = gameState;
        TeamColor = color;
    }
}
