using Events;
using Constants;
using System.Linq;
using UnityEngine;
using Game_Managing;
using Infrastructure;
using System.Collections;
using System.Collections.Generic;

namespace BotScripts
{
    public class BotLogic
    { 
        private readonly GameManage _gameManager;
        private readonly ICoroutineRunner _coroutineRunner;
        private static readonly int IsGlowing = Shader.PropertyToID("_IsGlowing");
        
        private List<Enums.Team> _botTeams = new();

        public BotLogic(GameManage gameManager, ICoroutineRunner coroutineRunner)
        {
            _gameManager = gameManager;
            _coroutineRunner = coroutineRunner;
            EventAggregator.Subscribe<GetTurn>(OnGetTurn);
            EventAggregator.Subscribe<AddBots>(AddBotsToList);
        }

        private void AddBotsToList(object arg1, AddBots data)
        {
            _botTeams = data.Teams;
        }

        private void OnGetTurn(object sender, GetTurn turnData)
        {
            foreach (var botTeam in _botTeams.Where(botTeam => turnData.GameState == Constants.Constants.TeamsDictionary[botTeam]))
            {
                _coroutineRunner.StartCoroutine(BotDelay(botTeam));
                return;
            }
        }

        private IEnumerator BotDelay(Enums.Team team)
        {
            yield return new WaitForSeconds(Constants.Constants.SpeedOfGame);
            TakeBotTurn(team);
        }

        private void TakeBotTurn(Enums.Team botTeam)
        {
            var botCells = _gameManager.Cells
                .Where(cell => cell.CellTeam == botTeam)
                .ToList();

            var opponentCells = _gameManager.Cells
                .Where(cell => cell.CellTeam != botTeam && cell.CellTeam != Enums.Team.None)
                .ToList();

            var botCellToAttack = BotExtensions.GetBotCellToAttack(botCells);
            if (botCellToAttack != null)
            {
                ProcessBotCellTurn(botCellToAttack, botTeam);
                return;
            }
            
            var closestBotCellToOpponentCell = BotExtensions.FindClosestBotCellToOpponentCell(botCells, opponentCells);
            if (closestBotCellToOpponentCell != null)
            {
                ProcessBotCellTurn(closestBotCellToOpponentCell, botTeam);
            }
        }

        private void ProcessBotCellTurn(CellLogic.Cell botCell, Enums.Team botTeam)
        {
            _gameManager.Cells.FirstOrDefault(cell => cell.CellTeam == botTeam)?.Material.SetInt(IsGlowing, 0);

            if (botCell.NumberOfDots != 3)
            {
                _coroutineRunner.StartCoroutine(GiveNextTurn(botTeam));
            }

            botCell.AddDot();
        }

        private IEnumerator GiveNextTurn(Enums.Team team)
        {
            yield return new WaitForSeconds(Constants.Constants.SpeedOfGame);
            EventAggregator.Post(this, new NextTurn { CellTeam = team });
        }
    }
}