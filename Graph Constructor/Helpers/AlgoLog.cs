using Graph_Constructor.Models;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace Graph_Constructor.Helpers
{
    public class AlgoLog
    {
        public string Title { get; set; }
        public string Detail { get; set; }

        public AlgoLog(string title, List<int> vertices)
        {
            Title = title;
            Detail = string.Join("→", vertices);
            Detail += Environment.NewLine;
        }
        public AlgoLog(string title, string details)
        {
            Title = title;
            Detail = details;
        }

        public void AddMoreDetails(Edge edge)
        {
            Detail += $"{edge.From.Id}→{edge.To.Id}={edge.Cost}";
            Detail += Environment.NewLine;
        }

        public void AddMoreDetails(List<int> vertices)
        {
            Detail += string.Join("→", vertices);
            Detail += Environment.NewLine;
        }
        public void AddMoreDetails(List<int> vertices , string details)
        {
            Detail += string.Join("→", vertices);
            Detail += " " + details;
            Detail += Environment.NewLine;
        }

        public void AddMoreDetails(string details)
        {
            Detail += details;
            Detail += Environment.NewLine;
        }

        public override string ToString()
        {
            return $"{Title}{Detail}\n";
        }

        public TextBlock GetLog()
        {
            var textBlock = new TextBlock
            {
                FontSize = 15,
                Margin = new System.Windows.Thickness(0, -5, 0, 0),
                FontFamily = new FontFamily("Consolas"),
                Text = ToString()
            };
            return textBlock;
        }
    }
}
