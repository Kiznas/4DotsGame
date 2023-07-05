using UnityEngine;
using UnityEngine.UI;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private GameManagerScript _gameManager;

    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private RectTransform _containerRect;

    private int _gridSize;

    public void GenerateGrid()
    {
        _gridSize = _gameManager.GridSize;

        ClearGrid();

        // Calculate the size of each cell based on the panel size
        float cellSize = Mathf.Min(_containerRect.rect.width / _gridSize, _containerRect.rect.height / _gridSize);

        // Calculate the number of cells and spacing
        int cellCount = _gridSize * _gridSize;

        // Adjust the cell size in the GridLayoutGroup
        _gridLayout.cellSize = new Vector2(cellSize, cellSize);

        // Calculate the total size of the grid
        float gridSizeX = cellSize * _gridSize;
        float gridSizeY = cellSize * _gridSize;

        // Adjust the size of the grid container
        _containerRect.sizeDelta = new Vector2(gridSizeX, gridSizeY);

        for (int i = 0; i < cellCount; i++)
        {
            GameObject cell = Instantiate(_cellPrefab, transform);

            RectTransform cellRect = cell.GetComponent<RectTransform>();
            int row = i / _gridSize;
            int column = i % _gridSize;
            float posX = cellSize * column;
            float posY = cellSize * row;
            cellRect.anchoredPosition = new Vector2(posX, -posY);
            cell.GetComponent<CellInstance>().CreateCellInstance(row, column);
        }
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
