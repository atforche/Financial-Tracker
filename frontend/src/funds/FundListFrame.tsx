import { type JSX, useState } from "react";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { Fund } from "@funds/ApiTypes";
import FundListFrameActionColumn from "@funds/FundListFrameActionColumn";
import FundListFrameActionColumnHeader from "@funds/FundListFrameActionColumnHeader";
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
  const { funds, isLoading, error, refetch } = useGetAllFunds();
  return (
    <ListFrame<Fund>
      name="Funds"
      headers={[
        <ColumnHeader key="name" content="Name" minWidth={170} align="left" />,
        <ColumnHeader
          key="description"
          content="Description"
          minWidth={170}
          align="left"
        />,
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
        <FundListFrameActionColumnHeader
          key="actions"
          setDialog={setDialog}
          setMessage={setMessage}
          refetch={refetch}
        />,
      ]}
      columns={(fund: Fund) => [
        <ColumnCell
          key="name"
          content={fund.name}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="description"
          content={fund.description}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="balance"
          content={formatCurrency(fund.currentBalance.balance)}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <ColumnCell
          key="available"
          content={formatCurrency(
            fund.currentBalance.balance -
              fund.currentBalance.pendingDebitAmount,
          )}
          align="left"
          isLoading={isLoading}
          isError={error !== null}
        />,
        <FundListFrameActionColumn
          key="actions"
          fund={fund}
          isLoading={isLoading}
          error={error}
          setDialog={setDialog}
          setMessage={setMessage}
          refetch={refetch}
        />,
      ]}
      getId={(fund: Fund) => fund.id}
      data={funds}
    >
      {dialog}
      <SuccessAlert message={message} />
      <ErrorAlert error={error} />
    </ListFrame>
  );
};

export default FundListFrame;
