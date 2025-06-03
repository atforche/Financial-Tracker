namespace Domain.FundConversions;

/// <summary>
/// Value object class representing the ID of an <see cref="FundConversion"/>
/// </summary>
public record FundConversionId : EntityId
{
    /// <summary>
    /// Constructs a new instance of this class. 
    /// This constructor should only even be used when creating a new Fund Conversion ID during Fund Conversion creation. 
    /// </summary>
    /// <param name="value">Value for this Fund Conversion ID</param>
    internal FundConversionId(Guid value)
        : base(value)
    {
    }
}

/// <summary>
/// Factory for constructing a Fund Conversion ID
/// </summary>
/// <param name="fundConversionRepository">Fund Conversion Repository</param>
public class FundConversionIdFactory(IFundConversionRepository fundConversionRepository)
{
    /// <summary>
    /// Creates a new Fund Conversion ID with the given value
    /// </summary>
    /// <param name="value">Value for this Fund Conversion ID</param>
    /// <returns>The newly created Fund Conversion ID</returns>
    public FundConversionId Create(Guid value)
    {
        if (!fundConversionRepository.DoesFundConversionWithIdExist(value))
        {
            throw new InvalidOperationException();
        }
        return new FundConversionId(value);
    }
}