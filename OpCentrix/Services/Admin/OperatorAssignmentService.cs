using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin
{
    public interface IOperatorAssignmentService
    {
        Task<List<MachineOperatorAssignment>> GetAssignmentsByMachineAsync(string machineId);
        Task<List<MachineOperatorAssignment>> GetAssignmentsByUserAsync(int userId);
        Task<MachineOperatorAssignment?> GetAsync(int id);
        Task<(bool Success, string? Warning, MachineOperatorAssignment? Assignment)> AssignOperatorAsync(string machineId, int userId, bool isPrimary, DateTime? from, DateTime? to, string actor);
        Task<bool> UnassignAsync(int assignmentId, string actor);
        Task<bool> SetPrimaryAsync(int assignmentId, string actor);
        Task<string?> CheckDoubleAssignmentWarningAsync(int userId, string? excludingMachineId = null);
    }

    public class OperatorAssignmentService : IOperatorAssignmentService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<OperatorAssignmentService> _logger;

        public OperatorAssignmentService(SchedulerContext context, ILogger<OperatorAssignmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<List<MachineOperatorAssignment>> GetAssignmentsByMachineAsync(string machineId)
        {
            return _context.Set<MachineOperatorAssignment>()
                .Where(a => a.MachineId == machineId)
                .Include(a => a.User)
                .OrderByDescending(a => a.IsActive)
                .ThenByDescending(a => a.IsPrimary)
                .ThenBy(a => a.User!.FullName)
                .AsNoTracking()
                .ToListAsync();
        }

        public Task<List<MachineOperatorAssignment>> GetAssignmentsByUserAsync(int userId)
        {
            return _context.Set<MachineOperatorAssignment>()
                .Where(a => a.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public Task<MachineOperatorAssignment?> GetAsync(int id)
        {
            return _context.Set<MachineOperatorAssignment>()
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<(bool Success, string? Warning, MachineOperatorAssignment? Assignment)> AssignOperatorAsync(
            string machineId, int userId, bool isPrimary, DateTime? from, DateTime? to, string actor)
        {
            try
            {
                // ensure user exists and is active
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
                if (user == null)
                {
                    return (false, "Selected user not found or inactive.", null);
                }

                // enforce one primary per machine (deactivate previous primaries if needed)
                if (isPrimary)
                {
                    var existingPrimary = await _context.Set<MachineOperatorAssignment>()
                        .Where(a => a.MachineId == machineId && a.IsPrimary && a.IsActive)
                        .ToListAsync();
                    foreach (var p in existingPrimary)
                    {
                        p.IsPrimary = false;
                        p.LastModifiedBy = actor;
                        p.LastModifiedDate = DateTime.UtcNow;
                    }
                }

                var warning = await CheckDoubleAssignmentWarningAsync(userId, machineId);

                var assignment = new MachineOperatorAssignment
                {
                    MachineId = machineId,
                    UserId = userId,
                    IsPrimary = isPrimary,
                    EffectiveFrom = from,
                    EffectiveTo = to,
                    IsActive = true,
                    CreatedBy = actor,
                    LastModifiedBy = actor,
                };
                _context.Set<MachineOperatorAssignment>().Add(assignment);
                await _context.SaveChangesAsync();

                assignment = await GetAsync(assignment.Id) ?? assignment;
                return (true, warning, assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning operator {UserId} to machine {MachineId}", userId, machineId);
                return (false, "Error assigning operator.", null);
            }
        }

        public async Task<bool> UnassignAsync(int assignmentId, string actor)
        {
            try
            {
                var entity = await _context.Set<MachineOperatorAssignment>().FirstOrDefaultAsync(a => a.Id == assignmentId);
                if (entity == null) return false;
                entity.IsActive = false;
                entity.LastModifiedBy = actor;
                entity.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unassigning operator assignment {AssignmentId}", assignmentId);
                return false;
            }
        }

        public async Task<bool> SetPrimaryAsync(int assignmentId, string actor)
        {
            try
            {
                var entity = await _context.Set<MachineOperatorAssignment>().FirstOrDefaultAsync(a => a.Id == assignmentId);
                if (entity == null) return false;

                // clear other primaries for this machine
                var others = await _context.Set<MachineOperatorAssignment>()
                    .Where(a => a.MachineId == entity.MachineId && a.IsPrimary && a.Id != assignmentId && a.IsActive)
                    .ToListAsync();
                foreach (var a in others)
                {
                    a.IsPrimary = false;
                    a.LastModifiedBy = actor;
                    a.LastModifiedDate = DateTime.UtcNow;
                }
                entity.IsPrimary = true;
                entity.LastModifiedBy = actor;
                entity.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary assignment {AssignmentId}", assignmentId);
                return false;
            }
        }

        public async Task<string?> CheckDoubleAssignmentWarningAsync(int userId, string? excludingMachineId = null)
        {
            try
            {
                var activeAssignments = await _context.Set<MachineOperatorAssignment>()
                    .Where(a => a.UserId == userId && a.IsActive)
                    .ToListAsync();
                if (!string.IsNullOrWhiteSpace(excludingMachineId))
                {
                    activeAssignments = activeAssignments.Where(a => a.MachineId != excludingMachineId).ToList();
                }
                if (activeAssignments.Count == 0) return null;

                var machines = string.Join(", ", activeAssignments.Select(a => a.MachineId).Distinct());
                return $"User already assigned to: {machines}. Proceed?";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking double assignment for user {UserId}", userId);
                return null;
            }
        }
    }
}
