using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTool01.Models
{
    public class obsCharacter
    {
        
       [Key]
        public required string CharName { get; set; }
        public required bool CharSelected { get; set; }
        
    }
}
