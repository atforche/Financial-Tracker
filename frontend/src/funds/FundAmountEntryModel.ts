/**
 * Model representing the data for a Fund Amount Entry Frame.
 */
interface FundAmountEntryModel {
  fundId: string | null;
  fundName: string | null;
  amount: number | null;
}

export default FundAmountEntryModel;
