namespace ConsoleExample;

// I'm not sure about grammar because I am not England :(
static class Strings
{
    #region Common
    public static string Greeting = 
        "Welcome to ExpressParser CLI utility! Type \"help\" to get command list.";

    public static string UnknownCommand(string cmd) =>
        $"Unknown command: \"{cmd}\". Type 'help' for more info.";

    public static string Help =
@"> 'exit' - close program
> 'help' - print this message
> 'clear' - clears the console
> 'create <NAME> <EXPRESSION>' - create expression with name <NAME>
> 'compile <EXPRESSION>' - create IL code for expression <EXPRESSION>
> 'set <EXPRESSION> <ARGUMENT> <VALUE>' - set argument <ARGUMENT> of expression <EXPRESSION> to <VALUE>
> 'eval <EXPRESSION>' - evaluates exression and prints result of evaluation
> 'eval-il <EXPRESSION>' - evaluates compiled code of exression and prints result of evaluation
> 'list <EXPRESSION>' - prints arguments (and values) of expression";
    #endregion
    #region Error while exectuing command
    public static string ErrorCreateExpr(Exception e) =>
    $"Error while parsing expression: {e}";

    public static string ErrorCompileExpr(Exception e) =>
        $"Error while compile expression: {e}";

    public static string ErrorEval(Exception e) =>
        $"Error while evaluating expression: {e}";

    public static string ErrorSetVal(Exception e) =>
        $"Error while setting value: {e}";
    #endregion
    #region Success/Result
    public static string CreateSuccess(string name) =>
    $"Created expression \"{name}\".";

    public static string CompileSuccess(string name) =>
        $"Compiled expression \"{name}\".";

    public static string SetSuccess(string expr, string arg, string val) =>
        $"Success: {expr}.{arg}={val}";

    public static string ListEmpty(string expr) =>
        $"Expression \"{expr}\" contains no arguments.";

    public static string PrintEval(double res) =>
        $"Evaluation result: {res}";

    public static string ListStart(string expr) =>
        $"Arguments of expression \"{expr}\":";

    public static string ListEntry(string arg, double val, bool isLast) =>
        $"{(isLast ? '╙' : '╟')}{arg}\t\t\t= {val}";
    #endregion
    #region Invalid command error
    public static string ErrorArgCount(int c) =>
        $"Error: {c} arguments excpected. Type \'help\' for more info.";

    public static string ErrorExprExists(string name) =>
        $"Expression \"{name}\" already exists.";

    public static string ErrorExprNotFound(string name) =>
        $"Expression \"{name}\" not found.";

    public static string ErrorKeyNotFound(string name, string expr) =>
        $"Key \"{name}\" not found in expression {expr}.";

    public static string ErrorInvalidDouble(string val) =>
        $"\"{val}\" is invalid number.";
    #endregion
}