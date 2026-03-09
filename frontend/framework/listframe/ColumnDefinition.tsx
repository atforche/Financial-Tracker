import type { JSX } from "react";

/**
 * Interface representing a column definition for a list frame.
 * @template T - The type of the data items that will be displayed in the list frame.
 */
interface ColumnDefinition<T> {
  readonly name: string;
  readonly headerContent: string | JSX.Element;
  readonly getBodyContent: (item: T) => string | JSX.Element;
  readonly alignment?: "center" | "left" | "right";
  readonly maxWidth?: number;
}

export default ColumnDefinition;
