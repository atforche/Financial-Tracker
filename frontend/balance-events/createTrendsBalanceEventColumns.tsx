import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import { BalanceEventType } from "@/balance-events/types";
import { Box } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type ColumnSortType from "@/framework/listframe/ColumnSortType";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import { formatCurrency } from "@/framework/currencyHelpers";
import { formatShortDate } from "@/framework/dateHelpers";

/**
 * Props for the createTrendsBalanceEventColumns function.
 */
interface TrendsBalanceEventListItem {
  readonly amount: number;
  readonly eventDate?: string | null;
  readonly isPosted: boolean;
  readonly transactionId: string;
  readonly type: BalanceEventType;
}

/**
 * Props for the ColumnSortProps interface.
 */
interface ColumnSortProps {
  readonly sortType: ColumnSortType | null;
  readonly onSort: (sortType: ColumnSortType | null) => void;
}

/**
 * Options for the createTrendsBalanceEventColumns function.
 */
interface SortPair<TSort> {
  readonly ascending: TSort;
  readonly descending: TSort;
}

/**
 * Options for the createTrendsBalanceEventColumns function.
 */
interface CreateTrendsBalanceEventColumnsOptions<
  T extends TrendsBalanceEventListItem,
  TSort,
> {
  readonly leadingColumns: readonly ColumnDefinition<T>[];
  readonly getSortProps: (
    ascendingSort: TSort,
    descendingSort: TSort,
  ) => ColumnSortProps;
  readonly dateSort: SortPair<TSort>;
  readonly typeSort?: SortPair<TSort>;
  readonly amountSort: SortPair<TSort>;
  readonly onOpen: (event: T) => void;
  readonly amountMinWidth?: number;
}

/**
 * Creates the common columns used by balance-event lists on trends pages.
 */
const createTrendsBalanceEventColumns = function <
  T extends TrendsBalanceEventListItem,
  TSort,
>({
  leadingColumns,
  getSortProps,
  dateSort,
  typeSort,
  amountSort,
  onOpen,
  amountMinWidth = 120,
}: CreateTrendsBalanceEventColumnsOptions<T, TSort>): ColumnDefinition<T>[] {
  const columns: ColumnDefinition<T>[] = [
    ...leadingColumns,
    {
      name: "date",
      headerContent: "Event Date",
      getBodyContent: (event) =>
        event.isPosted && typeof event.eventDate === "string"
          ? formatShortDate(new Date(`${event.eventDate}T00:00:00`))
          : "Pending",
      ...getSortProps(dateSort.ascending, dateSort.descending),
      minWidth: 130,
    },
  ];

  if (typeSort) {
    columns.push({
      name: "type",
      headerContent: "Type",
      getBodyContent: (event) => (
        <Box
          component="span"
          sx={{
            color:
              event.type === BalanceEventType.Debit
                ? "warning.dark"
                : "info.dark",
            fontWeight: 600,
          }}
        >
          {event.type === BalanceEventType.Debit ? "Debit" : "Credit"}
        </Box>
      ),
      ...getSortProps(typeSort.ascending, typeSort.descending),
      minWidth: 90,
    });
  }

  columns.push(
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (event) => formatCurrency(event.amount),
      ...getSortProps(amountSort.ascending, amountSort.descending),
      alignment: "right",
      minWidth: amountMinWidth,
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (event) => (
        <ListFrameActionButton
          size="small"
          color="primary"
          onClick={(clickEvent) => {
            clickEvent.stopPropagation();
            onOpen(event);
          }}
          ariaLabel={`Open transaction ${event.transactionId}`}
        >
          <ArrowForwardOutlined fontSize="small" color="action" />
        </ListFrameActionButton>
      ),
      alignment: "right",
      minWidth: 52,
      maxWidth: 52,
    },
  );

  return columns;
};

export default createTrendsBalanceEventColumns;
