/**
 * Interface representing the state of a form element, including its value and any associated error message.
 */
interface StateElement<T> {
  readonly value: T;
  readonly errorMessage?: string | null;
}

export default StateElement;
