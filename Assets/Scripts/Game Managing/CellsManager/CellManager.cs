using Events;
using System;
using CellLogic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ConstantValues;

namespace Game_Managing.CellsManager
{
    
    public class CellManager : IEventsUser
    {
        public CellManager(Cell[] cells) {
            Cells = cells;

            _cellQueue = new Queue<Cell>();
        }

        public readonly Cell[] Cells;
        private readonly Queue<Cell> _cellQueue;
        private readonly Stack<Cell> _stackToChange = new();

        private int _cellIndex;
        private bool _isProceeding;
        private HashSet<Enums.Team> _previouslyAliveTeams = new();

        public void Subscribe()
        {
            EventAggregator.Subscribe<CellAdded>(AddCellToArray);
            EventAggregator.Subscribe<AddToNearbyCells>(AddToNearbyCells);
        }

        public void Unsubscribe()
        {
            EventAggregator.Unsubscribe<CellAdded>(AddCellToArray);
            EventAggregator.Unsubscribe<AddToNearbyCells>(AddToNearbyCells);
        }

        private void AddCellToArray(object arg1, CellAdded cell) =>
            Cells[_cellIndex++] = cell.Cell;

        private async void AddToNearbyCells(object arg1, AddToNearbyCells cellData)
        {
            if (cellData != null)
            {
                Cell cell = cellData.Cell;
                _cellQueue.Enqueue(cell);
            }

            if (_isProceeding == false)
            {
                _isProceeding = true;
                if (cellData != null) await ProcessQueue(cellData.Cell.CellTeam);
            }
        }

        private async Task ProcessQueue(Enums.Team team)
        {
            while (_cellQueue.Count > 0)
            {
                List<Cell> cellsWaves = new();

                while (_cellQueue.Count > 0)
                {
                    var cellData = _cellQueue.Dequeue();
                    cellsWaves.Add(cellData);
                }

                foreach (var cellData in cellsWaves)
                {
                    CellManagerExtensions.ProcessCell(cellData, _stackToChange);
                }

                UpdateImages();
                await Task.Delay(TimeSpan.FromSeconds(Constants.SpeedOfGame));
            }

            var aliveTeams = CellManagerExtensions.CheckAliveTeams(_previouslyAliveTeams, Cells, this);

            _previouslyAliveTeams = aliveTeams;
            _isProceeding = false;



            EventAggregator.Post(this, new NextTurn { CellTeam = team });
        }

        private void UpdateImages()
        {
            foreach (var item in _stackToChange.ToList())
                item?.UpdateImage();
            _stackToChange.Clear();
            Resources.UnloadUnusedAssets();
        }
    }
}