using ExpressParser.Operations;
using System.Reflection;
using System.Reflection.Emit;

namespace ExpressParser;

/// <summary>
/// Предоставляет методы для парсинга и вычисления выражений.
/// </summary>
public partial class Expression
{
    private Operation rootOperation;
    private int argCount;
    #region Arguments related code
    internal Dictionary<string, double> arguments = new();

    /// <summary>
    /// Список переменных, найденных в выражении.
    /// Для установки значений используйте <see cref="SetArgument(string, double)"/>
    /// </summary>
    public IReadOnlyDictionary<string, double> Arguments => arguments;

    /// <summary>
    /// Updates value of some argument.
    /// </summary>
    /// <param name="name">Name of argument to update</param>
    /// <param name="value">New value of argument</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c></exception>
    /// <exception cref="ArgumentException">Current expression does not contain argument with name <paramref name="name"/></exception>
    public void SetArgument(string name, double value)
    {
        if (!arguments.ContainsKey(name ?? throw new ArgumentNullException(nameof(name))))
            throw new ArgumentException($"This expression does not contain \"{name}\" argument");
        if (double.IsNaN(value))
            throw new ArgumentException("Setting argument to NaN if forbidden.");
        arguments[name] = value;
    }
    #endregion

    /// <summary>
    /// Creates a new expression from a raw string.
    /// </summary>
    /// <param name="raw">String to parse.</param>
    public Expression(string raw)
    {
        rootOperation = Operation.Parse(raw, this);
        argCount = arguments.Count;
    }

    #region Evaluate related code

    /// <summary>
    /// Evaluates this expression.
    /// </summary>
    /// <seealso cref="EvaluateIL">
    /// <returns>Value of this expression.</returns>
    public double Evaluate() => rootOperation.Evaluate();

    /// <summary>
    /// Evaluates this expression using dynamically created IL assembly.
    /// Automaticcaly creates IL code if it is not created yet ().
    /// </summary>
    /// <returns>Value of this expression.</returns>
    public partial double EvaluateIL(); //ignore error here, source generator will fix this during compilation
    #endregion
    #region IL creation related code
    /// <summary>
    /// Is this expression compiled to IL code.
    /// </summary>
    public bool IsCompiled { get; private set; }
    
    private static Lazy<AssemblyBuilder> assembly = new(() => {
        AssemblyName name = new("ExpressParserEmitAssembly");
        return AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
    });
    private static Lazy<ModuleBuilder> module = new(() => assembly.Value
        .DefineDynamicModule("ExpressParserEmitModule"));

    private Type type = null;
    private MethodInfo calling = null;
    private Delegate @delegate = null;

    private static uint exprCounter = 0;

    /// <summary>
    /// Creates IL code for this expression.
    /// </summary>
    public void Compile()
    {
        if (IsCompiled) return;
        IsCompiled = true;
        var typeBuilder = module.Value.DefineType($"ExpressParserDyamicType{++exprCounter}");
        var methodBuilder = typeBuilder.DefineMethod("Eval", MethodAttributes.Public | MethodAttributes.Static);
        var argTypes = new Type[argCount];
        Array.Fill(argTypes, typeof(double));
        methodBuilder.SetParameters(argTypes);
        methodBuilder.SetReturnType(typeof(double));
        var ilGen = methodBuilder.GetILGenerator();
        rootOperation.GenerateIL(ilGen);
        ilGen.Emit(OpCodes.Ret);
        type = typeBuilder.CreateType();
        calling = type.GetMethod("Eval");
        @delegate = calling.CreateDelegate(GetRetType(argCount));
    }
    private static Assembly sysAssembly = typeof(Action).Assembly;
    private static Type GetRetType(int count)
    {
        var type = sysAssembly.GetType($"System.Func`{count+1}");
        var argTypes = new Type[count+1];
        Array.Fill(argTypes, typeof(double));
        return type.MakeGenericType(argTypes);
    }
    #endregion
}