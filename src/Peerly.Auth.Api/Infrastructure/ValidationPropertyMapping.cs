using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Peerly.Auth.ApplicationServices.Models.Common;

namespace Peerly.Auth.Api.Infrastructure;

/// <summary>
/// Кастомный маппер имен проперти для передачи валидационных ошибок через слои
/// добавлять правила не нужно в случае если имена проперти совпадают один-к-одному
/// если есть какое-то расхождение можно указать через expression какое имя проперти нужно будет использовать для Destination типа
/// либо добавить Ignore если маппинг не нужен.
/// Маппер позволяет провалидировать все зарегистрированные типы на старте приложение и в случае какого-то расхождение
/// выбросит ошибку.
/// </summary>
internal static class ValidationPropertyMapping
{
    private static readonly ConcurrentDictionary<(Type, Type), Dictionary<string, MappingPropertyRule>> s_mapping = new();

    /// <summary>
    /// Обновляет названия полей в <see cref="ErrorsCollection"/> для ранее зарегистрированных типов.
    /// </summary>
    /// <param name="errors">Ошибки валидации.</param>
    /// <typeparam name="TSource">Тип который использовался для валидации.</typeparam>
    /// <typeparam name="TDestination">Тип, проперти которого нужно будет использовать в ошибках валидации.</typeparam>
    /// <returns>Обновленный результат валидации</returns>
    /// <exception cref="Exception">
    /// Должен выбросить исключение если пара типов (TSource:TDestination) не была ранее добавлена в конфигурацию.
    /// </exception>
    public static IReadOnlyDictionary<string, string[]> Map<TSource, TDestination>(IReadOnlyDictionary<string, string[]> errors)
    {
        var key = (typeof(TSource), typeof(TDestination));
        if (!s_mapping.TryGetValue(key, out _))
            throw new Exception("ValidationPropertyMapping: Types pair was not configured.");

        return errors;
    }

    /// <summary>
    /// Добавить новый билдер для пары типов.
    /// </summary>
    /// <typeparam name="TSource">Исходный тип.</typeparam>
    /// <typeparam name="TDestination">Тип назначения.</typeparam>
    /// <returns>Экземпляр билдера.</returns>
    public static MapperBuilder<TSource, TDestination> AddMapping<TSource, TDestination>()
    {
        return new MapperBuilder<TSource, TDestination>();
    }

    /// <summary>
    /// Билдер для правил маппинга.
    /// </summary>
    /// <typeparam name="TSource">Исходный тип.</typeparam>
    /// <typeparam name="TDestination">Тип назначения.</typeparam>
    public class MapperBuilder<TSource, TDestination>
    {
        private readonly List<MappingBuilderItem> _mapping = new();

        private readonly (Type sourceType, Type destinationType)
            _key = (sourceType: typeof(TSource), destinationType: typeof(TDestination));

        /// <summary>
        /// Использую expressions позволяет указать путь к полям для маппинга
        /// можно указывать на вложенные типы или коллекции любой глубины.
        /// </summary>
        /// <param name="sourceProperty">Путь к полю в исходном типе.</param>
        /// <param name="destinationProperty">Путь к полю в типе назначения.</param>
        /// <returns>Возвращает текущий экземпляр билдера.</returns>
        public MapperBuilder<TSource, TDestination> ConfigureProperty(
            Expression<Func<TSource, object>> sourceProperty,
            Expression<Func<TDestination, object>> destinationProperty)
        {
            _mapping.Add(CreateMappingBuilderItem(sourceProperty, destinationProperty));

            return this;
        }

        public MapperBuilder<TSource, TDestination> IgnoreProperty(Expression<Func<TSource, object?>> sourceProperty)
        {
            _mapping.Add(CreateMappingBuilderItem(sourceProperty, null));

            return this;
        }

        /// <summary>
        /// Запускает процесс валидации и регистрации новых типов.
        /// </summary>
        /// <exception cref="Exception">
        /// Ошибка может возникнуть когда мы попытаемся добавить дубликат пары (TSource:TDestination).
        /// </exception>
        public void Build()
        {
            var mappingResult = AssemblyMappingRules(_mapping);

            ValidateTypes(_key.sourceType, _key.destinationType, mappingResult);

            s_mapping[_key] = mappingResult;
        }

        /// <summary>
        /// Собирает все правила в один объект.
        /// </summary>
        /// <param name="mapping">Набор правил.</param>
        /// <param name="section">Секция в деревер которую обрабатывает метод в данный момент.</param>
        /// <returns>
        /// Словарь маппинга
        /// где ключ это название поля из TSource, а значение это информация о поле TDestination и/или набор вложенных полей.
        /// </returns>
        private static Dictionary<string, MappingPropertyRule> AssemblyMappingRules(
            List<MappingBuilderItem> mapping,
            Dictionary<string, MappingPropertyRule>? section = null)
        {
            section ??= new Dictionary<string, MappingPropertyRule>();
            foreach (var builderItem in mapping)
            {
                var sourceProperty = builderItem.SourceName!;
                if (!section.TryGetValue(sourceProperty, out var rule))
                {
                    rule = new MappingPropertyRule
                    {
                        DestinationName = builderItem.DestinationName,
                        Ignored = builderItem.Ignored
                    };
                    section.Add(sourceProperty, rule);
                }

                if (builderItem.ChildProperties is not { Count: > 0 })
                    continue;

                rule.ChildProperties = AssemblyMappingRules(builderItem.ChildProperties, rule.ChildProperties);
            }

            return section;
        }

        /// <summary>
        /// Метод позволяет рекурсивно проходить типы и сравнивать их поля по названию. TSource берется за основу
        /// и ищется совпадения в TDestination
        /// </summary>
        /// <param name="sourceType">Базовый тип для проверки</param>
        /// <param name="destinationType">Тип где ищем совпадения</param>
        /// <param name="mappingRules">Набор правил описанием мапинга, если есть расхождения</param>
        /// <exception cref="Exception">
        /// Ошибка будет выброшена если у TSource есть какие-то проперти которые не могу быть замаплены и для них нет правил
        /// </exception>
        private void ValidateTypes(
            Type sourceType,
            Type destinationType,
            Dictionary<string, MappingPropertyRule>? mappingRules)
        {
            var sourceProps = TryGetProperties(sourceType);
            foreach (var sourceProp in sourceProps)
            {
                var propertyRule = mappingRules?.TryGetValue(sourceProp.Name, out var rule) == true
                    ? rule
                    : null;
                var destinationPropName = propertyRule?.DestinationName ?? sourceProp.Name;
                var destinationProp = TryGetProperties(destinationType)
                    .FirstOrDefault(x => x.Name == destinationPropName);
                if (destinationProp == null)
                {
                    if (propertyRule?.Ignored == true)
                        continue;

                    throw new Exception(
                        $"Property {sourceType.Name}.{sourceProp.Name} doesn't matching property for destination type {destinationType.Name}");
                }

                var nestedMapping = mappingRules?.TryGetValue(sourceProp.Name, out var mapping) == true
                    ? mapping
                    : null;
                ValidateTypes(sourceProp.PropertyType, destinationProp.PropertyType, nestedMapping?.ChildProperties);
            }
        }

        private PropertyInfo[] TryGetProperties(Type type)
        {
            // для массива получаем тип элементов и рекурсивно ищем поля
            if (type.IsArray)
            {
                var arrayItemType = type.GetElementType();
                return TryGetProperties(arrayItemType!);
            }

            // для generic коллекции получаем первый аргумент и также рекурсивно его обрабатываем
            if (type.IsGenericType && type.GetInterface(nameof(IEnumerable)) != null)
            {
                var genericArgument = type.GenericTypeArguments.First();
                return TryGetProperties(genericArgument);
            }

            // игнорируем системные типы
            if (type.Namespace!.StartsWith(nameof(System)))
                return [];

            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// Метод который позволяет разобрать expression дерево и построить путь к полю
        /// </summary>
        /// <param name="sourceProperty">Expression для исходного типа</param>
        /// <param name="destinationProperty">Expression для типа назначения</param>
        /// <returns>Результат парсинга</returns>
        /// <exception cref="Exception">
        /// Ошибка может быть выброшена если метод не смог определить тип Expression.
        /// </exception>
        private MappingBuilderItem CreateMappingBuilderItem(
            LambdaExpression sourceProperty,
            LambdaExpression? destinationProperty)
        {
            var sourceMember = FindMappingMemberExpression(sourceProperty);
            var destinationMember = destinationProperty != null
                ? FindMappingMemberExpression(destinationProperty)
                : null;
            var mapping = new MappingBuilderItem
            {
                SourceName = sourceMember.Member.Name,
                DestinationName = destinationMember?.Member.Name,
                Ignored = destinationMember == null
            };

            var expression = sourceMember.Expression;
            while (expression != null)
            {
                switch (expression)
                {
                    case MethodCallExpression methodCallExpression:
                        expression = methodCallExpression.Arguments.First();
                        break;

                    case MemberExpression memberExpression:
                        mapping = new MappingBuilderItem
                        {
                            SourceName = memberExpression.Member.Name,
                            ChildProperties = new List<MappingBuilderItem> { mapping }
                        };
                        expression = memberExpression.Expression;
                        break;

                    case { NodeType: ExpressionType.Parameter }:
                        expression = null;
                        break;

                    default:
                        throw new Exception("Unknown expression type.");
                }
            }

            return mapping;
        }

        private static MemberExpression FindMappingMemberExpression(Expression expression)
        {
            return expression switch
            {
                MemberExpression result => result,
                UnaryExpression unaryExpression => FindMappingMemberExpression(unaryExpression.Operand),
                LambdaExpression lambdaExpression => FindMappingMemberExpression(lambdaExpression.Body),
                _ => throw new Exception($"Unknown expression type {expression}.")
            };
        }
    }

    private sealed record MappingBuilderItem
    {
        public string? SourceName { get; set; }
        public string? DestinationName { get; set; }
        public bool Ignored { get; set; }
        public List<MappingBuilderItem>? ChildProperties { get; set; }
    }

    private sealed record MappingPropertyRule
    {
        public string? DestinationName { get; set; }
        public bool Ignored { get; set; }
        public Dictionary<string, MappingPropertyRule>? ChildProperties { get; set; }
    }
}
