using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManager.Models;
using HotelManager.Models.Enums;

namespace HotelManager.Interfaces
{
    public interface IWorkAssignmentService : IService<WorkAssignment>
    {
        Task<List<WorkAssignment>> GetAssignmentsByEmployeeAsync(int employeeId);
        Task<List<WorkAssignment>> GetAssignmentsByRoomAsync(string roomNumber);
        Task<List<WorkAssignment>> GetAssignmentsByTypeAsync(AssignmentType type);
        Task<List<WorkAssignment>> GetAssignmentsByStatusAsync(AssignmentStatus status);
        Task<List<WorkAssignment>> GetPendingAssignmentsAsync();
        Task<WorkAssignment> AssignWorkAsync(string roomNumber, int employeeId, AssignmentType type, int? assignedByEmployeeId = null, string? notes = null);
        Task<WorkAssignment> CompleteAssignmentAsync(int assignmentId, string? notes = null);
        Task<WorkAssignment> CancelAssignmentAsync(int assignmentId, string? notes = null);
        Task<List<Employee>> GetAvailableEmployeesForTypeAsync(AssignmentType type);
        Task<WorkAssignment?> GetActiveAssignmentForRoomAsync(string roomNumber, AssignmentType type);
        Task<bool> AutoAssignWorkAsync(string roomNumber, AssignmentType type, int? assignedByEmployeeId = null);
    }
} 