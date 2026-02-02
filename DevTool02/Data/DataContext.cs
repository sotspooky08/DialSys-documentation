using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DevTool01.Models;

namespace DevTool01.Data
{
    public partial class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<Models.Convo> Convos { get; set; }
        public DbSet<Models.obsCharacter> Characters { get; set; }
        public DbSet<Models.obsLocation> Locations { get; set; }
        public DbSet<Models.MainDial> MainDials { get; set; }
        
    }
}
