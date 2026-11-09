namespace NLox.Domain;

public class Interpreter(IErrorReporter errorReporter) : Expr.IVisitor<object?>
{
    public string? Interpret(Expr expression)
    {
        try
        {
            var value = Evaluate(expression);
            return Stringify(value);
        }
        catch (RuntimeError ex)
        {
            errorReporter.RuntimeError(ex);
        }

        return null;
    }
    
    object? Expr.IVisitor<object?>.VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case TokenType.EqualEqual:
                return object.Equals(left, right);
            case TokenType.BangEqual:
                return !object.Equals(left, right);
            
            case TokenType.Greater:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! > (double)right!;
            case TokenType.GreaterEqual:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! >= (double)right!;
            case TokenType.Less:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! < (double)right!;
            case TokenType.LessEqual:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! <= (double)right!;
            
            case TokenType.Minus:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! - (double)right!;
            case TokenType.Plus:
                if (left is double ld && right is double rd)
                    return ld + rd;
                if (left is string ls && right is string rs)
                    return ls + rs;
                throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.");
            case TokenType.Slash:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! / (double)right!;
            case TokenType.Star:
                CheckNumberOperands(expr.Operator, left, right);
                return (double)left! * (double)right!;
        }
        
        throw new InvalidOperationException($"Unexpected operator {expr.Operator.Type}");
    }

    object? Expr.IVisitor<object?>.VisitGroupingExpr(Expr.Grouping expr) =>
        Evaluate(expr.Expression);

    object? Expr.IVisitor<object?>.VisitLiteralExpr(Expr.Literal expr) =>
        expr.Value;

    object? Expr.IVisitor<object?>.VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);

        switch (expr.Operator.Type)
        {
            case TokenType.Minus:
                CheckNumberOperand(expr.Operator, right);
                return -(double)right!;
            case TokenType.Bang:
                return !IsTruthy(right);
            default:
                throw new InvalidOperationException($"Unexpected operator {expr.Operator.Type}");
        }
    }

    private void CheckNumberOperand(Token @operator, object? operand)
    {
        if (operand is double)
            return;
        
        throw new RuntimeError(@operator, "Operand must be a number.");
    }
    
    private void CheckNumberOperands(Token @operator, object? left, object? right)
    {
        if (left is double && right is double)
            return;
        
        throw new RuntimeError(@operator, "Operands must be numbers.");
    }

    private object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private bool IsTruthy(object? value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            _ => true
        };
    }
    
    private string Stringify(object? value)
    {
        if (value == null)
            return "nil";

        return value.ToString()!;
    }
}