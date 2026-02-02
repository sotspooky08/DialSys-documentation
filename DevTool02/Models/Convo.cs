
using CommunityToolkit.Mvvm.Messaging;

namespace DevTool01.Models
{
    public class Convo
    {
        public int ConvoId { get; set; }
        public required string CharNames { get; set; }
        public required string Location { get; set; }
        
        public  string? Remark { get; set; }

        public string? Condition { get; set; }
        
    }
}
