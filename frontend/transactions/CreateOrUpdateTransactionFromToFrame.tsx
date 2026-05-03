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
  readonly setDebitAccount: ((account: Account | null) => void) | null;
  readonly creditAccount: Account | null;
  readonly setCreditAccount: ((account: Account | null) => void) | null;
  readonly funds: Fund[];
  readonly debitFund: Fund | null;
  readonly setDebitFund: ((fund: Fund | null) => void) | null;
  readonly creditFund: Fund | null;
  readonly setCreditFund: ((fund: Fund | null) => void) | null;
}

/**
 * Component that displays the "From" and "To" frames for creating or updating a transaction.
 */
const CreateOrUpdateTransactionFromToFrame = function ({
  accounts,
  debitAccount,
  setDebitAccount,
  creditAccount,
  setCreditAccount,
  funds,
  debitFund,
  setDebitFund,
  creditFund,
  setCreditFund,
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
    if (creditAccount !== null) {
      return "accountType" in option;
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
              setDebitAccount === null || setDebitFund === null
                ? null
                : (newValue): void => {
                    if (newValue === null) {
                      setDebitAccount(null);
                      setDebitFund(null);
                    } else if ("type" in newValue) {
                      setDebitAccount(newValue);
                      setDebitFund(null);
                    } else {
                      setDebitAccount(null);
                      setDebitFund(newValue);
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
              setCreditAccount === null || setCreditFund === null
                ? null
                : (newValue): void => {
                    if (newValue === null) {
                      setCreditAccount(null);
                      setCreditFund(null);
                    } else if ("type" in newValue) {
                      setCreditAccount(newValue);
                      setCreditFund(null);
                    } else {
                      setCreditAccount(null);
                      setCreditFund(newValue);
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
