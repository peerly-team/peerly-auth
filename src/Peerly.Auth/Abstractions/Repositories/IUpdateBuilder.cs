using System;
using System.Linq.Expressions;

namespace Peerly.Auth.Abstractions.Repositories;

public interface IUpdateBuilder<T>
{
    IUpdateBuilder<T> Set<TProperty>(Expression<Func<T, TProperty>> propertyExpression, TProperty propertyValue);
}
