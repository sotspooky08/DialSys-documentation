using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class dial_ui_info : MonoBehaviour
{
    public static bool dial_visibility = false;
    public static string character_name;
    public static string location;
    public static bool opendial = false;
}

public class dialogue_manager : MonoBehaviour
{
    public GameObject choiceButtonPrefab;
    public Transform choiceContainer;
    public GameObject dialogue_panel;
    public TextMeshProUGUI body;
    public TextMeshProUGUI title;
    public Button continueButton;  

    private bool continueClicked = false;  

    int find_dial_index(string character,  string location) //Lookup conversation
    {
        int result=1;
        string allText = File.ReadAllText("Assets/DialData/DialIndex.txt");
        string[] lines = allText.Split('\n');
        foreach (string line in lines) {
            string[] elements = line.Split('-');
            if (elements[0] == character && elements[1] == location) {
                result = int.Parse(elements[2]);
            }
        }
        Debug.Log("opening dial no."+result);
        return result;
    }
    public void open_dial()
    {
        CanvasGroup group = dialogue_panel.GetComponent<CanvasGroup>();
        group.alpha = 1.0f;
    }

    public void close_dial()
    {
        CanvasGroup group = dialogue_panel.GetComponent<CanvasGroup>();
        group.alpha = 0.0f;
        Debug.Log("closed dialogue");
    }

    private void Start()
    {
        close_dial(); //dialogue panel invisbile until convo selected

        if (continueButton != null)  //Makes continueClicked true when continue button is clicked
            continueButton.onClick.AddListener(() => continueClicked = true);
    }

    void Update()
    {
        if (dial_ui_info.opendial) { //runs once
            open_dial();
            Dialogue d = DialogueParser.LoadDialogueFromIndex(find_dial_index(dial_ui_info.character_name,dial_ui_info.location));
            StartCoroutine(RunDialogue(d));
            dial_ui_info.opendial = false;
        }
    }

    void ShowText(string speaker, string text)
    {
        title.text = speaker;
        body.text = text;
    }

    IEnumerator ShowChoiceUIAndWait(string[] choices, Action<int> onChoiceSelected)
    {
        int selectedChoice = -1;

        // Clear old
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);

        // Create new
        for (int i = 0; i < choices.Length; i++) {
            int choiceIndex = i;
            GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = choices[i];

            btn.onClick.AddListener(() =>
            {
                selectedChoice = choiceIndex;
            });
        }

        // Wait for choice
        yield return new WaitUntil(() => selectedChoice != -1);

        // After choice
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);

        onChoiceSelected?.Invoke(selectedChoice);
    }

    private void ClearChoices()
    {
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);
    }


    void ApplyAction(string action)
    {
        Debug.Log("Action: " + action);
    }

    IEnumerator RunDialogue(Dialogue d)
    {
        var stack = new Stack<(List<DialogueNode> nodes, int nextIndex)>();
        List<DialogueNode> curr = d.Nodes;
        int i = 0;

        while (true) {
            if (i >= curr.Count) break;

            var node = curr[i];
            if (node is DialogueLine line) {
                ShowText(line.Speaker, line.Text);

                // wait for continue
                continueClicked = false;
                yield return new WaitUntil(() => continueClicked);

                i++;
            } else if (node is ActionNode act) {
                ApplyAction(act.Action);
                i++;
            } else if (node is ChoiceBlock cb) {
                int chosen = -1;
                yield return StartCoroutine(
                    ShowChoiceUIAndWait(cb.Options.Select(o => o.PlayerText).ToArray(),
                        (index) => chosen = index)
                );

                stack.Push((curr, i + 1));
                curr = cb.Options[chosen].Children;
                i = 0;
            } else if (node is EndNode) {
                if (stack.Count > 0) {
                    var state = stack.Pop();
                    curr = state.nodes;
                    i = state.nextIndex;
                } else ClearChoices();close_dial(); break;
            } else {
                i++;
            }
        }
    }
}

public static class DialogueParser
{
    private static Dictionary<int, Dialogue> allDialogues;

    private static void EnsureLoaded(string filePath = "Assets/DialData/DialData.txt")
    {
        if (allDialogues != null) return;

        string text = File.ReadAllText(filePath);
        var tokens = Lexer.Tokenize(text);
        var parser = new Parser(tokens);
        var dialogues = parser.ParseAll();

        allDialogues = new Dictionary<int, Dialogue>();
        foreach (var d in dialogues)
            allDialogues[d.Id] = d;
    }

    public static Dialogue LoadDialogueFromIndex(int id, string filePath = "Assets/DialData/DialData.txt")
    {
        EnsureLoaded(filePath);

        if (allDialogues.TryGetValue(id, out var d))
            return d;

        Debug.LogError($"Dialogue with ID {id} not found!");
        return null;
    }
}

public abstract class DialogueNode { }

public class DialogueLine : DialogueNode
{
    public string Speaker;
    public string Text;
}

public class ChoiceOption : DialogueNode
{
    public string PlayerText;
    public List<DialogueNode> Children = new List<DialogueNode>();
}

public class ActionNode : DialogueNode
{
    public string Action;
}

public class EndNode : DialogueNode { }

public class Dialogue
{
    public int Id;
    public List<DialogueNode> Nodes = new List<DialogueNode>();
}

class ChoiceBlock : DialogueNode
{
    public List<ChoiceOption> Options = new List<ChoiceOption>();
}

enum TokenType { Dial, CharacterLine, Quote, ChoiceStart, ChoiceIf, Action, End, BlockEnd, EOF, Unknown }

class Token
{
    public TokenType Type;
    public string Value;
    public int Line;

    public Token(TokenType type, string value, int line) { Type = type; Value = value; Line = line; }
    public override string ToString() => $"{Type}('{Value}')@{Line}";
}

class Lexer
{
    public static List<Token> Tokenize(string text)
    {
        var tokens = new List<Token>();
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        var dialRe = new Regex(@"^\s*dial\s+(\d+)\s*$", RegexOptions.IgnoreCase);
        var charLineRe = new Regex(@"^\s*([a-zA-Z0-9_]+)\s*:\s*""(.*)""\s*$");
        var choiceStart = new Regex(@"^\s*choice\s*{\s*$", RegexOptions.IgnoreCase);
        var choiceIfRe = new Regex(@"^\s*if\s+""(.+)""\s*$", RegexOptions.IgnoreCase);
        var endRe = new Regex(@"^\s*END\s*$", RegexOptions.IgnoreCase);
        var blockEndRe = new Regex(@"^\s*}\s*$");
        var actionRe = new Regex(@"^\s*([a-zA-Z0-9_]+\.[a-zA-Z0-9_\.+\-]*)\s*$");

        for (int i = 0; i < lines.Length; i++) {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("//")) continue;

            var m = dialRe.Match(line);
            if (m.Success) { tokens.Add(new Token(TokenType.Dial, m.Groups[1].Value, i + 1)); continue; }
            m = charLineRe.Match(line);
            if (m.Success) { tokens.Add(new Token(TokenType.CharacterLine, $"{m.Groups[1].Value}|{m.Groups[2].Value}", i + 1)); continue; }
            if (choiceStart.IsMatch(line)) { tokens.Add(new Token(TokenType.ChoiceStart, "", i + 1)); continue; }
            m = choiceIfRe.Match(line);
            if (m.Success) { tokens.Add(new Token(TokenType.ChoiceIf, m.Groups[1].Value, i + 1)); continue; }
            if (endRe.IsMatch(line)) { tokens.Add(new Token(TokenType.End, "END", i + 1)); continue; }
            if (blockEndRe.IsMatch(line)) { tokens.Add(new Token(TokenType.BlockEnd, "}", i + 1)); continue; }
            m = actionRe.Match(line);
            if (m.Success) { tokens.Add(new Token(TokenType.Action, m.Groups[1].Value, i + 1)); continue; }

            tokens.Add(new Token(TokenType.Unknown, line, i + 1));
        }

        tokens.Add(new Token(TokenType.EOF, "", lines.Length + 1));
        return tokens;
    }
}

class Parser
{
    List<Token> tokens;
    int pos = 0;
    public Parser(List<Token> tokens) { this.tokens = tokens; }

    Token Peek() => pos < tokens.Count ? tokens[pos] : tokens[tokens.Count - 1];
    Token Next() { var t = Peek(); pos++; return t; }
    bool Check(TokenType ttype) => Peek().Type == ttype;

    public List<Dialogue> ParseAll()
    {
        var result = new List<Dialogue>();
        while (!Check(TokenType.EOF)) {
            if (Check(TokenType.Dial)) {
                var tok = Next();
                var id = int.Parse(tok.Value);
                var body = ParseNodesUntil(new[] { TokenType.Dial, TokenType.EOF });
                result.Add(new Dialogue { Id = id, Nodes = body });
            } else Next();
        }
        return result;
    }

    List<DialogueNode> ParseNodesUntil(TokenType[] stopTokens)
    {
        var nodes = new List<DialogueNode>();
        while (Array.IndexOf(stopTokens, Peek().Type) < 0 && Peek().Type != TokenType.EOF) {
            var t = Peek();
            switch (t.Type) {
                case TokenType.CharacterLine:
                    var cl = Next().Value.Split('|', 2);
                    nodes.Add(new DialogueLine { Speaker = cl[0], Text = cl[1] });
                    break;
                case TokenType.Action:
                    nodes.Add(new ActionNode { Action = Next().Value });
                    break;
                case TokenType.ChoiceStart:
                    Next();
                    nodes.Add(ParseChoiceBlock());
                    break;
                case TokenType.End:
                    Next();
                    nodes.Add(new EndNode());
                    break;
                case TokenType.BlockEnd:
                    return nodes;
                default:
                    var bad = Next();
                    Debug.LogWarning($"Unexpected token {bad} at line {bad.Line}");
                    break;
            }
        }
        return nodes;
    }

    ChoiceBlock ParseChoiceBlock()
    {
        var cb = new ChoiceBlock();
        while (!Check(TokenType.BlockEnd) && !Check(TokenType.EOF)) {
            if (Check(TokenType.ChoiceIf)) {
                var playerText = Next().Value;
                var children = ParseNodesUntil(new[] { TokenType.ChoiceIf, TokenType.BlockEnd });
                cb.Options.Add(new ChoiceOption { PlayerText = playerText, Children = children });
            } else {
                var stray = Next();
                Debug.LogWarning($"Unexpected inside choice: {stray} at {stray.Line}");
            }
        }
        if (Check(TokenType.BlockEnd)) Next();
        return cb;
    }
}
