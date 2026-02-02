using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTool01.Models
{
    public abstract class DialNodes{}

    public class DialogueLine : DialNodes
    {
        public string Speaker { get; set; }
        public string Text { get; set; }
        public string? Action { get; set; }
        public DialogueLine(string speaker, string text, string? action)
        {
            Speaker = speaker;
            Text = text;
            Action = action;
        }
    }
    public class ChoiceOption
    {
        public string OptionText { get; set; }
        public List<DialNodes> Children { get; set; }
        public ChoiceOption(string optionText, List<DialNodes> children)
        {
            OptionText = optionText;
            Children = children;
        }
    }
    public class ChoiceBlock : DialNodes
    {
        public List<ChoiceOption> Options { get; set; }
        public ChoiceBlock(List<ChoiceOption> options)
        {
            Options = options;
        }
    }

}
