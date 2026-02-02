using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTool01.Models
{
    public class MainDial
    {

        [ForeignKey("Convo")]
        public int ConvoId { get; set; }
        [Key]
        public int Index { get; set; }
        public required string NextIndex { get; set; }
        
        public required string Dialogue { get; set; }
        public required string Speaker { get; set; }
        public string? Action { get; set; }
        
    }
}
