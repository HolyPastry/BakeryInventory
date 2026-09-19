
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridObjectStackingFeedbackUI : MonoBehaviour
{

    [SerializeField] private Transform _backgroundCellContainer;
    
    [SerializeField] private Color _highlightColor;

private Color _defaultColor;
    private List<Image> _backgroundCells = new();
    
   
    public void UpdateHighlight(bool isHighlighted)
    {
        if(_backgroundCells.Count == 0)
        {
            _backgroundCellContainer.GetComponentsInChildren(true, _backgroundCells);
            if(_backgroundCells.Count > 0)
                _defaultColor = _backgroundCells[0].color;
        }
        
        foreach (var cell in _backgroundCells)
            cell.color = isHighlighted ? _highlightColor : _defaultColor;
        
    }
   
}
