namespace Connextion;

public class Result : Result<object>
{
    Result(bool success, object? value, string? errorValue) : base(success, value, errorValue) { }

    public static Result Ok() => new(true, new {}, null);
    public new static Result Error(string errorValue) => new(false, default, errorValue);
}

public class Result<T> {
    readonly bool _success;
    readonly T? _value;
    readonly string? _errorValue;
 
    protected Result(bool success, T? value, string? errorValue)
    {
        _success = success;
        _value = value;
        _errorValue = errorValue;
    }
    
    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Error(string errorValue) => new(false, default, errorValue);
    public static explicit operator Result<T>(T value) => Ok(value);

    public Result Bind(Func<T, Result> transform)
    {
        return _success ? transform(_value!) : Result.Error(_errorValue!);
    }    
    
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> transform)
    {
        return (_success) ? transform(_value!) : Result<TOut>.Error(_errorValue!);
    }
    
    public Task<Result> BindAsync(Func<T, Task<Result>> transform)
    {
        if (_success)
        {
            var result = transform(_value!);
            return result;
        }
        else
        {
            var error = Result.Error(_errorValue!);
            return Task.FromResult(error);
        }
    }
    
    public Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> transform)
    {
        if (_success)
        {
            var result = transform(_value!);
            return result;
        }
        else
        {
            var error = Result<TOut>.Error(_errorValue!);
            return Task.FromResult(error);
        }
    }

    public Result<TOut> Map<TOut>(Func<T, TOut> transform)
    {
        if (_success)
        {
            var result = transform(_value!);
            return Result<TOut>.Ok(result);
        }
        else
        {
            return Result<TOut>.Error(_errorValue!);
        }
    }

    public Result<TOut> Map<TOut>(Func<TOut> transform)
    {
        if (_success)
        {
            var result = transform();
            return Result<TOut>.Ok(result);
        }
        else
        {
            return Result<TOut>.Error(_errorValue!);
        }
    }

    public T Default(Func<string, T> transformError)
    {
        return _success ? _value! : transformError(_errorValue!);
    }
}

public static class ResultExtensions
{
    public static Result<TOut> ToResult<TOut>(this TOut value) => Result<TOut>.Ok(value);
    
    public static async Task<Result<TOut>> MapAsync<TOut>(this Task<Result> resultTask, Func<TOut> transform)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Map(transform);
    }
    
    
    
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> transform)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Map(transform);
    }
    
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Result<TOut>> transform)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Bind(transform);
    }
    
    
    public static async Task<T> DefaultAsync<T>(this Task<Result<T>> resultTask, Func<string, T> transformError)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Default(transformError);
    }
}