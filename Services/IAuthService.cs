namespace WorkplaceTasks.API.Services;

public interface IAuthService
{
    // Devolve o Token e a Role se o login tiver sucesso, ou null se falhar. e precisa de string username e password
    (string Token, string Role)? Authenticate(string username, string password);

}