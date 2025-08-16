using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IHomeMedService
{
    // Prescription Management
    Task<JsonModel> CreatePrescriptionAsync(CreatePrescriptionDto createDto);
    Task<JsonModel> GetPrescriptionAsync(Guid id);
    Task<JsonModel> GetUserPrescriptionsAsync(Guid userId);
    Task<JsonModel> GetProviderPrescriptionsAsync(Guid providerId);
    Task<JsonModel> UpdatePrescriptionAsync(Guid id, UpdatePrescriptionDto updateDto);
    Task<JsonModel> DeletePrescriptionAsync(Guid id);
    
    // Prescription Workflow
    Task<JsonModel> SendPrescriptionToPharmacyAsync(Guid prescriptionId);
    Task<JsonModel> ConfirmPrescriptionAsync(Guid prescriptionId, string pharmacyReference);
    Task<JsonModel> DispensePrescriptionAsync(Guid prescriptionId);
    Task<JsonModel> ShipPrescriptionAsync(Guid prescriptionId, string trackingNumber);
    Task<JsonModel> DeliverPrescriptionAsync(Guid prescriptionId);
    
    // Medication Shipment Management
    Task<JsonModel> CreateShipmentAsync(CreateMedicationShipmentDto createDto);
    Task<JsonModel> GetShipmentAsync(Guid id);
    Task<JsonModel> GetUserShipmentsAsync(Guid userId);
    Task<JsonModel> UpdateShipmentAsync(Guid id, UpdateMedicationShipmentDto updateDto);
    Task<JsonModel> DeleteShipmentAsync(Guid id);
    
    // Shipment Workflow
    Task<JsonModel> ProcessShipmentAsync(Guid shipmentId);
    Task<JsonModel> ShipMedicationAsync(Guid shipmentId, string trackingNumber, string carrier);
    Task<JsonModel> DeliverMedicationAsync(Guid shipmentId);
    Task<JsonModel> ReturnShipmentAsync(Guid shipmentId, string reason);
    
    // Tracking and Status
    Task<JsonModel> GetTrackingStatusAsync(string trackingNumber);
    Task<JsonModel> GetEstimatedDeliveryAsync(Guid shipmentId);
    Task<JsonModel> UpdateTrackingInfoAsync(Guid shipmentId, string trackingNumber, string status);
    
    // Analytics and Reporting
    Task<JsonModel> GetPrescriptionAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GetShipmentAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<JsonModel> GeneratePrescriptionReportAsync(DateTime startDate, DateTime endDate, string format = "pdf");
    Task<JsonModel> GenerateShipmentReportAsync(DateTime startDate, DateTime endDate, string format = "pdf");
    
    // Pharmacy Integration (Database Level)
    Task<JsonModel> GetPharmacyIntegrationAsync();
    Task<JsonModel> TestPharmacyConnectionAsync();
    Task<JsonModel> SyncPrescriptionsAsync();
    Task<JsonModel> SyncShipmentsAsync();
    
    // Refill Management
    Task<JsonModel> CreateRefillRequestAsync(Guid prescriptionId);
    Task<JsonModel> GetRefillRequestsAsync(Guid userId);
    Task<JsonModel> ApproveRefillRequestAsync(Guid prescriptionId);
    Task<JsonModel> DenyRefillRequestAsync(Guid prescriptionId, string reason);
    
    // Inventory Management (Placeholder for HomeMed API)
    Task<JsonModel> CheckMedicationAvailabilityAsync(string medicationName, string dosage);
    Task<JsonModel> GetMedicationPriceAsync(string medicationName, string dosage);
    Task<JsonModel> ReserveMedicationAsync(string medicationName, string dosage, int quantity);
    
    // Auto-dispatch (Placeholder for HomeMed API)
    Task<JsonModel> TriggerAutoDispatchAsync(Guid prescriptionId);
    Task<JsonModel> ProcessAutoDispatchQueueAsync();
    Task<JsonModel> GetAutoDispatchQueueAsync();
} 