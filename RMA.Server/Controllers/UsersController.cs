using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Google.Cloud.Firestore;
using RMA.Server.Entities;
using RMA.Server.Services;
using RMA.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RMA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer,Local", Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly FirestoreRepository<UserAccount> _userRepo;
        private readonly FirestoreRepository<AuditLog> _auditLogRepo;
        private readonly IConfiguration _config;

        public UsersController(
            FirestoreRepository<UserAccount> userRepo,
            FirestoreRepository<AuditLog> auditLogRepo,
            IConfiguration config)
        {
            _userRepo = userRepo;
            _auditLogRepo = auditLogRepo;
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SystemUserDto>>> GetUsers()
        {
            try
            {
                var users = await _userRepo.GetAllAsync();
                var superAdminUsername = _config["SuperAdmin:Username"] ?? "admin";

                // Filter out Super Admin account
                users = users.Where(u => !u.Username.Equals(superAdminUsername, StringComparison.OrdinalIgnoreCase)).ToList();

                // If no normal users exist, seed default accounts to support normal flow
                if (!users.Any())
                {
                    var hashedPass = BCrypt.Net.BCrypt.HashPassword("123456");
                    var defaults = new List<UserAccount>
                    {
                        new() { Id = Guid.NewGuid().ToString(), Username = "sales", Email = "sales@songlinh.vn", Phone = "0912345679", Role = "Sales", Status = "Active", PasswordHash = hashedPass },
                        new() { Id = Guid.NewGuid().ToString(), Username = "tech", Email = "tech@songlinh.vn", Phone = "0912345680", Role = "Tech", Status = "Active", PasswordHash = hashedPass }
                    };

                    foreach (var u in defaults)
                    {
                        await _userRepo.AddAsync(u);
                    }
                    users = defaults;
                }

                var dtos = users.Select(u => new SystemUserDto
                {
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    Status = u.Status
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] SystemUserDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Username))
                {
                    return BadRequest("Tên đăng nhập không hợp lệ.");
                }

                var superAdminUsername = _config["SuperAdmin:Username"] ?? "admin";
                if (dto.Username.Equals(superAdminUsername, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Không được tạo tài khoản trùng với tên đăng nhập Super Admin.");
                }

                var users = await _userRepo.GetAllAsync();
                var existing = users.FirstOrDefault(u => u.Username.Equals(dto.Username, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    return BadRequest("Tên đăng nhập đã tồn tại trên hệ thống.");
                }

                if (string.IsNullOrWhiteSpace(dto.Password))
                {
                    return BadRequest("Mật khẩu là bắt buộc khi tạo tài khoản mới.");
                }

                var operatorName = User.Identity?.Name ?? "Unknown Admin";

                var newUser = new UserAccount
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = dto.Username,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Role = dto.Role,
                    Status = dto.Status,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
                };

                await _userRepo.AddAsync(newUser);

                var newValJson = JsonSerializer.Serialize(new { newUser.Username, newUser.Email, newUser.Phone, newUser.Role, newUser.Status });

                await LogAdminActionAsync(
                    action: "CREATE_USER",
                    details: $"Đã tạo mới tài khoản '{dto.Username}' (Quyền: {dto.Role}).",
                    oldValue: string.Empty,
                    newValue: newValJson,
                    user: operatorName
                );

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{username}")]
        public async Task<IActionResult> UpdateUser(string username, [FromBody] SystemUserDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Dữ liệu cập nhật không hợp lệ.");
                }

                var superAdminUsername = _config["SuperAdmin:Username"] ?? "admin";
                if (username.Equals(superAdminUsername, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Không thể chỉnh sửa tài khoản Super Admin.");
                }

                var users = await _userRepo.GetAllAsync();
                var existing = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    return NotFound("Không tìm thấy tài khoản để cập nhật.");
                }

                var operatorName = User.Identity?.Name ?? "Unknown Admin";
                var oldValJson = JsonSerializer.Serialize(new { existing.Username, existing.Email, existing.Phone, existing.Role, existing.Status });

                existing.Email = dto.Email;
                existing.Phone = dto.Phone;
                existing.Role = dto.Role;
                existing.Status = dto.Status;

                // Nếu admin nhập mật khẩu mới
                if (!string.IsNullOrWhiteSpace(dto.Password))
                {
                    existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                }

                await _userRepo.UpdateAsync(existing.Id, existing);

                var newValJson = JsonSerializer.Serialize(new { existing.Username, existing.Email, existing.Phone, existing.Role, existing.Status });

                await LogAdminActionAsync(
                    action: "UPDATE_USER",
                    details: $"Đã cập nhật tài khoản '{dto.Username}' (Quyền: {dto.Role}, Trạng thái: {dto.Status}).",
                    oldValue: oldValJson,
                    newValue: newValJson,
                    user: operatorName
                );

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            try
            {
                var superAdminUsername = _config["SuperAdmin:Username"] ?? "admin";
                if (username.Equals(superAdminUsername, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Không thể xóa tài khoản Super Admin.");
                }

                var users = await _userRepo.GetAllAsync();
                var userToDelete = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (userToDelete == null)
                {
                    return NotFound("Không tìm thấy tài khoản để xóa.");
                }

                var operatorName = User.Identity?.Name ?? "Unknown Admin";
                var oldValJson = JsonSerializer.Serialize(new { userToDelete.Username, userToDelete.Email, userToDelete.Phone, userToDelete.Role, userToDelete.Status });

                await _userRepo.DeleteAsync(userToDelete.Id);

                await LogAdminActionAsync(
                    action: "DELETE_USER",
                    details: $"Đã xóa tài khoản '{username}'.",
                    oldValue: oldValJson,
                    newValue: string.Empty,
                    user: operatorName
                );

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private async Task LogAdminActionAsync(string action, string details, string oldValue, string newValue, string user)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                User = user,
                Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                Details = details,
                OldValue = oldValue,
                NewValue = newValue
            };

            await _auditLogRepo.AddAsync(auditLog);
        }
    }
}
