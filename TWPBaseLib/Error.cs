using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzleGenerator
{
    public class Error
    {
        /// <summary>
        /// Who has an error
        /// </summary>
        public IErrorable Subject { get; }
        /// <summary>
        /// Because of who this error arised
        /// </summary>
        public List<IErrorable> Sources { get; }
        /// <summary>
        /// If this error is neutralized by Elimination rule
        /// </summary>
        public bool IsEliminated { get; set; }

        public void Eliminate() => IsEliminated = true;

        public Error(IErrorable subject, List<IErrorable> sources, bool isEliminated = false)
        {
            Subject = subject;
            Sources = sources;
            IsEliminated = isEliminated;
        }
    }
}
