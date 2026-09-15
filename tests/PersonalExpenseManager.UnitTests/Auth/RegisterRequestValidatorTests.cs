using PersonalExpenseManager.Application.Auth.Dtos;
using PersonalExpenseManager.Application.Auth.Validators;

namespace PersonalExpenseManager.UnitTests.Auth;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var request = new RegisterRequest("Ada", "Lovelace", "ada@example.com", "Str0ng!Passw0rd", "Str0ng!Passw0rd");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("short1!")] // too short
    [InlineData("alllowercase1!")] // no uppercase
    [InlineData("ALLUPPERCASE1!")] // no lowercase
    [InlineData("NoDigitsHere!")] // no digit
    [InlineData("NoSpecialChar1")] // no special character
    public void Weak_Password_Fails(string password)
    {
        var request = new RegisterRequest("Ada", "Lovelace", "ada@example.com", password, password);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Mismatched_ConfirmPassword_Fails()
    {
        var request = new RegisterRequest("Ada", "Lovelace", "ada@example.com", "Str0ng!Passw0rd", "Different1!");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Invalid_Email_Fails()
    {
        var request = new RegisterRequest("Ada", "Lovelace", "not-an-email", "Str0ng!Passw0rd", "Str0ng!Passw0rd");

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }
}
