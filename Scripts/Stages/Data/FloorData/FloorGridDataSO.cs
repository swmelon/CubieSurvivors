using Local.Scripts.Extensions;
using System;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "FloorGridData", menuName = "ScriptableObjects/Stages/Data/FloorData/FloorGridData")]
public class FloorGridDataSO : ScriptableObject
{
    public int length = 11;

    public GridDataCell[] grid;

    public bool readOnly = false;

    public Action<int[,], int, int>[] actions;


    private void OnEnable()
    {
        actions =new Action<int[,], int, int>[]
         {
            //  /x, y -> y, x^ -> x^, y^ -> y^, x -> x, y/  
            (array, x, y) => array[x, y] = grid[y * length + x].isOccupied ? 0 : -1,
            (array, x, y) => array[y, length - 1 - x] = grid[y * length + x].isOccupied ? 0 : -1,
            (array, x, y) => array[length - 1 - x, length - 1 - y] = grid[y * length + x].isOccupied ? 0 : -1,
            (array, x, y) => array[length - 1 - y, x] = grid[y * length + x].isOccupied ? 0 : -1,

            // /x^, y ->y^, x^ -> x, y^ -> y ,x -> x^,y/
            (array, x, y) => array[length - x - 1 , y] = grid[y * length + x].isOccupied ? 0 : -1,
            (array, x, y) => array[length - 1 - y, length - 1 - x] = grid[y * length + x].isOccupied ? 0 : -1,
            (array, x, y) => array[x, length - 1 - y] = grid[y * length + x].isOccupied ? 0 : -1,
            (array, x, y) => array[y, x] = grid[y * length + x].isOccupied ? 0 : -1,
         };
    }

    public GridDataCell GetCell(int x, int y)
    {
        return grid[y * length + x];
    }

    public void SetCell(int x, int y, GridDataCell cell)
    {
        grid[y * length + x] = cell;
    }

    public void InitializeGrid()
    {
        grid = new GridDataCell[length * length];
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = new GridDataCell();
        }
    }

    public int[,] ToArray()
    {
        int[,] array = new int[length, length];

        for (int y = 0; y < length; y++)
        {
            for (int x = 0; x < length; x++)
            {
                array[x, y] = grid[y * length + x].isOccupied ? 0 : -1;
            }
        }

        return array;
    }

    public int[,] ToAugmentedArray()
    {
        int[,] array = new int[length, length];
        Action<int[, ] ,int, int> selectedAction = actions.PickRandom();

        for (int y = 0; y < length; y++)
        {
            for (int x = 0; x < length; x++)
            {
                selectedAction(array, x, y);
            }
        }

        return array;
    }
}