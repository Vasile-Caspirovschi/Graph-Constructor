using Graph_Constructor.Helpers;
using Graph_Constructor.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;

namespace Graph_Constructor.Models
{
    public class AlgorithmSteps : List<AlgorithmStep>
    {
        private readonly Canvas _drawingArea;
        private int _index;

        public AlgorithmSteps(Canvas drawingArea)
        {
            _drawingArea = drawingArea;
            _index = -1;
        }

        public void ShowNextStep()
        {
            _index = Math.Min(_index + 1, Count - 1);
            ShowStep(_index);
        }

        public void ShowPreviousStep()
        {
            DrawingHelpers.ClearCanvasFromAnimationEffects(_drawingArea);
            _index = Math.Max(_index - 1, 0);
            for (int i = 0; i <= _index; i++)
                ShowStep(i);
        }

        private void ShowStep(int index)
        {
            var currentStep = this[index];
            MarkElement(currentStep.MarkedElement, currentStep.MarkedColor);
        }

        private void MarkElement(IMarkable element, Color colorToMark)
        {
            this[_index].CurrentColor = colorToMark;

            if (element.IsVertex())
                DrawingHelpers.MarkVertex(_drawingArea, (element as Vertex)!, colorToMark);
            else
                DrawingHelpers.MarkEdge(_drawingArea, (element as Edge)!, colorToMark);
        }
    }
}
