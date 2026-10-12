namespace StoreApi.Models;

public record LoginRequest(string Username, string Password);
public record UserDto(int Id, string Username, string FullName, string Role);
public record CreateUserRequest(string Username, string FullName, string Password, string Role);