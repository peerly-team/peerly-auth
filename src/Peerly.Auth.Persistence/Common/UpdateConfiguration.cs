using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Peerly.Auth.Persistence.Common.Abstractions;
using Peerly.Auth.Tools;

namespace Peerly.Auth.Persistence.Common;

internal sealed class UpdateConfiguration<T> : IUpdateConfiguration<T>
{
    private readonly T _instance;
    private readonly HashSet<string> _propertiesName;

    public UpdateConfiguration(T instance, IEnumerable<PropertyInfo> setProperties)
    {
        _instance = instance;
        _propertiesName = setProperties.Select(static propertyInfo => propertyInfo.Name).ToHashSet();
    }

    public Dictionary<string, object?> GetQueryParams()
    {
        var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

        var queryParams = new Dictionary<string, object?>();

        foreach (var propertyInfo in properties)
        {
            var propertyValue = propertyInfo.GetValue(_instance);

            if (propertyValue is Enum)
            {
                propertyValue = propertyValue.ToString();
            }

            queryParams.Add(GetParamNameCore(propertyInfo), propertyValue);

            var flagValue = _propertiesName.Contains(propertyInfo.Name);
            queryParams.Add(GetFlagParamNameCore(propertyInfo), flagValue);
        }

        return queryParams;
    }

    public string GetParamName<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        var propertyInfo = propertyExpression.ExtractDirectProperty();

        return GetParamNameCore(propertyInfo);
    }

    public string GetFlagParamName<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        var propertyInfo = propertyExpression.ExtractDirectProperty();

        return GetFlagParamNameCore(propertyInfo);
    }

    private static string GetParamNameCore(PropertyInfo propertyInfo)
    {
        return $"@{propertyInfo.Name}";
    }

    private static string GetFlagParamNameCore(PropertyInfo propertyInfo)
    {
        const string FlagPrefix = "SYSUPD_";

        return $"@{FlagPrefix}{propertyInfo.Name}";
    }
}
