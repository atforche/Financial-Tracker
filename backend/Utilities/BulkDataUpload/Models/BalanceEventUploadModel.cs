using System.Text.Json.Serialization;

namespace Utilities.BulkDataUpload.Models;

/// <summary>
/// Bulk data upload model representing a Balance Event
/// </summary>
[JsonDerivedType(typeof(TransactionAddedUploadModel), typeDiscriminator: "TransactionAdded")]
[JsonDerivedType(typeof(TransactionPostedUploadModel), typeDiscriminator: "TransactionPosted")]
[JsonDerivedType(typeof(FundConversionUploadModel), typeDiscriminator: "FundConversion")]
[JsonDerivedType(typeof(ChangeInValueUploadModel), typeDiscriminator: "ChangeInValue")]
public abstract class BalanceEventUploadModel
{
    /// <summary>
    /// Upload ID for this Balance Event
    /// </summary>
    public required Guid Id { get; init; }
}