import type { CreateFundRequest } from "@data/FundRepository";

// Temporary last number for creating funds
let fundKey = 1n;

/**
 * Type representing the key value of an fund.
 */
type FundKey = bigint;

/**
 * Data class representing a fund.
 */
class Fund {
  private readonly _id: FundKey;
  private _name = "";
  private _description: string | null = null;

  /**
   * Constructs a new instance of this class.
   * @param {CreateFundRequest} request - Request to create a Fund.
   */
  public constructor({ name, description }: CreateFundRequest) {
    this._id = fundKey++;
    this._name = name;
    this._description = description;
  }

  /**
   * Gets the key for this Fund.
   * @returns {FundKey} The key for this Fund.
   */
  public get key(): FundKey {
    return this._id;
  }

  /**
   * Gets the name for this Fund.
   * @returns {string} The name for this Fund.
   */
  public get name(): string {
    return this._name;
  }

  /**
   * Sets the name for this Fund.
   */
  public set name(value: string) {
    this._name = value;
  }

  /**
   * Gets the description for this Fund.
   * @returns {string} The description for this Fund.
   */
  public get description(): string | null {
    return this._description;
  }

  /**
   * Sets the description for this Fund.
   */
  public set description(value: string) {
    this._description = value;
  }
}

export { Fund, type FundKey };
