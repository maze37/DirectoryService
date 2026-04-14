using CSharpFunctionalExtensions;
using Shared.Result;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Location.ValueObjects;

public class Timezone : ValueObject
{
    public string Value { get; }
    
    private Timezone(string value)
    {
        Value = value;
    }

    public static Result<Timezone, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.ValueIsRequired("timezone");

        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(value);
            return new Timezone(value);
        }
        catch (TimeZoneNotFoundException)
        {
            return GeneralErrors.ValueIsInvalid("timezone", $"Часовой пояс '{value}' не найден. Используйте IANA код, например: Europe/Moscow");
        }
        catch (InvalidTimeZoneException)
        {
            return GeneralErrors.ValueIsInvalid("timezone", $"Часовой пояс '{value}' имеет некорректный формат.");
        }
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Timezone timezone) => timezone.Value;
}