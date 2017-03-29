using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPISImageProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            JobHost host = new JobHost();
            host.RunAndBlock();
        }
    }
}
