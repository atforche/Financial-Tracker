import type { SxProps, Theme } from "@mui/material/styles";
import type ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { JSX } from "react";

type ColumnSx = Exclude<SxProps<Theme>, readonly unknown[]>;

/**
 * Interface representing a column definition for a list frame.
 * @template T - The type of the data items that will be displayed in the list frame.
 */
interface ColumnDefinition<T> {
  readonly name: string;
  readonly headerContent: string | JSX.Element;
  readonly getBodyContent: (item: T) => string | JSX.Element | null;
  readonly sortType?: ColumnSortType | null;
  readonly onSort?: (sort: ColumnSortType | null) => void;
  readonly alignment?: "center" | "left" | "right";
  readonly maxWidth?: number;
  readonly minWidth?: number;
  readonly sx?: ColumnSx | null;
}

export default ColumnDefinition;
