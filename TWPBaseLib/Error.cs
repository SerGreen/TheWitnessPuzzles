using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheWitnessPuzzles
{
    public class Error
    {
        /// <summary>
        /// Who has an error
        /// </summary>
        public IErrorable Source { get; }
        /// <summary>
        /// If this error is neutralized by Elimination rule
        /// </summary>
        public bool IsEliminated { get; set; }

        public void Eliminate() => IsEliminated = true;

        public Error(IErrorable source, bool isEliminated = false)
        {
            Source = source;
            IsEliminated = isEliminated;
        }
    }
}
