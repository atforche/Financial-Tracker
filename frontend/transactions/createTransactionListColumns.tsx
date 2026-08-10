import { type Transaction, TransactionSort } from "@/transactions/types";
import {
  getTransactionDestinationLabel,
  getTransactionSourceLabel,
} from "@/transactions/transactionListHelpers";
import ArrowForwardOutlined from "@mui/icons-material/ArrowForwardOutlined";
import type ColumnDefinition from "@/framework/listframe/ColumnDefinition";
import ListFrameActionButton from "@/framework/listframe/ListFrameActionButton";
import createColumnSortProps from "@/framework/listframe/createColumnSortProps";
import { formatCurrency } from "@/framework/currencyHelpers";

/**
 * Options for creating transaction list columns.
 */
interface CreateTransactionListColumnsOptions {
  readonly currentSort: TransactionSort | null | undefined;
  readonly setSort: (sort: TransactionSort | null) => void;
  readonly openTransaction: (transaction: Transaction) => void;
  readonly includeAccountingPeriod?: boolean;
  readonly includeFullyPosted?: boolean;
}

/**
 * Creates the shared sortable columns used by transaction list frames.
 */
const createTransactionListColumns = function ({
  currentSort,
  setSort,
  openTransaction,
  includeAccountingPeriod = false,
  includeFullyPosted = false,
}: CreateTransactionListColumnsOptions): ColumnDefinition<Transaction>[] {
  const getSortProps = createColumnSortProps(currentSort, setSort);
  const columns: ColumnDefinition<Transaction>[] = [
    {
      name: "date",
      headerContent: "Date",
      getBodyContent: (transaction) => transaction.date,
      ...getSortProps(TransactionSort.Date, TransactionSort.DateDescending),
      minWidth: 125,
    },
    {
      name: "description",
      headerContent: "Description",
      getBodyContent: (transaction) => transaction.description,
      ...getSortProps(
        TransactionSort.Description,
        TransactionSort.DescriptionDescending,
      ),
      mobilePrimary: true,
      minWidth: 150,
    },
    {
      name: "source",
      headerContent: "Source",
      getBodyContent: getTransactionSourceLabel,
      ...getSortProps(TransactionSort.Source, TransactionSort.SourceDescending),
      minWidth: 100,
    },
    {
      name: "destination",
      headerContent: "Destination",
      getBodyContent: getTransactionDestinationLabel,
      ...getSortProps(
        TransactionSort.Destination,
        TransactionSort.DestinationDescending,
      ),
      minWidth: 100,
    },
    {
      name: "amount",
      headerContent: "Amount",
      getBodyContent: (transaction) => formatCurrency(transaction.amount),
      ...getSortProps(TransactionSort.Amount, TransactionSort.AmountDescending),
      alignment: "right",
      minWidth: 100,
    },
    {
      name: "actions",
      headerContent: "",
      getBodyContent: (transaction) => (
        <ListFrameActionButton
          size="small"
          color="primary"
          onClick={(event) => {
            event.stopPropagation();
            openTransaction(transaction);
          }}
          ariaLabel={`Open transaction ${transaction.id}`}
        >
          <ArrowForwardOutlined fontSize="small" color="action" />
        </ListFrameActionButton>
      ),
      alignment: "right",
      minWidth: 52,
      maxWidth: 52,
    },
  ];

  if (includeAccountingPeriod) {
    columns.splice(1, 0, {
      name: "accountingPeriod",
      headerContent: "Accounting Period",
      getBodyContent: (transaction) => transaction.accountingPeriodName,
      ...getSortProps(
        TransactionSort.AccountingPeriod,
        TransactionSort.AccountingPeriodDescending,
      ),
      minWidth: 165,
    });
  }

  if (includeFullyPosted) {
    columns.splice(-1, 0, {
      name: "fullyPosted",
      headerContent: "Fully Posted",
      getBodyContent: (transaction) => (transaction.fullyPosted ? "Yes" : "No"),
      ...getSortProps(
        TransactionSort.FullyPosted,
        TransactionSort.FullyPostedDescending,
      ),
      minWidth: 125,
    });
  }

  return columns;
};

export default createTransactionListColumns;
