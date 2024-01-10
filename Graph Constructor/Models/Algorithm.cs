using Graph_Constructor.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Graph_Constructor.Models
{
    public abstract class Algorithm : DependencyObject
    {
        protected readonly Graph graph = default!;
        protected readonly Canvas drawingArea = default!;
        protected readonly Vertex start = default!;
        protected readonly Vertex? target;

        protected Algorithm(Graph graph, Canvas drawingArea, Vertex start, Vertex? target)
        {
            this.graph = graph;
            this.drawingArea = drawingArea;
            this.start = start;
            this.target = target;
        }

        public abstract Task Execute();
        public abstract AlgoLog GetResults();
        public abstract void BindViewProperties(params Control[] viewControls);
        public abstract AlgorithmSteps GetSolvingSteps();

        public static readonly DependencyProperty ExecutionSpeedProperty =
            DependencyProperty.Register("ExecutionSpeed", typeof(double), typeof(Algorithm), new PropertyMetadata(1.0));

        public double ExecutionSpeed
        {
            get { return (double)GetValue(ExecutionSpeedProperty); }
            set { SetValue(ExecutionSpeedProperty, value); }
        }

        protected int SetExecutionDelay(int maxDelay)
        {
            double normalizedExecutionSpeed = ExecutionSpeed / 100.0;
            if (ExecutionSpeed > 20 && ExecutionSpeed < 60)
                maxDelay -= 100;

            int delay = (int)(maxDelay * (1 - normalizedExecutionSpeed));
            return delay;

        }
    }
}
