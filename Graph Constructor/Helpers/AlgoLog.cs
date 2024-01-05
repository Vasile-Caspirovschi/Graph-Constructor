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
            Detail = string.Join("->", vertices);
        }
        public AlgoLog(string title, string details)
        {
            Title = title;
            Detail = details;
        }

        public void AddMoreDetails(List<int> vertices)
        {
            Detail += string.Join("->", vertices);
            Detail += Environment.NewLine;
        }

        public void AddMoreDetails(List<int> vertices, string details)
        {
            Detail += string.Join("->", vertices);
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
                FontFamily = new FontFamily("Consolas"),
                Text = ToString()
            };
            return textBlock;
        }
    }
}
