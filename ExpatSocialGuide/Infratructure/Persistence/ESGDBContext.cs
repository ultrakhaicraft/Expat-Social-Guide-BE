using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infratructure.Persistence
{
    public class ESGDBContext : DbContext
    {
        public ESGDBContext(DbContextOptions<ESGDBContext> options) : base(options)
        {
        }
    }
}
