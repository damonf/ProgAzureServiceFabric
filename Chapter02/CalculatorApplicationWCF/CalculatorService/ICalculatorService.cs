using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorService
{
    [ServiceContract]
    public interface ICalculatorService
    {
        [OperationContract]
        Task<string> Add(int a, int b);
        [OperationContract]
        Task<string> Subtract(int a, int b);
    }
}
