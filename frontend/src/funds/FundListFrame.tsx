import { AddCircleOutline, ArrowForwardIos } from "@mui/icons-material";
import { type JSX, useState } from "react";
import ColumnButton from "@framework/listframe/ColumnButton";
import type ColumnDefinition from "@framework/listframe/ColumnDefinition";
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

  const columns: ColumnDefinition<Fund>[] = [
    {
      name: "name",
      headerContent: "Name",
      getBodyContent: (fund: Fund) => fund.name,
    },
    {
      name: "description",
      headerContent: "Description",
      getBodyContent: (fund: Fund) => fund.description,
    },
    {
      name: "balance",
      headerContent: "Posted Balance",
      getBodyContent: (fund: Fund) =>
        formatCurrency(fund.currentBalance.postedBalance),
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
      isLoading={isLoading}
      isError={error !== null}
    >
      {dialog}
      <SuccessAlert message={message} />
      <ErrorAlert error={error} />
    </ListFrame>
  );
};

export default FundListFrame;
