using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MPS.WebApi.Models
{
    public class BookChapter
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; }
        public int Number { get; set; }
    }

}
