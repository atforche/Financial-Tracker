import AccountType from "@core/fieldValues/AccountType";
import type { CreateAccountRequest } from "@data/AccountRepository";

// Temporary last number for creating accounts
let accountKey = 1n;

/**
 * Type representing the key value of an account.
 */
type AccountKey = bigint;

/**
 * Data class representing an account.
 */
class Account {
  private readonly _key: AccountKey;
  private _name = "";
  private readonly _type: AccountType;
  private _isActive: boolean;

  /**
   * Constructs a new instance of this class.
   * @param {CreateAccountRequest} request - Request to create an Account.
   */
  public constructor({ name, type, isActive = true }: CreateAccountRequest) {
    this._key = accountKey++;
    this._name = name;
    this._type = type;
    this._isActive = isActive;
  }

  /**
   * Gets the key for this Account.
   * @returns {AccountKey} The key for this Account.
   */
  public get key(): AccountKey {
    return this._key;
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
}

export { Account, type AccountKey, AccountType };
