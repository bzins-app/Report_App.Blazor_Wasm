using FluentValidation;

namespace Report_App_WASM.Client.Pages.UserAccount;

public class UserProfileEditValidator : AbstractValidator<UserProfile>
{
    public UserProfileEditValidator()
    {
        RuleFor(x => x.UserName)
            .MaximumLength(128)
            .NotEmpty();
        RuleFor(x => x.UserName)
            .MaximumLength(128);
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result =
            await ValidateAsync(ValidationContext<UserProfile>.CreateWithOptions((UserProfile)model,
                x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}