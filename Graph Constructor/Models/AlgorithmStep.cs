using Graph_Constructor.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;

namespace Graph_Constructor.Models
{
    public class AlgorithmStep
    {
        public Dictionary<IMarkable, Color> MarkedElements { get; set; }
        public Action<Canvas> ActionBeforeMarking { get; set; } = default!;
        public AlgorithmStep()
        {
            MarkedElements = new Dictionary<IMarkable, Color>();
        }

        public AlgorithmStep(Action<Canvas> actionBeforeMarking) : this()
        {
            ActionBeforeMarking = actionBeforeMarking;
        }

        public AlgorithmStep AddMarkedElement(IMarkable element, Color color)
        {
            MarkedElements.Add(element, color);
            return this;
        }
    }
}
