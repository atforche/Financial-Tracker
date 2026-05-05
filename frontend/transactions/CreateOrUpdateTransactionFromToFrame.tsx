import "@/transactions/CreateOrUpdateTransactionFromToFrame.css";
import type { Account, AccountIdentifier } from "@/accounts/types";
import type { Fund, FundIdentifier } from "@/funds/types";
import { type JSX, useState } from "react";
import AccountEntryField from "@/accounts/AccountEntryField";
import CaptionedFrame from "@/framework/view/CaptionedFrame";
import { ComboBoxEntryField } from "@/framework/forms/ComboBoxEntryField";
import FundEntryField from "@/funds/FundEntryField";
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
  readonly setDebitFrom:
    | ((newDebitAccount: Account | null, newDebitFund: Fund | null) => void)
    | null;
  readonly setCreditTo:
    | ((newCreditAccount: Account | null, newCreditFund: Fund | null) => void)
    | null;
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
  const [type, setType] = useState<"Account" | "Fund" | null>("Account");

  const debitFromAccountFilter = function (option: AccountIdentifier): boolean {
    if (creditAccount !== null) {
      return option.id !== creditAccount.id;
    }
    return true;
  };

  const debitFromFundFilter = function (option: FundIdentifier): boolean {
    if (option.name === "Unassigned") {
      return false;
    }
    if (creditFund !== null) {
      return option.id !== creditFund.id;
    }
    return true;
  };

  const creditToAccountFilter = function (option: AccountIdentifier): boolean {
    if (debitAccount !== null) {
      return option.id !== debitAccount.id;
    }
    return true;
  };

  const creditToFundFilter = function (option: FundIdentifier): boolean {
    if (option.name === "Unassigned") {
      return false;
    }
    if (debitFund !== null) {
      return option.id !== debitFund.id;
    }
    return true;
  };

  const onTypeChange = function (newType: "Account" | "Fund" | null): void {
    setType(newType);
    if (newType === "Account") {
      setDebitFrom?.(debitAccount, null);
      setCreditTo?.(creditAccount, null);
    } else if (newType === "Fund") {
      setDebitFrom?.(null, debitFund);
      setCreditTo?.(null, creditFund);
    } else {
      setDebitFrom?.(null, null);
      setCreditTo?.(null, null);
    }
  };

  return (
    <CaptionedFrame caption="From / To">
      <div className="create-or-update-transaction-from-to-frame">
        <Stack spacing={2} sx={{ marginTop: 2, maxWidth: "100%" }}>
          <ComboBoxEntryField<"Account" | "Fund">
            label="Account / Fund"
            options={[
              { label: "Account", value: "Account" },
              { label: "Fund", value: "Fund" },
            ]}
            value={
              type === null
                ? { label: "", value: null }
                : { label: type, value: type }
            }
            setValue={(newValue): void => {
              onTypeChange(newValue?.value ?? null);
            }}
          />
          {type === "Account" && (
            <Stack direction="row" spacing={2} sx={{ maxWidth: "100%" }}>
              <AccountEntryField
                label="Debit From"
                options={accounts}
                value={debitAccount}
                setValue={
                  setDebitFrom === null
                    ? null
                    : (newValue): void => {
                        setDebitFrom(
                          accounts.find(
                            (account) => account.id === newValue?.id,
                          ) ?? null,
                          null,
                        );
                      }
                }
                filter={debitFromAccountFilter}
              />
              <AccountEntryField
                label="Credit To"
                options={accounts}
                value={creditAccount}
                setValue={
                  setCreditTo === null
                    ? null
                    : (newValue): void => {
                        setCreditTo(
                          accounts.find(
                            (account) => account.id === newValue?.id,
                          ) ?? null,
                          null,
                        );
                      }
                }
                filter={creditToAccountFilter}
              />
            </Stack>
          )}
          {type === "Fund" && (
            <Stack direction="row" spacing={2} sx={{ maxWidth: "100%" }}>
              <FundEntryField
                label="Debit From"
                options={funds}
                value={debitFund}
                setValue={
                  setDebitFrom === null
                    ? null
                    : (newValue): void => {
                        setDebitFrom(
                          null,
                          funds.find((fund) => fund.id === newValue?.id) ??
                            null,
                        );
                      }
                }
                filter={debitFromFundFilter}
              />
              <FundEntryField
                label="Credit To"
                options={funds}
                value={creditFund}
                setValue={
                  setCreditTo === null
                    ? null
                    : (newValue): void => {
                        setCreditTo(
                          null,
                          funds.find((fund) => fund.id === newValue?.id) ??
                            null,
                        );
                      }
                }
                filter={creditToFundFilter}
              />
            </Stack>
          )}
        </Stack>
      </div>
    </CaptionedFrame>
  );
};

export default CreateOrUpdateTransactionFromToFrame;
