using Events;
using Utilities;
using System.Linq;
using UnityEngine;
using Game_Managing;
using ConstantValues;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace CellLogic
{
    public class CellInstance : MonoBehaviour, IEventsUser
    {
        [SerializeField] private Button button;
        [SerializeField] private ParticleSystem particle;
        public Image image;

        private List<Enums.Team> _botTeams;

        private Cell _cell;
        private bool _teamTurn;

        public void Subscribe()
        {
            EventAggregator.Subscribe<GetTurn>(SetTurn);
            EventAggregator.Subscribe<PrepareForNextTurn>(PrepareTurn);
            EventAggregator.Subscribe<AddBots>(AddBotsTeams);
        }

        public void Unsubscribe()
        {
            EventAggregator.Unsubscribe<GetTurn>(SetTurn);
            EventAggregator.Unsubscribe<PrepareForNextTurn>(PrepareTurn);
            EventAggregator.Unsubscribe<AddBots>(AddBotsTeams);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveAllListeners();
            Unsubscribe();
        }

        public void CreateCellInstance(int row, int column)
        {
            _cell = new Cell(row, column, this);
            button.onClick.AddListener(OnClick);
            EventAggregator.Post(this, new CellAdded { Cell = _cell });
            Subscribe();
        }

        private void AddBotsTeams(object arg1, AddBots data)
        {
            _botTeams = data.Teams;
        }

        private void PrepareTurn(object arg1, PrepareForNextTurn team)
        {
            if (team.CellTeam == _cell.CellTeam)
            {
                _teamTurn = false;
            }
        }

        private void SetTurn(object arg1, GetTurn turn)
        {
            if (_cell.CellTeam != Enums.Team.None)
            {
                var reversed = Constants.TeamsDictionary.ToDictionary(x => x.Value, x => x.Key);
                if (turn.GameState == Constants.TeamsDictionary[_cell.CellTeam] && !_botTeams.Contains(reversed[turn.GameState]))
                {
                    _teamTurn = true;
                }
            }
        }

        private void OnClick()
        {
            if (_cell.TeamColor != Color.white && _teamTurn)
            {
                EventAggregator.Post(this, new PrepareForNextTurn { CellTeam = _cell.CellTeam });
                if (_cell.NumberOfDots != 3) { StartCoroutine(NextTurnWithDelay()); }
                _cell.AddDot();
            }
        }

        private IEnumerator NextTurnWithDelay()
        {
            yield return new WaitForSeconds(Constants.SpeedOfGame);
            EventAggregator.Post(this, new NextTurn { CellTeam = _cell.CellTeam });
        }

        public IEnumerator AddToNearby()
        {
            yield return new WaitForSeconds(Constants.SpeedOfGame);

            StartCoroutine(SpreadAnimation(false, _cell.TeamColor));
            EventAggregator.Post(this, new AddToNearbyCells { Cell = _cell });
        }

        private IEnumerator SpreadAnimation(bool withDelay, Color teamColor)
        {
            if (withDelay)
            {
                yield return new WaitForSeconds(Constants.SpeedOfGame);
            }

            var o = gameObject;
            var system = Instantiate(particle, o.transform.position, o.transform.rotation, o.transform);
            var particleMain = system.main;
            particleMain.startColor = teamColor;
            Destroy(system.gameObject, 3f);
            yield return null;
        }

        internal void CreateImage(int numberOfDots, Cell[] neighbours, Enums.Team cellTeam) =>
            image.sprite = Bootstrapper.Instance.ImageCombiner.CombineImages(numberOfDots, neighbours, cellTeam);

        internal void ClearImage() =>
            image.sprite = ImageCombine.ClearImage();
    }
}