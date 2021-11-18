using System.Reflection.Emit;

namespace ExpressParser.Operations;

internal class ConstantOperation : Operation
{
    private double value;
    public ConstantOperation(double value, Expression expression)
        : base(expression) => this.value = value;

    public override double Evaluate() => value;
    public override void GenerateIL(ILGenerator il) => il.Emit(OpCodes.Ldc_R8, value);

    protected internal override Operation Clone(Expression newExpr) 
        => new ConstantOperation(value, newExpr);
}
