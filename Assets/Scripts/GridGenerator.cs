using CellLogic;
using UnityEngine;
using UnityEngine.UI;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private GameObject targetGameObject;
    [SerializeField] private RectTransform containerRect;

    public void GenerateGrid(int rows, int columns)
    {
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
            GameObject cell = Instantiate(cellPrefab, transform);

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
        targetGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(targetWidth, targetHeight);
    }

    private void ClearGrid() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
    }
}
