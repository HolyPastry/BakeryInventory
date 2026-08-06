using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bakery.Test
{

    public class TestGridContainerFit : MonoBehaviour
    {
        [SerializeField] private InputActionReference _triggerAction;
        [SerializeField] private GridContainer _gridContainer;
        [SerializeField] private GridInfo _gridInfo;

        private int _rotation = 0;
        private RotatableGrid _grid;
        private int _index = -1;

        void OnEnable()
        {
            _triggerAction.action.performed += OnTriggerPerformed;
        }
        void OnDisable()
        {
            _triggerAction.action.performed -= OnTriggerPerformed;
        }

        private void OnTriggerPerformed(InputAction.CallbackContext context)
        {
            TestFitIn();
        }

        [ContextMenu("Test FitIn")]
        public void TestFitIn()
        {
            if (_index > _gridContainer.GridInfo.Coordinates.Count)
            {
                Debug.Log("Test completed.");
                _index = -1;

                return;
            }
            if (_index == -1)
                InitiateTest();
            else
                ContinueToNextStep();

        }

        private void ContinueToNextStep()
        {
            if (_rotation >= 3)
            {
                _rotation = 0;
                _index++;
                if (_index >= _gridContainer.GridInfo.Coordinates.Count)
                {
                    Debug.Log("Test completed.");
                    Inventory.Events.Grids.OnItemRemoved?.Invoke(_gridContainer, _grid);
                    return;
                }
            }
            else
            {
                _rotation++;
            }
            var coordinate = _gridContainer.GridInfo.Coordinates[_index];
            bool fits = _gridContainer.FitIn(_grid, _rotation, coordinate);
            Inventory.Events.Grids.OnItemPlaced?.Invoke(_gridContainer, _grid);
            Debug.Log($"Testing FitIn for rotation {_rotation} at coordinate {coordinate}: {(fits ? "Fits" : "Does not fit")}");

        }

        private void InitiateTest()
        {
            _grid = new RotatableGrid(_gridInfo);
            _index = 0;
            _rotation = 0;
            var coordinate = _gridContainer.GridInfo.Coordinates[_index];
            var fitIn = _gridContainer.FitIn(_grid, _rotation, coordinate);
            Inventory.Events.Grids.OnItemAdded?.Invoke(_gridContainer, _grid);
            Debug.Log($"Testing FitIn for rotation {_rotation} at coordinate {coordinate}: {(fitIn ? "Fits" : "Does not fit")}");



        }
    }
}