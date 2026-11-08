using System.Diagnostics;

namespace NLox.Domain;

public abstract class Expr
{
    public interface IVisitor<out T>
    {
        T VisitBinaryExpr(Binary expr);
        T VisitGroupingExpr(Grouping expr);
        T VisitLiteralExpr(Literal expr);
        T VisitUnaryExpr(Unary expr);
    }

    public abstract T Accept<T>(IVisitor<T> visitor);
    
    public class Binary(Expr left, Token @operator, Expr right) : Expr
    {
        public Expr Left => left;
        public Token Operator => @operator;
        public Expr Right => right;

        [DebuggerStepThrough]
        public override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitBinaryExpr(this);
    }
    
    public class Grouping(Expr expression) : Expr
    {
        public Expr Expression => expression;
        
        [DebuggerStepThrough]
        public override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitGroupingExpr(this);
    }
    
    public class Literal(object? value) : Expr
    {
        public object? Value => value;
        
        [DebuggerStepThrough]
        public override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitLiteralExpr(this);
    }
    
    public class Unary(Token @operator, Expr right) : Expr
    {
        public Token Operator => @operator;
        public Expr Right => right;
        
        [DebuggerStepThrough]
        public override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitUnaryExpr(this);
    }
}