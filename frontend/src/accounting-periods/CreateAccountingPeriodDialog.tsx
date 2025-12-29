import { type JSX, useState } from "react";
import { Button } from "@mui/material";
import Dialog from "@framework/dialog/Dialog";
import ErrorAlert from "@framework/alerts/ErrorAlert";
import { ErrorCode } from "@data/api";
import IntegerEntryField from "@framework/dialog/IntegerEntryField";
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
  return (
    <Dialog
      title="Add Accounting Period"
      content={
        <>
          <IntegerEntryField
            label="Year"
            value={year}
            setValue={setYear}
            error={
              error?.details.find(
                (detail) =>
                  detail.errorCode === ErrorCode.InvalidAccountingPeriodYear,
              ) ?? null
            }
          />
          <IntegerEntryField
            label="Month"
            value={month}
            setValue={setMonth}
            error={
              error?.details.find(
                (detail) =>
                  detail.errorCode === ErrorCode.InvalidAccountingPeriodMonth,
              ) ?? null
            }
          />
          <ErrorAlert
            error={error}
            detailFilter={(detail) =>
              detail.errorCode !== ErrorCode.InvalidAccountingPeriodYear &&
              detail.errorCode !== ErrorCode.InvalidAccountingPeriodMonth
            }
          />
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
