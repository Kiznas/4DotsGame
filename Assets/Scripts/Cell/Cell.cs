using System.Threading.Tasks;
using UnityEngine;

namespace Cell
{
    public class Cell
    {
        private (Cell, Cell, Cell, Cell) _neighbours;

        public int PosRow { get; }
        public int PosColumn { get; }
        public int NumberOfDots { get; private set; }
        public Enums.Team CellTeam { get; private set; }
        public Color TeamColor { get; private set; } = Color.white;
        public Material Material { get; private set; }
        public CellInstance CellInstance { get; }

        public (Cell top, Cell right, Cell bottom, Cell left) Neighbours
        {
            get => _neighbours;
            set => _neighbours = value;
        }

        public Cell(int posX, int posY, CellInstance cellInstance)
        {
            PosRow = posX;
            PosColumn = posY;
            CellInstance = cellInstance;
        }

        public void AddDot()
        {
            NumberOfDots++;
            if (NumberOfDots >= 4) {
                UpdateImage();
                CellInstance.StartCoroutine(CellInstance.AddToNearby());
            }
            else {
                UpdateImage();
            }
        }
        public void SetTeam(Color teamColor, Material material, Enums.Team team)
        {
            CellTeam = team;
            TeamColor = teamColor;
            Material = material;
            if (Material != null)
                Material.color = teamColor;
            
            CellInstance.image.material = Material;
        }
        public void ClearCell()
        {
            CellTeam = Enums.Team.None;
            TeamColor = Color.white;
            NumberOfDots = 0;
            CellInstance.imageCombiner.ClearImage((Texture2D)CellInstance.image.mainTexture);
        }

        private void UpdateImage()
        {
            if (NumberOfDots != 0 && CellTeam != Enums.Team.None)
            {
                CellInstance.imageCombiner.CombineImages(NumberOfDots, Neighbours, CellTeam);
            }
        }
        public async Task UpdateImageAsync()
        {
            if (NumberOfDots != 0 && CellTeam != Enums.Team.None)
            {
                await CellInstance.imageCombiner.CombineImagesAsync(NumberOfDots, Neighbours, CellTeam);
            }
        }
    }
}