using FluentValidation;
using Peerly.Auth.V1;

namespace Peerly.Auth.Api.Validators.Auth;

internal sealed class V1RegisterRequestValidator : AbstractValidator<V1RegisterRequest>
{
    public V1RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty();

        RuleFor(x => x.Role)
            .Must(role => role is Role.Teacher or Role.Student);
    }
}
