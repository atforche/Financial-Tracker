import type { Account, AccountIdentifier } from "@/accounts/types";
import AccountEntryField from "@/accounts/AccountEntryField";
import { Box } from "@mui/material";
import type { JSX } from "react";
import TransactionSection from "@/transactions/workspace/TransactionSection";

interface TransactionAccountPairSectionProps {
  readonly title: string;
  readonly description: string;
  readonly accounts: Account[];
  readonly leftLabel: string;
  readonly rightLabel: string;
  readonly leftAccount: Account | null;
  readonly rightAccount: Account | null;
  readonly setLeftAccount: ((account: Account | null) => void) | null;
  readonly setRightAccount: ((account: Account | null) => void) | null;
  readonly leftFilter?: ((account: AccountIdentifier) => boolean) | null;
  readonly rightFilter?: ((account: AccountIdentifier) => boolean) | null;
}

/**
 * Displays a pair of account selectors for transaction source and destination fields.
 */
const TransactionAccountPairSection = function ({
  title,
  description,
  accounts,
  leftLabel,
  rightLabel,
  leftAccount,
  rightAccount,
  setLeftAccount,
  setRightAccount,
  leftFilter = null,
  rightFilter = null,
}: TransactionAccountPairSectionProps): JSX.Element {
  return (
    <TransactionSection title={title} description={description}>
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns:
            "repeat(auto-fit, minmax(min(100%, 240px), 1fr))",
        }}
      >
        <AccountEntryField
          label={leftLabel}
          options={accounts}
          value={leftAccount}
          setValue={
            setLeftAccount === null
              ? null
              : (newValue): void => {
                  setLeftAccount(
                    accounts.find((account) => account.id === newValue?.id) ??
                      null,
                  );
                }
          }
          filter={leftFilter}
        />
        <AccountEntryField
          label={rightLabel}
          options={accounts}
          value={rightAccount}
          setValue={
            setRightAccount === null
              ? null
              : (newValue): void => {
                  setRightAccount(
                    accounts.find((account) => account.id === newValue?.id) ??
                      null,
                  );
                }
          }
          filter={rightFilter}
        />
      </Box>
    </TransactionSection>
  );
};

export default TransactionAccountPairSection;
