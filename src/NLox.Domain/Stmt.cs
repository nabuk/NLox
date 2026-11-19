using System.Diagnostics;

namespace NLox.Domain;

public abstract class Stmt
{
    public interface IVisitor<out T>
    {
        T VisitExpressionStmt(Expression stmt);
        T VisitPrintStmt(Print stmt);
        T VisitVarStmt(Var stmt);
        T VisitBlockStmt(Block stmt);
    }
    
    public abstract T Accept<T>(IVisitor<T> visitor);

    public class Expression(Expr expression) : Stmt
    {
        public Expr InnerExpression => expression;
        
        [DebuggerStepThrough]
        public override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitExpressionStmt(this);
    }

    public class Print(Expr expression) : Stmt
    {
        public Expr InnerExpression => expression;
        
        [DebuggerStepThrough]
        public override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitPrintStmt(this);
    }
    
    public class Var(Token name, Expr? initializer) : Stmt
    {
        public Token Name => name;
        public Expr? Initializer => initializer;
        
        [DebuggerStepThrough]
        public override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitVarStmt(this);
    }

    public class Block(List<Stmt?> statements) : Stmt
    {
        public List<Stmt?> Statements => statements;
        
        [DebuggerStepThrough]
        public override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitBlockStmt(this);
    }
}