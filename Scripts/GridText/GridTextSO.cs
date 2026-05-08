
using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GridTextSO", menuName = "ScriptableObjects/GridTextSO", order = SOAssetMenuIndex.Stage)]
public class GridTextSO : ScriptableObject
{
    public string symbol;
    public int width = 5;
    public int height = 7;
    public int Width => width;
    public int Height => height;

    public GridDataCell[] grid;

    public bool readOnly = false;

    public GridDataCell GetCell(int x, int y)
    {
        return grid[y * width + x];
    }

    public void SetCell(int x, int y, GridDataCell cell)
    {
        grid[y * width + x] = cell;
    }

    public void InitializeGrid()
    {
        grid = new GridDataCell[width * height];
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = new GridDataCell();
        }
    }

    // InitializeGrid and other methods...
}