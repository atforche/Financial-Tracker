import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type Fund, FundSortOrder } from "@funds/ApiTypes";
import { type JSX, useState } from "react";
import ColumnButton from "@framework/listframe/ColumnButton";
import type ColumnDefinition from "@framework/listframe/ColumnDefinition";
import ColumnHeaderButton from "@framework/listframe/ColumnHeaderButton";
import ColumnSortType from "@framework/listframe/ColumnSortType";
import CreateOrUpdateFundDialog from "@funds/CreateOrUpdateFundDialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import FundDialog from "@funds/FundDialog";
import ListFrame from "@framework/listframe/ListFrame";
import SuccessAlert from "@framework/alerts/SuccessAlert";
import formatCurrency from "@framework/formatCurrency";
import useGetAllFunds from "@funds/useGetAllFunds";

/**
 * Component that provides a list of Funds and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns JSX element representing a list of Funds with various action buttons.
 */
const FundListFrame = function (): JSX.Element {
  const [dialog, setDialog] = useState<JSX.Element | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [sortBy, setSortBy] = useState<FundSortOrder | null>(null);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const { funds, totalCount, isLoading, error, refetch } = useGetAllFunds({
    sortBy,
    page,
    rowsPerPage,
  });

  const columns: ColumnDefinition<Fund>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (fund: Fund) => fund.name,
      sortType:
        sortBy === FundSortOrder.Name
          ? ColumnSortType.Ascending
          : sortBy === FundSortOrder.NameDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(FundSortOrder.Name);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(FundSortOrder.NameDescending);
        } else {
          setSortBy(null);
        }
      },
    },
    {
      name: "description",
      headerContent: "Description",
      getBodyContent: (fund: Fund) => fund.description,
      sortType:
        sortBy === FundSortOrder.Description
          ? ColumnSortType.Ascending
          : sortBy === FundSortOrder.DescriptionDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(FundSortOrder.Description);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(FundSortOrder.DescriptionDescending);
        } else {
          setSortBy(null);
        }
      },
    },
    {
      name: "balance",
      headerContent: "Posted Balance",
      getBodyContent: (fund: Fund) =>
        formatCurrency(fund.currentBalance.postedBalance),
      sortType:
        sortBy === FundSortOrder.Balance
          ? ColumnSortType.Ascending
          : sortBy === FundSortOrder.BalanceDescending
            ? ColumnSortType.Descending
            : null,
      onSort: (sortType: ColumnSortType | null): void => {
        if (sortType === ColumnSortType.Ascending) {
          setSortBy(FundSortOrder.Balance);
        } else if (sortType === ColumnSortType.Descending) {
          setSortBy(FundSortOrder.BalanceDescending);
        } else {
          setSortBy(null);
        }
      },
    },
    {
      name: "actions",
      headerContent: (
        <ColumnHeaderButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {
            setDialog(
              <CreateOrUpdateFundDialog
                fund={null}
                setFund={null}
                onClose={(success) => {
                  setDialog(null);
                  if (success) {
                    setMessage("Fund added successfully.");
                  }
                  refetch();
                }}
              />,
            );
            setMessage(null);
          }}
        />
      ),
      getBodyContent: (fund: Fund) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            setDialog(
              <FundDialog
                inputFund={fund}
                setMessage={setMessage}
                onClose={(needsRefetch) => {
                  setDialog(null);
                  if (needsRefetch) {
                    refetch();
                  }
                }}
              />,
            );
            setMessage(null);
          }}
        />
      ),
      alignment: "right",
    },
  ];

  return (
    <ListFrame<Fund>
      name="Funds"
      columns={columns}
      getId={(fund: Fund) => fund.id}
      data={funds}
      totalCount={totalCount}
      isLoading={isLoading}
      isError={error !== null}
      page={page}
      setPage={setPage}
      rowsPerPage={rowsPerPage}
      setRowsPerPage={setRowsPerPage}
    >
      {dialog}
      <SuccessAlert message={message} />
      <ErrorAlert error={error} />
    </ListFrame>
  );
};

export default FundListFrame;
