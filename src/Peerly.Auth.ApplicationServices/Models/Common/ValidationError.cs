using System;
using System.Collections.Generic;
using System.Linq;
using Peerly.Auth.Models.Shared;

namespace Peerly.Auth.ApplicationServices.Models.Common;

public sealed record ValidationError
{
    private ValidationError(
        string? errorDetail = null,
        IReadOnlyDictionary<string, string>? extensions = null,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        ErrorDetail = errorDetail;
        Extensions = extensions;
        Errors = errors;
    }

    public string? ErrorDetail { get; }
    public IReadOnlyDictionary<string, string>? Extensions { get; private init; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    public static ValidationError From(ErrorMessage errorMessage, ResponsibleManager responsibleManager)
    {
        var errors = new Dictionary<string, string[]>
        {
            [string.Empty] = [errorMessage.Value]
        };
        var extensions = new Dictionary<string, string>
        {
            [ExtensionKeyNames.ResponsibleManagerLogin] = responsibleManager.Login,
            [ExtensionKeyNames.ResponsibleManagerName] = responsibleManager.Name
        };
        return new ValidationError(errors: errors, extensions: extensions);
    }

    public static ValidationError From(ErrorMessage errorMessage)
    {
        var errors = new Dictionary<string, string[]>
        {
            [string.Empty] = [errorMessage.Value]
        };
        return new ValidationError(errors: errors);
    }

    public static ValidationError From(IEnumerable<ErrorMessage> errorMessages)
    {
        var errors = new Dictionary<string, string[]>
        {
            [string.Empty] = errorMessages.Select(errorMessage => errorMessage.Value).ToArray()
        };
        return new ValidationError(errors: errors);
    }

    public static ValidationError From(Uri link)
    {
        ArgumentNullException.ThrowIfNull(link);

        var extensions = new Dictionary<string, string>
        {
            [ExtensionKeyNames.ErrorDetailsFileLink] = link.ToString()
        };

        return new ValidationError(extensions: extensions);
    }

    public static ValidationError From(ValidationFailure validationFailure)
    {
        var errors = new Dictionary<string, string[]>
        {
            [string.Empty] = validationFailure.ErrorMessages.Select(m => m.Value).ToArray()
        };
        return new ValidationError(errors: errors);
    }

    public static ValidationError From(ErrorsCollection errorsCollection)
    {
        return new ValidationError(errors: errorsCollection);
    }

    // Ключи транслируются на клиентскую часть, перед переименованием или удалением нужно
    // уведомить/согласовать это с фронтенд разработчиком
    private static class ExtensionKeyNames
    {
        public const string ErrorDetailsFileLink = "ErrorDetailsFileLink";
        public const string ResponsibleManagerLogin = "ResponsibleManagerLogin";
        public const string ResponsibleManagerName = "ResponsibleManagerName";
    }
}
