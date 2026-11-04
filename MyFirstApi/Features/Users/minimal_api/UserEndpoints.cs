using static MyApiProject.Handlers.UserHandlers;

namespace MyApiProject.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        // สร้าง Group ที่มี Route Prefix เป็น "/api/users"
        // และจัดกลุ่มแท็กใน Scalar/OpenAPI ให้ชื่อ "Users"
        var group = routes.MapGroup("/api/users").WithTags("Users");
        // กำหนด Route ย่อย (สังเกตว่าจะเริ่มจาก "/" เพราะมี prefix ข้างบนแล้ว)
        group.MapGet("/", GetAllUsers);
        group.MapGet("/{id:int}", GetUserById);
        // group.MapPost("/", CreateUser);
        group.MapPut("/{id:int}", UpdateUser);
        group.MapDelete("/{id:int}", DeleteUser);

        return group;
    }
}