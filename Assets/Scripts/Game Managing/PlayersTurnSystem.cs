using System.Collections.Generic;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game_Managing
{
    public class PlayersTurnSystem : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button randomPlayerButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Toggle[] playerBotToggles;
        [SerializeField] private TMP_Text winText;
        [Header("Essentials")]
        [SerializeField] private GameManage gameManager;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Image backgroundImage;

        private List<Player> _players;
        private int _currentPlayerIndex;
        private Color _prevPlayerColor;

        public Enums.GameStates gameState;
        private readonly int _colorToGradient = Shader.PropertyToID("_colorToGradient");
        private readonly int _prevColorToGradient = Shader.PropertyToID("_prevColorToGradient");
        private readonly int _startTime = Shader.PropertyToID("_startTime");

        private void Start()
        {
            randomPlayerButton.onClick.AddListener(RandomStartingPlayer);
            _players = new List<Player>();
            EventAggregator.Subscribe<Initialization>(InitializePlayers);
        }

        private void OnDestroy()
        {
            randomPlayerButton.onClick.RemoveAllListeners();
            EventAggregator.Unsubscribe<NextTurn>(ChangeTurn);
            EventAggregator.Unsubscribe<PlayerLost>(PLost);
            EventAggregator.Unsubscribe<Initialization>(InitializePlayers);
        }

        private void InitializePlayers(object arg1, Initialization data)
        {
            int numberOfPlayers = gameManager.NumberOfPlayer;
            _players.Clear();

            backgroundImage.material.SetColor(_colorToGradient, Color.black);
            backgroundImage.material.SetColor(_prevColorToGradient, Color.black);

            ChangeShaderValues(Color.black, Color.black);

            for (int i = 1; i < numberOfPlayers + 1; i++)
            {
                _players.Add(new Player(Constants.Player + i, (Enums.Team)i, (Enums.GameStates)i, data.TeamsColorList[i - 1]));
                playerBotToggles[i - 1].gameObject.SetActive(true);
            }

            randomPlayerButton.gameObject.SetActive(true);
        }

        private void RandomStartingPlayer()
        {
            var numberOfPlayers = gameManager.NumberOfPlayer;
            gameState = (Enums.GameStates)Random.Range(1, numberOfPlayers + 1);

            EventAggregator.Subscribe<NextTurn>(ChangeTurn);
            EventAggregator.Subscribe<PlayerLost>(PLost);

            List<Enums.Team> teams = new();

            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (playerBotToggles[i].isOn)
                {
                    teams.Add(_players[i].Team);
                }

                playerBotToggles[i].gameObject.SetActive(false);
            }

            EventAggregator.Post(this, new AddBots { Teams = teams });
            EventAggregator.Post(this, new GetTurn { GameState = gameState });

            _currentPlayerIndex = _players.FindIndex(player => player.GameState == gameState);

            randomPlayerButton.gameObject.SetActive(false);
            restartButton.gameObject.SetActive(true);

            ChangeShaderValues(Color.black, _players[_currentPlayerIndex].TeamColor);
        }

        private void ChangeTurn(object arg1, NextTurn turnData)
        {
            if (gameState != Enums.GameStates.Win)
            {
                ChangeTurnToNextPlayer();
            }
        }

        private void PLost(object arg1, PlayerLost data)
        {
            var currentPlayerIndex = _players.FindIndex(player => player.Name == data.PlayerName);
            if (currentPlayerIndex != -1)
            {
                _prevPlayerColor = _players[currentPlayerIndex].TeamColor;
                _players.RemoveAt(currentPlayerIndex);
            }

            if (_players.Count <= 1)
            {
                gameState = Enums.GameStates.Win;
                winPanel.SetActive(true);
                Color teamColor = _players[0].TeamColor;
                winText.text = " WINNER: " + _players[0].Name;
                winText.color = teamColor;
            }
        }

        private void ChangeTurnToNextPlayer()
        {
            int nextPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;

            while (nextPlayerIndex == _currentPlayerIndex)
            {
                nextPlayerIndex = (nextPlayerIndex + 1) % _players.Count;
            }

            if (_currentPlayerIndex < _players.Count) _prevPlayerColor = _players[_currentPlayerIndex].TeamColor;
            ChangeShaderValues(_prevPlayerColor, _players[nextPlayerIndex].TeamColor);

            _currentPlayerIndex = nextPlayerIndex;
            gameState = _players[_currentPlayerIndex].GameState;

            EventAggregator.Post(this, new GetTurn { GameState = gameState });
        }

        private void ChangeShaderValues(Color previousColor, Color nextColor)
        {
            backgroundImage.material.SetFloat(_startTime, Time.time);
            backgroundImage.material.SetColor(_prevColorToGradient, previousColor);
            backgroundImage.material.SetColor(_colorToGradient, nextColor);
        }
    }

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