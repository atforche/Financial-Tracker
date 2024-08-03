import FieldValue from "@core/fieldValues/FieldValue";

/**
 * AccountType field value that represents the different types of accounts.
 */
class AccountType extends FieldValue {
  /**
   * A Standard account represents a standard checking or savings account.
   */
  public static readonly Standard: AccountType = new AccountType("Standard");

  /**
   * A Debt account represents a credit card or loan account.
   */
  public static readonly Debt: AccountType = new AccountType("Debt");

  /**
   * An Investment account represents a retirement or brokerage account.
   */
  public static readonly Investment: AccountType = new AccountType(
    "Investment",
  );

  /**
   * Collection of all AccountTypes.
   */
  public static override readonly Collection = [
    AccountType.Standard,
    AccountType.Debt,
    AccountType.Investment,
  ];
}

export default AccountType;
