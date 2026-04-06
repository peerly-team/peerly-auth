using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Peerly.Auth.Tools;

public static class ExpressionExtensions
{
    public static PropertyInfo ExtractDirectProperty<T, TProperty>(this Expression<Func<T, TProperty>> propertyExpression)
    {
        ArgumentNullException.ThrowIfNull(propertyExpression);

        return propertyExpression.Body is MemberExpression { Member: PropertyInfo propertyInfo }
            ? propertyInfo
            : throw new ArgumentException("Provided expression should be a property expression.", nameof(propertyExpression));
    }
}
