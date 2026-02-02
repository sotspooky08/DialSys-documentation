using DevTool01.Data;
using DevTool01.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTool01.Services
{
    public class DBInputService
    {
        private readonly DataContext _dataContext;
        private readonly List<DialNodes> _dialNodes;
        public int _lastIndex;
        public List<MainDial> _tempRecord= [];
        public DBInputService(DataContext DIDataContext,List<DialNodes> DIdialNodes,int DILastIndex)
        {
            _dialNodes = DIdialNodes;
            _dataContext = DIDataContext;
            _lastIndex = DILastIndex;
        }
        public void InputToDB()
        {
            
            
        }
        public void InputChunks(List<DialNodes> _listNodes, int lastindex)
        {
            int _lastTempIndex = lastindex;

            /*
            while (_listNodes.Count > 0) {
                var currentNode = _listNodes[0];
                _listNodes.RemoveAt(0);
                switch (currentNode) {
                    case DialogueLine dialogueLine:
                        _tempRecord.Add(new MainDial
                        {
                            Speaker = dialogueLine.Speaker,
                            Dialogue = dialogueLine.Text,
                            NextIndex = _lastIndex + 1
                        });
                        _lastTempIndex++;
                        _lastIndex++;
                        break;
                    case ChoiceBlock choiceBlock:
                        string ChoiceIndexList = "";
                        int Cindex = _lastIndex;
                        foreach (ChoiceOption choice in choiceBlock.Options) {
                            ChoiceIndexList += (Cindex + 1).ToString() + ",";
                            Cindex++;
                        }
                        ChoiceIndexList = ChoiceIndexList.TrimEnd(',');

                        _tempRecord[_lastTempIndex - 1].ChoiceIndex = ChoiceIndexList;
                        foreach (ChoiceOption choice in choiceBlock.Options) {
                            InputChunks(choice.Children,_lastTempIndex);
                        }
                        break;
                    case ActionNode actionNode:
                        string _action= actionNode.Action;
                        break;
                    case EndNode endNode:
                        _tempRecord[lastindex-1].NextIndex = 1;
                        break;
                    default:
                        throw new InvalidOperationException("Unknown node type");
                }
            }
            foreach (var record in _tempRecord) {
                _dataContext.MainDials.Add(record);
            }
            _dataContext.SaveChanges();*/
        }
    }
}
