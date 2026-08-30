using System.Linq.Expressions;

namespace OpenDorm.Application.Tests.Helpers;

public class TestAsyncEnumerable<T>(IEnumerable<T> enumerable) : IAsyncEnumerable<T>, IQueryable<T>
{
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(enumerable.GetEnumerator());
    }

    // IQueryable implementation
    public Type ElementType => typeof(T);
    public Expression Expression => enumerable.AsQueryable().Expression;
    public IQueryProvider Provider => new TestAsyncQueryProvider<T>(enumerable.AsQueryable().Provider);
    public IEnumerator<T> GetEnumerator() => enumerable.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

public class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public T Current => inner.Current;

    public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());

    public ValueTask DisposeAsync()
    {
        inner.Dispose();
        return ValueTask.CompletedTask;
    }
}

public class TestAsyncQueryProvider<T>(IQueryProvider inner) : IQueryProvider
{
    public IQueryable CreateQuery(Expression expression) =>
        new TestAsyncEnumerable<T>(
            inner.CreateQuery<object>(expression) as IEnumerable<T> ?? []);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
        new TestAsyncEnumerable<TElement>(
            inner.CreateQuery<TElement>(expression));

    public object? Execute(Expression expression) => inner.Execute(expression);

    public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);
}