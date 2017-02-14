using Jint;
using Jint.Native;
using Jint.Parser;
using Jint.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Daniel
{
    class Runtime
    {
        public const string commentPattern = @"[\s]*(\*|\()[^\*|\)]*(\*|\))[\s]*";
        public Engine Engine { private set; get; }
        public Dictionary<string, JsValue> JsFuncs { private set; get; }
        public Dictionary<string, Tuple<string, Action<string>>> MetaCommands { private set; get; }
        public Runtime() : this(new Engine())
        {
        }
        public Runtime(Engine eng)
        {
            Engine = eng;
            Engine.SetValue("log", new Action<object>(Console.WriteLine));
            Engine.SetValue("delay", new Action<int>(delegate (int a) { Thread.Sleep(a); }));
            Engine.SetValue("register", new Action<string, JsValue>(delegate (string name, JsValue a) { JsFuncs[name] = a; }));
            JsFuncs = new Dictionary<string, JsValue>();
            MetaCommands = new Dictionary<string, Tuple<string, Action<string>>>()
            {
                { "call", new Tuple<string, Action<string>>(@"(^| )(call) [\w]+( with [\w, ]+)?($|(?!\w))", new Action<string>(CallFunction)) },
                { "say", new Tuple<string, Action<string>>(@"(^| )(say) .+$", new Action<string>(SayMeta)) },
            };
        }
        public void Execute(CodeBase code)
        {
            code = RemoveComments(code);
            for (int i = 0; i < code.Code.Length; i++)
            {
                string input = code.Code[i];
                if (input == "")
                    continue;
                if (Regex.IsMatch(input.ToLower(), @"(^| )(leave|exit)($|(?!\w))"))
                    break;
                try
                {
                    foreach (string key in MetaCommands.Keys)
                    {
                        Match mt = Regex.Match(input.ToLower(), MetaCommands[key].Item1);
                        if (mt.Success)
                        {
                            MetaCommands[key].Item2(input.Substring(mt.Index, mt.Length));
                            break;
                        }
                    }
                }
                catch (Exception e)
                when (e is JavaScriptException || e is ArgumentException || e is KeyNotFoundException)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void CallFunction(string mt)
        {
            mt = RegexHelper.ReplaceIn(mt, @""".+""", " ", "`");
            string[] res = mt.Trim().Split(' ');
            res = RegexHelper.ReplaceIn(res, @"""", "");
            res = RegexHelper.ReplaceIn(res, "`", " ");
            if (res.Length == 2)
                Engine.GetValue(res[1]).Invoke();
            else if (res.Length > 3 && res[2] == "with")
            {
                List<string> d = new List<string>(string.Join(" ", res.Skip<string>(3)).Split(','));
                JsValue[] values = new JsValue[d.Count];
                for (int x = 0; x < d.Count; x++)
                {
                    values[x] = new JsValue(d[x].Trim());
                }
                JsFuncs[res[1]].Invoke(values);
            }
        }
        private void SayMeta(string mt)
        {
            mt = RegexHelper.ReplaceIn(mt, @""".+""", " ", "`");
            string[] res = mt.Trim().Split(' ');
            res = RegexHelper.ReplaceIn(res, @"""", "");
            res = RegexHelper.ReplaceIn(res, "`", " ");
            if (res.Length >= 2)
            {
                string d = string.Join(" ", res.Skip<string>(1));
                JsFuncs["say"].Invoke(d);
            }
        }

        public void LoadingScripts(string[] scripts)
        {
            foreach (string a in scripts)
            {
                try
                {
                    Engine.Execute(File.ReadAllText(a));
                }
                catch (Exception e)
                when (e is JavaScriptException || e is ParserException)
                {
                    Console.WriteLine("Error while loading {0}\n{1}", a, e.Message);
                }
            }
        }
        public void Init()
        {
            try
            {
                Engine.GetValue("init").Invoke();
            }
            catch (Exception e)
            when (e is ArgumentException)
            {
                Console.WriteLine("Init function is missed or corrupted.");
            }
            catch (Exception e)
            when (e is JavaScriptException || e is InvalidCastException)
            {
                Console.WriteLine("Error while init function proceed.\n{0}", e.Message);
            }
        }
        public static CodeBase RemoveComments(CodeBase input)
        {
            input.Code = Regex.Replace(string.Join("\n", input.Code), commentPattern, "").Split('\n');
            return input;
        }
    }
}
