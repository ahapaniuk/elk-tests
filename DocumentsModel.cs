using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elk_tests
{
    internal class DocumentsModel
    {
        public IEnumerable<DocumentWithHighlight> Documents { get; set; }
        public long Total { get; set; }
    }
}
