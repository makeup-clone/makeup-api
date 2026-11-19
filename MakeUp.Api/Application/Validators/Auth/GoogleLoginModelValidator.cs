using Application.Models;
using FluentValidation;

namespace Application.Validators.Auth;

public class GoogleLoginModelValidator : AbstractValidator<GoogleLoginModel>
{
    public GoogleLoginModelValidator()
    {
        RuleFor(x => x.GoogleIdToken)
            .NotEmpty().WithMessage("Не вдалося виконати вхід через Google. Спробуйте ще раз.");
    }
}
