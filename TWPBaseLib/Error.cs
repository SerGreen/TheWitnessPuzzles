using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzleGenerator
{
    public class Error
    {
        // Who has an error
        public IErrorable Subject { get; }
        // Because of who this error arised
        public List<IErrorable> Sources { get; }

        public Error(IErrorable subject, List<IErrorable> sources)
        {
            Subject = subject;
            Sources = sources;
        }
    }
}
