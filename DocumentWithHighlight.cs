using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elk_tests
{
    internal class DocumentWithHighlight
    {
        public SearchPart Document { get; set; }
        public IEnumerable<string> Hightlight { get; set; }
    }
}
