import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type JSX, useState } from "react";
import type { Account } from "@accounts/ApiTypes";
import AccountDialog from "@accounts/AccountDialog";
import ColumnButton from "@framework/listframe/ColumnButton";
import type ColumnDefinition from "@framework/listframe/ColumnDefinition";
import ColumnHeaderButton from "@framework/listframe/ColumnHeaderButton";
import CreateAccountDialog from "@accounts/CreateAccountDialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import ListFrame from "@framework/listframe/ListFrame";
import SuccessAlert from "@framework/alerts/SuccessAlert";
import formatCurrency from "@framework/formatCurrency";
import useGetAllAccounts from "@accounts/useGetAllAccounts";

/**
 * Component that provides a list of Accounts and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns JSX element representing a list of Accounts with various action buttons.
 */
const AccountListFrame = function (): JSX.Element {
  const [dialog, setDialog] = useState<JSX.Element | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const { accounts, totalCount, isLoading, error, refetch } = useGetAllAccounts(
    { page, rowsPerPage },
  );

  const columns: ColumnDefinition<Account>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (account: Account) => account.name,
    },
    {
      name: "type",
      headerContent: "Type",
      getBodyContent: (account: Account) => account.type,
    },
    {
      name: "balance",
      headerContent: "Posted Balance",
      getBodyContent: (account: Account) =>
        formatCurrency(account.currentBalance.postedBalance),
    },
    {
      name: "available",
      headerContent: "Available to Spend",
      getBodyContent: (account: Account) =>
        formatCurrency(
          account.currentBalance.postedBalance -
            account.currentBalance.pendingDebitAmount,
        ),
    },
    {
      name: "actions",
      headerContent: (
        <ColumnHeaderButton
          label="Add"
          icon={<AddCircleOutline />}
          onClick={() => {
            setDialog(
              <CreateAccountDialog
                onClose={(success) => {
                  setDialog(null);
                  if (success) {
                    setMessage("Account added successfully.");
                  }
                  refetch();
                }}
              />,
            );
            setMessage(null);
          }}
        />
      ),
      getBodyContent: (account: Account) => (
        <ColumnButton
          label="View"
          icon={<ArrowForwardIos />}
          onClick={() => {
            setDialog(
              <AccountDialog
                inputAccount={account}
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
    <ListFrame<Account>
      name="Accounts"
      columns={columns}
      getId={(account: Account) => account.id}
      data={accounts}
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

export default AccountListFrame;
