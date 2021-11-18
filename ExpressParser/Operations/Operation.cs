using System.Reflection.Emit;
using System.Text;
using static ExpressParser.Utils;

namespace ExpressParser.Operations;

/// <summary>
/// Base class for all internal operations. Also
/// used to create extension operations.
/// </summary>
public abstract class Operation
{
    protected Expression expression;

    /// <summary>
    /// Creates new operation.
    /// </summary>
    /// <param name="expression">The expression in which this operation is located</param>
    public Operation(Expression expression) => this.expression = expression;
    
    /// <summary>
    /// Evaluates current operation.
    /// </summary>
    /// <returns>The result of evaluation</returns>
    public abstract double Evaluate();
    
    /// <summary>
    /// Emit instructions for this operation in specified <see cref="ILGenerator"/>
    /// </summary>
    /// <param name="il"><see cref="ILGenerator"/> to emit instructions</param>
    public abstract void GenerateIL(ILGenerator il);

    internal static Operation Parse(ReadOnlySpan<char> raw, Expression context)
    {
        //trim paired parentheses
        while (raw[0]=='(' && GetPair(raw, 0, '(', ')')==raw.Length-1)
            raw = raw.Slice(1, raw.Length-2);
        // should ingore unary minus and everything in parentheses
        // when searching operations, so remove them
        string pattern = GetPattern(raw);
        // searching simple operations
        char[][] preds = new char[][] {
            new char[] { '+', '-' },
            new char[] { '*', '/' },
            new char[] { '^' }
        };
        foreach (var pred in preds) {
            int index = pattern.IndexOfAny(pred);
            if (index > -1) return new SimpleOperation(
                Parse(raw.Slice(0, index), context),
                Parse(raw.Slice(index+1), context),
                pattern[index], context
            ); 
        }
        // constant
        string str = raw.GetString();
        if (double.TryParse(str, out double value))
            return new ConstantOperation(value, context);
        // extension operation
        int paramStart = raw.IndexOf('(');
        if(paramStart != -1 && GetPair(raw, paramStart, '(', ')') == raw.Length-1)
        {
            var name = str.Substring(0, paramStart);
            if(context.Extensions.ContainsKey(name))
            {
                var args = str.Substring(paramStart+1, str.Length-paramStart-2)
                    .Split(',')
                    .Select(s => Parse(s.Trim().AsSpan(), context));
                return context.Extensions[name](context, args.ToArray());
            }
        }
        // argument
        return new FieldOperation(str, context);
    }
    private static string GetPattern(ReadOnlySpan<char> raw)
    {
        StringBuilder ret = new();
        //remove everything in parentheses
        int prevIndex = 0, curIndex = raw.IndexOf('('), pair, offset;
        while(curIndex != -1)
        {
            pair = GetPair(raw, curIndex, '(', ')');
            ret.Append(raw.Slice(prevIndex, curIndex-prevIndex))
               .Append('#', pair-curIndex+1);
            prevIndex = pair+1;
            offset = Math.Min(raw.Length-1, prevIndex+1);
            curIndex = raw.Slice(offset).IndexOf('(');
            if(curIndex != -1) curIndex+=offset;
        }
        ret.Append(raw.Slice(prevIndex));
        //remove unary minus
        if (ret[0] == '-') ret[0] = '#';
        for (int i = 1; i < ret.Length; i++)
            if (ret[i]=='-' && raw[i-1] != ')' && (raw[i-1]<'0' || raw[i-1]>'9'))
                ret[i] = '#';
        return ret.ToString();
    }
    internal abstract protected Operation Clone(Expression newExpr);
}