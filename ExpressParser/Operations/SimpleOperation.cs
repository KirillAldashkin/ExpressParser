using System.Reflection;
using System.Reflection.Emit;

namespace ExpressParser.Operations;

internal class SimpleOperation : Operation
{
    private Operation left, right;
    private char opType;
    public SimpleOperation(Operation left,
                           Operation right,
                           char opType,
                           Expression expression) : base(expression)
    {
        this.left = left;
        this.right = right;
        this.opType = opType;
    }

    public override double Evaluate() => opType switch
    {
        '+' => left.Evaluate() + right.Evaluate(),
        '-' => left.Evaluate() - right.Evaluate(),
        '*' => left.Evaluate() * right.Evaluate(),
        '/' => left.Evaluate() / right.Evaluate(),
        '^' => Math.Pow(left.Evaluate(), right.Evaluate()),
        _ => throw new Exception($"Invalid operation: {opType}")
    };

    private static MethodInfo pow = typeof(Math).GetMethod("Pow");
    public override void GenerateIL(ILGenerator il)
    {
        left.GenerateIL(il);
        right.GenerateIL(il);
        if (opType == '+') il.Emit(OpCodes.Add);
        if (opType == '-') il.Emit(OpCodes.Sub);
        if (opType == '*') il.Emit(OpCodes.Mul);
        if (opType == '/') il.Emit(OpCodes.Div);
        if (opType == '^') il.Emit(OpCodes.Call, pow);
    }

    protected internal override Operation Clone(Expression newExpr) => new SimpleOperation(
        left.Clone(newExpr), 
        right.Clone(newExpr), 
        opType, newExpr
    );
}
