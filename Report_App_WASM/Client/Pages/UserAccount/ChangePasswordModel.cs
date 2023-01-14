using FluentValidation;
using Report_App_WASM.Client.Services;
using Report_App_WASM.Shared;

namespace Report_App_WASM.Client.Pages.UserAccount;

public class ChangePasswordModel
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public IdentityDefaultOptions? Options { get; set; }
}

public class ChangePasswordModelValidator : AbstractValidator<ChangePasswordModel>
{
    public ChangePasswordModelValidator(CommonLocalizationService localizer)
    {
        RuleFor(p => p.NewPassword).NotEmpty().WithMessage(localizer.Get("Your password cannot be empty"))
            .MinimumLength(6).WithMessage(localizer.Get("Your password length must be at least 6"));
        RuleFor(p => p.NewPassword).NotEmpty().WithMessage(localizer.Get("Your password cannot be empty"))
            .Matches(@"[A-Z]+").When(v => v.Options!.PasswordRequireUppercase)
            .WithMessage(localizer.Get("Your password must contain at least one uppercase letter"));
        RuleFor(p => p.NewPassword).NotEmpty().WithMessage(localizer.Get("Your password cannot be empty"))
            .Matches(@"[a-z]+").When(v => v.Options!.PasswordRequireLowercase)
            .WithMessage(localizer.Get("Your password must contain at least one lowercase letter"));
        RuleFor(p => p.NewPassword).NotEmpty().WithMessage(localizer.Get("Your password cannot be empty"))
            .Matches(@"[0-9]+").When(v => v.Options!.PasswordRequireDigit)
            .WithMessage(localizer.Get("Your password must contain at least one number"));
        RuleFor(p => p.NewPassword).NotEmpty().WithMessage(localizer.Get("Your password cannot be empty"))
            .Matches(@"[\!\?\*\.]+").When(v => v.Options!.PasswordRequireNonAlphanumeric)
            .WithMessage(localizer.Get("Your password must contain at least one (!? *.)"));
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage(localizer.Get("The passwords entered must be equal"));
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result =
            await ValidateAsync(ValidationContext<ChangePasswordModel>.CreateWithOptions((ChangePasswordModel)model,
                x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}