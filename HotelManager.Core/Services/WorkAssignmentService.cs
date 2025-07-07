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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WorkAssignmentService> _logger;
        private readonly IAuditService? _auditService;

        public WorkAssignmentService(IUnitOfWork unitOfWork, ILogger<WorkAssignmentService> logger, IAuditService? auditService = null)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<WorkAssignment> CreateAsync(WorkAssignment entity)
        {
            try
            {
                _logger.LogInformation("Creating work assignment for room {RoomNumber}", entity.RoomNumber);

                await _unitOfWork.WorkAssignments.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

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
            if (_unitOfWork is HotelManager.Repositories.UnitOfWork uowImpl && uowImpl.WorkAssignmentRepository != null)
            {
                var result = await uowImpl.WorkAssignmentRepository.GetByIdAsync(id);
                if (result == null)
                    throw new EntityNotFoundException("WorkAssignment", id);
                return result;
            }
            
            var genericResult = await _unitOfWork.WorkAssignments.GetByIdAsync(id);
            if (genericResult == null)
                throw new EntityNotFoundException("WorkAssignment", id);
            return genericResult;
        }

        public async Task<IEnumerable<WorkAssignment>> GetAllAsync()
        {
            if (_unitOfWork is HotelManager.Repositories.UnitOfWork uowImpl && uowImpl.WorkAssignmentRepository != null)
                return await uowImpl.WorkAssignmentRepository.GetAllAsync();
            return await _unitOfWork.WorkAssignments.GetAllAsync();
        }

        public async Task<WorkAssignment> UpdateAsync(WorkAssignment entity)
        {
            try
            {
                WorkAssignment existing;
                if (_unitOfWork is HotelManager.Repositories.UnitOfWork uowImpl && uowImpl.WorkAssignmentRepository != null)
                {
                    existing = await uowImpl.WorkAssignmentRepository.GetByIdAsync(entity.Id);
                }
                else
                {
                    existing = await _unitOfWork.WorkAssignments.GetByIdAsync(entity.Id);
                }
                
                if (existing == null)
                    throw new EntityNotFoundException("WorkAssignment", entity.Id);

                existing.Status = entity.Status;
                existing.Notes = entity.Notes;
                existing.CompletedDate = entity.CompletedDate;

                await _unitOfWork.WorkAssignments.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

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
                WorkAssignment assignment;
                if (_unitOfWork is HotelManager.Repositories.UnitOfWork uowImpl && uowImpl.WorkAssignmentRepository != null)
                {
                    assignment = await uowImpl.WorkAssignmentRepository.GetByIdAsync(id);
                }
                else
                {
                    assignment = await _unitOfWork.WorkAssignments.GetByIdAsync(id);
                }
                
                if (assignment == null)
                    throw new EntityNotFoundException("WorkAssignment", id);

                await _unitOfWork.WorkAssignments.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

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
            if (_unitOfWork is HotelManager.Repositories.UnitOfWork uowImpl && uowImpl.WorkAssignmentRepository != null)
            {
                var allAssignments = await uowImpl.WorkAssignmentRepository.GetAllAsync();
                return allAssignments
                    .Where(w => w.EmployeeId == employeeId)
                    .OrderByDescending(w => w.AssignedDate)
                    .ToList();
            }
            
            var genericAssignments = await _unitOfWork.WorkAssignments.GetAllAsync();
            return genericAssignments
                .Where(w => w.EmployeeId == employeeId)
                .OrderByDescending(w => w.AssignedDate)
                .ToList();
        }

        public async Task<List<WorkAssignment>> GetAssignmentsByRoomAsync(string roomNumber)
        {
            if (_unitOfWork is HotelManager.Repositories.UnitOfWork uowImpl && uowImpl.WorkAssignmentRepository != null)
            {
                var allAssignments = await uowImpl.WorkAssignmentRepository.GetAllAsync();
                return allAssignments
                    .Where(w => w.RoomNumber == roomNumber)
                    .OrderByDescending(w => w.AssignedDate)
                    .ToList();
            }
            
            var genericAssignments = await _unitOfWork.WorkAssignments.GetAllAsync();
            return genericAssignments
                .Where(w => w.RoomNumber == roomNumber)
                .OrderByDescending(w => w.AssignedDate)
                .ToList();
        }

        public async Task<List<WorkAssignment>> GetAssignmentsByTypeAsync(AssignmentType type)
        {
            if (_unitOfWork is HotelManager.Repositories.UnitOfWork uowImpl && uowImpl.WorkAssignmentRepository != null)
            {
                var allAssignments = await uowImpl.WorkAssignmentRepository.GetAllAsync();
                return allAssignments
                    .Where(w => w.Type == type)
                    .OrderByDescending(w => w.AssignedDate)
                    .ToList();
            }
            
            var genericAssignments = await _unitOfWork.WorkAssignments.GetAllAsync();
            return genericAssignments
                .Where(w => w.Type == type)
                .OrderByDescending(w => w.AssignedDate)
                .ToList();
        }

        public async Task<List<WorkAssignment>> GetAssignmentsByStatusAsync(AssignmentStatus status)
        {
            if (_unitOfWork is HotelManager.Repositories.UnitOfWork uowImpl && uowImpl.WorkAssignmentRepository != null)
            {
                var allAssignments = await uowImpl.WorkAssignmentRepository.GetAllAsync();
                return allAssignments
                    .Where(w => w.Status == status)
                    .OrderByDescending(w => w.AssignedDate)
                    .ToList();
            }
            
            var genericAssignments = await _unitOfWork.WorkAssignments.GetAllAsync();
            return genericAssignments
                .Where(w => w.Status == status)
                .OrderByDescending(w => w.AssignedDate)
                .ToList();
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

                WorkAssignment assignment;
                if (_unitOfWork is HotelManager.Repositories.UnitOfWork uowImpl && uowImpl.WorkAssignmentRepository != null)
                {
                    assignment = await uowImpl.WorkAssignmentRepository.GetByIdAsync(assignmentId);
                }
                else
                {
                    assignment = await _unitOfWork.WorkAssignments.GetByIdAsync(assignmentId);
                }
                
                if (assignment == null)
                    throw new EntityNotFoundException("WorkAssignment", assignmentId);

                if (assignment.Status == AssignmentStatus.Completed)
                    throw new BusinessException("Assignment is already completed");

                assignment.Status = AssignmentStatus.Completed;
                assignment.CompletedDate = DateTime.Now;
                if (!string.IsNullOrEmpty(notes))
                    assignment.Notes = assignment.Notes + (string.IsNullOrEmpty(assignment.Notes) ? "" : "; ") + notes;

                await _unitOfWork.WorkAssignments.UpdateAsync(assignment);
                await _unitOfWork.SaveChangesAsync();

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
                _logger.LogInformation("Cancelling assignment {AssignmentId}", assignmentId);

                WorkAssignment assignment;
                if (_unitOfWork is HotelManager.Repositories.UnitOfWork uowImpl && uowImpl.WorkAssignmentRepository != null)
                {
                    assignment = await uowImpl.WorkAssignmentRepository.GetByIdAsync(assignmentId);
                }
                else
                {
                    assignment = await _unitOfWork.WorkAssignments.GetByIdAsync(assignmentId);
                }
                
                if (assignment == null)
                    throw new EntityNotFoundException("WorkAssignment", assignmentId);

                if (assignment.Status == AssignmentStatus.Cancelled)
                    throw new BusinessException("Assignment is already cancelled");

                assignment.Status = AssignmentStatus.Cancelled;
                if (!string.IsNullOrEmpty(notes))
                    assignment.Notes = assignment.Notes + (string.IsNullOrEmpty(assignment.Notes) ? "" : "; ") + notes;

                await _unitOfWork.WorkAssignments.UpdateAsync(assignment);
                await _unitOfWork.SaveChangesAsync();

                await _auditService?.LogBusinessOperationAsync("Cancel", "WorkAssignment", assignmentId.ToString(), "System", true, $"Cancelled assignment {assignmentId}");

                _logger.LogInformation("Successfully cancelled assignment {AssignmentId}", assignmentId);
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
            var allEmployees = await _unitOfWork.Employees.GetAllAsync();
            var requiredPosition = GetRequiredPositionForAssignmentType(type);
            
            return allEmployees
                .Where(e => e.Position == requiredPosition)
                .ToList();
        }

        public async Task<WorkAssignment?> GetActiveAssignmentForRoomAsync(string roomNumber, AssignmentType type)
        {
            if (_unitOfWork is HotelManager.Repositories.UnitOfWork uowImpl && uowImpl.WorkAssignmentRepository != null)
            {
                var allAssignments = await uowImpl.WorkAssignmentRepository.GetAllAsync();
                return allAssignments
                    .FirstOrDefault(w => w.RoomNumber == roomNumber && 
                                       w.Type == type && 
                                       (w.Status == AssignmentStatus.Pending || w.Status == AssignmentStatus.InProgress));
            }
            
            var genericAssignments = await _unitOfWork.WorkAssignments.GetAllAsync();
            return genericAssignments
                .FirstOrDefault(w => w.RoomNumber == roomNumber && 
                    w.Type == type && 
                    (w.Status == AssignmentStatus.Pending || w.Status == AssignmentStatus.InProgress));
        }

        public async Task<bool> AutoAssignWorkAsync(string roomNumber, AssignmentType type, int? assignedByEmployeeId = null)
        {
            try
            {
                var availableEmployees = await GetAvailableEmployeesForTypeAsync(type);
                if (!availableEmployees.Any())
                {
                    _logger.LogWarning("No available employees for assignment type {Type}", type);
                    return false;
                }

                var activeAssignment = await GetActiveAssignmentForRoomAsync(roomNumber, type);
                if (activeAssignment != null)
                {
                    _logger.LogWarning("Room {RoomNumber} already has an active assignment of type {Type}", roomNumber, type);
                    return false;
                }

                var selectedEmployee = availableEmployees.First();
                await AssignWorkAsync(roomNumber, selectedEmployee.Id, type, assignedByEmployeeId);

                _logger.LogInformation("Auto-assigned work for room {RoomNumber} to employee {EmployeeId}", roomNumber, selectedEmployee.Id);
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
            return position == GetRequiredPositionForAssignmentType(type);
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