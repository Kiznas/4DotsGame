using System.Collections.Generic;
using Constants;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game_Managing.Initialization
{
    public class InitializeTurnSystem
    {
        private readonly Image _backgroundImage;
        private readonly GameObject _winPanel;
        private readonly GameManage _gameManager;
        private readonly TMP_Text _winText;
        private readonly Toggle[] _playerBotToggles;
        private readonly Button _restartButton;
        private readonly Button _startButton;
        private readonly Color[] _colors;

        public InitializeTurnSystem(Image backgroundImage, GameObject winPanel, GameManage gameManager, TMP_Text winText, Toggle[] playerBotToggles, Button restartButton, Button startButton, Color[] colors)
        {
            _players = new List<Player>();
            _backgroundImage = backgroundImage;
            _winPanel = winPanel;
            _gameManager = gameManager;
            _winText = winText;
            _playerBotToggles = playerBotToggles;
            _restartButton = restartButton;
            _startButton = startButton;
            _colors = colors;
            _startButton.onClick.AddListener(RandomStartingPlayer);
            
            InitializePlayers();
        }
        
        private readonly List<Player> _players;
        private int _currentPlayerIndex;
        private Color _prevPlayerColor;
        private void InitializePlayers()
        {
            int numberOfPlayers = _gameManager.NumberOfPlayer;
            _players.Clear();

            for (int i = 1; i < numberOfPlayers + 1; i++)
            {
                _players.Add(new Player(Constants.Constants.Player + i, (Enums.Team)i, (Enums.GameStates)i, _colors[i - 1]));
                _playerBotToggles[i - 1].gameObject.SetActive(true);
            }

            _startButton.gameObject.SetActive(true);
            
            new PlayersTurnSystem(_backgroundImage, _winPanel, _winText, _players);
        }

        private void RandomStartingPlayer()
        {
            List<Enums.Team> teams = new();
            InitializeToggles(_players.Count, teams);
            
            EventAggregator.Post(this, new AddBots { Teams = teams });
            EventAggregator.Post(this, new NextTurn { CellTeam = (Enums.Team)RandomizeStartingPlayer() });
            
            ChangeActiveButtons();
        }

        private int RandomizeStartingPlayer() =>
            Random.Range(1, _players.Count);

        private void ChangeActiveButtons()
        {
            _startButton.gameObject.SetActive(false);
            _restartButton.gameObject.SetActive(true);
        }

        private void InitializeToggles(int numberOfPlayers, List<Enums.Team> teams)
        {
            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (_playerBotToggles[i].isOn)
                {
                    teams.Add(_players[i].Team);
                }

                _playerBotToggles[i].gameObject.SetActive(false);
            }
        }
    }
}