using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using HotelManager.Data;
using HotelManager.Interfaces;
using HotelManager.Models;
using HotelManager.Models.Enums;
using HotelManager.Exceptions;

namespace HotelManager.Services
{
    public class WorkAssignmentService : IWorkAssignmentService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WorkAssignmentService> _logger;
        private readonly IAuditService? _auditService;

        public WorkAssignmentService(IServiceScopeFactory scopeFactory, ILogger<WorkAssignmentService> logger, IAuditService? auditService = null)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<WorkAssignment> CreateAsync(WorkAssignment entity)
        {
            try
            {
                _logger.LogInformation("Creating work assignment for room {RoomNumber}, employee {EmployeeId}, type {Type}", 
                    entity.RoomNumber, entity.EmployeeId, entity.Type);

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

                // Validate room exists
                var room = await context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == entity.RoomNumber);
                if (room == null)
                    throw new EntityNotFoundException("Room", entity.RoomNumber);

                // Validate employee exists and has correct position
                var employee = await context.Employees.FirstOrDefaultAsync(e => e.Id == entity.EmployeeId);
                if (employee == null)
                    throw new EntityNotFoundException("Employee", entity.EmployeeId);

                // Check if employee position matches assignment type
                if (!IsValidEmployeeForAssignmentType(employee.Position, entity.Type))
                    throw new BusinessException($"Employee position {employee.Position} is not valid for assignment type {entity.Type}");

                // Check for existing active assignment
                var existingAssignment = await GetActiveAssignmentForRoomAsync(entity.RoomNumber, entity.Type);
                if (existingAssignment != null)
                    throw new BusinessException($"Room {entity.RoomNumber} already has an active {entity.Type} assignment");

                entity.AssignedDate = DateTime.Now;
                entity.Status = AssignmentStatus.Pending;

                context.WorkAssignments.Add(entity);
                await context.SaveChangesAsync();

                await _auditService?.LogBusinessOperationAsync("Create", "WorkAssignment", entity.Id.ToString(), "System", true, $"Created assignment for room {entity.RoomNumber}");

                _logger.LogInformation("Successfully created work assignment {AssignmentId}", entity.Id);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work assignment");
                throw;
            }
        }

        public async Task<WorkAssignment> GetByIdAsync(int id)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            
            var result = await context.WorkAssignments
                .Include(w => w.Employee)
                .Include(w => w.AssignedByEmployee)
                .Include(w => w.Room)
                .FirstOrDefaultAsync(w => w.Id == id);
            
            if (result == null)
                throw new EntityNotFoundException("WorkAssignment", id);
                
            return result;
        }

        public async Task<IEnumerable<WorkAssignment>> GetAllAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            
            return await context.WorkAssignments
                .Include(w => w.Employee)
                .Include(w => w.AssignedByEmployee)
                .Include(w => w.Room)
                .OrderByDescending(w => w.AssignedDate)
                .ToListAsync();
        }

        public async Task<WorkAssignment> UpdateAsync(WorkAssignment entity)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

                var existing = await context.WorkAssignments.FindAsync(entity.Id);
                if (existing == null)
                    throw new EntityNotFoundException("WorkAssignment", entity.Id);

                existing.Status = entity.Status;
                existing.Notes = entity.Notes;
                existing.CompletedDate = entity.CompletedDate;

                await context.SaveChangesAsync();

                await _auditService?.LogBusinessOperationAsync("Update", "WorkAssignment", entity.Id.ToString(), "System", true, $"Updated assignment {entity.Id}");

                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work assignment {Id}", entity.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

                var assignment = await context.WorkAssignments.FindAsync(id);
                if (assignment == null)
                    throw new EntityNotFoundException("WorkAssignment", id);

                context.WorkAssignments.Remove(assignment);
                await context.SaveChangesAsync();

                await _auditService?.LogBusinessOperationAsync("Delete", "WorkAssignment", id.ToString(), "System", true, $"Deleted assignment {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting work assignment {Id}", id);
                throw;
            }
        }

        public async Task<List<WorkAssignment>> GetAssignmentsByEmployeeAsync(int employeeId)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            
            return await context.WorkAssignments
                .Include(w => w.Employee)
                .Include(w => w.AssignedByEmployee)
                .Include(w => w.Room)
                .Where(w => w.EmployeeId == employeeId)
                .OrderByDescending(w => w.AssignedDate)
                .ToListAsync();
        }

        public async Task<List<WorkAssignment>> GetAssignmentsByRoomAsync(string roomNumber)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            
            return await context.WorkAssignments
                .Include(w => w.Employee)
                .Include(w => w.AssignedByEmployee)
                .Include(w => w.Room)
                .Where(w => w.RoomNumber == roomNumber)
                .OrderByDescending(w => w.AssignedDate)
                .ToListAsync();
        }

        public async Task<List<WorkAssignment>> GetAssignmentsByTypeAsync(AssignmentType type)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            
            return await context.WorkAssignments
                .Include(w => w.Employee)
                .Include(w => w.AssignedByEmployee)
                .Include(w => w.Room)
                .Where(w => w.Type == type)
                .OrderByDescending(w => w.AssignedDate)
                .ToListAsync();
        }

        public async Task<List<WorkAssignment>> GetAssignmentsByStatusAsync(AssignmentStatus status)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            
            return await context.WorkAssignments
                .Include(w => w.Employee)
                .Include(w => w.AssignedByEmployee)
                .Include(w => w.Room)
                .Where(w => w.Status == status)
                .OrderByDescending(w => w.AssignedDate)
                .ToListAsync();
        }

        public async Task<List<WorkAssignment>> GetPendingAssignmentsAsync()
        {
            return await GetAssignmentsByStatusAsync(AssignmentStatus.Pending);
        }

        public async Task<WorkAssignment> AssignWorkAsync(string roomNumber, int employeeId, AssignmentType type, int? assignedByEmployeeId = null, string? notes = null)
        {
            var assignment = new WorkAssignment
            {
                RoomNumber = roomNumber,
                EmployeeId = employeeId,
                Type = type,
                AssignedByEmployeeId = assignedByEmployeeId,
                Notes = notes,
                Status = AssignmentStatus.Pending,
                AssignedDate = DateTime.Now
            };

            return await CreateAsync(assignment);
        }

        public async Task<WorkAssignment> CompleteAssignmentAsync(int assignmentId, string? notes = null)
        {
            try
            {
                _logger.LogInformation("Completing assignment {AssignmentId}", assignmentId);

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

                var assignment = await context.WorkAssignments.FindAsync(assignmentId);
                if (assignment == null)
                    throw new EntityNotFoundException("WorkAssignment", assignmentId);

                if (assignment.Status == AssignmentStatus.Completed)
                    throw new BusinessException("Assignment is already completed");

                assignment.Status = AssignmentStatus.Completed;
                assignment.CompletedDate = DateTime.Now;
                if (!string.IsNullOrEmpty(notes))
                    assignment.Notes = assignment.Notes + (string.IsNullOrEmpty(assignment.Notes) ? "" : "; ") + notes;

                await context.SaveChangesAsync();

                await _auditService?.LogBusinessOperationAsync("Complete", "WorkAssignment", assignmentId.ToString(), "System", true, $"Completed assignment {assignmentId}");

                _logger.LogInformation("Successfully completed assignment {AssignmentId}", assignmentId);
                return assignment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing assignment {AssignmentId}", assignmentId);
                throw;
            }
        }

        public async Task<WorkAssignment> CancelAssignmentAsync(int assignmentId, string? notes = null)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

                var assignment = await context.WorkAssignments.FindAsync(assignmentId);
                if (assignment == null)
                    throw new EntityNotFoundException("WorkAssignment", assignmentId);

                assignment.Status = AssignmentStatus.Cancelled;
                if (!string.IsNullOrEmpty(notes))
                    assignment.Notes = assignment.Notes + (string.IsNullOrEmpty(assignment.Notes) ? "" : "; ") + notes;

                await context.SaveChangesAsync();

                await _auditService?.LogBusinessOperationAsync("Cancel", "WorkAssignment", assignmentId.ToString(), "System", true, $"Cancelled assignment {assignmentId}");

                return assignment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling assignment {AssignmentId}", assignmentId);
                throw;
            }
        }

        public async Task<List<Employee>> GetAvailableEmployeesForTypeAsync(AssignmentType type)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

            var requiredPosition = GetRequiredPositionForAssignmentType(type);
            
            // Get employees with correct position who don't have active assignments
            var availableEmployees = await context.Employees
                .Where(e => e.Position == requiredPosition)
                .Where(e => !context.WorkAssignments.Any(w => 
                    w.EmployeeId == e.Id && 
                    (w.Status == AssignmentStatus.Pending || w.Status == AssignmentStatus.InProgress)))
                .ToListAsync();

            return availableEmployees;
        }

        public async Task<WorkAssignment?> GetActiveAssignmentForRoomAsync(string roomNumber, AssignmentType type)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            
            return await context.WorkAssignments
                .Include(w => w.Employee)
                .Include(w => w.AssignedByEmployee)
                .Include(w => w.Room)
                .FirstOrDefaultAsync(w => 
                    w.RoomNumber == roomNumber && 
                    w.Type == type && 
                    (w.Status == AssignmentStatus.Pending || w.Status == AssignmentStatus.InProgress));
        }

        public async Task<bool> AutoAssignWorkAsync(string roomNumber, AssignmentType type, int? assignedByEmployeeId = null)
        {
            try
            {
                _logger.LogInformation("Auto-assigning {Type} work for room {RoomNumber}", type, roomNumber);

                // Check if there's already an active assignment
                var existingAssignment = await GetActiveAssignmentForRoomAsync(roomNumber, type);
                if (existingAssignment != null)
                {
                    _logger.LogInformation("Room {RoomNumber} already has active {Type} assignment", roomNumber, type);
                    return false;
                }

                // Get available employees
                var availableEmployees = await GetAvailableEmployeesForTypeAsync(type);
                if (!availableEmployees.Any())
                {
                    _logger.LogWarning("No available employees found for {Type} assignment", type);
                    return false;
                }

                // Simple round-robin assignment (could be enhanced with load balancing)
                var selectedEmployee = availableEmployees.OrderBy(e => e.Id).First();

                await AssignWorkAsync(roomNumber, selectedEmployee.Id, type, assignedByEmployeeId, "Auto-assigned");

                _logger.LogInformation("Successfully auto-assigned {Type} work for room {RoomNumber} to employee {EmployeeId}", 
                    type, roomNumber, selectedEmployee.Id);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-assigning work for room {RoomNumber}", roomNumber);
                return false;
            }
        }

        private static bool IsValidEmployeeForAssignmentType(EmployeePosition position, AssignmentType type)
        {
            return type switch
            {
                AssignmentType.Cleaning => position == EmployeePosition.Cleaner,
                AssignmentType.Maintenance => position == EmployeePosition.Technician,
                AssignmentType.Inspection => position == EmployeePosition.Manager || position == EmployeePosition.Receptionist,
                _ => false
            };
        }

        private static EmployeePosition GetRequiredPositionForAssignmentType(AssignmentType type)
        {
            return type switch
            {
                AssignmentType.Cleaning => EmployeePosition.Cleaner,
                AssignmentType.Maintenance => EmployeePosition.Technician,
                AssignmentType.Inspection => EmployeePosition.Manager,
                _ => throw new ArgumentException($"Unknown assignment type: {type}")
            };
        }
    }
} 