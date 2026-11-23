using System.Diagnostics;

namespace NLox.Domain;

public abstract class Stmt
{
    public interface IVisitor<out T>
    {
        T VisitFunctionStmt(Function stmt);
        T VisitReturnStmt(Return stmt);
        T VisitExpressionStmt(Expression stmt);
        T VisitIfStmt(If stmt);
        T VisitWhileStmt(While stmt);
        T VisitPrintStmt(Print stmt);
        T VisitVarStmt(Var stmt);
        T VisitBlockStmt(Block stmt);
    }
    
    public abstract T Accept<T>(IVisitor<T> visitor);

    public class Function(Token name, IReadOnlyList<Token> parameters, IReadOnlyList<Stmt> body) : Stmt
    {
        public Token Name => name;
        public IReadOnlyList<Token> Parameters => parameters;
        public IReadOnlyList<Stmt> Body => body;
        
        [DebuggerStepThrough]
        public override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitFunctionStmt(this);
    }

    public class Return(Token keyword, Expr? value) : Stmt
    {
        public Token Keyword => keyword;
        public Expr? Value => value;
        
        [DebuggerStepThrough]
        public override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitReturnStmt(this);
    }

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

    public class Block(IReadOnlyList<Stmt> statements) : Stmt
    {
        public IReadOnlyList<Stmt> Statements => statements;
        
        [DebuggerStepThrough]
        public override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitBlockStmt(this);
    }

    public class If(Expr condition, Stmt thenBranch, Stmt? elseBranch) : Stmt
    {
        public Expr Condition => condition;
        public Stmt ThenBranch => thenBranch;
        public Stmt? ElseBranch => elseBranch;
        
        [DebuggerStepThrough]
        public override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitIfStmt(this);
    }
    
    public class While(Expr condition, Stmt body) : Stmt
    {
        public Expr Condition => condition;
        public Stmt Body => body;
        
        [DebuggerStepThrough]
        public override T Accept<T>(IVisitor<T> visitor) =>
            visitor.VisitWhileStmt(this);
    }
}