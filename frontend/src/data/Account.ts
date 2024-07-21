/**
 * Enum representing the different types of Accounts.
 */
enum AccountType {
  Standard,
  Debt,
  Investment,
}

/**
 * Data class representing an account.
 */
class Account {
  private _isActive = false;
  private _name = "";
  private readonly _type: AccountType;

  /**
   * Constructs a new instance of this class.
   * @param {string} name - Name for this Account.
   * @param {AccountType} type - Type for this Account.
   * @param {boolean} isActive - IsActive flag for this Account.
   */
  public constructor(name: string, type: AccountType, isActive: boolean) {
    this.name = name;
    this._type = type;
    this.isActive = isActive;
  }

  /**
   * Gets the isActive flag for this Account.
   * @returns {boolean} The isActive flag for this Account.
   */
  public get isActive(): boolean {
    return this._isActive;
  }

  /**
   * Sets the isActive flag for this Account.
   */
  public set isActive(value: boolean) {
    this._isActive = value;
  }

  /**
   * Gets the name for this Account.
   * @returns {string} The name for this Account.
   */
  public get name(): string {
    return this._name;
  }

  /**
   * Sets the name for this Account.
   */
  public set name(value: string) {
    this._name = value;
  }

  /**
   * Gets the type for this Account.
   * @returns {string} The type for this Account.
   */
  public get type(): AccountType {
    return this._type;
  }
}

export { Account, AccountType };
