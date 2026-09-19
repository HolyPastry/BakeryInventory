
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


namespace Bakery 
{
public class GridObjectStackingFeedbackUI : MonoBehaviour
{

    [SerializeField] private Transform _backgroundCellContainer;
    
    [SerializeField] private Color _highlightColor;

    private Color _defaultColor;
    private List<Image> _backgroundCells = new();
    
    void OnEnable()
    {
        Inventory.Events.Controller.OnCleanHighlight += CleanHighlight;
    }

    void OnDisable()
    {
        Inventory.Events.Controller.OnCleanHighlight -= CleanHighlight;
    }

    private void CleanHighlight()
    {
        UpdateHighlight(false);
    }
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
}