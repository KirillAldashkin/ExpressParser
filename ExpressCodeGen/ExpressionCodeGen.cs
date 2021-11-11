using Microsoft.CodeAnalysis;
using System.Text;

namespace ExpressCodeGen;

/// <summary>
/// Generates EvaluateIL method to avoid using Delegate.InvokeDynamic
/// or MethodInfo.Invoke, which are much slower.
/// </summary>
[Generator]
public class ExpressionCodeGen : ISourceGenerator
{
    private const string begin = @"namespace ExpressParser;

public partial class Expression
{
    public partial double EvaluateIL() {
        var args = arguments.Values.ToArray();
        return argCount switch
        {
             0 => ((Func<double>)@delegate).Invoke(),
";
    private const string end = @"             _ => throw new Exception()
        };
    }
}";

    // What should be generated:
    /*
    public partial double EvaluateIL() {
        var args = arguments.Values.ToArray();
        return argCount switch
        {
             0 => ((Func<double>)@delegate).Invoke(),
             1 => ((Func<double, double>)@delegate).Invoke(args[0]),
            ...
            15 => ((Func<double, ... double>)@delegate).Invoke(args[0], ... args[14]),
            16 => ((Func<double, ... double>)@delegate).Invoke(args[0], ... args[14], args[15]),
             _ => throw new Exception()
        };
    }
    */
    public void Execute(GeneratorExecutionContext context)
    {
        StringBuilder srcBuilder = new(begin);
        for (int i = 1; i <= 16; i++)
        {
            if (i < 10) srcBuilder.Append(' ');
            srcBuilder.Append("            ").Append(i).Append(" => ((Func<");
            for (int j = 0; j < i; j++) srcBuilder.Append("double, ");
            srcBuilder.Append("double>)@delegate).Invoke(");
            for (int j = 0; j < i-1; j++) srcBuilder.Append("args[").Append(j).Append("], ");
            srcBuilder.Append("args[").Append(i-1).Append("]),\n");
        }
        srcBuilder.Append(end);
        context.AddSource("GeneratedEvaluateIL", srcBuilder.ToString());
    }
    public void Initialize(GeneratorInitializationContext context) { }
}
