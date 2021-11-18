using ExpressParser.Operations;
using System.Reflection.Emit;

namespace ExpressParser;

public static class Extensions
{
    /// <summary>
    /// Adds a specified expression as extension operation to this expression.
    /// </summary>
    /// <param name="extensions">Collection of extensions to add new extesion operation</param>
    /// <param name="name">Name of extension operation</param>
    /// <param name="value">Expression to add</param>
    public static void AddExpression(this IDictionary<string, ExtensionProvider> extensions,
                                     string name, Expression value) =>
        extensions.Add(name, (context, args) => new ExpressionOperation(context, value, args));
    private class ExpressionOperation : Operation
    {
        private Expression value;
        private (string key, Operation arg)[] arguments;
        
        public ExpressionOperation(Expression expression,
                                   Expression value,
                                   Operation[] arguments) : base(expression)
        {
            this.value = (Expression)value.Clone();
            this.arguments = value.arguments.Keys.Zip(arguments, (key, arg) => (key, arg)).ToArray();
        }

        public override double Evaluate()
        {
            //update arguments which may be changed
            foreach (var (key, arg) in arguments) value.arguments[key] = arg.Evaluate();
            return value.Evaluate();
        }

        public override void GenerateIL(ILGenerator il)
        {
            //don't need to update arguments here, because they are added to IL code
            value.Compile();
            foreach (var (_, arg) in arguments) arg.GenerateIL(il);
            il.Emit(OpCodes.Call, value.@delegate.Method);
        }

        protected internal override Operation Clone(Expression newExpr) => 
            new ExpressionOperation(
                newExpr, (Expression)value.Clone(),
                arguments.Select((a) => a.arg.Clone(newExpr)).ToArray()
            );
    }
}
