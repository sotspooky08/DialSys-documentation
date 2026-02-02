using DevTool01.Data;
using DevTool01.Models;
using Microsoft.Maui.Graphics.Text;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DevTool01;

public partial class EditConvoPage : ContentPage
{
    private readonly DataContext _dataContext;
    public ObservableCollection<DialNodes> ObscDialNodes { get; } = new ObservableCollection<DialNodes>();


    private string _remark = string.Empty;
    private string _condition = string.Empty;

    public List<string> _charNames { get; set; }
    public string _location { get; set; }
    public int LastIndex { get; set; }
    public List<MainDial> _TempMainDial { get; set; }
    public DialNodeList RootList { get; set; }
    public int BlockIndex { get; set; } = 1;

    public EditConvoPage(DataContext dataContext, List<string> CharNames, string Location, string? Remark, string? Condition)
    {
        InitializeComponent();
        _dataContext = dataContext;
        BindingContext = this;
        _TempMainDial = new List<MainDial>();

        LastIndex = _dataContext.MainDials.Count<MainDial>();

        _charNames = CharNames;
        _location = Location;
        _remark = Remark ?? "";
        _condition = Condition ?? "";

        RootList = new DialNodeList(this);
        RootNode.Children.Add(RootList);
    }
    public class locActionLayout : HorizontalStackLayout
    {
        Picker actionPicker = new() { Title = "Select Action(optional)", HorizontalOptions = LayoutOptions.Fill};

        Picker expressionPicker = new() { Title = "Select Expression", HorizontalOptions = LayoutOptions.Fill };
        Entry UpdateStats = new() { Placeholder = "Enter stat update details", HorizontalOptions = LayoutOptions.Fill };
        Editor otherAction = new() { Placeholder = "Enter other action details", HorizontalOptions = LayoutOptions.Fill };

        public locActionLayout()
        {
            actionPicker.Items.Add("None");
            actionPicker.Items.Add("Expression");
            actionPicker.Items.Add("Update stats");
            actionPicker.Items.Add("other");
            actionPicker.SelectedIndex = 0;
            actionPicker.SelectedIndexChanged += (s, e) => {
                if (actionPicker.SelectedItem is string selectedAction) {
                    switch (selectedAction) {
                        case "None":
                            RemoveRest();
                            break;
                        case "Expression":
                            selectedExpression(this);
                            break;
                        case "Update stats":
                            selectedUpdateStats(this);
                            break;
                        case "other": {
                                selectedUpdateStats(this);
                                break;
                            }
                    }
                }
            };

            expressionPicker.Items.Add("Happy");
            expressionPicker.Items.Add("Sad");
            expressionPicker.Items.Add("Angry");

            Children.Add(actionPicker);
        }

        public void selectedExpression(Layout layout) {
            RemoveRest();
            Children.Add(expressionPicker);

        }
        public void selectedUpdateStats(Layout layout)
        {
            RemoveRest();
            Children.Add(UpdateStats);
        }
        public void selectedOtherAction(Layout layout)
        {
            RemoveRest();
            Children.Add(otherAction);
        }
        public void RemoveRest() {
            if (Children.Contains(expressionPicker)) {
                Children.Remove(expressionPicker);
            }
            if (Children.Contains(UpdateStats)) {
                Children.Remove(UpdateStats);
            }
            if (Children.Contains(otherAction)) {
                Children.Remove(otherAction);
            }
        }
        public string GetActionDetails() {
            if (Children.Contains(expressionPicker)) {
                return "Expression:" + (expressionPicker.SelectedItem?.ToString() ?? "None");
            }
            if (Children.Contains(UpdateStats)) {
                return "UpdateStats:" + UpdateStats.Text;
            }
            if (Children.Contains(otherAction)) {
                return "OtherAction:" + otherAction.Text;
            }
            return "None";
        }
    }
    public class locDialogueLine : HorizontalStackLayout
    {
        public int mainIndex;
        public string NextIndex = "";
        public MainDial? _mainDial;

        public Label labelMainIndex = new() { FontSize=30 };

        public Picker Speaker = new() { Title = "Select Speaker", HorizontalOptions = LayoutOptions.Fill };

        public Editor dialEditor = new() { Text = "", HeightRequest = 100, Placeholder = "Enter Dialogue", HorizontalOptions = LayoutOptions.Fill };

        public locActionLayout ActionLayout = new();
        public Button removeBtn = new() { Text = "Remove", HorizontalOptions = LayoutOptions.End};

        public VerticalStackLayout UpDown = new() { Spacing = 10 };
        public Button Up = new() { Text = "UP", WidthRequest = 50 };
        public Button Down = new() { Text = "DOWN", WidthRequest = 50 };
        public locDialogueLine(EditConvoPage parentPage)
        {
            Spacing = 10;
            HorizontalOptions = LayoutOptions.Fill;
            Padding = 10;

            removeBtn.Clicked += parentPage.RemoveNode;

            Up.Clicked += parentPage.MoveUp;
            Down.Clicked += parentPage.MoveDown;

            UpDown.Children.Add(Up);
            UpDown.Children.Add(Down);

            foreach (var name in parentPage._charNames) {
                Speaker.Items.Add(name);
            }

            Children.Add(labelMainIndex);
            Children.Add(Speaker); ;
            Children.Add(dialEditor);
            Children.Add(ActionLayout);
            Children.Add(UpDown);
            Children.Add(removeBtn);
        }
    }

    public class ChoiceBlockLayout : VerticalStackLayout
    {
        public readonly EditConvoPage _parentPage;

        public ChoiceBlockLayout(EditConvoPage parentPage)
        {
            _parentPage = parentPage;
            Spacing = 10;
            Children.Add(returnChoiceBlockLayout());
        }
        public List<Choice> cblChoices = new();
        public Layout returnChoiceBlockLayout()
        {
            HorizontalStackLayout main_hsl = new();
            VerticalStackLayout vsl = new VerticalStackLayout
            {
                Padding = 10,
                Spacing = 10,
            };
            Button RemoveBtn = new Button() { Text = "Remove Choice Block", BackgroundColor = Colors.Red, HorizontalOptions = LayoutOptions.Start };
            RemoveBtn.Clicked += _parentPage.RemoveNode;
            Border border = new Border()
            {
                StrokeThickness = 2,
                Padding = 5,
                MinimumHeightRequest = 100,
                Stroke = Colors.White,
                Content = vsl
            };
            Button AddChoiceBtn = new Button() { Text = "+ Add Choice", HorizontalOptions = LayoutOptions.Start };
            AddChoiceBtn.Clicked += _parentPage.AddChoiceBlock;
            vsl.Children.Add(RemoveBtn);
            vsl.Children.Add(AddChoiceBtn);

            var spacer = new BoxView() { WidthRequest = 50 };
            main_hsl.Children.Add(spacer);
            main_hsl.Children.Add(vsl);

            return main_hsl;
        }
    }



    public class Choice : VerticalStackLayout
    {
        public int _CBIndex { get; set; }
        public Editor PlayerChoice { get; set; }

        public DialNodeList DialogueList { get; set; }
        private readonly EditConvoPage _parentPage;
        HorizontalStackLayout hsl1 = new() { Spacing = 10 };
        public Choice(EditConvoPage parentPage, Layout parentNode)
        {
            DialogueList = new DialNodeList(parentPage);

            PlayerChoice = new() { Text = "", HeightRequest = 50, Placeholder = "Enter Player Choice" };
            _parentPage = parentPage;
            returnChoice();
        }
        public void returnChoice()
        {
            Button RemoveChoice = new Button() { Text = "X", BackgroundColor = Colors.Red };
            RemoveChoice.Clicked += _parentPage.RemoveNode;
            Editor choiceText = new() {
                Text = "",
                MinimumHeightRequest = 100,
                Placeholder = "Enter Dialogue for this choice"
            };
            hsl1.Children.Add(PlayerChoice);
            hsl1.Children.Add(RemoveChoice);
            Children.Add(hsl1);
            Children.Add(DialogueList);
        }
    }


    public class DialNodeList : VerticalStackLayout
    {
        private readonly EditConvoPage _parentPage;
        public List<locDialogueLine> listDialLines { get; set; }
        public int StepIndex { get; set; }
        public DialNodeList(EditConvoPage parentPage)
        {
            _parentPage = parentPage;
            Button AddDial = new() { Text = "Add Dialogue", HorizontalOptions = LayoutOptions.Start };
            Button AddChoice = new() { Text = "Diverge", HorizontalOptions = LayoutOptions.Start };
            AddDial.Clicked += AddDialNode;
            
            AddChoice.Clicked += AddChoiceBlockLayout;

            Children.Add(AddChoice);
            Children.Add(AddDial);
            listDialLines = new List<locDialogueLine>();
        }
        public List<MainDial> Indexiate(List<MainDial> _finalMainDial, int _baseIndex)
        {
            StepIndex = 1+ _baseIndex;
            if (Parent is Choice cb) {
                if (cb.Parent.Parent.Parent.Parent is DialNodeList dnl) {
                    StepIndex = dnl.StepIndex;
                }
            }
            foreach (locDialogueLine ldl in listDialLines) {
                ldl.mainIndex = StepIndex;
                ldl.labelMainIndex.Text = StepIndex.ToString();
                ldl.Children.Remove(ldl.Children[0]);
                ldl.Children.Insert(0, ldl.labelMainIndex);
                //_parentPage.DisplayAlert("Indexing", "Assigned Index " + StepIndex + " to Dialogue Line", "OK");
                StepIndex++;
                ldl._mainDial = new MainDial
                {
                    Index = ldl.mainIndex,
                    Dialogue = ldl.dialEditor.Text,
                    Speaker = ldl.Speaker.SelectedItem?.ToString() ?? "Unknown",
                    Action = ldl.ActionLayout.GetActionDetails(),
                    NextIndex = StepIndex.ToString()
                };
                ldl.NextIndex = StepIndex.ToString();
                _finalMainDial.Add(ldl._mainDial);
            }
            if (Children[Children.Count-1] is ChoiceBlockLayout cbl){
                if (listDialLines.Count > 0) {
                var lastLDL = listDialLines[listDialLines.Count - 1]; //-1 to skip the ChoiceBlockLayout
                var lastMDL = _finalMainDial[_finalMainDial.Count - 1];
                    if (cbl.Children[0] is HorizontalStackLayout hsl) {
                        if (hsl.Children[1] is VerticalStackLayout vsl) {
                            var listVSL = vsl.Children.ToList();
                            if (listVSL.Count > 1) {
                                int choices = listVSL.Count;
                                int countChoices = choices;
                                int upcounter = 2;
                                while (upcounter < countChoices) {
                                    var choice = listVSL[upcounter]; //+1 to skip the Remove and Add buttons
                                    if (choice is Choice c) {
                                        var choiceDial = new MainDial
                                        {
                                            Speaker = "_player_",
                                            Dialogue = c.PlayerChoice.Text,
                                            NextIndex = (StepIndex + 1).ToString(),
                                            Index = StepIndex
                                        };
                                        _finalMainDial.Add(choiceDial);
                                        if (upcounter < countChoices && StepIndex.ToString() != lastMDL.NextIndex) {
                                            lastLDL.NextIndex += ("," + StepIndex.ToString());
                                            lastMDL.NextIndex += ("," + StepIndex.ToString());
                                        }
                                        StepIndex++;
                                        if (c.DialogueList is DialNodeList dnl) {
                                            var _subMainDial = new List<MainDial>();

                                            dnl.Indexiate(_subMainDial, StepIndex);
                                            if (_subMainDial.Count > 0) {
                                                _subMainDial[_subMainDial.Count - 1].NextIndex = "0";
                                            } else {
                                                choiceDial.NextIndex = "0";
                                            }
                                                foreach (var subDial in _subMainDial) {
                                                    _finalMainDial.Add(subDial);
                                                }
                                            StepIndex = dnl.StepIndex;
                                        } else {
                                            _parentPage.DisplayAlert("Error", "Choice's DialogueList is not DialNodeList", "OK");
                                        }
                                    } else if (choice is Button btn) {
                                        _parentPage.DisplayAlert("Info", btn.Text, "OK");
                                    } else {
                                        _parentPage.DisplayAlert("Error", "Child in ChoiceBlockLayout is not Choice", "OK");
                                        _parentPage.DisplayAlert("Error", "Offending Child: " + choice.ToString(), "OK");
                                    }
                                    upcounter++;
                                    
                                }
                                //_parentPage.DisplayAlert("chocies", "The choice indexes: "+ lastLDL.NextIndex, "ok");
                            } else {
                                _parentPage.DisplayAlert("Error", "No Choices found in ChoiceBlockLayout", "OK");
                            }
                        } else {
                            _parentPage.DisplayAlert("Error", "ChoiceBlockLayout's first child's second child is not VerticalStackLayout", "OK");
                        }
                    } else {
                        _parentPage.DisplayAlert("Error", "ChoiceBlockLayout's first child is not HorizontalStackLayout", "OK");
                    }
                }
            }
            return _finalMainDial;
        }
        public void AddDialNode(object? sender, EventArgs e)
        {
            if (sender is Button btn) {
                if (btn.Parent is DialNodeList dnl) {
                    locDialogueLine newDialLine = new(_parentPage);
                    newDialLine.mainIndex = StepIndex + dnl.Children.Count-2;
                    if (dnl.Children[1] is Button btn1) {
                        dnl.Children.Add(newDialLine);
                        dnl.listDialLines.Add(newDialLine);
                    } else {
                        dnl.Children.Insert(dnl.Children.Count - 1, newDialLine);
                        dnl.listDialLines.Add(newDialLine);
                    }
                } else {
                    _parentPage.DisplayAlert("Error", "Parent is not DialNodeList", "OK");
                }
            }
        }
        public void AddChoiceBlockLayout(object? sender, EventArgs e)
        {
            if (sender is Button btn) {
                if (btn.Parent is DialNodeList dnl) {
                    ChoiceBlockLayout CBL = new(_parentPage);
                    dnl.Children.Add(CBL);
                    dnl.Children.Remove(btn);
                }
            }
        }
    }
    public void MoveUp(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.Parent.Parent.Parent is DialNodeList _dialNodeList && btn.Parent.Parent is locDialogueLine HSL) {
            if (_dialNodeList.Children.IndexOf(HSL) is int intex) {
                if (_dialNodeList.Children[1] is Button) {
                    if (intex > 2) {
                        _dialNodeList.Children.Remove(HSL);
                        _dialNodeList.Children.Insert(intex - 1, HSL);
                        _dialNodeList.listDialLines.Remove(HSL);
                        _dialNodeList.listDialLines.Insert(intex - 3, HSL);
                    } else {
                        DisplayAlert("Error", "Cannot move up further", "OK");
                    }
                } else {
                    if (intex > 1) {
                        _dialNodeList.Children.Remove(HSL);
                        _dialNodeList.Children.Insert(intex - 1, HSL);
                        _dialNodeList.listDialLines.Remove(HSL);
                        _dialNodeList.listDialLines.Insert(intex - 3, HSL);
                    } else {
                        DisplayAlert("Error", "Cannot move up further", "OK");
                    }
                }
            }
        } else if (sender is Button btn1) {
            DisplayAlert("Error", "Cannot move up\n" + (btn1.Parent.Parent).ToString(), "OK");
        }
    }
    public void MoveDown(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.Parent.Parent.Parent is DialNodeList _dialNodeList && btn.Parent.Parent is locDialogueLine HSL) {
            if (_dialNodeList.Children.IndexOf(HSL) is int intex) {
                if (_dialNodeList.Children[1] is Button) {
                    if (intex < _dialNodeList.Children.Count - 1) {
                        _dialNodeList.Children.Remove(HSL);
                        _dialNodeList.Children.Insert(intex + 1, HSL);
                        _dialNodeList.listDialLines.Remove(HSL);
                        _dialNodeList.listDialLines.Insert(intex-1, HSL);
                    } else {
                        DisplayAlert("Error", "Cannot move down further", "OK");
                    }
                } else {
                    if (intex < _dialNodeList.Children.Count - 2) {
                        _dialNodeList.Children.Remove(HSL);
                        _dialNodeList.Children.Insert(intex-2, HSL);
                        _dialNodeList.listDialLines.Remove(HSL);
                        _dialNodeList.listDialLines.Insert(intex-1, HSL);
                    } else {
                        DisplayAlert("Error", "Cannot move down further", "OK");
                    }
                }
            }
        } else if (sender is Button btn1) {
            DisplayAlert("Error", "Cannot move down\n" + (btn1.Parent.Parent).ToString(), "OK");
        }
    }
    
    public void AddChoiceBlock(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.Parent.Parent.Parent is ChoiceBlockLayout cbl && btn.Parent is VerticalStackLayout vsl) {
            var n = new Choice(this, cbl);
            vsl.Children.Add(n);
            cbl.cblChoices.Add(n);
        } else if (sender is Button btn1) {
            DisplayAlert("Error", (btn1.Parent.Parent).ToString(), "OK");
        }
    }
    public async void RemoveNode(object? sender, EventArgs e)
    {
        //remove dialogue line
        if (sender is Button btn && btn.Parent is locDialogueLine hsl) {
            bool confirm = await DisplayAlert("You suree??", "Would you like to remove the dialogue line? All text entered here will be lost permanantly", "OK", "NO");
            if (confirm) {
                if (hsl.Parent is DialNodeList dnl) {
                    dnl.Children.Remove(hsl);
                    dnl.listDialLines.Remove(hsl);
                } else {
                    await DisplayAlert("HELA", "NOP", "okayy");
                }
            }
        //remove CBL
        } else if (sender is Button btn1 && btn1.Parent.Parent.Parent is ChoiceBlockLayout cbl && cbl.Parent is Layout parent) {
            bool confirm = await DisplayAlert("You suree??", "All content branching underneath the node will be lost permanantly", "OK", "NO");
            if (confirm) {
                bool confirm2 = await DisplayAlert("Are you REALLY sure??", "This action cannot be undone", "YES", "NO");
                if (confirm2) {
                    bool confirm3 = await DisplayAlert("removing entire CHOICE BLOCK LAYOUT", "Are you absolutely certain you want to ENTIRE CHOICE LAYOUT, note that this will remove all the CHOICES underneath it as well", "DELETE", "CANCEL");
                    if (confirm3) {
                        Button DivergeBtn = new() { Text = "Diverge", HorizontalOptions = LayoutOptions.Start };
                        if (cbl.Parent is DialNodeList dnl) {
                            DivergeBtn.Clicked += dnl.AddChoiceBlockLayout;
                        }

                        parent.Children.Insert(0, DivergeBtn);
                        parent.Remove(cbl);
                    }
                }
            }
        //remove choice
        } else if (sender is Button btn2 && btn2.Parent.Parent is Choice cb1 && cb1.Parent is Layout _parent) {
            bool confirm = await DisplayAlert("You suree??", "All content branching underneath the node will be lost permanantly", "OK", "NO");
            if (confirm) {
                bool confirm2 = await DisplayAlert("removing entire CHOICE NODE", "This action cannot be undone", "YES", "NO");
                if (confirm2) {
                    _parent.Remove(cb1);
                    if (_parent.Parent.Parent is ChoiceBlockLayout cbl1) {
                        cbl1.cblChoices.Remove(cb1);
                    }
                }
            }
        }
    }
    private int GetBaseIndex()
    {
        int baseIndex = 0;
        foreach (MainDial md in _dataContext.MainDials) {
            if (md.Index > baseIndex) {
                baseIndex = md.Index;
            }
        }
        return baseIndex;
    }
    private void SaveConvo(object sender, EventArgs e)
    {
        RootList.StepIndex = 1;
        List<MainDial> FinalMainDial = new List<MainDial>();
        FinalMainDial = RootList.Indexiate(FinalMainDial, GetBaseIndex());

        foreach (var md in FinalMainDial) {
            md.ConvoId = _dataContext.Convos.Count() + 1;
            _dataContext.MainDials.Add(md);
        }
        string charNames = string.Join(",", _charNames);
        _dataContext.Convos.Add(new Convo
        {
            ConvoId = _dataContext.Convos.Count() + 1,
            CharNames = charNames,
            Location = _location,
            Remark = _remark,
            Condition = _condition
        });
        foreach (var chars in _dataContext.Characters) {
            chars.CharSelected = false;
        }
        foreach (var locs in _dataContext.Locations) {
            locs.LocSelected = false;
        }
        _dataContext.SaveChanges();
        DisplayAlert("Indexiation Complete", "Last Step Index: " + RootList.StepIndex, "OK");
        Navigation.PushModalAsync(new MainPage(_dataContext));
    }
}