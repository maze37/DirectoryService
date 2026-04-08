namespace Shared.Result;

/// <summary>
/// Коды ошибок для использования в Error
/// </summary>
public static class ErrorCodes
{
    public const int NotFound = 404;
    public const int BadRequest = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int Conflict = 409;
    public const int InternalServerError = 500;
    
    public const int ValidationError = 1001;
    public const int DuplicateError = 1002;
    public const int ConcurrencyError = 1003;
    
    public static string GetMessage(int code) => code switch
    {
        NotFound => "Ресурс не найден",
        BadRequest => "Некорректный запрос",
        Unauthorized => "Не авторизован",
        Forbidden => "Доступ запрещен",
        Conflict => "Конфликт данных",
        InternalServerError => "Внутренняя ошибка сервера",
        ValidationError => "Ошибка валидации",
        DuplicateError => "Дубликат данных",
        ConcurrencyError => "Конфликт одновременного доступа",
        _ => "Неизвестная ошибка"
    };
}