 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonDataContextDriver.Inputs
{
    public interface IGeneratedClass
    {
        string Namespace { get; set; }
        string ClassName { get; set; }
        string ClassDefinition { get; set; }
        bool Success { get; set; }
        Exception Error { get; set; }

        IJsonInput OriginalInput { get; set; }
    }

}
