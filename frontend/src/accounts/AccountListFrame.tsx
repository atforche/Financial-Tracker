import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type JSX, useState } from "react";
import type { Account } from "@accounts/ApiTypes";
import AccountDialog from "@accounts/AccountDialog";
import ColumnButton from "@framework/listframe/ColumnButton";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
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
  const { accounts, isLoading, error, refetch } = useGetAllAccounts();
  return (
    <ListFrame<Account>
      name="Accounts"
      headers={[
        <ColumnHeader key="name" content="Name" minWidth={170} align="left" />,
        <ColumnHeader key="type" content="Type" minWidth={170} align="left" />,
        <ColumnHeader
          key="balance"
          content="Posted Balance"
          minWidth={170}
          align="left"
        />,
        <ColumnHeader
          key="available"
          content="Available to Spend"
          minWidth={170}
          align="left"
        />,
        <ColumnHeader
          key="actions"
          content={
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
          }
          minWidth={125}
          align="right"
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
        <ColumnCell
          key="view"
          content={
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
          }
          align="right"
          isLoading={isLoading}
          isError={error !== null}
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
