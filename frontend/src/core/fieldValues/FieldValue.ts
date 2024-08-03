/**
 * Base class that all FieldValues extend from. A FieldValue represents a strongly typed constant value
 * that avoids the limitations of TypeScript enums.
 */
class FieldValue {
  private readonly value;

  /**
   * Collection of all instances of this FieldValue.
   */
  public static readonly Collection: FieldValue[] = [];

  /**
   * Constructs a new instance of this class.
   * @param {string} value - Value for this FieldValue.
   */
  protected constructor(value: string) {
    this.value = value;
  }

  /**
   * Returns the string representation for this FieldValue.
   * @returns {string} The string representation for this FieldValue.
   */
  public toString(): string {
    return this.value;
  }
}

export default FieldValue;
