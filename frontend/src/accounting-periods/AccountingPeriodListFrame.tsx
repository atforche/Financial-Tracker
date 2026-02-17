import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type JSX, useState } from "react";
import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import AccountingPeriodDialog from "@accounting-periods/AccountingPeriodDialog";
import { Checkbox } from "@mui/material";
import ColumnButton from "@framework/listframe/ColumnButton";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import ColumnHeaderButton from "@framework/listframe/ColumnHeaderButton";
import CreateAccountingPeriodDialog from "@accounting-periods/CreateAccountingPeriodDialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import ListFrame from "@framework/listframe/ListFrame";
import SuccessAlert from "@framework/alerts/SuccessAlert";
import useGetAllAccountingPeriods from "@accounting-periods/useGetAllAccountingPeriods";

/**
 * Component that provides a list of Accounting Period and makes the basic create, read, update, and delete
 * operations available on them.
 * @returns JSX element representing a list of Accounting Period with various action buttons.
 */
const AccountingPeriodListFrame = function (): JSX.Element {
  const [dialog, setDialog] = useState<JSX.Element | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const { accountingPeriods, isLoading, error, refetch } =
    useGetAllAccountingPeriods();
  return (
    <ListFrame<AccountingPeriod>
      name="Accounting Periods"
      headers={[
        <ColumnHeader key="period" content="Period" align="left" />,
        <ColumnHeader key="isOpen" content="Is Open" align="left" />,
        <ColumnHeader
          key="actions"
          content={
            <ColumnHeaderButton
              label="Add"
              icon={<AddCircleOutline />}
              onClick={() => {
                setDialog(
                  <CreateAccountingPeriodDialog
                    onClose={(success) => {
                      setDialog(null);
                      if (success) {
                        setMessage("Accounting Period added successfully.");
                      }
                      refetch();
                    }}
                  />,
                );
                setMessage(null);
              }}
            />
          }
          align="right"
        />,
      ]}
      columns={(accountingPeriod: AccountingPeriod) => [
        <ColumnCell
          key="period"
          content={accountingPeriod.name}
          align="left"
        />,
        <ColumnCell
          key="isOpen"
          content={<Checkbox checked={accountingPeriod.isOpen} />}
          align="left"
        />,
        <ColumnCell
          key="view"
          content={
            <ColumnButton
              label="View"
              icon={<ArrowForwardIos />}
              onClick={() => {
                setDialog(
                  <AccountingPeriodDialog
                    inputAccountingPeriod={accountingPeriod}
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
        />,
      ]}
      getId={(accountingPeriod: AccountingPeriod) => accountingPeriod.id}
      data={accountingPeriods}
      isLoading={isLoading}
      isError={error !== null}
    >
      {dialog}
      <SuccessAlert message={message} />
      <ErrorAlert error={error} />
    </ListFrame>
  );
};

export default AccountingPeriodListFrame;
