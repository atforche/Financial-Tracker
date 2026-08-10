import type { SxProps, Theme } from "@mui/material/styles";
import type ColumnSortType from "@/framework/listframe/ColumnSortType";
import type { ReactNode } from "react";

type ColumnSx = Exclude<SxProps<Theme>, readonly unknown[]>;

/**
 * Interface representing a column definition for a list frame.
 * @template T - The type of the data items that will be displayed in the list frame.
 */
interface ColumnDefinitionBase<T> {
  readonly name: string;
  readonly headerContent: ReactNode;
  readonly getBodyContent: (item: T) => ReactNode;
  readonly alignment?: "center" | "left" | "right";
  readonly maxWidth?: number;
  readonly minWidth?: number;
  /** Whether this column should be emphasized as the primary value on mobile. */
  readonly mobilePrimary?: boolean;
  readonly sx?: ColumnSx | null;
}

/**
 * Interface representing a column definition for a list frame with optional sorting capabilities.
 */
type ColumnDefinition<T> = ColumnDefinitionBase<T> &
  (
    | {
        readonly sortType: ColumnSortType | null;
        readonly onSort: (sort: ColumnSortType | null) => void;
      }
    | {
        readonly sortType?: never;
        readonly onSort?: never;
      }
  );

export default ColumnDefinition;
