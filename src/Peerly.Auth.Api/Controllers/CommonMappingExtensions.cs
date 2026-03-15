using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Google.Protobuf;
using Peerly.Auth.Api.Infrastructure;
using Peerly.Auth.ApplicationServices.Models.Common;
using CommonProto = Peerly.Auth.V1;

namespace Peerly.Auth.Api.Controllers;

internal static class CommonMappingExtensions
{
    public static CommonProto.ValidationError ToProto<TSource, TDestination>(this ValidationError validationError)
        where TDestination : IMessage<TDestination>
    {
        var mappedErrors = validationError.Errors != null
            ? ValidationPropertyMapping.Map<TSource, TDestination>(validationError.Errors)
            : ImmutableDictionary<string, string[]>.Empty;

        return new CommonProto.ValidationError
        {
            Errors =
            {
                mappedErrors
                    .ToDictionary(
                        keySelector: error => error.Key,
                        elementSelector: error => MapErrorMessagesCollection(error.Value))
            },
            Extensions =
            {
                validationError.Extensions
                    ?.ToDictionary(
                        keySelector: extension => extension.Key,
                        elementSelector: extension => extension.Value)
                ?? (IDictionary<string, string>)ImmutableDictionary<string, string>.Empty
            },
            ErrorDetail = validationError.ErrorDetail
        };

        static CommonProto.ValidationError.Types.ErrorMessagesCollection MapErrorMessagesCollection(string[] errorMessages)
        {
            return new CommonProto.ValidationError.Types.ErrorMessagesCollection
            {
                ErrorMessage = { errorMessages }
            };
        }
    }

    public static CommonProto.OtherError ToProto(this OtherError otherError)
    {
        return new CommonProto.OtherError
        {
            Type = otherError.Type.ToProto(),
            Message = otherError.Message?.Value
        };
    }

    private static CommonProto.OtherError.Types.ErrorType ToProto(this ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.PermissionDenied => CommonProto.OtherError.Types.ErrorType.PermissionDenied,
            ErrorType.NotFound => CommonProto.OtherError.Types.ErrorType.NotFound,
            ErrorType.Conflict => CommonProto.OtherError.Types.ErrorType.Conflict,
            _ => throw new ArgumentOutOfRangeException(nameof(errorType), errorType, null)
        };
    }
}
