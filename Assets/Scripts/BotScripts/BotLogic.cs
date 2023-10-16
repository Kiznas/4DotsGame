using Events;
using CellLogic;
using System.Linq;
using UnityEngine;
using ConstantValues;
using Infrastructure;
using System.Collections;
using System.Collections.Generic;
using Events.EventsManager;
using Game_Managing.CellsManager;

namespace BotScripts
{
    public class BotLogic : IEventsUser
    {
        private readonly CellManager _cellManager;
        private readonly ICoroutineRunner _coroutineRunner;
        private static readonly int IsGlowing = Shader.PropertyToID("_IsGlowing");
        
        private List<Enums.Team> _botTeams = new();
        private List<Cell> _botCells = new();
        private List<Cell> _opponentCells = new();

        public BotLogic(ICoroutineRunner coroutineRunner, CellManager cellManager)
        {
            _cellManager = cellManager;
            _coroutineRunner = coroutineRunner;
        }

        public void Subscribe()
        {
            EventAggregator.Subscribe<GetTurn>(OnGetTurn);
            EventAggregator.Subscribe<AddBots>(AddBotsToList);
        }

        public void Unsubscribe()
        {
            EventAggregator.Unsubscribe<GetTurn>(OnGetTurn);
            EventAggregator.Unsubscribe<AddBots>(AddBotsToList);
        }

        private void AddBotsToList(object arg1, AddBots data)
        {
            _botTeams = data.Teams;
        }

        private void OnGetTurn(object sender, GetTurn turnData)
        {
            if(_botTeams.Contains(turnData.Team)){
                _coroutineRunner.StartCoroutine(BotDelay(turnData.Team));
            }
        }

        private IEnumerator BotDelay(Enums.Team team)
        {
            yield return new WaitForSeconds(Constants.SpeedOfGame);
            TakeBotTurn(team);
        }

        private void TakeBotTurn(Enums.Team botTeam)
        {
            _botCells = _cellManager.Cells
                .Where(cell => cell.CellTeam == botTeam)
                .ToList();

            _opponentCells = _cellManager.Cells
                .Where(cell => cell.CellTeam != botTeam && cell.CellTeam != Enums.Team.None)
                .ToList();

            var botCellToAttack = BotExtensions.GetBotCellToAttack(_botCells);
            if (botCellToAttack != null)
            {
                ProcessBotCellTurn(botCellToAttack, botTeam);
                return;
            }
            
            var closestBotCellToOpponentCell = BotExtensions.FindClosestBotCellToOpponentCell(_botCells, _opponentCells);
            if (closestBotCellToOpponentCell != null)
            {
                ProcessBotCellTurn(closestBotCellToOpponentCell, botTeam);
            }
        }

        private void ProcessBotCellTurn(Cell botCell, Enums.Team botTeam)
        {
            _cellManager.Cells.FirstOrDefault(cell => cell.CellTeam == botTeam)?.Material.SetInt(IsGlowing, 0);

            if (botCell.NumberOfDots != 3)
            {
                _coroutineRunner.StartCoroutine(GiveNextTurn(botTeam));
            }

            botCell.AddDot();
        }

        private IEnumerator GiveNextTurn(Enums.Team team)
        {
            yield return new WaitForSeconds(Constants.SpeedOfGame);
            EventAggregator.Post(this, new NextTurn { CellTeam = team });
        }
    }
}