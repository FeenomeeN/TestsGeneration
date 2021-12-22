using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestsGeneratorLib.Info;

namespace TestsGeneratorLib
{
    public interface ITestGenerate
    {
        public Dictionary<string, string> GenerateTests(FileInfo fileInfo);
    }
}