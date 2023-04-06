using Furion.DynamicApiController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FurStart.Controllers
{
    public class MyFirstApp : IDynamicApiController
    {
        public string Get()
        {
            return $"Hello {nameof(Furion)}";
        }
    }
}
