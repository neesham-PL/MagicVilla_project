using MagicVilla_Web.Models.Dto;

namespace MagicVilla_Web.Services.IServices
{
    public interface IAuthService
    {
        Task<T> LoginAsync<T>(LoginRequestDTO objToCreate); // string username, string password
        Task<T> RegisterAsync<T>(RegisterationRequestDTO objToCreate);
    }
}
