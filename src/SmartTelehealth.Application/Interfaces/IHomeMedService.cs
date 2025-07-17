using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IHomeMedService
{
    // Prescription Management
    Task<ApiResponse<PrescriptionDto>> CreatePrescriptionAsync(CreatePrescriptionDto createDto);
    Task<ApiResponse<PrescriptionDto>> GetPrescriptionAsync(Guid id);
    Task<ApiResponse<IEnumerable<PrescriptionDto>>> GetUserPrescriptionsAsync(Guid userId);
    Task<ApiResponse<IEnumerable<PrescriptionDto>>> GetProviderPrescriptionsAsync(Guid providerId);
    Task<ApiResponse<PrescriptionDto>> UpdatePrescriptionAsync(Guid id, UpdatePrescriptionDto updateDto);
    Task<ApiResponse<bool>> DeletePrescriptionAsync(Guid id);
    
    // Prescription Workflow
    Task<ApiResponse<bool>> SendPrescriptionToPharmacyAsync(Guid prescriptionId);
    Task<ApiResponse<bool>> ConfirmPrescriptionAsync(Guid prescriptionId, string pharmacyReference);
    Task<ApiResponse<bool>> DispensePrescriptionAsync(Guid prescriptionId);
    Task<ApiResponse<bool>> ShipPrescriptionAsync(Guid prescriptionId, string trackingNumber);
    Task<ApiResponse<bool>> DeliverPrescriptionAsync(Guid prescriptionId);
    
    // Medication Shipment Management
    Task<ApiResponse<MedicationShipmentDto>> CreateShipmentAsync(CreateMedicationShipmentDto createDto);
    Task<ApiResponse<MedicationShipmentDto>> GetShipmentAsync(Guid id);
    Task<ApiResponse<IEnumerable<MedicationShipmentDto>>> GetUserShipmentsAsync(Guid userId);
    Task<ApiResponse<MedicationShipmentDto>> UpdateShipmentAsync(Guid id, UpdateMedicationShipmentDto updateDto);
    Task<ApiResponse<bool>> DeleteShipmentAsync(Guid id);
    
    // Shipment Workflow
    Task<ApiResponse<bool>> ProcessShipmentAsync(Guid shipmentId);
    Task<ApiResponse<bool>> ShipMedicationAsync(Guid shipmentId, string trackingNumber, string carrier);
    Task<ApiResponse<bool>> DeliverMedicationAsync(Guid shipmentId);
    Task<ApiResponse<bool>> ReturnShipmentAsync(Guid shipmentId, string reason);
    
    // Tracking and Status
    Task<ApiResponse<string>> GetTrackingStatusAsync(string trackingNumber);
    Task<ApiResponse<DateTime>> GetEstimatedDeliveryAsync(Guid shipmentId);
    Task<ApiResponse<bool>> UpdateTrackingInfoAsync(Guid shipmentId, string trackingNumber, string status);
    
    // Analytics and Reporting
    Task<ApiResponse<PrescriptionAnalyticsDto>> GetPrescriptionAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<MedicationShipmentAnalyticsDto>> GetShipmentAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<byte[]>> GeneratePrescriptionReportAsync(DateTime startDate, DateTime endDate, string format = "pdf");
    Task<ApiResponse<byte[]>> GenerateShipmentReportAsync(DateTime startDate, DateTime endDate, string format = "pdf");
    
    // Pharmacy Integration (Database Level)
    Task<ApiResponse<PharmacyIntegrationDto>> GetPharmacyIntegrationAsync();
    Task<ApiResponse<bool>> TestPharmacyConnectionAsync();
    Task<ApiResponse<bool>> SyncPrescriptionsAsync();
    Task<ApiResponse<bool>> SyncShipmentsAsync();
    
    // Refill Management
    Task<ApiResponse<PrescriptionDto>> CreateRefillRequestAsync(Guid prescriptionId);
    Task<ApiResponse<IEnumerable<PrescriptionDto>>> GetRefillRequestsAsync(Guid userId);
    Task<ApiResponse<bool>> ApproveRefillRequestAsync(Guid prescriptionId);
    Task<ApiResponse<bool>> DenyRefillRequestAsync(Guid prescriptionId, string reason);
    
    // Inventory Management (Placeholder for HomeMed API)
    Task<ApiResponse<bool>> CheckMedicationAvailabilityAsync(string medicationName, string dosage);
    Task<ApiResponse<decimal>> GetMedicationPriceAsync(string medicationName, string dosage);
    Task<ApiResponse<bool>> ReserveMedicationAsync(string medicationName, string dosage, int quantity);
    
    // Auto-dispatch (Placeholder for HomeMed API)
    Task<ApiResponse<bool>> TriggerAutoDispatchAsync(Guid prescriptionId);
    Task<ApiResponse<bool>> ProcessAutoDispatchQueueAsync();
    Task<ApiResponse<IEnumerable<PrescriptionDto>>> GetAutoDispatchQueueAsync();
} 