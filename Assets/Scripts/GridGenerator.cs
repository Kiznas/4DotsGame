using UnityEngine;
using UnityEngine.UI;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private GameManagerScript _gameManager;

    [SerializeField] private GameObject _targetGameObject;

    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private RectTransform _containerRect;

    public void GenerateGrid(int rows, int collumns)
    {
        // Ensure the grid size is at least 1x1
        rows = Mathf.Max(rows, 1);
        collumns = Mathf.Max(collumns, 1);

        ClearGrid();

        // Calculate the size of each cell based on the smaller dimension of the container
        float cellSize = Mathf.Min(_containerRect.rect.width / rows, _containerRect.rect.height / collumns);

        // Limit the cell size to a maximum of 1000x1000
        cellSize = Mathf.Min(cellSize, 1000);

        // Calculate the number of cells and spacing
        int cellCount = rows * collumns;

        // Adjust the cell size in the GridLayoutGroup
        _gridLayout.cellSize = new Vector2(cellSize, cellSize);

        // Calculate the total size of the grid
        float gridSizeXTotal = cellSize * rows;
        float gridSizeYTotal = cellSize * collumns;

        // Adjust the size of the grid container
        _containerRect.sizeDelta = new Vector2(gridSizeXTotal, gridSizeYTotal);

        for (int i = 0; i < cellCount; i++)
        {
            GameObject cell = Instantiate(_cellPrefab, transform);

            RectTransform cellRect = cell.GetComponent<RectTransform>();
            int row = i / rows;
            int column = i % rows;
            float posX = cellSize * column;
            float posY = cellSize * row;
            cellRect.anchoredPosition = new Vector2(posX, -posY);
            cell.GetComponent<CellInstance>().CreateCellInstance(row, column);
        }

        // Adjust the size of the target GameObject to match the _gridLayout size + 10 pixels
        float targetWidth = gridSizeXTotal + 10;
        float targetHeight = gridSizeYTotal + 10;
        _targetGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(targetWidth, targetHeight);
    }

    private void ClearGrid()
    {
        // Destroy all existing cells
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
