using System;
using System.ComponentModel.DataAnnotations;

namespace VendorPortalAPI.Models
{
    public class VendorProfile
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CellNumber { get; set; }
        public string CompanyName { get; set; }
        public string Category { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public TaxCompliance TaxCompliance { get; set; }
        public BankInformation BankInformation { get; set; }
        public BBBEEInformation BBBEEInformation { get; set; }
        public FinancialStatement FinancialStatement { get; set; }
        public bool IsProfileComplete { get; set; }
        public bool IsTaxComplete { get; set; }
        public bool IsBankComplete { get; set; }
        public bool IsBBBEEComplete { get; set; }
        public bool IsFinancialComplete { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public string Status { get; set; } = "Draft";
    }

    public class TaxCompliance
    {
        public int Id { get; set; }
        public int VendorProfileId { get; set; }
        public string TradeName { get; set; }
        public string IncomeTax { get; set; }
        public string ValueAddedTax { get; set; }
        public string TaxPayerReferenceNumber { get; set; }
        public string PayAsYouEarn { get; set; }
        public string TaxPayerName { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Pin { get; set; }
        public VendorProfile VendorProfile { get; set; }
    }

    public class BankInformation
    {
        public int Id { get; set; }
        public int VendorProfileId { get; set; }
        public string CompanyName { get; set; }
        public string BankName { get; set; }
        public string BankBranch { get; set; }
        public string BankCode { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public VendorProfile VendorProfile { get; set; }
    }

    public class BBBEEInformation
    {
        public int Id { get; set; }
        public int VendorProfileId { get; set; }
        public string CompanyName { get; set; }
        public string CertificateNumber { get; set; }
        public int BEELevel { get; set; }
        public DateTime DateOfIssue { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal BlackOwnership { get; set; }
        public decimal BlackWomenOwnership { get; set; }
        public decimal BlackYouth { get; set; }
        public decimal BlackDisabled { get; set; }
        public decimal BlackUnemployed { get; set; }
        public decimal BlackPeopleRuralAreas { get; set; }
        public decimal BlackMilitaryVeterans { get; set; }
        public VendorProfile VendorProfile { get; set; }
    }

    public class FinancialStatement
    {
        public int Id { get; set; }
        public int VendorProfileId { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal NetIncome { get; set; }
        public decimal Revenue { get; set; }
        public decimal CashFlow { get; set; }
        public DateTime StatementDate { get; set; }
        public VendorProfile VendorProfile { get; set; }
    }
}