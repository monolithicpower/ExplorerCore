using MPS.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MPS.WebApi.Services
{
    public interface IRegistersService
    {
        void Add(Register register);
        void AddRange(IEnumerable<Register> regs);
        IEnumerable<Register> GetAll();
    }
}
