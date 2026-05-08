using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GridTextManagerSO", menuName = "ScriptableObjects/GridTextManagerSO", order = SOAssetMenuIndex.Stage)]
public class GridTextManagerSO : ScriptableObject
{
    [SerializeField]
    private List<GridTextSO> gridTexts = new List<GridTextSO>();

    
    private Dictionary<string, GridTextSO> gridTextDict = new Dictionary<string, GridTextSO>();

    private void OnEnable()
    {
        foreach (var gridText in gridTexts)
        {
            gridTextDict[gridText.symbol] = gridText;
        }
    }
    
    public bool TryGetGridText(string symbol, out GridTextSO gridText)
    {
        return gridTextDict.TryGetValue(symbol, out gridText);
    }
}
