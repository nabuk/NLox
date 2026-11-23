namespace NLox.Domain;

public class Interpreter : Stmt.IVisitor<object?>, Expr.IVisitor<object?>
{
    private readonly IConsole console;
    private readonly IErrorReporter errorReporter;
    private readonly EnvironmentScope globals;
    private EnvironmentScope environment;

    public Interpreter(IConsole console, IErrorReporter errorReporter)
    {
        this.console = console;
        this.errorReporter = errorReporter;
        this.globals = new();
        this.environment = globals;
        DefineClock();
    }
    
    public EnvironmentScope Globals => globals;

    public void Interpret(IEnumerable<Stmt> statements)
    {
        try
        {
            foreach (var stmt in statements)
                Execute(stmt);
        }
        catch (RuntimeError ex)
        {
            errorReporter.RuntimeError(ex);
        }
    }
    
    public void ExecuteBlock(IEnumerable<Stmt> statements, EnvironmentScope environment)
    {
        var previous = this.environment;
        try
        {
            this.environment = environment;
            foreach (var stmt in statements)
                Execute(stmt);
        }
        finally
        {
            this.environment = previous;
        }
    }
    
    object? Stmt.IVisitor<object?>.VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.InnerExpression);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitFunctionStmt(Stmt.Function stmt)
    {
        var function = new LoxFunction(stmt, environment);
        environment.Define(stmt.Name.Lexeme, function);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitReturnStmt(Stmt.Return stmt)
    {
        object? value = null;
        if (stmt.Value != null)
            value = Evaluate(stmt.Value);

        throw new Return(value);
    }

    object? Stmt.IVisitor<object?>.VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
            Execute(stmt.ThenBranch);
        else if (stmt.ElseBranch != null)
            Execute(stmt.ElseBranch);

        return null;
    }

    object? Stmt.IVisitor<object?>.VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate((stmt.InnerExpression));
        var str = Stringify(value);
        console.PrintLine(str);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitVarStmt(Stmt.Var stmt)
    {
        object? value = null;
        if (stmt.Initializer != null)
            value = Evaluate(stmt.Initializer);
        
        environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    object? Stmt.IVisitor<object?>.VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
            Execute(stmt.Body);

        return null;
    }

    object? Stmt.IVisitor<object?>.VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new EnvironmentScope(environment));
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

    object? Expr.IVisitor<object?>.VisitCallExpr(Expr.Call expr)
    {
        var callee = Evaluate(expr.Callee) ?? throw new InvalidOperationException("Expected callee");
        var arguments = expr.Arguments.Select(Evaluate).ToArray();
        var function = callee as ILoxCallable
                       ?? throw new RuntimeError(expr.Paren, "Can only call functions and classes.");
        if (function.Arity != arguments.Length)
            throw new RuntimeError(expr.Paren, $"Expected {function.Arity} arguments, but got {arguments.Length}.");
        
        return function.Call(this, arguments);
    }

    object? Expr.IVisitor<object?>.VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);
        if (expr.Operator.Type == TokenType.Or)
        {
            if (IsTruthy(left))
                return left;
        }
        else
        {
            if (!IsTruthy(left))
                return left;
        }

        return Evaluate(expr.Right);
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

    object? Expr.IVisitor<object?>.VisitVariableExpr(Expr.Variable expr) =>
        environment.Get(expr.Name);

    object? Expr.IVisitor<object?>.VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);
        environment.Assign(expr.Name, value);
        return value;
    }

    private void Execute(Stmt stmt) =>
        stmt.Accept(this);

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

    private void DefineClock()
    {
        this.globals.Define(
            "clock",
            new NativeLoxCallable(
                (_, _) => DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0,
                0));
    }
}