using Assets;
using CellLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
    public class GridGenerator
    {
        private readonly GameObject _backgroundGameObject;
        private readonly GameObject _gridGameObject;

        public GridGenerator(GameObject backgroundGameObject, GameObject gridGameObject)
        {
            _backgroundGameObject = backgroundGameObject;
            _gridGameObject = gridGameObject;
        }

        public void GenerateGrid(int rows, int columns)
        {
            var cellPrefab = Resources.Load<GameObject>(AssetsPath.CellPrefabPath);
            var gridLayout = _gridGameObject.GetComponent<GridLayoutGroup>();
            var containerRect = _gridGameObject.GetComponent<RectTransform>();
            
            rows = Mathf.Max(rows, 3);
            columns = Mathf.Max(columns, 3);

            ClearGrid();
        
            var rect = containerRect.rect;
            float cellSize = Mathf.Min(rect.width / rows, rect.height / columns);

            cellSize = Mathf.Min(cellSize, 1000);
            int cellCount = rows * columns;
            gridLayout.cellSize = new Vector2(cellSize, cellSize);
        
            float gridSizeXTotal = cellSize * rows;
            float gridSizeYTotal = cellSize * columns;

            containerRect.sizeDelta = new Vector2(gridSizeXTotal, gridSizeYTotal);

            for (int i = 0; i < cellCount; i++) {
                GameObject cell = Object.Instantiate(cellPrefab, _gridGameObject.transform, false);

                RectTransform cellRect = cell.GetComponent<RectTransform>();
                int row = i / rows;
                int column = i % rows;
                float posX = cellSize * column;
                float posY = cellSize * row;
                cellRect.anchoredPosition = new Vector2(posX, -posY);
                cell.GetComponent<CellInstance>().CreateCellInstance(row, column);
            }

            var targetWidth = gridSizeXTotal + 40;
            var targetHeight = gridSizeYTotal + 40;
            _backgroundGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(targetWidth, targetHeight);
        }

        private void ClearGrid() {
            foreach (Transform child in _gridGameObject.transform) {
                Object.Destroy(child.gameObject);
            }
        }
    }
}
