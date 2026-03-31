using CSharpFunctionalExtensions;
using ValueObject = Shared.Base.ValueObject;

namespace DirectoryService.Domain.Location.ValueObjects;

public class Timezone : ValueObject
{
    public string Value { get; }
    
    private Timezone(string value)
    {
        Value = value;
    }

    public static Result<Timezone> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Timezone>("Часовой пояс не может быть пустым.");

        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(value);
            
            return Result.Success(new Timezone(value));
        }
        catch (TimeZoneNotFoundException)
        {
            return Result.Failure<Timezone>($"Часовой пояс '{value}' не найден. Используйте IANA код, например: Europe/Moscow");
        }
        catch (InvalidTimeZoneException)
        {
            return Result.Failure<Timezone>($"Часовой пояс '{value}' имеет некорректный формат.");
        }
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Timezone timezone) => timezone.Value;
}