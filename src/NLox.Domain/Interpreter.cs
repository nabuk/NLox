namespace NLox.Domain;

public class Interpreter : Expr.IVisitor<object?>
{
    object? Expr.IVisitor<object?>.VisitBinaryExpr(Expr.Binary expr)
    {
        throw new NotImplementedException();
    }

    object? Expr.IVisitor<object?>.VisitGroupingExpr(Expr.Grouping expr)
    {
        throw new NotImplementedException();
    }

    object? Expr.IVisitor<object?>.VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    object? Expr.IVisitor<object?>.VisitUnaryExpr(Expr.Unary expr)
    {
        throw new NotImplementedException();
    }
}