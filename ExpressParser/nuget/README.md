# Express~~ion~~Parser
Simple mathematical expression parser capable of
generating IL code to speed up calculations.
**For more info see [GitHub repository](https://github.com/KirillAldashkin/ExpressParser)**
#### Last update: [1.1.0] Extension operations
Allows you to define some operations that can be
used in expression string like functions: `a+c*f(100,a)`
where `f(,)` is user-defined operaion. Example:
```CSharp
using ExpressParser;
using ExpressParser.Operations;
using System.Reflection;
using System.Reflection.Emit;

Dictionary<string, ExtensionProvider> extesions = new();
extesions.Add("sqrt", (context, args) => new SqrtOperation(context, args.Single()));
Expression expr = new("sqrt(a)", extesions);
expr.SetArgument("a", 1048576);
expr.Evaluate(); //1024

class SqrtOperation : Operation
{
    private Operation value;
    public SqrtOperation(Expression expression, Operation value) 
        : base(expression) => this.value = value;

    public override double Evaluate() => Math.Sqrt(value.Evaluate());

    private static MethodInfo sqrt = typeof(Math).GetMethod("Sqrt");
    public override void GenerateIL(ILGenerator il)
    {
        value.GenerateIL(il);
        il.Emit(OpCodes.Call, sqrt);
    }

    public override Operation Clone(Expression newExpr) => 
        new SqrtOperation(newExpr, value.Clone());
}
```