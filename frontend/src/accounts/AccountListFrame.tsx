import { type JSX, useState } from "react";
import type { Account } from "@accounts/ApiTypes";
import AccountListFrameActionColumn from "@accounts/AccountListFrameActionColumn";
import AccountListFrameActionColumnHeader from "@accounts/AccountListFrameActionColumnHeader";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
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
  const { accounts, isLoading, error, refetch } = useGetAllAccounts();
  return (
    <ListFrame<Account>
      name="Accounts"
      headers={[
        <ColumnHeader key="name" content="Name" minWidth={170} align="left" />,
        <ColumnHeader key="type" content="Type" minWidth={170} align="left" />,
        <ColumnHeader
          key="balance"
          content="Current Balance"
          minWidth={170}
          align="left"
        />,
        <ColumnHeader
          key="available"
          content="Available to Spend"
          minWidth={170}
          align="left"
        />,
        <AccountListFrameActionColumnHeader
          key="actions"
          setDialog={setDialog}
          setMessage={setMessage}
          refetch={refetch}
        />,
      ]}
      columns={(account: Account) => [
        <ColumnCell
          key="name"
          content={account.name}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="type"
          content={account.type}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="balance"
          content={formatCurrency(account.currentBalance.balance)}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="available"
          content={formatCurrency(
            account.currentBalance.balance -
              account.currentBalance.pendingDebitAmount,
          )}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <AccountListFrameActionColumn
          key="actions"
          account={account}
          isLoading={isLoading}
          error={error}
          setDialog={setDialog}
          setMessage={setMessage}
          refetch={refetch}
        />,
      ]}
      getId={(account: Account) => account.id}
      data={accounts}
    >
      {dialog}
      <SuccessAlert message={message} />
      <ErrorAlert error={error} />
    </ListFrame>
  );
};

export default AccountListFrame;
