using ExpressParser.Operations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;

namespace ExpressParser;

/// <summary>
/// Delegate that creates new extension operations.
/// </summary>
/// <param name="context">Expression in which the operation being created is located</param>
/// <param name="arguments">Arguments of the operation being created</param>
/// <returns>New instance of extension operation.</returns>
public delegate Operation ExtensionProvider(Expression context, Operation[] arguments);

/// <summary>
/// Provides methods to parse and evaluates expressions.
/// </summary>
public partial class Expression
{
    private Operation rootOperation;
    private int argCount;
    internal IReadOnlyDictionary<string, ExtensionProvider> Extensions;
    
    #region Arguments related code
    internal Dictionary<string, double> arguments = new();

    /// <summary>
    /// List of arguments <see cref="SetArgument(string, double)"/>
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
        : this(raw, new Dictionary<string, ExtensionProvider>()) { }
    // TODO: replace with optional parameters in next major release,
    // because removing public Expression(string) is breaking change
    // (at binary level)
    public Expression(string raw, IReadOnlyDictionary<string, ExtensionProvider> extensions)
    {
        this.Extensions = extensions;
        rootOperation = Operation.Parse(raw.AsSpan(), this);
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
    /// </summary>
    /// <returns>Value of this expression.</returns>
    public partial double EvaluateIL(); //source generator will fix this during compilation
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

    private TypeInfo type = null;
    private MethodInfo calling = null;
    [SuppressMessage("CodeQuality", "IDE0052")] //source generator will use this field during compilation
    private Delegate @delegate = null;

    private static uint exprCounter = 0;

    /// <summary>
    /// Creates IL code for this expression.
    /// </summary>
    public void Compile()
    {
        if (IsCompiled) return;
        //build type
        var typeBuilder = module.Value.DefineType($"ExpressParserDyamicType{++exprCounter}");
        //build method
        var methodBuilder = typeBuilder.DefineMethod("Eval", MethodAttributes.Public | MethodAttributes.Static);
        methodBuilder.SetParameters(GetTypes<double>(argCount));
        methodBuilder.SetReturnType(typeof(double));
        var ilGen = methodBuilder.GetILGenerator();
        rootOperation.GenerateIL(ilGen);
        ilGen.Emit(OpCodes.Ret);
        //create method delegate
        type = typeBuilder.CreateTypeInfo();
        calling = type.GetMethod("Eval");
        @delegate = calling.CreateDelegate(GetRetType(argCount));
        IsCompiled = true;
    }
    private static Assembly sysAssembly = typeof(Action).Assembly;
    private static Type GetRetType(int count)
    {
        var type = sysAssembly.GetType($"System.Func`{count+1}");
        return type.MakeGenericType(GetTypes<double>(count+1));
    }
    private static Type[] GetTypes<T>(int count)
    {
        var ret = new Type[count];
        var toFill = typeof(T);
        for (int i = 0; i < count; i++) ret[i] = toFill;
        return ret;
    }
    #endregion
}