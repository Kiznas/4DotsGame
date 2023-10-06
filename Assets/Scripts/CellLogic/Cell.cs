using ConstantValues;
using UnityEngine;

namespace CellLogic
{
    public class Cell
    {
        public int PosRow { get; }
        public int PosColumn { get; }
        public int NumberOfDots { get; private set; }
        public Enums.Team CellTeam { get; private set; }
        public Color TeamColor { get; private set; } = Color.white;
        public Material Material { get; private set; }
        public CellInstance CellInstance { get; }

        public Cell[] Neighbours { get; set; }

        public Cell(int posX, int posY, CellInstance cellInstance)
        {
            PosRow = posX;
            PosColumn = posY;
            CellInstance = cellInstance;
            Neighbours = new Cell[4];
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
            CellInstance.ClearImage((Texture2D)CellInstance.image.mainTexture);
        }

        public void UpdateImage()
        {
            if (NumberOfDots != 0 && CellTeam != Enums.Team.None)
            {
                CellInstance.CreateImage(NumberOfDots, Neighbours, CellTeam);
            }
        }
    }
}