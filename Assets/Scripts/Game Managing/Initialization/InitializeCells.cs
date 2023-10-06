using System.Linq;
using CellLogic;
using ConstantValues;
using UnityEngine;

namespace Game_Managing.Initialization
{
    public static class InitializeCells
    {
        public static void AddCells(int rows, int columns, int numberOfPlayers, Cell[] cells, Color[] playersColors, Material[] playersMaterials) {
            foreach (var cell in cells) {
                AddNeighbours(cell, cells);
            }

            int[] playersPosX = { 1, rows - 2, 1, rows - 2 };
            int[] playersPosY = { 1, columns - 2, columns - 2, 1 };

            for (int i = 0; i < numberOfPlayers; i++) {
                int posX = playersPosX[i % 4];
                int posY = playersPosY[i % 4];

                AddDotsToCells(posX, posY, playersColors[i], playersMaterials[i], (Enums.Team)i + 1, cells);
            }
        }
        private static void AddDotsToCells(int posColumn, int posRow, Color teamColor, Material material, Enums.Team team, Cell[] cells) {
            Cell cell = GetCellAtPos(posColumn, posRow, cells);
            cell.SetTeam(teamColor, material, team);
            for (var i = 0; i < 3; i++) { cell.AddDot(); }
        }
        
        private static void AddNeighbours(Cell cell, Cell[] cells)
        {
            int posX = cell.PosColumn;
            int posY = cell.PosRow;

            cell.Neighbours[0] = GetCellAtPos(posX, posY - 1, cells);
            cell.Neighbours[1] = GetCellAtPos(posX + 1, posY, cells);
            cell.Neighbours[2] = GetCellAtPos(posX, posY + 1, cells);
            cell.Neighbours[3] = GetCellAtPos(posX - 1, posY, cells);
        }
        
        private static Cell GetCellAtPos(int posColumn, int posRow, Cell[] cells) {
            return cells.FirstOrDefault(cell => cell.PosColumn == posColumn && cell.PosRow == posRow);
        }
    }
}