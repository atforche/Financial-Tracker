import "@/transactions/CreateOrUpdateTransactionFromToFrame.css";
import type { Account } from "@/accounts/types";
import AccountOrFundEntryField from "@/transactions/AccountOrFundEntryField";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import type { Fund } from "@/funds/types";
import type { JSX } from "react";
import { Stack } from "@mui/material";

/**
 * Props for the CreateOrUpdateTransactionFromToFrame component.
 */
interface CreateOrUpdateTransactionFromToFrameProps {
  readonly accounts: Account[];
  readonly debitAccount: Account | null;
  readonly creditAccount: Account | null;
  readonly funds: Fund[];
  readonly debitFund: Fund | null;
  readonly creditFund: Fund | null;
  readonly setDebitFrom: ((newDebitAccount: Account | null, newDebitFund: Fund | null) => void) | null;
  readonly setCreditTo: ((newCreditAccount: Account | null, newCreditFund: Fund | null) => void) | null;
}

/**
 * Component that displays the "From" and "To" frames for creating or updating a transaction.
 */
const CreateOrUpdateTransactionFromToFrame = function ({
  accounts,
  debitAccount,
  creditAccount,
  funds,
  debitFund,
  creditFund,
  setDebitFrom,
  setCreditTo,
}: CreateOrUpdateTransactionFromToFrameProps): JSX.Element {
  const debitFromFilter = function (option: Account | Fund): boolean {
    if (creditFund !== null) {
      return !("type" in option);
    }
    if (creditAccount !== null) {
      return "type" in option;
    }
    return true;
  };

  const creditToFilter = function (option: Account | Fund): boolean {
    if (debitFund !== null) {
      return !("type" in option);
    }
    if (debitAccount !== null) {
      return "type" in option;
    }
    return true;
  };

  return (
    <CaptionedFrame caption="From / To">
      <div className="create-or-update-transaction-from-to-frame">
        <Stack
          direction="row"
          spacing={2}
          sx={{ marginTop: 2, maxWidth: "100%" }}
        >
          <AccountOrFundEntryField
            label="Debit From"
            options={[...accounts, ...funds]}
            value={debitAccount ?? debitFund}
            setValue={
              setDebitFrom === null
                ? null
                : (newValue): void => {
                    if (newValue === null) {
                      setDebitFrom(null, null);
                    } else if ("type" in newValue) {
                      setDebitFrom(newValue, null);
                    } else {
                      setDebitFrom(null, newValue);
                    }
                  }
            }
            filter={debitFromFilter}
          />
          <AccountOrFundEntryField
            label="Credit To"
            options={[...accounts, ...funds]}
            value={creditAccount ?? creditFund}
            setValue={
              setCreditTo === null
                ? null
                : (newValue): void => {
                    if (newValue === null) {
                      setCreditTo(null, null);
                    } else if ("type" in newValue) {
                      setCreditTo(newValue, null);
                    } else {
                      setCreditTo(null, newValue);
                    }
                  }
            }
            filter={creditToFilter}
          />
        </Stack>
      </div>
    </CaptionedFrame>
  );
};

export default CreateOrUpdateTransactionFromToFrame;
