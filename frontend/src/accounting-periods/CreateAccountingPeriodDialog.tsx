import { type JSX, useState } from "react";
import ApiErrorHandler from "@data/ApiErrorHandler";
import { Button } from "@mui/material";
import type { CreateAccountingPeriodRequest } from "@accounting-periods/ApiTypes";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import IntegerEntryField from "@framework/dialog/IntegerEntryField";
import nameof from "@data/nameof";
import useCreateAccountingPeriod from "@accounting-periods/useCreateAccountingPeriod";

/**
 * Props for the CreateAccountingPeriodDialog component.
 * @param onClose - Callback to perform when this dialog is closed.
 */
interface CreateAccountingPeriodDialogProps {
  readonly onClose: (success: boolean) => void;
}

/**
 * Component that presents the user with a dialog to create an Accounting Period.
 * @param props - Props for the CreateAccountingPeriodDialog component.
 * @returns JSX element representing the CreateAccountingPeriodDialog.
 */
const CreateAccountingPeriodDialog = function ({
  onClose,
}: CreateAccountingPeriodDialogProps): JSX.Element {
  const [year, setYear] = useState<number | null>(null);
  const [month, setMonth] = useState<number | null>(null);
  const { isRunning, isSuccess, error, createAccountingPeriod } =
    useCreateAccountingPeriod({
      year: year ?? 0,
      month: month ?? 0,
    });
  if (isSuccess) {
    onClose(true);
  }
  const errorHandler = error ? new ApiErrorHandler(error) : null;
  return (
    <Dialog
      title="Add Accounting Period"
      content={
        <>
          <IntegerEntryField
            label="Year"
            value={year}
            setValue={setYear}
            errorHandler={errorHandler}
            errorKey={nameof<CreateAccountingPeriodRequest>("year")}
          />
          <IntegerEntryField
            label="Month"
            value={month}
            setValue={setMonth}
            errorHandler={errorHandler}
            errorKey={nameof<CreateAccountingPeriodRequest>("month")}
          />
          <ErrorAlert errorHandler={errorHandler} />
        </>
      }
      actions={
        <>
          <Button
            onClick={() => {
              onClose(false);
            }}
            disabled={isRunning}
          >
            Cancel
          </Button>
          <Button onClick={createAccountingPeriod} disabled={isRunning}>
            Add
          </Button>
        </>
      }
    />
  );
};

export default CreateAccountingPeriodDialog;
