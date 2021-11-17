using ExpressParser;
using static ConsoleExample.Strings;

Dictionary<string, Expression> expressions = new();
Console.WriteLine(Greeting);
while(true)
{
    string command = Console.ReadLine().Trim();
    if (command == "exit") break;
    else if (command == "clear") Console.Clear();
    else if (command == "help") Console.WriteLine(Help);
    else if (command.StartsWith("compile")) Compile(command);
    else if (command.StartsWith("create")) Create(command);
    else if (command.StartsWith("set")) Set(command);
    else if (command.StartsWith("eval-il")) Eval(command, true);
    else if (command.StartsWith("eval")) Eval(command, false);
    else if (command.StartsWith("list")) List(command);
    else Console.WriteLine(UnknownCommand(command));
}

string[] GetTokens(string cmd, int start) => cmd[start..]
    .Trim().Split(' ').Where(s => s.Length > 0).ToArray();

void Create(string cmd)
{
    var tokens = GetTokens(cmd, 6);
    if(tokens.Length != 2)
        Console.WriteLine(ErrorArgCount(2));
    else if(expressions.ContainsKey(tokens[0]))
        Console.WriteLine(ErrorExprExists(tokens[0]));
    else
    {
        Expression expr = null;
        try
        {
            expr = new(tokens[1]);
        }
        catch (Exception e)
        {
            Console.WriteLine(ErrorCreateExpr(e));
        }
        finally
        {
            expressions.Add(tokens[0], expr);
            Console.WriteLine(CreateSuccess(tokens[0]));
        }
    }
}

void Set(string cmd)
{
    var tokens = GetTokens(cmd, 3);
    if (tokens.Length != 3)
        Console.WriteLine(ErrorArgCount(3));
    else if (!expressions.ContainsKey(tokens[0]))
        Console.WriteLine(ErrorExprNotFound(tokens[0]));
    else if (!expressions[tokens[0]].Arguments.ContainsKey(tokens[1]))
        Console.WriteLine(ErrorKeyNotFound(tokens[0], tokens[1]));
    else if(!double.TryParse(tokens[2], out double value))
        Console.WriteLine(ErrorInvalidDouble(tokens[2]));
    else
    {
        try
        {
            expressions[tokens[0]].SetArgument(tokens[1], value);
        }
        catch (Exception e)
        {
            Console.WriteLine(ErrorSetVal(e));
        }
        finally
        {
            Console.WriteLine(SetSuccess(tokens[0], tokens[1], tokens[2]));
        }
    }
}

void Eval(string cmd, bool compiled)
{
    var tokens = GetTokens(cmd, compiled ? 7 : 4); //compiled ? "eval-il" : "eval"
    if (tokens.Length != 1)
        Console.WriteLine(ErrorArgCount(1));
    else if (!expressions.ContainsKey(tokens[0]))
        Console.WriteLine(ErrorExprNotFound(tokens[0]));
    else
    {
        double res = 0;
        try
        {
            res = compiled ? 
                expressions[tokens[0]].EvaluateIL() : 
                expressions[tokens[0]].Evaluate();
        }
        catch (Exception e)
        {
            Console.WriteLine(ErrorEval(e));
        }
        finally
        {
            Console.WriteLine(PrintEval(res));
        }
    }
}

void List(string cmd)
{
    var tokens = GetTokens(cmd, 4);
    if (tokens.Length != 1)
        Console.WriteLine(ErrorArgCount(1));
    else if (!expressions.ContainsKey(tokens[0]))
        Console.WriteLine(ErrorExprNotFound(tokens[0]));
    else
    {
        Expression expr = expressions[tokens[0]];
        if(expr.Arguments.Count==0)
            Console.WriteLine(ListEmpty(tokens[0]));
        else
        {
            Console.WriteLine(ListStart(tokens[0]));
            var iter = expr.Arguments.GetEnumerator();
            iter.MoveNext();
            bool isLast = false;
            while (!isLast)
                Console.WriteLine(ListEntry(
                    iter.Current.Key, 
                    iter.Current.Value,
                    isLast = !iter.MoveNext()
                ));
        }
    }
}

void Compile(string cmd)
{
    var tokens = GetTokens(cmd, 7);
    if(tokens.Length != 1)
        Console.WriteLine(ErrorArgCount(1));
    else if(!expressions.ContainsKey(tokens[0]))
        Console.WriteLine(ErrorExprNotFound(tokens[0]));
    else
    {
        try
        {
            expressions[tokens[0]].Compile();
        }
        catch (Exception e)
        {
            Console.WriteLine(ErrorCompileExpr(e));
        }
        finally
        {
            Console.WriteLine(CompileSuccess(tokens[0]));
        }
    }
}