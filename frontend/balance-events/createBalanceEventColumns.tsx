import { BalanceEventType } from "@/balance-events/types";
import { Box } from "@mui/material";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import type ColumnSortType from "@/framework/listframe/ColumnSortType";
import KeyboardArrowRight from "@mui/icons-material/KeyboardArrowRight";
import { formatCurrency } from "@/framework/currencyHelpers";
import { formatLongDate } from "@/framework/dateHelpers";

/**
 * Represents a balance event item used for creating columns in the balance event list.
 */
interface BalanceEventListItem {
  readonly amount: number;
  readonly description: string;
  readonly eventDate?: string | null;
  readonly isPosted: boolean;
  readonly type: BalanceEventType;
}

/**
 * Options for creating balance event columns.
 */
interface CreateBalanceEventColumnsOptions<T extends BalanceEventListItem> {
  readonly getPreviousBalance?: (event: T) => number;
  readonly getNewBalance?: (event: T) => number;
  readonly getCounterpartyContent?: (event: T) => string;
  readonly counterpartySortProps?: {
    readonly sortType: ColumnSortType | null;
    readonly onSort: (sortType: ColumnSortType | null) => void;
  };
}

/**
 * Creates the common columns used by workspace balance-event lists.
 */
const createBalanceEventColumns = function <T extends BalanceEventListItem>({
  getPreviousBalance,
  getNewBalance,
  getCounterpartyContent,
  counterpartySortProps,
}: CreateBalanceEventColumnsOptions<T>): readonly ColumnDefinition<T>[] {
  const columns: ColumnDefinition<T>[] = [
    {
      name: "date",
      headerContent: "Event Date",
      getBodyContent: (event) =>
        event.isPosted && typeof event.eventDate === "string"
          ? formatLongDate(new Date(`${event.eventDate}T00:00:00`))
          : "Pending",
      minWidth: 135,
    },
    {
      name: "description",
      headerContent: "Description",
      getBodyContent: (event) => event.description,
      mobilePrimary: true,
      minWidth: 180,
    },
    {
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
      minWidth: 130,
    },
  ];

  if (getCounterpartyContent !== undefined) {
    if (counterpartySortProps === undefined) {
      columns.push({
        name: "counterparty",
        headerContent: "From / To",
        getBodyContent: getCounterpartyContent,
        minWidth: 190,
      });
    } else {
      columns.push({
        name: "counterparty",
        headerContent: "From / To",
        getBodyContent: getCounterpartyContent,
        ...counterpartySortProps,
        minWidth: 190,
      });
    }
  }

  columns.push({
    name: "amount",
    headerContent: "Amount",
    getBodyContent: (event) => formatCurrency(event.amount),
    alignment: "right",
    minWidth: 120,
  });

  if (getPreviousBalance && getNewBalance) {
    columns.push(
      {
        name: "before",
        headerContent: "Balance Before",
        getBodyContent: (event) => formatCurrency(getPreviousBalance(event)),
        alignment: "right",
        minWidth: 150,
      },
      {
        name: "after",
        headerContent: "Balance After",
        getBodyContent: (event) => formatCurrency(getNewBalance(event)),
        alignment: "right",
        minWidth: 150,
      },
    );
  }

  columns.push({
    name: "actions",
    headerContent: "",
    getBodyContent: () => (
      <Box
        sx={{
          alignItems: "center",
          color: "text.secondary",
          display: "flex",
          justifyContent: "center",
          minHeight: 40,
        }}
      >
        <KeyboardArrowRight fontSize="small" />
      </Box>
    ),
    alignment: "center",
    minWidth: 52,
    maxWidth: 52,
  });

  return columns;
};

export default createBalanceEventColumns;
