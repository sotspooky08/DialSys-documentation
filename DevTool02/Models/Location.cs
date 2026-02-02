using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTool01.Models
{
    public class obsLocation
    {

        [Key]
        public required string LocName { get; set; }
        public required bool LocSelected { get; set; }

    }
}
