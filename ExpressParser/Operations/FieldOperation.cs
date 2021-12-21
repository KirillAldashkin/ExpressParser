using System.Reflection.Emit;

namespace ExpressParser.Operations;

internal class FieldOperation : Operation
{
    private string name;
    public FieldOperation(string name, Expression expression) : base(expression)
    {
        this.name = name;
        if (!expression.arguments.ContainsKey(name))
            expression.arguments.Add(name, 0);
    }

    public override double Evaluate() => expression.Arguments[name];
    public override void GenerateIL(ILGenerator il)
    {
        int index = expression.Arguments.Keys.ToList().IndexOf(name);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldc_I4, index);
        il.Emit(OpCodes.Ldelem_R8);
    }

    protected internal override Operation Clone(Expression newExpr) =>
        new FieldOperation(name, newExpr);
}