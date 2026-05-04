using FluentValidation;
using Peerly.Auth.V1;

namespace Peerly.Auth.Api.Validators.Auth;

internal sealed class V1LoginRequestValidator : AbstractValidator<V1LoginRequest>
{
    public V1LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
