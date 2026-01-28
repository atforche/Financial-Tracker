import { type JSX, useState } from "react";
import type { AccountingPeriod } from "@accounting-periods/ApiTypes";
import AccountingPeriodListFrameActionColumn from "@accounting-periods/AccountingPeriodListFrameActionColumn";
import AccountingPeriodListFrameActionColumnHeader from "@accounting-periods/AccountingPeriodListFrameActionColumnHeader";
import { Checkbox } from "@mui/material";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
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
        <ColumnHeader
          key="period"
          content="Period"
          minWidth={100}
          align="left"
        />,
        <ColumnHeader
          key="isOpen"
          content="Is Open"
          minWidth={100}
          align="left"
        />,
        <AccountingPeriodListFrameActionColumnHeader
          key="actions"
          setDialog={setDialog}
          setMessage={setMessage}
          refetch={refetch}
        />,
      ]}
      columns={(accountingPeriod: AccountingPeriod) => [
        <ColumnCell
          key="period"
          content={accountingPeriod.name}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="isOpen"
          content={<Checkbox checked={accountingPeriod.isOpen} />}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <AccountingPeriodListFrameActionColumn
          key="actions"
          accountingPeriod={accountingPeriod}
          isLoading={isLoading}
          error={error}
          setDialog={setDialog}
          setMessage={setMessage}
          refetch={refetch}
        />,
      ]}
      getId={(accountingPeriod: AccountingPeriod) => accountingPeriod.id}
      data={accountingPeriods}
    >
      {dialog}
      <SuccessAlert message={message} />
      <ErrorAlert error={error} />
    </ListFrame>
  );
};

export default AccountingPeriodListFrame;
