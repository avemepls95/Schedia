namespace Avemepls.Auth.Bearer;

public class UserData<TId>
    where TId : struct
{
    public required TId Id { get; set; }
    public required string? Email { get; set; }
    public required string UserName { get; set; }
    public required string? FullName { get; set; }
}