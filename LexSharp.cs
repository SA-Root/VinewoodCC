using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace LexSharp
{
    class TaskArgs
    {
        public int s;
        public int e;
        public int workerid;
        public TaskArgs(int ss, int ee, int wid)
        {
            s = ss;
            e = ee;
            workerid = wid;
        }
    }
    public class Program
    {
        public string OutputFile { get; set; }
        private readonly Regex RxFloat = new Regex(@"\A(0[xX][\dA-Fa-f]+(\.[\dA-Fa-f]+)?[pP][\+\-]?\d+[flFL]?|\d*(\.\d*([eE][\+\-]?\d+)?|[eE][\+\-]?\d+)[flFL]?)", RegexOptions.Compiled);
        private readonly Regex RxInteger = new Regex(@"\A((((0x|0X)[\dA-Fa-f]+)|\d+|(0[0-7]+))(llu|ull|LLU|ULL|LU|lu|ul|UL|ll|LL|[ulUL])?)", RegexOptions.Compiled);
        private readonly Regex RxChar = new Regex(@"\A([uUL]?'(\\U[\dA-Fa-f]{8}|\\u[\dA-Fa-f]{4}|\\x[\dA-Fa-f]{1,2}|\\0?[0-7]{1,2}|\\""|\\'|\\\?|\\\\|\\a|\\b|\\f|\\n|\\r|\\t|\\v\\\n|[^'\\])')", RegexOptions.Compiled);
        private readonly Regex RxIdentifier = new Regex(@"\A([_a-zA-Z](_|\w)*)", RegexOptions.Compiled);
        private readonly Regex RxKeywords = new Regex(@"\A(auto|break|case|char|const|continue|default|double|do|else|enum|extern|float|for|goto|if|inline|int|long|register|restrict|return|short|signed|sizeof||struct|switch|typedef|union|unsigned|void|volatile|while)", RegexOptions.Compiled);
        private readonly Regex RxOperators = new Regex(@"\A(%:%:|%:|\#\#|<:|:>|<%|%>|(<<|>>|&|\*|/|%|\+|-|!|=|\||\^|>|<)=|&&|\|\||\.\.\.|--|<<|>>|\+\+|->|[&/~!%<>:;=,\-\*\+\[\]\(\)\{\}\.\^\|\?\#])", RegexOptions.Compiled);
        private readonly Regex RxString = new Regex(@"\A((u8|u|U|L)?""(\\U[\dA-Fa-f]{8}|\\u[\dA-Fa-f]{4}|\\x[\dA-Fa-f]{1,2}|\\0?[0-7]{1,2}|\\""|\\'|\\\?|\\\\|\\a|\\b|\\f|\\n|\\r|\\t|\\v|[^\\""])*"")", RegexOptions.Compiled);
        private readonly string TokenTemplate = "[@{0},{1}:{2}='{3}',<{4}>,{5}:{6}]";
        private readonly string TokenTemplateP = "{0}:{1}='{2}',<{3}>,{4}:{5}]";
        private readonly string TokenTemplatePPrefix = "[@{0},";
        private ImmutableArray<string> SourceCode { get; set; }
        private ImmutableArray<int> PreviousCharacters { get; set; }
        private List<string> Tokens { get; set; } = new List<string>();
        private List<string>[] TokensP { get; set; }
        private int ReadCodeFile(string fpath)
        {
            var tmpCode = new List<string>();
            var tmpPrev = new List<int>();
            try
            {
                var stream = new FileStream(fpath, FileMode.Open, FileAccess.Read);
                using (var reader = new StreamReader(stream))
                {
                    tmpPrev.Add(0);
                    while (!reader.EndOfStream)
                    {
                        tmpCode.Add(reader.ReadLine());
                        tmpPrev.Add(tmpPrev[tmpPrev.Count - 1] + 2 + tmpCode[tmpCode.Count - 1].Length);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine($"Could not open file: {fpath}");
                return 1;
            }
            SourceCode = tmpCode.ToImmutableArray();
            PreviousCharacters = tmpPrev.ToImmutableArray();
            return 0;
        }
        private int WriteTokens(string fpath)
        {
            try
            {
                var stream = new FileStream(fpath, FileMode.Create, FileAccess.Write);
                using (var writer = new StreamWriter(stream))
                {
                    foreach (var l in Tokens)
                    {
                        writer.WriteLine(l);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine($"Could not write file: {fpath}");
                return 1;
            }
            return 0;
        }
        private void GenToken(int TokenIndex, int StartRow, int EndRow, string Code, string TokenType, int StartLine, int StartRow2)
        => Tokens.Add(String.Format(TokenTemplate, TokenIndex, StartRow, EndRow, Code, TokenType, StartLine, StartRow2));
        private void AddEOF(string fpath)
        {
            string OneLine = "";
            try
            {
                var stream = new FileStream(fpath, FileMode.Open, FileAccess.Read);
                using (var reader = new StreamReader(stream))
                {
                    OneLine = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            if (OneLine.Length >= 1)
            {
                //current line EOF
                if (OneLine[OneLine.Length - 1] != '\n')
                {
                    //one line
                    if (SourceCode.Length == 1)
                    {
                        GenToken(Tokens.Count, PreviousCharacters[1] - 2, PreviousCharacters[1] - 3, "<EOF>", "EOF", SourceCode.Length, SourceCode[0].Length);
                    }
                    //multi-line
                    else
                    {
                        GenToken(Tokens.Count, PreviousCharacters[PreviousCharacters.Length - 1] - 2, PreviousCharacters[PreviousCharacters.Length - 1] - 3, "<EOF>", "EOF", SourceCode.Length, SourceCode[SourceCode.Length - 1].Length);
                    }
                    return;
                }
                //next line EOF
                GenToken(Tokens.Count, PreviousCharacters[PreviousCharacters.Length - 1] + 1, PreviousCharacters[PreviousCharacters.Length - 1], "<EOF>", "EOF", SourceCode.Length + 1, 0);
            }
        }
        private int ProcessLine(int lindex)
        {
            var line = SourceCode[lindex];
            int Offset = 0;
            while (Offset < line.Length)
            {
                if (line[Offset] == ' '||line[Offset] == '\t')
                {
                    ++Offset;
                    continue;
                }
                //Operators
                var match = RxOperators.Match(line.Substring(Offset));
                if (match.Success)
                {
                    GenToken(Tokens.Count, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, $"'{match.Value}'", lindex + 1, Offset);
                    Offset += match.Value.Length;
                    continue;
                }
                //String
                match = RxString.Match(line.Substring(Offset));
                if (match.Success)// && match.Index == Offset
                {
                    GenToken(Tokens.Count, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, "StringLiteral", lindex + 1, Offset);
                    Offset += match.Value.Length;
                    continue;
                }
                //Char
                match = RxChar.Match(line.Substring(Offset));
                if (match.Success)
                {
                    GenToken(Tokens.Count, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, "CharacterConstant", lindex + 1, Offset);
                    Offset += match.Value.Length;
                    continue;
                }
                //FloatingConstant
                match = RxFloat.Match(line.Substring(Offset));
                if (match.Success)// && match.Index == Offset
                {
                    GenToken(Tokens.Count, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, "FloatingConstant", lindex + 1, Offset);
                    Offset += match.Value.Length;
                    continue;
                }
                //IntegerConstant
                match = RxInteger.Match(line.Substring(Offset));
                if (match.Success)// && match.Index == Offset
                {
                    GenToken(Tokens.Count, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, "IntegerConstant", lindex + 1, Offset);
                    Offset += match.Value.Length;
                    continue;
                }
                //Keywords
                match = RxKeywords.Match(line.Substring(Offset));
                if (Offset + match.Value.Length >= line.Length
                || (!Char.IsLetterOrDigit(line[Offset + match.Value.Length])
                && line[Offset + match.Value.Length] != '_'))
                {
                    if (match.Success)
                    {
                        GenToken(Tokens.Count, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, $"'{match.Value}'", lindex + 1, Offset);
                        Offset += match.Value.Length;
                        continue;
                    }
                }
                //Identifier
                match = RxIdentifier.Match(line.Substring(Offset));
                if (match.Success)
                {
                    GenToken(Tokens.Count, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, "Identifier", lindex + 1, Offset);
                    Offset += match.Value.Length;
                    continue;
                }
                ++Offset;
            }
            return 0;
        }
        private int ProcessLineP()
        {
            var Cores = Environment.ProcessorCount;
            TokensP = new List<string>[Cores];
            var tasks = new Task[Cores];
            var OneJob = SourceCode.Length / Cores;
            for (int i = 0; i < Cores; ++i)
            {
                TokensP[i] = new List<string>();
                if (i == Cores - 1 && Cores * OneJob != SourceCode.Length)
                {
                    tasks[i] = Task.Factory.StartNew(ProcessLineWorker, new TaskArgs(i * OneJob, SourceCode.Length - 1, i));
                }
                else
                {
                    tasks[i] = Task.Factory.StartNew(ProcessLineWorker, new TaskArgs(i * OneJob, (i + 1) * OneJob - 1, i));
                }
            }
            Task.WaitAll(tasks);
            MergeTokens();
            return 0;
        }
        private void MergeTokens()
        {
            for (int i = 0; i < Environment.ProcessorCount; ++i)
            {
                foreach (var t in TokensP[i])
                {
                    Tokens.Add(String.Format(TokenTemplatePPrefix, Tokens.Count) + t);
                }
            }
        }
        private int ProcessLineWorker(object tt)
        {
            var t = tt as TaskArgs;
            var s = t.s;
            var e = t.e;
            var workerid = t.workerid;
            for (var j = s; j <= e; ++j)
            {
                var lindex = j;
                var line = SourceCode[lindex];
                int Offset = 0;
                while (Offset < line.Length)
                {
                    if (line[Offset] == ' ')
                    {
                        ++Offset;
                        continue;
                    }
                    //Operators
                    var match = RxOperators.Match(line.Substring(Offset));
                    if (match.Success)
                    {
                        TokensP[workerid].Add(String.Format(TokenTemplateP, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, $"'{match.Value}'", lindex + 1, Offset));
                        Offset += match.Value.Length;
                        continue;
                    }
                    //String
                    match = RxString.Match(line.Substring(Offset));
                    if (match.Success)// && match.Index == Offset
                    {
                        TokensP[workerid].Add(String.Format(TokenTemplateP, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, "StringLiteral", lindex + 1, Offset));
                        Offset += match.Value.Length;
                        continue;
                    }
                    //Char
                    match = RxChar.Match(line.Substring(Offset));
                    if (match.Success)
                    {
                        TokensP[workerid].Add(String.Format(TokenTemplateP, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, "CharacterConstant", lindex + 1, Offset));
                        Offset += match.Value.Length;
                        continue;
                    }
                    //FloatingConstant
                    match = RxFloat.Match(line.Substring(Offset));
                    if (match.Success)// && match.Index == Offset
                    {
                        TokensP[workerid].Add(String.Format(TokenTemplateP, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, "FloatingConstant", lindex + 1, Offset));
                        Offset += match.Value.Length;
                        continue;
                    }
                    //IntegerConstant
                    match = RxInteger.Match(line.Substring(Offset));
                    if (match.Success)// && match.Index == Offset
                    {
                        TokensP[workerid].Add(String.Format(TokenTemplateP, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, "IntegerConstant", lindex + 1, Offset));
                        Offset += match.Value.Length;
                        continue;
                    }
                    //Keywords
                    match = RxKeywords.Match(line.Substring(Offset));
                    if (Offset + match.Value.Length >= line.Length
                    || (!Char.IsLetterOrDigit(line[Offset + match.Value.Length])
                    && line[Offset + match.Value.Length] != '_'))
                    {
                        if (match.Success)
                        {
                            TokensP[workerid].Add(String.Format(TokenTemplateP, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, $"'{match.Value}'", lindex + 1, Offset));
                            Offset += match.Value.Length;
                            continue;
                        }
                    }
                    //Identifier
                    match = RxIdentifier.Match(line.Substring(Offset));
                    if (match.Success)
                    {
                        TokensP[workerid].Add(String.Format(TokenTemplateP, PreviousCharacters[lindex] + Offset, PreviousCharacters[lindex] + Offset + match.Value.Length - 1, match.Value, "Identifier", lindex + 1, Offset));
                        Offset += match.Value.Length;
                        continue;
                    }
                    ++Offset;
                }
            }
            return 0;
        }
        public void Run(string arg)
        {
            //test.Run();
            var start = System.DateTime.Now;
            Console.WriteLine($"Reading \"{arg}\"...");
            if (ReadCodeFile(arg) != 0) return;
            OutputFile = arg.Substring(0, arg.LastIndexOf('.')) + ".tokens";
            Console.WriteLine("Generating tokens...");
            //ProcessLineP();
            for (int i = 0; i < SourceCode.Length; ++i)
            {
                ProcessLine(i);
            }
            AddEOF(arg);
            Console.WriteLine($"{Tokens.Count} tokens generated.");
            Console.WriteLine($"Writing to \"{OutputFile}\"...");
            WriteTokens(OutputFile);
            var end = System.DateTime.Now;
            Console.WriteLine($"Done in {(end - start).TotalMilliseconds}ms.");
        }
        public void Run2(string arg)
        {
            if (ReadCodeFile(arg) != 0) return;
            OutputFile = arg.Substring(0, arg.LastIndexOf('.')) + ".tokens";
            Console.WriteLine("Generating tokens...");
            for (int i = 0; i < SourceCode.Length; ++i)
            {
                ProcessLine(i);
            }
            AddEOF(arg);
            Console.WriteLine($"{Tokens.Count} tokens generated.");
            WriteTokens(OutputFile);
        }
    }
}
