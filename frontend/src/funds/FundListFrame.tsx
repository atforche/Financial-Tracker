import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type JSX, useState } from "react";
import ColumnButton from "@framework/listframe/ColumnButton";
import ColumnCell from "@framework/listframe/ColumnCell";
import ColumnHeader from "@framework/listframe/ColumnHeader";
import ColumnHeaderButton from "@framework/listframe/ColumnHeaderButton";
import CreateOrUpdateFundDialog from "@funds/CreateOrUpdateFundDialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import type { Fund } from "@funds/ApiTypes";
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
  const { funds, isLoading, error, refetch } = useGetAllFunds();
  return (
    <ListFrame<Fund>
      name="Funds"
      headers={[
        <ColumnHeader key="name" content="Name" align="left" />,
        <ColumnHeader
          key="description"
          content="Description"
          align="left"
        />,
        <ColumnHeader
          key="balance"
          content="Posted Balance"
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
          }
          align="right"
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
          key="view"
          content={
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
          }
          align="right"
          isLoading={isLoading}
          isError={error !== null}
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
