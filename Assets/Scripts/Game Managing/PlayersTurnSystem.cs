using System.Collections.Generic;
using Constants;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game_Managing
{
    public class PlayersTurnSystem
    {
        private readonly int _colorToGradient = Shader.PropertyToID("_colorToGradient");
        private readonly int _prevColorToGradient = Shader.PropertyToID("_prevColorToGradient");
        private readonly int _startTime = Shader.PropertyToID("_startTime");
        private readonly List<Player> _players;
        private readonly Image _backgroundImage;
        private readonly GameObject _winPanel;
        private readonly TMP_Text _winText;
        
        private int _currentPlayerIndex;
        private Color _prevPlayerColor;

        public Enums.GameStates GameState;

        public PlayersTurnSystem(Image backgroundImage, GameObject winPanel, TMP_Text winText, List<Player> players)
        {
            _players = players;
            _backgroundImage = backgroundImage;
            _winPanel = winPanel;
            _winText = winText;
            
            _backgroundImage.material.SetColor(_colorToGradient, Color.black);
            _backgroundImage.material.SetColor(_prevColorToGradient, Color.black);

            ChangeShaderValues(Color.black, Color.black);
            
            EventAggregator.Subscribe<NextTurn>(ChangeTurn);
            EventAggregator.Subscribe<PlayerLost>(PlayerLost);
            
            EventAggregator.Post(this, new GetTurn { GameState = GameState });
        }

        private void ChangeTurn(object arg1, NextTurn turnData)
        {
            if (GameState != Enums.GameStates.Win)
            {
                int nextPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;

                while (nextPlayerIndex == _currentPlayerIndex)
                {
                    nextPlayerIndex = (nextPlayerIndex + 1) % _players.Count;
                }

                if (_currentPlayerIndex < _players.Count) _prevPlayerColor = _players[_currentPlayerIndex].TeamColor;
                ChangeShaderValues(_prevPlayerColor, _players[nextPlayerIndex].TeamColor);

                _currentPlayerIndex = nextPlayerIndex;
                GameState = _players[_currentPlayerIndex].GameState;

                EventAggregator.Post(this, new GetTurn { GameState = GameState });
            }
        }

        private void PlayerLost(object arg1, PlayerLost data)
        {
            var currentPlayerIndex = _players.FindIndex(player => player.Name == data.PlayerName);
            if (currentPlayerIndex != -1)
            {
                _prevPlayerColor = _players[currentPlayerIndex].TeamColor;
                _players.RemoveAt(currentPlayerIndex);
            }

            if (_players.Count <= 1)
            {
                GameState = Enums.GameStates.Win;
                _winPanel.SetActive(true);
                Color teamColor = _players[0].TeamColor;
                _winText.text = " WINNER: " + _players[0].Name;
                _winText.color = teamColor;
            }
        }

        private void ChangeShaderValues(Color previousColor, Color nextColor)
        {
            _backgroundImage.material.SetFloat(_startTime, Time.time);
            _backgroundImage.material.SetColor(_prevColorToGradient, previousColor);
            _backgroundImage.material.SetColor(_colorToGradient, nextColor);
        }
    }
}