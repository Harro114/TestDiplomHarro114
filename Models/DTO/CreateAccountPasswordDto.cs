namespace Diplom.Models.DTO;

public class CreateAccountPasswordDto
{
    public int AccountId { get; set; }         // Идентификатор существующего аккаунта
    public string Password { get; set; }      // Пароль для генерации

}