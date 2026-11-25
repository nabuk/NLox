namespace NLox.Domain;

public class Resolver(Interpreter interpreter, IErrorReporter errorReporter) : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{
    private readonly Stack<Dictionary<string, bool>> scopes = new();
    private FunctionType currentFunction = FunctionType.None;
    
    public void Resolve(IEnumerable<Stmt> stmts)
    {
        foreach (var stmt in stmts)
            Resolve(stmt);
    }

    object? Stmt.IVisitor<object?>.VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }
    
    object? Stmt.IVisitor<object?>.VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer != null)
            Resolve(stmt.Initializer);
        Define(stmt.Name);
        return null;
    }

    object? Expr.IVisitor<object?>.VisitVariableExpr(Expr.Variable expr)
    {
        if (scopes.Count > 0 && scopes.Peek().TryGetValue(expr.Name.Lexeme, out var variable) && !variable)
            errorReporter.Error(expr.Name, "Can't read local variable in its own initializer.");

        ResolveLocal(expr, expr.Name);
        return null;
    }

    object? Expr.IVisitor<object?>.VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);
        ResolveFunction(stmt, FunctionType.Function);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.InnerExpression);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);
        if (stmt.ElseBranch != null)
            Resolve(stmt.ElseBranch);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.InnerExpression);
        return null;
    }
    
    object? Stmt.IVisitor<object?>.VisitReturnStmt(Stmt.Return stmt)
    {
        if (currentFunction == FunctionType.None)
            errorReporter.Error(stmt.Keyword, "Can't return from top-level code.");
        
        if (stmt.Value != null)
            Resolve(stmt.Value);
        return null;
    }
    
    object? Stmt.IVisitor<object?>.VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
        return null;
    }
    
    object? Expr.IVisitor<object?>.VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }
    
    object? Expr.IVisitor<object?>.VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);
        foreach (var arg in expr.Arguments)
            Resolve(arg);
        return null;
    }
    
    object? Expr.IVisitor<object?>.VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }
    
    object? Expr.IVisitor<object?>.VisitLiteralExpr(Expr.Literal expr)
    {
        return null;
    }
    
    object? Expr.IVisitor<object?>.VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }
    
    object? Expr.IVisitor<object?>.VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }

    private void ResolveFunction(Stmt.Function function, FunctionType type)
    {
        var enclosingFunction = currentFunction;
        currentFunction = type;
        
        BeginScope();
        foreach (var parameter in function.Parameters)
        {
            Declare(parameter);
            Define(parameter);
        }
        
        Resolve(function.Body);
        EndScope();

        currentFunction = enclosingFunction;
    }

    private void Resolve(Expr expr) => expr.Accept(this);
    
    private void Resolve(Stmt stmt) => stmt.Accept(this);

    private void ResolveLocal(Expr expr, Token name)
    {
        var depth = 0;
        foreach (var scope in scopes)
        {
            if (scope.ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(expr, depth);
                return;
            }
            depth++;
        }
    }

    private void Declare(Token name)
    {
        if (scopes.Count == 0)
            return;
        
        var scope = scopes.Peek();
        if (scope.ContainsKey(name.Lexeme))
            errorReporter.Error(name, "Already a variable with this name in this scope.");
        
        scope[name.Lexeme] = false;
    }

    private void Define(Token name)
    {
        if (scopes.Count == 0)
            return;

        scopes.Peek()[name.Lexeme] = true;
    }
    
    private void BeginScope() =>
        scopes.Push(new Dictionary<string, bool>());

    private void EndScope() =>
        scopes.Pop();
}