using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Peerly.Auth.Abstractions.Repositories;
using Peerly.Auth.Persistence.Common.Abstractions;
using Peerly.Auth.Tools;

namespace Peerly.Auth.Persistence.Common;

internal sealed class UpdateBuilder<T> : IUpdateBuilder<T>
{
    private readonly HashSet<string> _propertyNames = [];
    private readonly List<PropertyValueInfo> _propertyValues = [];

    public IUpdateBuilder<T> Set<TProperty>(Expression<Func<T, TProperty>> propertyExpression, TProperty propertyValue)
    {
        var propertyInfo = propertyExpression.ExtractDirectProperty();

        if (!propertyInfo.CanWrite)
        {
            throw new ArgumentException($"{propertyInfo.Name} should be a writable property.", nameof(propertyExpression));
        }

        if (!_propertyNames.Add(propertyInfo.Name))
        {
            throw new ArgumentException("Property with the same name was already configured.", nameof(propertyExpression));
        }

        _propertyValues.Add(new PropertyValueInfo(propertyInfo, propertyValue));

        return this;
    }

    public IUpdateConfiguration<T> Build()
    {
        var instance = Activator.CreateInstance<T>();

        foreach (var (propertyInfo, value) in _propertyValues)
        {
            propertyInfo.SetValue(instance, value);
        }

        var configuredProperties = _propertyValues.Select(propertyValue => propertyValue.PropertyInfo);

        return new UpdateConfiguration<T>(instance, configuredProperties);
    }

    private sealed record PropertyValueInfo(PropertyInfo PropertyInfo, object? Value);
}
