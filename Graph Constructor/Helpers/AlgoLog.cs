using System.Collections.Generic;

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

        public override string ToString()
        {
            return $"{Title}{Detail}\n";
        }
    }
}
