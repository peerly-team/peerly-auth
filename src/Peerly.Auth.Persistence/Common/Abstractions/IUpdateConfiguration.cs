using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Peerly.Auth.Persistence.Common.Abstractions;

internal interface IUpdateConfiguration<T>
{
    Dictionary<string, object?> GetQueryParams();
    string GetParamName<TProperty>(Expression<Func<T, TProperty>> propertyExpression);
    string GetFlagParamName<TProperty>(Expression<Func<T, TProperty>> propertyExpression);
}
