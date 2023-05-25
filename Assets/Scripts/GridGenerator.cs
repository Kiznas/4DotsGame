using UnityEngine;
using UnityEngine.UI;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private int _gridSize = 3; // Adjust this value to change the grid size
    [SerializeField] private GameObject _cellPrefab; // Prefab for the grid cell

    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private RectTransform _containerRect;
    private void Start()
    {
        GenerateGrid();
    }

    [ContextMenu("ReGenerate grid")]
    private void GenerateGrid()
    {
        ClearGrid();

        // Calculate the size of each cell based on the panel size
        float cellSize = Mathf.Min(_containerRect.rect.width / _gridSize, _containerRect.rect.height / _gridSize);

        // Calculate the number of cells and spacing
        int cellCount = _gridSize * _gridSize;
        Vector2 spacing = _gridLayout.spacing;

        // Adjust the cell size in the GridLayoutGroup
        _gridLayout.cellSize = new Vector2(cellSize, cellSize);

        // Calculate the total size of the grid
        float gridSizeX = (cellSize + spacing.x) * _gridSize - spacing.x;
        float gridSizeY = (cellSize + spacing.y) * _gridSize - spacing.y;

        // Adjust the size of the grid container
        _containerRect.sizeDelta = new Vector2(gridSizeX, gridSizeY);

        // Generate cells
        for (int i = 0; i < cellCount; i++)
        {
            // Instantiate a new cell from the prefab
            GameObject cell = Instantiate(_cellPrefab, transform);

            // Position the cell within the grid
            RectTransform cellRect = cell.GetComponent<RectTransform>();
            int row = i / _gridSize;
            int column = i % _gridSize;
            float posX = (cellSize + spacing.x) * column;
            float posY = (cellSize + spacing.y) * row;
            cellRect.anchoredPosition = new Vector2(posX, -posY);
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
