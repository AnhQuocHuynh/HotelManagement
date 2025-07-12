using HotelManager.Data;
using HotelManager.Interfaces;
using HotelManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using HotelManager.Exceptions;
using Microsoft.Extensions.Logging;
using HotelManager.Core.Models;
using HotelManager.Core.Interfaces;
using HotelManager.Core.Repositories;

namespace HotelManager.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MaintenanceService> _logger;
        private readonly IMaintenanceRepository _maintenanceRepository;

        public MaintenanceService(IUnitOfWork unitOfWork, ILogger<MaintenanceService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _maintenanceRepository = unitOfWork.Maintenances;
        }

        public async Task AddMaintenanceAsync(Maintenance maintenance)
        {
            await _maintenanceRepository.AddAsync(maintenance);
            await _maintenanceRepository.SaveChangesAsync();
        }


        public async Task<List<MaintenanceReport>> GetAllReportsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all maintenance reports");
                var reports = await _unitOfWork.MaintenanceReports.GetAllAsync();
                var result = reports.OrderByDescending(r => r.ReportedDate).ThenBy(r => r.IsResolved).ToList();
                _logger.LogInformation("Retrieved {Count} maintenance reports", result.Count);
                return result;
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception when getting maintenance reports: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when getting maintenance reports: {Message}", ex.Message);
                throw new BusinessException($"Lỗi khi lấy danh sách báo cáo bảo trì: {ex.Message}", ex, "Lỗi khi lấy danh sách báo cáo bảo trì.", "MAINTENANCE_GETALL_ERROR");
            }
        }

        public async Task UpdateReportAsync(MaintenanceReport report)
        {
            try
            {
                _logger.LogInformation("Updating maintenance report. ReportId: {ReportId}", report.Id);
                await _unitOfWork.MaintenanceReports.UpdateAsync(report);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Maintenance report updated successfully. ReportId: {ReportId}", report.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when updating maintenance report: {Message}", ex.Message);
                throw;
            }
        }
    }
}
